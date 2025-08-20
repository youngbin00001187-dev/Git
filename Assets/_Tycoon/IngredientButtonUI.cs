using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class IngredientButtonUI : MonoBehaviour
{
    [Header("UI ���")]
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI quantityText;

    // �� ��ư�� � ��Ḧ ��ǥ�ϴ��� ����
    private IngredientData currentIngredient;

    // TycoonUIManager�� �� �Լ��� ȣ���Ͽ� ��ư�� ������ �����մϴ�.
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

    // === �߰��� �κ�: ������ �̹��� ������Ʈ Getter ===
    /// <summary>
    /// �� ��ư�� ������ Image ������Ʈ�� ��ȯ�մϴ�.
    /// </summary>
    public Image GetIconImage()
    {
        return iconImage;
    }
    // ===============================================

    // === �߰��� �κ�: ��� ������ Getter ===
    /// <summary>
    /// �� ��ư�� ��ǥ�ϴ� IngredientData�� ��ȯ�մϴ�.
    /// </summary>
    public IngredientData GetIngredientData()
    {
        return currentIngredient;
    }
    // ======================================

    public void OnIngredientButtonClick()
    {
        Debug.Log(currentIngredient.ingredientName + " ��ư�� Ŭ���Ǿ����ϴ�!");
    }
}