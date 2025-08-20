using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 심법 선택 목록에 들어갈 개별 버튼의 UI와 기능을 담당합니다.
/// </summary>
public class SimbeopButtonUI : MonoBehaviour
{
    [Header("UI Elements")]
    public Image icon;
    public TextMeshProUGUI simbeopName;
    public Button button;

    private SimbeopDataSO mySimbeopData;

    /// <summary>
    /// 이 버튼의 내용을 설정합니다.
    /// </summary>
    public void Setup(SimbeopDataSO data)
    {
        mySimbeopData = data;
        icon.sprite = data.icon;
        simbeopName.text = data.simbeopName;
        button.onClick.AddListener(OnButtonClicked);
    }

    private void OnButtonClicked()
    {
        // 클릭되면 '두뇌'인 DeckBuildingManager에게 심법을 장착하라고 알립니다.
        DeckBuildingManager.instance.EquipSimbeop(mySimbeopData);
        // UI 전체를 새로고침하여 변경사항을 즉시 반영합니다.
        DeckBuildingUIManager.instance.RefreshAllUI();
    }
}
