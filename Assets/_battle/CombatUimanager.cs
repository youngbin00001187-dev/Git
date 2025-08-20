using UnityEngine;
using UnityEngine.EventSystems; // IPointerEnterHandler, IPointerExitHandler�� ���� �ʿ�
using System.Collections; // �ڷ�ƾ�� ���� �ʿ�

/// <summary>
/// ���� ���� UI�� �Ѱ� �����ϴ� ��ũ��Ʈ�Դϴ�.
/// �� ��ũ��Ʈ�� �ڵ� �г��� RectTransform�� �ִ� GameObject�� �ٿ��ּ���.
/// </summary>
public class CombatUIManager : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // �ڵ� �г� �ִϸ��̼� ���� (���� HandPanelUI ���)
    [Header("�ڵ� �г� �ִϸ��̼� ����")]
    [Tooltip("�ڵ� �г��� ��ҵǾ��� ���� ��Ŀ�� ��ġ�Դϴ�.")]
    public Vector2 handShrunkAnchoredPosition = new Vector2(0, 0); // ��: ȭ�� ���� �ϴ� ���� (��Ŀ�� ���� �ٸ�)
    [Tooltip("�ڵ� �г��� Ȯ��Ǿ��� ���� ��Ŀ�� ��ġ�Դϴ�.")]
    public Vector2 handExpandedAnchoredPosition = new Vector2(0, 200); // ��: ȭ�� �ϴ� �߾� (��Ŀ�� ���� �ٸ�)

    [Tooltip("�ڵ� �г��� ��ҵǾ��� ���� ũ���Դϴ� (width, height).")]
    public Vector2 handShrunkSizeDelta = new Vector2(200, 100); // ��: ���� ũ��
    [Tooltip("�ڵ� �г��� Ȯ��Ǿ��� ���� ũ���Դϴ� (width, height).")]
    public Vector2 handExpandedSizeDelta = new Vector2(800, 200); // ��: ū ũ��

    [Tooltip("�ڵ� �г� �ִϸ��̼ǿ� �ɸ��� �ð��Դϴ�.")]
    public float handAnimationDuration = 0.3f; // �ִϸ��̼� ���� �ð�

    private RectTransform handPanelRectTransform;
    private Coroutine handAnimationCoroutine;
    private bool isHandPanelExpanded = false;

    // �ٸ� UI �гε� (�߰��� �� �ִ� ��ҵ�)
    [Header("�ٸ� UI �гε�")]
    [Tooltip("���� ���� �׼� �г� GameObject�Դϴ�.")]
    public GameObject actionPanel; // ����: �׼� �г�
    [Tooltip("���� ���� �÷��̾� ü�� �г� GameObject�Դϴ�.")]
    public GameObject playerHealthPanel; // ����: �÷��̾� ü�� �г�
    // ... �ʿ��� �ٸ� UI ��ҵ��� ���⿡ �߰��Ͽ� �ν����Ϳ��� �����ϼ���.

    void Awake()
    {
        handPanelRectTransform = GetComponent<RectTransform>();
        if (handPanelRectTransform == null)
        {
            Debug.LogError("[CombatUIManager] Hand Panel�� RectTransform�� �����ϴ�! �� ��ũ��Ʈ�� RectTransform�� �ִ� GameObject�� �ٿ��� �մϴ�.");
            enabled = false; // ��ũ��Ʈ ��Ȱ��ȭ
            return;
        }

        // �ʱ� ���¸� �ڵ� �г��� ��ҵ� ��ġ�� ũ��� ����
        handPanelRectTransform.anchoredPosition = handShrunkAnchoredPosition;
        handPanelRectTransform.sizeDelta = handShrunkSizeDelta;
    }

    void Start()
    {
        // ���� ���� �� �ڵ� �г��� ��� ���·� ����
        ShrinkHandPanel();
        // �ٸ� �гε��� �ʱ� ���� ���� (��: Ȱ��ȭ/��Ȱ��ȭ)
        // �ʿ信 ���� ���⿡ �ٸ� �гε��� �ʱ� ���� ������ �߰��ϼ���.
        if (actionPanel != null) actionPanel.SetActive(true); // ����: �׼� �г��� �⺻������ Ȱ��ȭ
        if (playerHealthPanel != null) playerHealthPanel.SetActive(true); // ����: �÷��̾� ü�� �гε� Ȱ��ȭ
    }

    /// <summary>
    /// ���콺 �����Ͱ� �ڵ� �г� ���� �������� �� ȣ��˴ϴ�.
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        ExpandHandPanel();
    }

    /// <summary>
    /// ���콺 �����Ͱ� �ڵ� �г� ������ ������ �� ȣ��˴ϴ�.
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        ShrinkHandPanel();
    }

    /// <summary>
    /// �ڵ� �г��� Ȯ�� ���·� �ִϸ��̼��մϴ�.
    /// </summary>
    public void ExpandHandPanel()
    {
        if (isHandPanelExpanded) return;
        isHandPanelExpanded = true;
        if (handAnimationCoroutine != null) StopCoroutine(handAnimationCoroutine);
        handAnimationCoroutine = StartCoroutine(AnimatePanel(handPanelRectTransform, handExpandedAnchoredPosition, handExpandedSizeDelta, handAnimationDuration));
    }

    /// <summary>
    /// �ڵ� �г��� ��� ���·� �ִϸ��̼��մϴ�.
    /// </summary>
    public void ShrinkHandPanel()
    {
        if (!isHandPanelExpanded && handAnimationCoroutine == null) return; // �̹� ��ҵǾ� �ְ� �ִϸ��̼� ���� �ƴϸ� ����
        isHandPanelExpanded = false;
        if (handAnimationCoroutine != null) StopCoroutine(handAnimationCoroutine);
        handAnimationCoroutine = StartCoroutine(AnimatePanel(handPanelRectTransform, handShrunkAnchoredPosition, handShrunkSizeDelta, handAnimationDuration));
    }

    /// <summary>
    /// ������ RectTransform�� ��ġ�� ũ�⸦ ��ǥ ������ �ε巴�� �ִϸ��̼��մϴ�.
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

        // �ִϸ��̼� ���� �� ��Ȯ�� ��ǥ ������ ����
        panelToAnimate.anchoredPosition = targetPosition;
        panelToAnimate.sizeDelta = targetSize;
        // �ڵ� �г� �ִϸ��̼� �ڷ�ƾ�� nullify (�ٸ� �гο� ����� ��츦 ���)
        if (panelToAnimate == handPanelRectTransform)
        {
            handAnimationCoroutine = null;
        }
    }

    // --- �ٸ� UI �г��� �����ϴ� ���� �Լ� ---

    /// <summary>
    /// �׼� �г��� Ȱ��ȭ�մϴ�.
    /// </summary>
    public void ShowActionPanel()
    {
        if (actionPanel != null) actionPanel.SetActive(true);
    }

    /// <summary>
    /// �׼� �г��� ��Ȱ��ȭ�մϴ�.
    /// </summary>
    public void HideActionPanel()
    {
        if (actionPanel != null) actionPanel.SetActive(false);
    }

    /// <summary>
    /// �÷��̾� ü�� �г��� Ȱ��ȭ�մϴ�.
    /// </summary>
    public void ShowPlayerHealthPanel()
    {
        if (playerHealthPanel != null) playerHealthPanel.SetActive(true);
    }

    /// <summary>
    /// �÷��̾� ü�� �г��� ��Ȱ��ȭ�մϴ�.
    /// </summary>
    public void HidePlayerHealthPanel()
    {
        if (playerHealthPanel != null) playerHealthPanel.SetActive(false);
    }

    // �ʿ��ϴٸ� �ٸ� �г��� ���� �ִϸ��̼� �Լ��� �߰� �����մϴ�.
}