using UnityEngine;
using TMPro;

/// <summary>
/// ī�� �� ����(�̸�, ����)�� ǥ���ϴ� UI�� �����մϴ�.
/// Ȱ��ȭ�Ǹ� ���콺 Ŀ���� ����ٴմϴ�.
/// </summary>
public class CardDetailView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private GameObject contentObject; // �г� ��ü�� ���� �ѱ� ���� ����

    // ���� ���⿡ �߰� ����
    [Header("���콺 ���� ����")]
    [Tooltip("���콺 Ŀ���κ��� UI�� �󸶳� ������ ǥ�õ��� ���մϴ�.")]
    [SerializeField] private Vector2 followOffset = new Vector2(20f, -20f);

    private bool isFollowing = false; // ���� ���콺�� ���� ������ ����
    private RectTransform rectTransform; // ��ġ�� �ű� UI�� RectTransform
    // ���� �߰� �Ϸ� ����

    private void Awake()
    {
        // ���� ���⿡ �߰� ����
        // RectTransform ������Ʈ�� �̸� ã�ƵӴϴ�.
        rectTransform = GetComponent<RectTransform>();
        // ���� �߰� �Ϸ� ����

        Hide();
    }

    // ���� Update �Լ� �߰� ����
    private void Update()
    {
        // isFollowing ������ ���� �� ������ ��ġ�� �����մϴ�.
        if (isFollowing)
        {
            // ���� ���콺 ��ġ�� �������� ���� ������ UI ��ġ�� �����մϴ�.
            // UI ĵ������ Screen Space - Overlay ����� �� �� �۵��մϴ�.
            rectTransform.position = (Vector2)Input.mousePosition + followOffset;
        }
    }
    // ���� �߰� �Ϸ� ����

    /// <summary>
    /// ī�� �����͸� �޾ƿ� UI �ؽ�Ʈ�� ä��� �г��� �����ݴϴ�.
    /// </summary>
    public void Show(CardDataSO cardData)
    {
        if (cardData == null) return;

        nameText.text = cardData.cardName;
        descriptionText.text = cardData.description;
        contentObject.SetActive(true);

        // ���� ���⿡ �߰� ����
        isFollowing = true; // ���콺 ���� ����
        // ���� �߰� �Ϸ� ����
    }

    /// <summary>
    /// �� ���� �г��� ����ϴ�.
    /// </summary>
    public void Hide()
    {
        contentObject.SetActive(false);

        // ���� ���⿡ �߰� ����
        isFollowing = false; // ���콺 ���� ����
        // ���� �߰� �Ϸ� ����
    }
}