using UnityEngine;
using TMPro; // TextMeshPro를 사용하기 위해 필요
using System.Collections.Generic;
using UnityEngine.UI; // Image 컴포넌트를 사용하기 위해 필요
using DG.Tweening; // DOTween 사용을 위해 필요
using System; // Action 이벤트를 위해 추가
using System.Collections; // 코루틴을 위해 추가

public class TycoonUIManager : MonoBehaviour
{
    public static TycoonUIManager instance;

    public static event Action OnDayUIReadyAndRequestCustomerSpawn;

    [Header("보상 패널 UI")]
    public GameObject rewardPanel; // 보상 패널의 최상위 부모 GameObject
    public Transform dishSpawnPointForAnimation; // 요리 아이콘이 냄비에서 나올 시작 위치
    public Transform customerDishReceivePoint; // 요리 아이콘이 손님에게 도착할 위치
    public float dishAnimationDuration = 0.8f;
    [Tooltip("요리 도착 후 흔들리는 애니메이션 지속 시간.")]
    public float dishShakeDuration = 0.3f;
    [Tooltip("요리 도착 후 흔들리는 강도.")]
    public float dishShakeStrength = 10f;
    [Tooltip("요리 도착 후 사라지는 애니메이션 지속 시간.")]
    public float dishFadeOutDuration = 0.3f;

    [Header("자원 표시 UI")]
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI reputationText;

    [Header("요리 UI 요소")]
    public GameObject cookingUIPanel;
    public Transform ingredientGridParent;
    public GameObject ingredientButtonPrefab;
    public Transform potIngredientsParent;

    [Header("애니메이션 공통 프리펩")]
    [Tooltip("모든 재료가 냄비로 날아갈 때 사용될 공통 UI 이미지 프리펩입니다. Image 컴포넌트를 포함해야 합니다.")]
    public GameObject animatedFlightImagePrefab;

    [Header("냄비 연출 설정")]
    public float ingredientMoveDuration = 0.5f;

    [Header("전투 UI 요소")]
    [Tooltip("요리 패널 활성화 시 비활성화할 공격 버튼 GameObject를 연결하세요.")]
    public GameObject attackButtonGameObject;

    [Tooltip("요리 패널 안에 있는 '요리하기' 버튼을 연결하세요.")]
    public Button cookButton;

    [Header("페이즈별 UI 패널")]
    [Tooltip("낮 활동에 필요한 모든 UI 요소의 부모 GameObject를 연결하세요.")]
    public GameObject dayUIPanel;
    [Tooltip("밤 활동에 필요한 모든 UI 요소의 부모 GameObject를 연결하세요.")]
    public GameObject nightUIPanel;

    public enum UIMode { None, Day, Night }

    private List<GameObject> currentPotUIObjects = new List<GameObject>();


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

    void OnEnable()
    {
        DayManager.OnDayStarted += OnDayStartedHandler;
        DayManager.OnNightPhaseEntered += OnNightPhaseEnteredHandler;
        CookingManager.OnCookingCompleted += OnCookingCompletedHandler;
        DayManager.OnCookingFinishedAndEvaluate += HandleCookedDishResult;
    }

    void OnDisable()
    {
        DayManager.OnDayStarted -= OnDayStartedHandler;
        DayManager.OnNightPhaseEntered -= OnNightPhaseEnteredHandler;
        CookingManager.OnCookingCompleted -= OnCookingCompletedHandler;
        DayManager.OnCookingFinishedAndEvaluate -= HandleCookedDishResult;
    }

    private void OnDayStartedHandler()
    {
        SetUIMode(UIMode.Day);
    }

    private void OnNightPhaseEnteredHandler()
    {
        SetUIMode(UIMode.Night);
    }

    // === 변경된 부분: 올바른 함수 이름 호출 ===
    private void OnCookingCompletedHandler(CustomerData.CookingTier tier, CustomerOrder order)
    {
        Debug.Log("TycoonUIManager: 요리 완성 이벤트 수신!");
        ShowRewardPanelMinimal(tier, order); // ShowRewardPanel -> ShowRewardPanelMinimal
        StartCoroutine(AnimateDishToCustomerRoutineMinimal(tier, order)); // AnimateDishToCustomerRoutine -> AnimateDishToCustomerRoutineMinimal
    }
    // ======================================


    void UpdatePotUI()
    {
        // Debug.Log("UpdatePotUI 호출됨 (현재 아무 동작 없음)");
    }

    void Start()
    {
        if (cookingUIPanel != null)
        {
            cookingUIPanel.SetActive(false);
        }
        rewardPanel?.SetActive(false);
        UpdateResourceUI();
    }

