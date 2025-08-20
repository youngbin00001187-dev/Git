using UnityEngine;
using System.Collections.Generic;
using System; // Action 이벤트를 위해 필요
using System.Linq; // LINQ 확장 메서드를 위해 필요 (Sum 등)

public class CookingManager : MonoBehaviour
{
    public static CookingManager instance;

    // 요리가 완전히 완료되고 평가 결과가 나왔을 때 발행 (보상 패널, 애니메이션 등 트리거)
    public static event Action<CustomerData.CookingTier, CustomerOrder> OnCookingCompleted;

    [Header("요리 설정")]
    public int maxIngredientsInPot = 10; // 냄비에 들어갈 수 있는 최대 재료 수

    public CustomerOrder activeCustomerOrder;

    [Header("현재 상태 (디버그용)")]
    public List<IngredientData> currentPotIngredients = new List<IngredientData>(); // 냄비에 들어있는 재료 리스트


    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 냄비에 재료를 추가하는 함수 (이제 인벤토리에서 즉시 차감)
    public void AddIngredientToPot(IngredientData ingredient)
    {
        // 1. 냄비에 재료가 최대 개수 이상인지 확인
        if (currentPotIngredients.Count >= maxIngredientsInPot)
        {
            Debug.LogWarning("냄비에 재료를 더 이상 추가할 수 없습니다! (최대 " + maxIngredientsInPot + "개)");
            return;
        }

        // === 변경된 부분: GlobalManager 인벤토리에서 재료 존재 및 수량 확인 후 차감 ===
        if (GlobalManager.instance != null)
        {
            if (GlobalManager.instance.inventory.ContainsKey(ingredient))
            {
                // 무한 재료는 차감 로직을 건너뜀
                if (GlobalManager.instance.inventory[ingredient] != int.MaxValue)
                {
                    GlobalManager.instance.inventory[ingredient]--;
                    Debug.Log($"인벤토리에서 '{ingredient.ingredientName}' 1개 차감됨. 남은 수량: {GlobalManager.instance.inventory[ingredient]}");

                    // 수량이 0이 되면 인벤토리에서 제거
                    if (GlobalManager.instance.inventory[ingredient] <= 0)
                    {
                        GlobalManager.instance.inventory.Remove(ingredient);
                        Debug.Log($"인벤토리에서 '{ingredient.ingredientName}'이(가) 모두 소진되어 제거되었습니다.");
                    }
                }
                else
                {
                    Debug.Log($"'{ingredient.ingredientName}'은(는) 무한 재료이므로 인벤토리에서 차감하지 않습니다.");
                }
            }
            else
            {
                Debug.LogWarning($"인벤토리에 '{ingredient.ingredientName}' 재료가 없어 냄비에 추가할 수 없습니다. (차감 불가)");
                return; // 인벤토리에 없으면 추가하지 않음
            }
        }
        else
        {
            Debug.LogError("GlobalManager 인스턴스를 찾을 수 없어 인벤토리 차감 로직을 수행할 수 없습니다.");
            return; // GlobalManager 없으면 추가하지 않음
        }

        // 3. 인벤토리에서 성공적으로 차감/확인되었으면 냄비에 추가
        currentPotIngredients.Add(ingredient);
        Debug.Log("냄비에 '" + ingredient.ingredientName + "' 추가! 현재 재료 수: " + currentPotIngredients.Count);

        // OnPotChanged 이벤트는 이전에 제거되었거나 사용되지 않고 있으므로 주석 처리 (현재 TycoonUIManager에서 직접 업데이트)
        // OnPotChanged?.Invoke(); 
    }

    public void ClearPot()
    {
        currentPotIngredients.Clear();
        Debug.Log("냄비를 비웠습니다.");

        if (TycoonUIManager.instance != null)
        {
            TycoonUIManager.instance.ClearPotUI(); // UI에서도 냄비 비우기
        }
        // OnPotChanged 이벤트는 이전에 제거되었거나 사용되지 않고 있으므로 주석 처리
        // OnPotChanged?.Invoke(); 
    }

