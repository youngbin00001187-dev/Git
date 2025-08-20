using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class AttackButtonAnimator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("��ư �̹��� ���")]
    [Tooltip("Į�� �̹��� (��¦ ������ �̹���)�� RectTransform�� �����ϼ���.")]
    public RectTransform scabbardImageRect;
    [Tooltip("Į �̹��� (���� ������ �̹���)�� RectTransform�� �����ϼ���.")]
    public RectTransform swordImageRect;

    // �ʱ� �� (�ڵ� ����)
    [Header("�ʱ� �� (�ڵ� ����)")]
    public Vector2 _debug_originalScabbardPos;
    public Vector3 _debug_originalScabbardScale = Vector3.one;
    public Vector2 _debug_originalSwordPos;
    public Vector3 _debug_originalSwordScale = Vector3.one;

    [Header("�ִϸ��̼� ���� ����")]
    [Tooltip("���콺 ȣ��/��Ż �ִϸ��̼��� ���� �ð� (��).")]
    public float animationDuration = 0.2f;
    [Tooltip("�ִϸ��̼��� ������ � (Ease). �ε巯�� ������ �����մϴ�.")]
    public Ease animationEase = Ease.OutQuad;

    [Header("Į�� (Scabbard) �ִϸ��̼�")]
    [Tooltip("���콺 ȣ�� �� Į�� �̹����� ���� �̵��� ��� Y ��ġ (�ȼ� ����).")]
    public float scabbardHoverOffsetY = 10f;
    [Tooltip("���콺 ȣ�� �� Į�� �̹����� Ŀ�� ������ ���� (��: 1.1 = 110%).")]
    [Range(1f, 2f)]
    public float scabbardHoverScale = 1.1f;

    [Header("Į (Sword) �ִϸ��̼�")]
    [Tooltip("���콺 ȣ�� �� Į �̹����� ���� �ö� ��� Y ��ġ (�ȼ� ����).")]
    public float swordPullOffsetY = 50f;
    [Tooltip("���콺 ȣ�� �� Į �̹����� Ŀ�� ������ ���� (��: 1.2 = 120%).")]
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
        // ��ư�� ��Ȱ��ȭ ������ ���� �ִϸ��̼� �������� ����
        if (!gameObject.activeSelf) return;
        AnimateOnHover(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // ��ư�� ��Ȱ��ȭ ������ ���� �ִϸ��̼� �������� ����
        if (!gameObject.activeSelf) return;
        AnimateOnHover(false);
    }

    private void AnimateOnHover(bool isHovering)
    {
        if (scabbardImageRect == null || swordImageRect == null)
        {
            Debug.LogWarning("AttackButtonAnimator: �̹��� RectTransform�� �Ҵ���� �ʾҽ��ϴ�! �ν����� â�� Ȯ���ϼ���.");
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

    // === �߰��� �Լ�: �ܺο��� ��ư Ȱ��/��Ȱ��ȭ ���� ===
    /// <summary>
    /// ���� ��ư�� GameObject�� Ȱ��/��Ȱ��ȭ�մϴ�.
    /// </summary>
    /// <param name="isActive">true�� Ȱ��ȭ, false�� ��Ȱ��ȭ</param>
    public void SetButtonActive(bool isActive)
    {
        // ��Ȱ��ȭ�� �� ���� ���� �ִϸ��̼��� ��� ���� �����ϰ� ����
        if (!isActive)
        {
            AnimateOnHover(false); // �ִϸ��̼��� ���� ���� ��Ŵ
            scabbardImageRect.DOKill(true); // Ȥ�� �� �ܿ� Ʈ�� ���� ����
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