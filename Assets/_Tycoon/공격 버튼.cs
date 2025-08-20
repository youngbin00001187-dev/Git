using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class AttackButtonAnimator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("버튼 이미지 요소")]
    [Tooltip("칼집 이미지 (살짝 떠오를 이미지)의 RectTransform을 연결하세요.")]
    public RectTransform scabbardImageRect;
    [Tooltip("칼 이미지 (많이 떠오를 이미지)의 RectTransform을 연결하세요.")]
    public RectTransform swordImageRect;

    // 초기 값 (자동 저장)
    [Header("초기 값 (자동 저장)")]
    public Vector2 _debug_originalScabbardPos;
    public Vector3 _debug_originalScabbardScale = Vector3.one;
    public Vector2 _debug_originalSwordPos;
    public Vector3 _debug_originalSwordScale = Vector3.one;

    [Header("애니메이션 동작 설정")]
    [Tooltip("마우스 호버/이탈 애니메이션의 지속 시간 (초).")]
    public float animationDuration = 0.2f;
    [Tooltip("애니메이션의 움직임 곡선 (Ease). 부드러움 정도를 조절합니다.")]
    public Ease animationEase = Ease.OutQuad;

    [Header("칼집 (Scabbard) 애니메이션")]
    [Tooltip("마우스 호버 시 칼집 이미지가 위로 이동할 상대 Y 위치 (픽셀 단위).")]
    public float scabbardHoverOffsetY = 10f;
    [Tooltip("마우스 호버 시 칼집 이미지가 커질 스케일 비율 (예: 1.1 = 110%).")]
    [Range(1f, 2f)]
    public float scabbardHoverScale = 1.1f;

    [Header("칼 (Sword) 애니메이션")]
    [Tooltip("마우스 호버 시 칼 이미지가 뽑혀 올라갈 상대 Y 위치 (픽셀 단위).")]
    public float swordPullOffsetY = 50f;
    [Tooltip("마우스 호버 시 칼 이미지가 커질 스케일 비율 (예: 1.2 = 120%).")]
    [Range(1f, 2f)]
    public float swordPullScale = 1.2f;


    private Vector2 originalScabbardPos;
    private Vector3 originalScabbardScale;
    private Vector2 originalSwordPos;
    private Vector3 originalSwordScale;


    private void Awake()
    {
        if (scabbardImageRect != null)
        {
            originalScabbardPos = scabbardImageRect.anchoredPosition;
            originalScabbardScale = scabbardImageRect.localScale;
            _debug_originalScabbardPos = originalScabbardPos;
            _debug_originalScabbardScale = originalScabbardScale;
        }
        if (swordImageRect != null)
        {
            originalSwordPos = swordImageRect.anchoredPosition;
            originalSwordScale = swordImageRect.localScale;
            _debug_originalSwordPos = originalSwordPos;
            _debug_originalSwordScale = originalSwordScale;
        }
    }

    private void OnDestroy()
    {
        if (scabbardImageRect != null)
        {
            scabbardImageRect.DOKill(true);
        }
        if (swordImageRect != null)
        {
            swordImageRect.DOKill(true);
        }
        DOTween.Kill(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // 버튼이 비활성화 상태일 때는 애니메이션 실행하지 않음
        if (!gameObject.activeSelf) return;
        AnimateOnHover(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // 버튼이 비활성화 상태일 때는 애니메이션 실행하지 않음
        if (!gameObject.activeSelf) return;
        AnimateOnHover(false);
    }

    private void AnimateOnHover(bool isHovering)
    {
        if (scabbardImageRect == null || swordImageRect == null)
        {
            Debug.LogWarning("AttackButtonAnimator: 이미지 RectTransform이 할당되지 않았습니다! 인스펙터 창을 확인하세요.");
            return;
        }

        scabbardImageRect.DOKill();
        swordImageRect.DOKill();

        if (isHovering)
        {
            scabbardImageRect.DOAnchorPosY(originalScabbardPos.y + scabbardHoverOffsetY, animationDuration)
                             .SetEase(animationEase);
            scabbardImageRect.DOScale(originalScabbardScale * scabbardHoverScale, animationDuration)
                             .SetEase(animationEase);

            swordImageRect.DOAnchorPosY(originalSwordPos.y + swordPullOffsetY, animationDuration)
                          .SetEase(animationEase);
            swordImageRect.DOScale(originalSwordScale * swordPullScale, animationDuration)
                          .SetEase(animationEase);
        }
        else
        {
            scabbardImageRect.DOAnchorPosY(originalScabbardPos.y, animationDuration)
                             .SetEase(animationEase);
            scabbardImageRect.DOScale(originalScabbardScale, animationDuration)
                             .SetEase(animationEase);

            swordImageRect.DOAnchorPosY(originalSwordPos.y, animationDuration)
                          .SetEase(animationEase);
            swordImageRect.DOScale(originalSwordScale, animationDuration)
                          .SetEase(animationEase);
        }
    }

    // === 추가된 함수: 외부에서 버튼 활성/비활성화 제어 ===
    /// <summary>
    /// 공격 버튼의 GameObject를 활성/비활성화합니다.
    /// </summary>
    /// <param name="isActive">true면 활성화, false면 비활성화</param>
    public void SetButtonActive(bool isActive)
    {
        // 비활성화될 때 진행 중인 애니메이션을 즉시 원상 복구하고 종료
        if (!isActive)
        {
            AnimateOnHover(false); // 애니메이션을 원상 복귀 시킴
            scabbardImageRect.DOKill(true); // 혹시 모를 잔여 트윈 강제 종료
            swordImageRect.DOKill(true);
        }

        gameObject.SetActive(isActive);
    }
    // ===================================================

    private void OnValidate()
    {
        if (scabbardImageRect != null)
        {
            _debug_originalScabbardPos = scabbardImageRect.anchoredPosition;
            _debug_originalScabbardScale = scabbardImageRect.localScale;
        }
        if (swordImageRect != null)
        {
            _debug_originalSwordPos = swordImageRect.anchoredPosition;
            _debug_originalSwordScale = swordImageRect.localScale;
        }
    }
}