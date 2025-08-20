using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;

public class CardUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public TextMeshProUGUI nameText;
    public Image artImage;
    public Transform visualsTransform;
    private CardDataSO cardData;
    private CardManager cardManager;
    private GameManager gameManager;
    private Vector3 originalScale;

    // ▼▼▼ 여기에 추가 ▼▼▼
    [Header("호버 딜레이 설정")]
    [Tooltip("마우스를 올리고 몇 초 후에 상세 정보가 표시될지 설정합니다.")]
    [SerializeField] private float hoverDelay = 0.5f;

    private Coroutine showDetailCoroutine; // 딜레이 코루틴을 제어하기 위한 변수
    // ▲▲▲ 추가 완료 ▲▲▲

    public void Setup(CardDataSO data, CardManager manager)
    {
        this.cardData = data;
        this.cardManager = manager;
        this.gameManager = GameManager.Instance;
        if (nameText != null) nameText.text = cardData.cardName;
        if (artImage != null) artImage.sprite = cardData.cardImage;
        if (visualsTransform != null) originalScale = visualsTransform.localScale;
    }

    // ▼▼▼ 수정된 OnPointerEnter ▼▼▼
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (visualsTransform == null || cardManager == null) return;

        // 즉시 실행되던 로직
        visualsTransform.localScale = originalScale * 1.1f;
        visualsTransform.localPosition = new Vector3(0, 30, 0);

        // 상세 정보 표시는 코루틴을 통해 딜레이를 줍니다.
        showDetailCoroutine = StartCoroutine(ShowDetailsAfterDelay());
    }

    // ▼▼▼ 수정된 OnPointerExit ▼▼▼
    public void OnPointerExit(PointerEventData eventData)
    {
        if (visualsTransform == null || cardManager == null) return;

        // 상세 정보 표시 코루틴이 실행 중이었다면(딜레이를 기다리는 중이었다면) 즉시 중지시킵니다.
        if (showDetailCoroutine != null)
        {
            StopCoroutine(showDetailCoroutine);
            showDetailCoroutine = null;
        }

        // 즉시 실행되던 로직
        visualsTransform.localScale = originalScale;
        visualsTransform.localPosition = Vector3.zero;

        // 상세 정보 창을 숨기는 것은 즉시 실행합니다.
        cardManager.HandleCardHoverExit();
    }

    // ▼▼▼ 새로 추가된 코루틴 ▼▼▼
    private IEnumerator ShowDetailsAfterDelay()
    {
        // 설정된 hoverDelay 시간만큼 기다립니다.
        yield return new WaitForSeconds(hoverDelay);

        // 시간이 지난 후 상세 정보를 표시하라고 CardManager에 요청합니다.
        cardManager.HandleCardHoverEnter(cardData);
        showDetailCoroutine = null; // 코루틴 실행 완료 후 변수 초기화
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (gameManager == null || cardManager == null) return;
        var currentPhase = gameManager.GetCurrentPhase();

        if (currentPhase == GameManager.BattlePhase.PlayerTurn_CardSelection)
        {
            if (transform.parent == cardManager.handTransform)
            {
                cardManager.SelectCard(this.gameObject, this.cardData);
            }
            else if (transform.parent == cardManager.actionPanelTransform)
            {
                cardManager.ReturnCardToHand(this.gameObject, this.cardData);
            }
        }
        else if (currentPhase == GameManager.BattlePhase.ActionPhase)
        {
            if (transform.parent == cardManager.actionPanelTransform)
            {
                cardManager.OnActionPanelCardClicked(this.cardData, this.gameObject);
            }
        }
    }
}