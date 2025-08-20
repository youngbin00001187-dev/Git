using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 카드 목록에 들어갈 개별 버튼의 UI와 기능을 담당합니다.
/// </summary>
public class CardButtonUI : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI cardName;
    public Image cardImage;
    public Button button;
    public Image disabledOverlay;
    public GameObject inDeckIndicator;

    private CardDataSO myCardData;
    private bool isOwnedListButton;

    /// <summary>
    /// 이 버튼이 가진 CardDataSO를 반환합니다.
    /// </summary>
    public CardDataSO GetCardData()
    {
        return myCardData;
    }

    public void SetupForOwnedList(CardDataSO data, bool isUsable, bool isAlreadyInDeck)
    {
        myCardData = data;
        cardName.text = data.cardName;
        cardImage.sprite = data.cardImage;
        isOwnedListButton = true;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnButtonClicked);

        bool isDisabled = !isUsable || isAlreadyInDeck;
        disabledOverlay.gameObject.SetActive(isDisabled);
        button.interactable = !isDisabled;

        if (inDeckIndicator != null)
        {
            inDeckIndicator.SetActive(isAlreadyInDeck);
        }
    }

    public void SetupForDeckList(CardDataSO data)
    {
        myCardData = data;
        cardName.text = data.cardName;
        cardImage.sprite = data.cardImage;
        isOwnedListButton = false;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnButtonClicked);

        disabledOverlay.gameObject.SetActive(false);
        button.interactable = true;

        if (inDeckIndicator != null)
        {
            inDeckIndicator.SetActive(false);
        }
    }

    private void OnButtonClicked()
    {
        if (isOwnedListButton)
        {
            // '소유한 카드'라면, '장착 애니메이션'을 요청합니다.
            DeckBuildingUIManager.instance.AnimateAndEquipCard(this);
        }
        else
        {
            // '덱에 있는 카드'라면, '장착 해제 애니메이션'을 요청합니다.
            DeckBuildingUIManager.instance.AnimateAndUnequipCard(this);
        }
    }
}