    // '요리하기' 버튼을 눌렀을 때 호출될 최종 요리 함수
    public void FinalizeCooking()
    {
        if (currentPotIngredients.Count == 0)
        {
            Debug.Log("냄비가 비어있습니다!");
            return;
        }

        Dictionary<string, int> finalProperties = new Dictionary<string, int>();

        // === 변경된 부분: FinalizeCooking에서는 재료 속성 계산만 하고 차감은 하지 않습니다. ===
        // 차감은 AddIngredientToPot에서 이미 처리되었습니다.
        foreach (IngredientData ingredient in currentPotIngredients)
        {
            foreach (IngredientProperty property in ingredient.properties)
            {
                // TODO: 숙련도 보너스 및 곱셈/전역 배율 로직은 여기에 추가되어야 합니다.
                // 현재 코드에서는 이 부분이 빠져 있으므로, 이전 논의에 따라 다시 추가해야 합니다.
                if (finalProperties.ContainsKey(property.propertyName))
                {
                    finalProperties[property.propertyName] += property.value;
                }
                else
                {
                    finalProperties.Add(property.propertyName, property.value);
                }
            }
        }
        // ==============================================================================

        Debug.Log("===== 요리 완성! =====");
        foreach (var finalProp in finalProperties)
        {
            Debug.Log(finalProp.Key + ": " + finalProp.Value);
        }
        Debug.Log("======================");

        if (activeCustomerOrder != null)
        {
            CustomerData.CookingTier resultTier = EvaluateDishQuality(finalProperties, activeCustomerOrder);
            Debug.Log($"요리 결과: {resultTier}!");

            OnCookingCompleted?.Invoke(resultTier, activeCustomerOrder);
            Debug.Log("요리 완성 이벤트 발행됨!");
        }
        else
        {
            Debug.LogWarning("현재 활성화된 손님 주문이 없습니다. 요리 평가를 건너뜀.");
        }

        ClearPot(); // 냄비 비우기

        if (TycoonUIManager.instance != null)
        {
            TycoonUIManager.instance.CloseCookingUI();
            Debug.Log("요리 패널이 닫혔습니다.");
        }
    }

    private CustomerData.CookingTier EvaluateDishQuality(Dictionary<string, int> dishProperties, CustomerOrder order)
    {
        if (order.desiredProperties == null || order.desiredProperties.Length == 0)
        {
            Debug.LogWarning("손님 주문에 요구 속성이 정의되어 있지 않습니다. 평범으로 처리.");
            return CustomerData.CookingTier.Normal;
        }

        int satisfiedPropertyCount = 0;

        foreach (CustomerPropertyRequirement req in order.desiredProperties)
        {
            if (dishProperties.TryGetValue(req.propertyName, out int actualValue))
            {
                if (actualValue >= req.requiredValue)
                {
                    satisfiedPropertyCount++;
                    Debug.Log($"속성 '{req.propertyName}': 요구치 {req.requiredValue}, 실제 {actualValue} -> 만족!");
                }
                else
                {
                    Debug.Log($"속성 '{req.propertyName}': 요구치 {req.requiredValue}, 실제 {actualValue} -> 불만족.");
                }
            }
            else
            {
                Debug.Log($"속성 '{req.propertyName}': 요리에 해당 속성이 없습니다 -> 불만족.");
            }
        }

        Debug.Log($"총 만족시킨 속성 개수: {satisfiedPropertyCount} / {order.desiredProperties.Length}");

        if (satisfiedPropertyCount >= order.greatSuccessThreshold)
        {
            return CustomerData.CookingTier.GreatSuccess;
        }
        else if (satisfiedPropertyCount >= order.successThreshold)
        {
            return CustomerData.CookingTier.Success;
        }
        else if (satisfiedPropertyCount >= order.normalThreshold)
        {
            return CustomerData.CookingTier.Normal;
        }
        else
        {
            return CustomerData.CookingTier.Failure;
        }
    }
}