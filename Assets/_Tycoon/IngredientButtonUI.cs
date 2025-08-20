using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class IngredientButtonUI : MonoBehaviour
{
    [Header("UI 요소")]
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI quantityText;

    // 이 버튼이 어떤 재료를 대표하는지 저장
    private IngredientData currentIngredient;

    // TycoonUIManager가 이 함수를 호출하여 버튼의 내용을 설정합니다.
    public void Setup(IngredientData data, int quantity)
    {
        currentIngredient = data;

        if (iconImage != null && data.icon != null)
        {
            iconImage.sprite = data.icon;
        }
        if (nameText != null)
        {
            nameText.text = data.ingredientName;
        }
        if (quantityText != null)
        {
            quantityText.text = "x" + quantity;
        }
    }

    // === 추가된 부분: 아이콘 이미지 컴포넌트 Getter ===
    /// <summary>
    /// 이 버튼의 아이콘 Image 컴포넌트를 반환합니다.
    /// </summary>
    public Image GetIconImage()
    {
        return iconImage;
    }
    // ===============================================

    // === 추가된 부분: 재료 데이터 Getter ===
    /// <summary>
    /// 이 버튼이 대표하는 IngredientData를 반환합니다.
    /// </summary>
    public IngredientData GetIngredientData()
    {
        return currentIngredient;
    }
    // ======================================

    public void OnIngredientButtonClick()
    {
        Debug.Log(currentIngredient.ingredientName + " 버튼이 클릭되었습니다!");
    }
}