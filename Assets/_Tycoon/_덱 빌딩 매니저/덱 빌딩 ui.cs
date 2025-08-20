using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeckBuildingUIManager : MonoBehaviour
{
    public static DeckBuildingUIManager instance;

    private DeckBuildingManager deckBuildingManager;
    private GlobalManager globalManager;

    private enum LeftPanelState { Simbeops, Cards }
    private LeftPanelState currentLeftPanelState;

    [Header("UI Prefabs")]
    public GameObject simbeopButtonPrefab;
    public GameObject cardButtonPrefab;

    [Header("오른쪽 패널 (상태창)")]
    public Image equippedSimbeopIcon;
    public TextMeshProUGUI equippedSimbeopName;
    public TextMeshProUGUI equippedSimbeopDesc;
    public TextMeshProUGUI statsText;
    public Transform deckCardListParent;
    public Button showCardListButton;
    public Button showSimbeopListButton;

    [Header("왼쪽 패널 (공용 선택창)")]
    public GameObject selectionPanel;
    public Transform selectionListParent;

    [Header("하단 패널")]
    public Button confirmButton;
    public TextMeshProUGUI deckCountText;

    void Awake()
    {
        if (instance == null) { instance = this; }
        else { Destroy(gameObject); }

        // ▼▼▼ 이 두 줄을 Start()에서 Awake()로 옮깁니다 ▼▼▼
        deckBuildingManager = DeckBuildingManager.instance;
        globalManager = GlobalManager.instance;
    }

    void Start()
    {
        // Awake()로 옮겼으므로 아래 두 줄은 삭제하거나 주석 처리합니다.
        // deckBuildingManager = DeckBuildingManager.instance;
        // globalManager = GlobalManager.instance;

        showSimbeopListButton.onClick.AddListener(ShowSimbeopSelection);
        showCardListButton.onClick.AddListener(ShowCardSelection);
        confirmButton.onClick.AddListener(OnConfirmButtonClicked);

        // Start()가 처음 호출될 때 한 번만 실행되도록 RefreshAllUI() 호출을 유지합니다.
        RefreshAllUI();
    }
    public void RefreshAllUI()
    {
        UpdateRightPanel();
        if (currentLeftPanelState == LeftPanelState.Cards)
        {
            ShowCardSelection();
        }
        else
        {
            ShowSimbeopSelection();
        }
    }

    private void UpdateRightPanel()
    {
        UpdateEquippedSimbeopPanel();
        UpdateDeckPanel();
        UpdateDeckCountText();
    }

    public void ShowSimbeopSelection()
    {
        currentLeftPanelState = LeftPanelState.Simbeops;
        selectionPanel.SetActive(true);
        foreach (Transform child in selectionListParent)
        {
            Destroy(child.gameObject);
        }

        foreach (var simbeop in globalManager.ownedSimbeops)
        {
            GameObject buttonObj = Instantiate(simbeopButtonPrefab, selectionListParent);
            buttonObj.GetComponent<SimbeopButtonUI>().Setup(simbeop);
        }
    }

    public void ShowCardSelection()
    {
        currentLeftPanelState = LeftPanelState.Cards;
        selectionPanel.SetActive(true);
        foreach (Transform child in selectionListParent)
        {
            Destroy(child.gameObject);
        }

        foreach (var card in globalManager.playerCardCollection)
        {
            // ▼▼▼ 이 조건을 추가합니다 ▼▼▼
            if (deckBuildingManager.GetCurrentDeck().Contains(card))
            {
                continue; // 덱에 이미 있으면 이 카드는 버튼을 생성하지 않고 건너뜁니다.
            }
            // ▲▲▲ 조건 추가 끝 ▲▲▲

            GameObject buttonObj = Instantiate(cardButtonPrefab, selectionListParent);
            bool isUsable = deckBuildingManager.IsCardUsable(card);

            // 이제 'isAlreadyInDeck'은 항상 false이므로, 마지막 인자를 false로 고정합니다.
            buttonObj.GetComponent<CardButtonUI>().SetupForOwnedList(card, isUsable, false);
        }
    }

    void UpdateEquippedSimbeopPanel()
    {
        if (globalManager.equippedSimbeop != null)
        {
            var simbeop = globalManager.equippedSimbeop;
            equippedSimbeopIcon.sprite = simbeop.icon;
            equippedSimbeopName.text = simbeop.simbeopName;
            equippedSimbeopDesc.text = simbeop.description;

            // ▼▼▼ 수정된 부분 (요청사항 3) ▼▼▼
            // DeckBuildingManager로부터 '남은 능력치'를 계산한 결과를 가져와 표시합니다.
            FiveElementsStats remainingStats = deckBuildingManager.GetRemainingStats();
            statsText.text = $"금: {remainingStats.metal} 목: {remainingStats.wood} 수: {remainingStats.water} 화: {remainingStats.fire} 토: {remainingStats.earth}";
            // ▲▲▲ 수정된 부분 ▲▲▲
        }
        else
        {
            equippedSimbeopName.text = "장착된 심법 없음";
            equippedSimbeopDesc.text = "심법을 선택하여 장착하십시오.";
            statsText.text = "금: 0 목: 0 수: 0 화: 0 토: 0";
        }
    }
    public void SetLeftPanelStateToCards()
    {
        currentLeftPanelState = LeftPanelState.Cards;
    }
    void UpdateDeckPanel()
    {
        foreach (Transform child in deckCardListParent)
        {
            Destroy(child.gameObject);
        }

        foreach (var card in deckBuildingManager.GetCurrentDeck())
        {
            GameObject buttonObj = Instantiate(cardButtonPrefab, deckCardListParent);
            buttonObj.GetComponent<CardButtonUI>().SetupForDeckList(card);
        }
    }
    public void AnimateAndEquipCard(CardButtonUI cardButtonToAnimate)
    {
        StartCoroutine(AnimateAndEquipCard_Coroutine(cardButtonToAnimate));
    }

    // 기존의 AnimateAndUnequipCard 함수를 이걸로 교체
    public void AnimateAndUnequipCard(CardButtonUI cardButtonToAnimate)
    {
        StartCoroutine(AnimateAndUnequipCard_Coroutine(cardButtonToAnimate));
    }

    // 새로 추가되는 코루틴 함수
    private IEnumerator AnimateAndEquipCard_Coroutine(CardButtonUI cardButtonToAnimate)
    {
        CardDataSO cardData = cardButtonToAnimate.GetCardData();
        if (!DeckBuildingManager.instance.IsCardUsable(cardData))
        {
            yield break; // 코루틴 종료
        }

        // 1. 목적지(오른쪽 덱 패널)에 투명한 자리 표시자를 먼저 생성
        GameObject placeholder = new GameObject("CardPlaceholder");
        placeholder.AddComponent<LayoutElement>(); // 크기 제어를 위해 LayoutElement 추가
        placeholder.transform.SetParent(deckCardListParent, false);

        // 2. Layout Group이 자리표시자를 포함하여 위치를 재계산하도록 한 프레임 대기
        yield return null;

        // 3. 재계산이 끝난 자리 표시자의 최종 위치를 목표 지점으로 설정
        Vector3 targetPosition = placeholder.transform.position;

        // 4. 원본 버튼의 복제본을 생성하여 애니메이션 실행
        GameObject clone = Instantiate(cardButtonToAnimate.gameObject, this.transform.root);
        clone.transform.position = cardButtonToAnimate.transform.position;
        clone.GetComponent<Button>().interactable = false;

        // 원본 버튼은 투명화
        cardButtonToAnimate.GetComponent<CanvasGroup>().alpha = 0;

        float animationDuration = 0.3f;
        clone.transform.DOMove(targetPosition, animationDuration)
            .SetEase(Ease.InOutQuad)
            .OnComplete(() => {
                // 5. 애니메이션 완료 후 모든 임시 오브젝트(복제본, 자리표시자)를 파괴하고 UI 새로고침
                Destroy(clone);
                Destroy(placeholder);
                DeckBuildingManager.instance.AddCardToBattleDeck(cardData);
                RefreshAllUI();
            });
    }

    // 새로 추가되는 코루틴 함수
    private IEnumerator AnimateAndUnequipCard_Coroutine(CardButtonUI cardButtonToAnimate)
    {
        CardDataSO cardData = cardButtonToAnimate.GetCardData();

        // 1. 목적지(왼쪽 카드 목록)에 투명한 자리 표시자를 먼저 생성합니다.
        GameObject placeholder = new GameObject("CardPlaceholder");
        placeholder.AddComponent<LayoutElement>();
        placeholder.transform.SetParent(selectionListParent, false);

        // 2. Layout Group이 위치를 재계산하도록 한 프레임 대기합니다.
        yield return null;

        // 3. 자리 표시자의 최종 위치를 애니메이션 목표 지점으로 설정합니다.
        Vector3 targetPosition = placeholder.transform.position;

        // 4. 원본 버튼의 복제본을 생성하여 애니메이션을 준비합니다.
        GameObject clone = Instantiate(cardButtonToAnimate.gameObject, this.transform.root);
        clone.transform.position = cardButtonToAnimate.transform.position;
        clone.GetComponent<Button>().interactable = false;

        // 5. 원본 버튼은 투명하게 만들어 자리를 차지하게만 둡니다.
        cardButtonToAnimate.GetComponent<CanvasGroup>().alpha = 0;

        // 6. 복제본을 목표 지점까지 이동시키는 애니메이션을 실행합니다.
        float animationDuration = 0.3f;
        clone.transform.DOMove(targetPosition, animationDuration)
            .SetEase(Ease.InOutQuad)
            .OnComplete(() => {
                // 7. 애니메이션 완료 후 모든 임시 오브젝트(복제본, 자리표시자)를 파괴하고 UI를 새로고침합니다.
                Destroy(clone);
                Destroy(placeholder);
                DeckBuildingManager.instance.RemoveCardFromBattleDeck(cardData);
                SetLeftPanelStateToCards();
                RefreshAllUI();
            });
    }
    void UpdateDeckCountText()
    {
        int maxDeckSize = 30;
        deckCountText.text = $"{deckBuildingManager.GetCurrentDeck().Count} / {maxDeckSize}";
    }

    private void OnConfirmButtonClicked()
    {
        deckBuildingManager.ConfirmDeck();
        Debug.Log("덱 편성이 완료되었습니다.");
    }
}