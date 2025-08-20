using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class CardUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public TextMeshProUGUI nameText;
    public Image artImage;
    public Transform visualsTransform;
    private CardDataSO cardData;
    private CardManager cardManager;
    private GameManager gameManager;
    private Vector3 originalScale;

    public void Setup(CardDataSO data, CardManager manager)
    {
        this.cardData = data;
        this.cardManager = manager;
        this.gameManager = GameManager.Instance; // GameManager 인스턴스 참조
        if (nameText != null) nameText.text = cardData.cardName;
        if (artImage != null) artImage.sprite = cardData.cardImage;
        if (visualsTransform != null) originalScale = visualsTransform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (visualsTransform == null || cardManager == null) return;
        visualsTransform.localScale = originalScale * 1.1f;
        visualsTransform.localPosition = new Vector3(0, 30, 0);
        cardManager.HandleCardHoverEnter(cardData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (visualsTransform == null || cardManager == null) return;
        visualsTransform.localScale = originalScale;
        visualsTransform.localPosition = Vector3.zero;
        cardManager.HandleCardHoverExit();
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