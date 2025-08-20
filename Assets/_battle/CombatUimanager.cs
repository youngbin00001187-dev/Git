using UnityEngine;
using UnityEngine.EventSystems; // IPointerEnterHandler, IPointerExitHandler를 위해 필요
using System.Collections; // 코루틴을 위해 필요

/// <summary>
/// 전투 씬의 UI를 총괄 관리하는 스크립트입니다.
/// 이 스크립트를 핸드 패널의 RectTransform이 있는 GameObject에 붙여주세요.
/// </summary>
public class CombatUIManager : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // 핸드 패널 애니메이션 설정 (기존 HandPanelUI 기능)
    [Header("핸드 패널 애니메이션 설정")]
    [Tooltip("핸드 패널이 축소되었을 때의 앵커드 위치입니다.")]
    public Vector2 handShrunkAnchoredPosition = new Vector2(0, 0); // 예: 화면 우측 하단 구석 (앵커에 따라 다름)
    [Tooltip("핸드 패널이 확대되었을 때의 앵커드 위치입니다.")]
    public Vector2 handExpandedAnchoredPosition = new Vector2(0, 200); // 예: 화면 하단 중앙 (앵커에 따라 다름)

    [Tooltip("핸드 패널이 축소되었을 때의 크기입니다 (width, height).")]
    public Vector2 handShrunkSizeDelta = new Vector2(200, 100); // 예: 작은 크기
    [Tooltip("핸드 패널이 확대되었을 때의 크기입니다 (width, height).")]
    public Vector2 handExpandedSizeDelta = new Vector2(800, 200); // 예: 큰 크기

    [Tooltip("핸드 패널 애니메이션에 걸리는 시간입니다.")]
    public float handAnimationDuration = 0.3f; // 애니메이션 지속 시간

    private RectTransform handPanelRectTransform;
    private Coroutine handAnimationCoroutine;
    private bool isHandPanelExpanded = false;

    // 다른 UI 패널들 (추가될 수 있는 요소들)
    [Header("다른 UI 패널들")]
    [Tooltip("전투 씬의 액션 패널 GameObject입니다.")]
    public GameObject actionPanel; // 예시: 액션 패널
    [Tooltip("전투 씬의 플레이어 체력 패널 GameObject입니다.")]
    public GameObject playerHealthPanel; // 예시: 플레이어 체력 패널
    // ... 필요한 다른 UI 요소들을 여기에 추가하여 인스펙터에서 연결하세요.

    void Awake()
    {
        handPanelRectTransform = GetComponent<RectTransform>();
        if (handPanelRectTransform == null)
        {
            Debug.LogError("[CombatUIManager] Hand Panel의 RectTransform이 없습니다! 이 스크립트는 RectTransform이 있는 GameObject에 붙여야 합니다.");
            enabled = false; // 스크립트 비활성화
            return;
        }

        // 초기 상태를 핸드 패널의 축소된 위치와 크기로 설정
        handPanelRectTransform.anchoredPosition = handShrunkAnchoredPosition;
        handPanelRectTransform.sizeDelta = handShrunkSizeDelta;
    }

    void Start()
    {
        // 게임 시작 시 핸드 패널을 축소 상태로 시작
        ShrinkHandPanel();
        // 다른 패널들의 초기 상태 설정 (예: 활성화/비활성화)
        // 필요에 따라 여기에 다른 패널들의 초기 상태 로직을 추가하세요.
        if (actionPanel != null) actionPanel.SetActive(true); // 예시: 액션 패널은 기본적으로 활성화
        if (playerHealthPanel != null) playerHealthPanel.SetActive(true); // 예시: 플레이어 체력 패널도 활성화
    }

    /// <summary>
    /// 마우스 포인터가 핸드 패널 위로 진입했을 때 호출됩니다.
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        ExpandHandPanel();
    }

    /// <summary>
    /// 마우스 포인터가 핸드 패널 밖으로 나갔을 때 호출됩니다.
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        ShrinkHandPanel();
    }

    /// <summary>
    /// 핸드 패널을 확대 상태로 애니메이션합니다.
    /// </summary>
    public void ExpandHandPanel()
    {
        if (isHandPanelExpanded) return;
        isHandPanelExpanded = true;
        if (handAnimationCoroutine != null) StopCoroutine(handAnimationCoroutine);
        handAnimationCoroutine = StartCoroutine(AnimatePanel(handPanelRectTransform, handExpandedAnchoredPosition, handExpandedSizeDelta, handAnimationDuration));
    }

    /// <summary>
    /// 핸드 패널을 축소 상태로 애니메이션합니다.
    /// </summary>
    public void ShrinkHandPanel()
    {
        if (!isHandPanelExpanded && handAnimationCoroutine == null) return; // 이미 축소되어 있고 애니메이션 중이 아니면 리턴
        isHandPanelExpanded = false;
        if (handAnimationCoroutine != null) StopCoroutine(handAnimationCoroutine);
        handAnimationCoroutine = StartCoroutine(AnimatePanel(handPanelRectTransform, handShrunkAnchoredPosition, handShrunkSizeDelta, handAnimationDuration));
    }

    /// <summary>
    /// 지정된 RectTransform의 위치와 크기를 목표 값으로 부드럽게 애니메이션합니다.
    /// </summary>
    private IEnumerator AnimatePanel(RectTransform panelToAnimate, Vector2 targetPosition, Vector2 targetSize, float duration)
    {
        Vector2 startPosition = panelToAnimate.anchoredPosition;
        Vector2 startSize = panelToAnimate.sizeDelta;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            panelToAnimate.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, elapsedTime / duration);
            panelToAnimate.sizeDelta = Vector2.Lerp(startSize, targetSize, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 애니메이션 종료 후 정확한 목표 값으로 설정
        panelToAnimate.anchoredPosition = targetPosition;
        panelToAnimate.sizeDelta = targetSize;
        // 핸드 패널 애니메이션 코루틴만 nullify (다른 패널에 재사용될 경우를 대비)
        if (panelToAnimate == handPanelRectTransform)
        {
            handAnimationCoroutine = null;
        }
    }

    // --- 다른 UI 패널을 제어하는 예시 함수 ---

    /// <summary>
    /// 액션 패널을 활성화합니다.
    /// </summary>
    public void ShowActionPanel()
    {
        if (actionPanel != null) actionPanel.SetActive(true);
    }

    /// <summary>
    /// 액션 패널을 비활성화합니다.
    /// </summary>
    public void HideActionPanel()
    {
        if (actionPanel != null) actionPanel.SetActive(false);
    }

    /// <summary>
    /// 플레이어 체력 패널을 활성화합니다.
    /// </summary>
    public void ShowPlayerHealthPanel()
    {
        if (playerHealthPanel != null) playerHealthPanel.SetActive(true);
    }

    /// <summary>
    /// 플레이어 체력 패널을 비활성화합니다.
    /// </summary>
    public void HidePlayerHealthPanel()
    {
        if (playerHealthPanel != null) playerHealthPanel.SetActive(false);
    }

    // 필요하다면 다른 패널을 위한 애니메이션 함수도 추가 가능합니다.
}