    public void UpdateResourceUI()
    {
        if (GlobalManager.instance != null)
        {
            goldText.text = "금전: " + GlobalManager.instance.gold;
            reputationText.text = "명성: " + GlobalManager.instance.reputation;
        }
        else
        {
            Debug.LogError("GlobalManager 인스턴스를 찾을 수 없습니다!");
        }
    }

    public void SetUIMode(UIMode mode)
    {
        if (dayUIPanel == null || nightUIPanel == null)
        {
            Debug.LogError("TycoonUIManager: Day UI Panel 또는 Night UI Panel이 할당되지 않았습니다!");
            return;
        }

        switch (mode)
        {
            case UIMode.Day:
                dayUIPanel.SetActive(true);
                nightUIPanel.SetActive(false);
                if (cookingUIPanel != null) cookingUIPanel.SetActive(false);
                if (rewardPanel != null) rewardPanel.SetActive(false);
                if (attackButtonGameObject != null)
                {
                    AttackButtonAnimator animator = attackButtonGameObject.GetComponent<AttackButtonAnimator>();
                    if (animator != null) animator.SetButtonActive(true);
                    else attackButtonGameObject.SetActive(true);
                }
                if (cookButton != null) cookButton.interactable = true;

                Debug.Log("UI 모드 전환: 낮 (경영) UI 활성화");

                OnDayUIReadyAndRequestCustomerSpawn?.Invoke();
                Debug.Log("낮 UI 준비됨, 손님 스폰 요청 이벤트 발행!");
                break;

            case UIMode.Night:
                dayUIPanel.SetActive(false);
                nightUIPanel.SetActive(true);
                if (cookingUIPanel != null) cookingUIPanel.SetActive(false);
                if (rewardPanel != null) rewardPanel.SetActive(false);
                if (attackButtonGameObject != null)
                {
                    AttackButtonAnimator animator = attackButtonGameObject.GetComponent<AttackButtonAnimator>();
                    if (animator != null) animator.SetButtonActive(false);
                    else attackButtonGameObject.SetActive(false);
                }
                if (cookButton != null) cookButton.interactable = false;

                Debug.Log("UI 모드 전환: 밤 (활동) UI 활성화");
                break;
        }
    }

    public void OpenCookingUI()
    {
        Debug.Log("OpenCookingUI Called! Attempting to activate cooking panel.");
        if (cookingUIPanel != null)
        {
            cookingUIPanel.SetActive(true);
            PopulateIngredientGrid();

            if (attackButtonGameObject != null)
            {
                AttackButtonAnimator animator = attackButtonGameObject.GetComponent<AttackButtonAnimator>();
                if (animator != null)
                {
                    animator.SetButtonActive(false);
                }
                else
                {
                    attackButtonGameObject.SetActive(false);
                }
                Debug.Log("공격 버튼 비활성화.");
            }
            if (cookButton != null)
            {
                cookButton.interactable = true;
                Debug.Log("'요리하기' 버튼 활성화.");
            }
        }
        else
        {
            Debug.LogError("Cooking UI Panel is not assigned in TycoonUIManager!");
        }
    }

    public void CloseCookingUI()
    {
        if (cookingUIPanel != null)
        {
            cookingUIPanel.SetActive(false);

            if (attackButtonGameObject != null)
            {
                AttackButtonAnimator animator = attackButtonGameObject.GetComponent<AttackButtonAnimator>();
                if (animator != null)
                {
                    animator.SetButtonActive(true);
                }
                else
                {
                    attackButtonGameObject.SetActive(true);
                }
                Debug.Log("공격 버튼 활성화.");
            }
            if (cookButton != null)
            {
                cookButton.interactable = true;
                Debug.Log("'요리하기' 버튼 활성화.");
            }
        }
    }

