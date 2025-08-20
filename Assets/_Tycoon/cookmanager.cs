using UnityEngine;
using System.Collections.Generic;
using System; // Action �̺�Ʈ�� ���� �ʿ�
using System.Linq; // LINQ Ȯ�� �޼��带 ���� �ʿ� (Sum ��)

public class CookingManager : MonoBehaviour
{
    public static CookingManager instance;

    // �丮�� ������ �Ϸ�ǰ� �� ����� ������ �� ���� (���� �г�, �ִϸ��̼� �� Ʈ����)
    public static event Action<CustomerData.CookingTier, CustomerOrder> OnCookingCompleted;

    [Header("�丮 ����")]
    public int maxIngredientsInPot = 10; // ���� �� �� �ִ� �ִ� ��� ��

    public CustomerOrder activeCustomerOrder;

    [Header("���� ���� (����׿�)")]
    public List<IngredientData> currentPotIngredients = new List<IngredientData>(); // ���� ����ִ� ��� ����Ʈ


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

    // ���� ��Ḧ �߰��ϴ� �Լ� (���� �κ��丮���� ��� ����)
    public void AddIngredientToPot(IngredientData ingredient)
    {
        // 1. ���� ��ᰡ �ִ� ���� �̻����� Ȯ��
        if (currentPotIngredients.Count >= maxIngredientsInPot)
        {
            Debug.LogWarning("���� ��Ḧ �� �̻� �߰��� �� �����ϴ�! (�ִ� " + maxIngredientsInPot + "��)");
            return;
        }

        // === ����� �κ�: GlobalManager �κ��丮���� ��� ���� �� ���� Ȯ�� �� ���� ===
        if (GlobalManager.instance != null)
        {
            if (GlobalManager.instance.inventory.ContainsKey(ingredient))
            {
                // ���� ���� ���� ������ �ǳʶ�
                if (GlobalManager.instance.inventory[ingredient] != int.MaxValue)
                {
                    GlobalManager.instance.inventory[ingredient]--;
                    Debug.Log($"�κ��丮���� '{ingredient.ingredientName}' 1�� ������. ���� ����: {GlobalManager.instance.inventory[ingredient]}");

                    // ������ 0�� �Ǹ� �κ��丮���� ����
                    if (GlobalManager.instance.inventory[ingredient] <= 0)
                    {
                        GlobalManager.instance.inventory.Remove(ingredient);
                        Debug.Log($"�κ��丮���� '{ingredient.ingredientName}'��(��) ��� �����Ǿ� ���ŵǾ����ϴ�.");
                    }
                }
                else
                {
                    Debug.Log($"'{ingredient.ingredientName}'��(��) ���� ����̹Ƿ� �κ��丮���� �������� �ʽ��ϴ�.");
                }
            }
            else
            {
                Debug.LogWarning($"�κ��丮�� '{ingredient.ingredientName}' ��ᰡ ���� ���� �߰��� �� �����ϴ�. (���� �Ұ�)");
                return; // �κ��丮�� ������ �߰����� ����
            }
        }
        else
        {
            Debug.LogError("GlobalManager �ν��Ͻ��� ã�� �� ���� �κ��丮 ���� ������ ������ �� �����ϴ�.");
            return; // GlobalManager ������ �߰����� ����
        }

        // 3. �κ��丮���� ���������� ����/Ȯ�εǾ����� ���� �߰�
        currentPotIngredients.Add(ingredient);
        Debug.Log("���� '" + ingredient.ingredientName + "' �߰�! ���� ��� ��: " + currentPotIngredients.Count);

        // OnPotChanged �̺�Ʈ�� ������ ���ŵǾ��ų� ������ �ʰ� �����Ƿ� �ּ� ó�� (���� TycoonUIManager���� ���� ������Ʈ)
        // OnPotChanged?.Invoke(); 
    }

    public void ClearPot()
    {
        currentPotIngredients.Clear();
        Debug.Log("���� ������ϴ�.");

        if (TycoonUIManager.instance != null)
        {
            TycoonUIManager.instance.ClearPotUI(); // UI������ ���� ����
        }
        // OnPotChanged �̺�Ʈ�� ������ ���ŵǾ��ų� ������ �ʰ� �����Ƿ� �ּ� ó��
        // OnPotChanged?.Invoke(); 
    }

    // '�丮�ϱ�' ��ư�� ������ �� ȣ��� ���� �丮 �Լ�
    public void FinalizeCooking()
    {
        if (currentPotIngredients.Count == 0)
        {
            Debug.Log("���� ����ֽ��ϴ�!");
            return;
        }

        Dictionary<string, int> finalProperties = new Dictionary<string, int>();

        // === ����� �κ�: FinalizeCooking������ ��� �Ӽ� ��길 �ϰ� ������ ���� �ʽ��ϴ�. ===
        // ������ AddIngredientToPot���� �̹� ó���Ǿ����ϴ�.
        foreach (IngredientData ingredient in currentPotIngredients)
        {
            foreach (IngredientProperty property in ingredient.properties)
            {
                // TODO: ���õ� ���ʽ� �� ����/���� ���� ������ ���⿡ �߰��Ǿ�� �մϴ�.
                // ���� �ڵ忡���� �� �κ��� ���� �����Ƿ�, ���� ���ǿ� ���� �ٽ� �߰��ؾ� �մϴ�.
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

        Debug.Log("===== �丮 �ϼ�! =====");
        foreach (var finalProp in finalProperties)
        {
            Debug.Log(finalProp.Key + ": " + finalProp.Value);
        }
        Debug.Log("======================");

        if (activeCustomerOrder != null)
        {
            CustomerData.CookingTier resultTier = EvaluateDishQuality(finalProperties, activeCustomerOrder);
            Debug.Log($"�丮 ���: {resultTier}!");

            OnCookingCompleted?.Invoke(resultTier, activeCustomerOrder);
            Debug.Log("�丮 �ϼ� �̺�Ʈ �����!");
        }
        else
        {
            Debug.LogWarning("���� Ȱ��ȭ�� �մ� �ֹ��� �����ϴ�. �丮 �򰡸� �ǳʶ�.");
        }

        ClearPot(); // ���� ����

        if (TycoonUIManager.instance != null)
        {
            TycoonUIManager.instance.CloseCookingUI();
            Debug.Log("�丮 �г��� �������ϴ�.");
        }
    }

    private CustomerData.CookingTier EvaluateDishQuality(Dictionary<string, int> dishProperties, CustomerOrder order)
    {
        if (order.desiredProperties == null || order.desiredProperties.Length == 0)
        {
            Debug.LogWarning("�մ� �ֹ��� �䱸 �Ӽ��� ���ǵǾ� ���� �ʽ��ϴ�. ������� ó��.");
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
                    Debug.Log($"�Ӽ� '{req.propertyName}': �䱸ġ {req.requiredValue}, ���� {actualValue} -> ����!");
                }
                else
                {
                    Debug.Log($"�Ӽ� '{req.propertyName}': �䱸ġ {req.requiredValue}, ���� {actualValue} -> �Ҹ���.");
                }
            }
            else
            {
                Debug.Log($"�Ӽ� '{req.propertyName}': �丮�� �ش� �Ӽ��� �����ϴ� -> �Ҹ���.");
            }
        }

        Debug.Log($"�� ������Ų �Ӽ� ����: {satisfiedPropertyCount} / {order.desiredProperties.Length}");

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