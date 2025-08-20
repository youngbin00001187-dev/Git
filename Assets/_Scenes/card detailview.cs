using UnityEngine;
using TMPro;

/// <summary>
/// 카드 상세 정보(이름, 설명)를 표시하는 UI를 관리합니다.
/// 활성화되면 마우스 커서를 따라다닙니다.
/// </summary>
public class CardDetailView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private GameObject contentObject; // 패널 자체를 끄고 켜기 위한 참조

    // ▼▼▼ 여기에 추가 ▼▼▼
    [Header("마우스 추적 설정")]
    [Tooltip("마우스 커서로부터 UI가 얼마나 떨어져 표시될지 정합니다.")]
    [SerializeField] private Vector2 followOffset = new Vector2(20f, -20f);

    private bool isFollowing = false; // 현재 마우스를 추적 중인지 여부
    private RectTransform rectTransform; // 위치를 옮길 UI의 RectTransform
    // ▲▲▲ 추가 완료 ▲▲▲

    private void Awake()
    {
        // ▼▼▼ 여기에 추가 ▼▼▼
        // RectTransform 컴포넌트를 미리 찾아둡니다.
        rectTransform = GetComponent<RectTransform>();
        // ▲▲▲ 추가 완료 ▲▲▲

        Hide();
    }

    // ▼▼▼ Update 함수 추가 ▼▼▼
    private void Update()
    {
        // isFollowing 상태일 때만 매 프레임 위치를 갱신합니다.
        if (isFollowing)
        {
            // 현재 마우스 위치에 오프셋을 더한 값으로 UI 위치를 설정합니다.
            // UI 캔버스가 Screen Space - Overlay 모드일 때 잘 작동합니다.
            rectTransform.position = (Vector2)Input.mousePosition + followOffset;
        }
    }
    // ▲▲▲ 추가 완료 ▲▲▲

    /// <summary>
    /// 카드 데이터를 받아와 UI 텍스트를 채우고 패널을 보여줍니다.
    /// </summary>
    public void Show(CardDataSO cardData)
    {
        if (cardData == null) return;

        nameText.text = cardData.cardName;
        descriptionText.text = cardData.description;
        contentObject.SetActive(true);

        // ▼▼▼ 여기에 추가 ▼▼▼
        isFollowing = true; // 마우스 추적 시작
        // ▲▲▲ 추가 완료 ▲▲▲
    }

    /// <summary>
    /// 상세 정보 패널을 숨깁니다.
    /// </summary>
    public void Hide()
    {
        contentObject.SetActive(false);

        // ▼▼▼ 여기에 추가 ▼▼▼
        isFollowing = false; // 마우스 추적 중지
        // ▲▲▲ 추가 완료 ▲▲▲
    }
}