    void PopulateIngredientGrid()
    {
        foreach (Transform child in ingredientGridParent)
        {
            Destroy(child.gameObject);
        }

        Dictionary<IngredientData, int> inventory = GlobalManager.instance.inventory;

        foreach (var item in inventory)
        {
            IngredientData data = item.Key;
            int quantity = item.Value;

            GameObject newButton = Instantiate(ingredientButtonPrefab, ingredientGridParent);
            IngredientButtonUI buttonUI = newButton.GetComponent<IngredientButtonUI>();

            if (buttonUI != null)
            {
                buttonUI.Setup(data, quantity);

                newButton.GetComponent<Button>().onClick.AddListener(() =>
                {
                    if (CookingManager.instance != null)
                    {
                        int currentQuantityInInventory = 0;
                        // bool isInfinite = false; // === isInfinite 변수 제거 ===
                        if (GlobalManager.instance.inventory.TryGetValue(data, out currentQuantityInInventory))
                        {
                            if (currentQuantityInInventory == int.MaxValue)
                            {
                                // isInfinite = true; // === isInfinite 변수 사용 제거 ===
                            }
                            else if (currentQuantityInInventory <= 0)
                            {
                                Debug.LogWarning($"인벤토리에 '{data.ingredientName}' 재료가 충분하지 않습니다. (현재 0개)");
                                return;
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"인벤토리에 '{data.ingredientName}' 재료가 없습니다.");
                            return;
                        }

                        if (CookingManager.instance.currentPotIngredients.Count >= CookingManager.instance.maxIngredientsInPot)
                        {
                            Debug.LogWarning("냄비가 가득 차서 재료를 더 이상 추가할 수 없습니다. 애니메이션을 시작하지 않습니다.");
                            return;
                        }

                        CookingManager.instance.AddIngredientToPot(data);
                        TycoonUIManager.instance.AnimateIngredientToPot(buttonUI);

                        int updatedQuantityInInventory = 0;
                        if (GlobalManager.instance.inventory.TryGetValue(data, out updatedQuantityInInventory))
                        {
                            buttonUI.Setup(data, updatedQuantityInInventory);
                        }
                        else
                        {
                            Debug.Log($"'{data.ingredientName}' 재료가 모두 소진되어 재료 패널에서 제거합니다.");
                            Destroy(newButton);
                        }
                    }
                });
            }
            else
            {
                Debug.LogError("재료 버튼 프리팹에 IngredientButtonUI 컴포넌트가 없습니다!");
            }
        }
    }

    public void AnimateIngredientToPot(IngredientButtonUI sourceButtonUI)
    {
        if (sourceButtonUI == null || sourceButtonUI.GetIngredientData() == null || sourceButtonUI.GetIconImage() == null)
        {
            Debug.LogError("AnimateIngredientToPot: 전달된 IngredientButtonUI 또는 그 내부 참조가 null입니다!");
            return;
        }

        RectTransform sourceRect = sourceButtonUI.GetComponent<RectTransform>();
        Image sourceImage = sourceButtonUI.GetIconImage();
        IngredientData ingredientData = sourceButtonUI.GetIngredientData();

        Canvas rootCanvas = sourceRect.GetComponentInParent<Canvas>();

        if (rootCanvas == null)
        {
            Debug.LogError("소스 버튼의 Canvas 컴포넌트를 찾을 수 없습니다.");
            return;
        }

        RectTransform canvasRect = rootCanvas.GetComponent<RectTransform>();

        GameObject animatedIngredientIcon = null;
        Image animatedImageComponent = null;
        RectTransform animatedIconRect = null;

        if (animatedFlightImagePrefab == null)
        {
            Debug.LogError("AnimateIngredientToPot: animatedFlightImagePrefab이 할당되지 않았습니다! 인스펙터 창을 확인하세요.");
            return;
        }

        animatedIngredientIcon = Instantiate(animatedFlightImagePrefab, rootCanvas.transform);
        animatedImageComponent = animatedIngredientIcon.GetComponent<Image>();

        if (animatedImageComponent == null)
        {
            Debug.LogError("AnimateIngredientToPot: animatedFlightImagePrefab에 Image 컴포넌트가 없습니다! Image 컴포넌트가 있는 UI 프리펩을 할당하세요.");
            Destroy(animatedIngredientIcon);
            return;
        }

        Sprite spriteToAnimate = ingredientData.animatedFlightSprite;
        if (spriteToAnimate == null)
        {
            spriteToAnimate = ingredientData.icon;
            if (spriteToAnimate == null)
            {
                Debug.LogError($"AnimateIngredientToPot: '{ingredientData.ingredientName}' 재료에 animatedFlightSprite도 없고 icon도 없습니다! 애니메이션 불가.");
                Destroy(animatedIngredientIcon);
                return;
            }
        }

        animatedImageComponent.sprite = spriteToAnimate;
        animatedImageComponent.SetNativeSize();
        animatedImageComponent.raycastTarget = false;

        animatedIngredientIcon.name = $"{ingredientData.ingredientName}_FlyingIcon";

        animatedIconRect = animatedIngredientIcon.GetComponent<RectTransform>();
        if (animatedIconRect != null)
        {
            animatedIconRect.localScale = Vector3.one;
        }
        else
        {
            Debug.LogWarning("AnimateIngredientToPot: 생성된 애니메이션 오브젝트에 RectTransform이 없습니다. UI 애니메이션이 예상대로 작동하지 않을 수 있습니다.");
        }

        if (animatedIngredientIcon == null)
        {
            Debug.LogError("AnimateIngredientToPot: 애니메이션할 아이콘 GameObject 생성 실패!");
            return;
        }

        Vector2 startLocalPositionInCanvas;
        Camera renderCamera = null;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, RectTransformUtility.WorldToScreenPoint(null, sourceRect.position), renderCamera, out startLocalPositionInCanvas))
        {
            if (animatedIconRect != null)
            {
                animatedIconRect.anchoredPosition = startLocalPositionInCanvas;
            }
            else
            {
                Vector3 worldPosition = canvasRect.transform.TransformPoint(new Vector3(startLocalPositionInCanvas.x, startLocalPositionInCanvas.y, 0));
                animatedIngredientIcon.transform.position = worldPosition;
                Debug.LogWarning("AnimateIngredientToPot: 3D 모델에 대한 시작 위치 설정. World Space Canvas의 깊이를 고려했는지 확인하세요.");
            }
        }
        else
        {
            Debug.LogError($"[AnimateIngredientToPot] Failed to convert screen point to local point in canvas rectangle! Check Canvas Render Mode. Source World Pos: {sourceRect.position}");
            Destroy(animatedIngredientIcon);
            return;
        }

        if (potIngredientsParent == null)
        {
            Debug.LogError("냄비 아이콘이 도달할 'potIngredientsParent'가 TycoonUIManager에 할당되지 않았습니다!");
            Destroy(animatedIngredientIcon);
            return;
        }

        RectTransform potRect = potIngredientsParent.GetComponent<RectTransform>();
        if (potRect == null)
        {
            Debug.LogError("냄비 부모 Transform (potIngredientsParent)에 RectTransform 컴포넌트가 없습니다!");
            Destroy(animatedIngredientIcon);
            return;
        }

        Vector2 randomLocalPosInPot = GetRandomLocalPositionInRect(potRect);
        Vector3 potTargetWorldPos = potRect.TransformPoint(randomLocalPosInPot);

        Vector2 screenTargetPos = RectTransformUtility.WorldToScreenPoint(null, potTargetWorldPos);

        Vector2 targetLocalPositionInCanvas;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenTargetPos, renderCamera, out targetLocalPositionInCanvas))
        {
            if (animatedIconRect != null)
            {
                animatedIconRect.DOAnchorPos(targetLocalPositionInCanvas, ingredientMoveDuration)
                    .SetEase(Ease.OutQuad)
                    .OnComplete(() =>
                    {
                        animatedIngredientIcon.transform.SetParent(potIngredientsParent);
                        animatedIconRect.anchoredPosition = randomLocalPosInPot;
                        currentPotUIObjects.Add(animatedIngredientIcon);
                    });
            }
            else
            {
                animatedIngredientIcon.transform.DOMove(potTargetWorldPos, ingredientMoveDuration)
                    .SetEase(Ease.OutQuad)
                    .OnComplete(() =>
                    {
                        animatedIngredientIcon.transform.SetParent(potIngredientsParent);
                        currentPotUIObjects.Add(animatedIngredientIcon);
                    });
            }
        }
        else
        {
            Debug.LogError($"냄비 패널 내 랜덤 위치를 캔버스 로컬 좌표로 변환 실패! Pot Target World Pos: {potTargetWorldPos}");
            Destroy(animatedIngredientIcon);
        }
    }

    private Vector2 GetRandomLocalPositionInRect(RectTransform rectTransform)
    {
        float paddingX = rectTransform.rect.width * 0.1f;
        float paddingY = rectTransform.rect.height * 0.1f;

        float randomX = UnityEngine.Random.Range(rectTransform.rect.xMin + paddingX, rectTransform.rect.xMax - paddingX);
        float randomY = UnityEngine.Random.Range(rectTransform.rect.yMin + paddingY, rectTransform.rect.yMax - paddingY);

        return new Vector2(randomX, randomY);
    }

    public void ClearPotUI()
    {
        Debug.Log("TycoonUIManager.ClearPotUI() 호출됨. 냄비 UI를 비웁니다.");
        foreach (GameObject uiObject in currentPotUIObjects)
        {
            if (uiObject != null)
            {
                Destroy(uiObject);
            }
        }
        currentPotUIObjects.Clear();
    }

    // 완성 요리 패널을 표시하는 함수 (최소화 버전)
    private void ShowRewardPanelMinimal(CustomerData.CookingTier tier, CustomerOrder order)
    {
        if (rewardPanel == null)
        {
            Debug.LogWarning("TycoonUIManager: rewardPanel이 할당되지 않았습니다. 완성 요리 패널을 표시할 수 없습니다.");
            return;
        }

        rewardPanel.SetActive(true);
        Debug.Log("완성 요리 패널 활성화됨 (최소화된 버전).");
    }

    // 요리 아이콘 애니메이션 코루틴 (최소화된 버전)
    private IEnumerator AnimateDishToCustomerRoutineMinimal(CustomerData.CookingTier tier, CustomerOrder order)
    {
        if (dishSpawnPointForAnimation == null || customerDishReceivePoint == null || animatedFlightImagePrefab == null)
        {
            Debug.LogWarning("TycoonUIManager: 요리 애니메이션을 위한 필수 UI 요소가 할당되지 않았습니다! (최소화 버전) 애니메이션을 실행할 수 없습니다.");
            DayManager.instance.ProceedToRewardProcessingPhase(tier, order);
            yield break;
        }

        GameObject animatedDish = Instantiate(animatedFlightImagePrefab, transform.root);
        Image animatedDishImage = animatedDish.GetComponent<Image>();
        RectTransform animatedDishRect = animatedDish.GetComponent<RectTransform>();

        if (animatedDishImage == null || animatedDishRect == null)
        {
            Debug.LogError("animatedFlightImagePrefab에 Image 또는 RectTransform 컴포넌트가 없습니다! (최소화 버전) 애니메이션 불가.");
            Destroy(animatedDish);
            DayManager.instance.ProceedToRewardProcessingPhase(tier, order);
            yield break;
        }

        animatedDishImage.sprite = null;
        animatedDishImage.raycastTarget = false;
        animatedDish.name = $"Dish_Flying_Minimal";

        animatedDishRect.position = dishSpawnPointForAnimation.position;
        Vector3 targetPosition = customerDishReceivePoint.position;

        // 요리 아이콘 이동 애니메이션
        Sequence dishSequence = DOTween.Sequence(); // 여러 트윈을 순서대로 실행하기 위한 시퀀스
        dishSequence.Append(animatedDishRect.DOMove(targetPosition, dishAnimationDuration).SetEase(Ease.OutQuad));

        // 요리 도착 후 흔들림 애니메이션
        dishSequence.Append(animatedDishRect.DOAnchorPosY(animatedDishRect.anchoredPosition.y + 10f, dishShakeDuration * 0.5f).SetEase(Ease.OutQuad));
        dishSequence.Append(animatedDishRect.DOAnchorPosY(animatedDishRect.anchoredPosition.y, dishShakeDuration * 0.5f).SetEase(Ease.InQuad));

        // 요리 사라짐 애니메이션
        dishSequence.Append(animatedDishImage.DOFade(0f, dishFadeOutDuration)); // 알파값 0으로 페이드 아웃

        dishSequence.OnComplete(() =>
        {
            Debug.Log("요리 아이콘 애니메이션 완료 (최소화 버전).");
            Destroy(animatedDish);

            // 애니메이션 완료 후 DayManager에게 보상 처리 단계로 진행하라고 알림
            DayManager.instance.ProceedToRewardProcessingPhase(tier, order);
        });

        yield return dishSequence.WaitForCompletion(); // 시퀀스가 끝날 때까지 코루틴 대기
    }

    // DayManager.OnCookingFinishedAndEvaluate 이벤트를 받아 보상 지급 로직을 수행
    public void HandleCookedDishResult(CustomerData.CookingTier tier, CustomerOrder customerOrder)
    {
        if (customerOrder == null)
        {
            Debug.LogError("TycoonManager: 요리 결과를 처리할 CustomerOrder가 null입니다.");
            return;
        }

        CustomerOrder.RewardData rewards = customerOrder.GetRewardForTier(tier);

        if (GlobalManager.instance != null)
        {
            GlobalManager.instance.gold += rewards.gold;
            GlobalManager.instance.reputation += rewards.reputation;
            UpdateResourceUI();
            Debug.Log($"최종 보상 지급 (TycoonUIManager 처리): 금전 +{rewards.gold}, 명성 +{rewards.reputation} (등급: {tier})");
        }
        else
        {
            Debug.LogError("GlobalManager 인스턴스를 찾을 수 없어 보상을 지급할 수 없습니다!");
        }

        if (DayManager.instance != null)
        {
            DayManager.instance.ProceedToCustomerDeparturePhase(tier, CustomerSpawnManager.instance.currentSpawnedCustomerData, CustomerSpawnManager.instance.currentSpawnedCustomerOrder);
        }
        else
        {
            Debug.LogError("DayManager 인스턴스를 찾을 수 없어 손님 퇴장 단계를 시작할 수 없습니다!");
        }
    }
}