using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ī�� �ϳ��� ��� �����͸� �����ϴ� ScriptableObject�Դϴ�.
/// ī���� �⺻ ����, �ɷ�ġ �䱸��, �׸��� ���� �ൿ(Action)�� �ΰ� ȿ��(Effect)��
/// �������� ����(������)�� �̰����� �����մϴ�.
/// </summary>
[CreateAssetMenu(fileName = "New Card", menuName = "�������� �ܻ��� ����/Card Data", order = 1)]
public class CardDataSO : ScriptableObject
{
    [Header("ī�� �⺻ ����")]
    public string cardID;
    public string cardName;
    public Sprite cardImage;
    [TextArea(3, 5)]
    public string description;

    [Header("�ǵ� ǥ�� ����")]
    [Tooltip("�� ī�带 ���� ����ϰų� �÷��̾ ȣ���� �� ǥ�õ� ���� �����Դϴ�.")]
    public List<Vector2Int> intentPredictionRange = new List<Vector2Int>();

    [Header("���� ���� (���� �ý��ۿ�)")]
    [Tooltip("�� ī�尡 ���� ������ ������ ��Ÿ���� ID�Դϴ�. (��: �±ر�=1, �Ҹ���=2)")]
    public int martialArtID = 0;

    [Header("�ɹ� �䱸 �ɷ�ġ")]
    [Tooltip("�� ī�带 ���� ���Խ�Ű�� ���� �ʿ��� �ּ� ���� �ɷ�ġ�Դϴ�.")]
    public FiveElementsStats requiredStats;

    [Header("ī�� �׼� ������")]
    [Tooltip("�� ī�带 ������� �� ������� �ߵ��� '�ֿ� �ൿ(Action)' ����Դϴ�. (��: �̵�, ����)")]
    [SerializeReference] // �ڽ� Ŭ����(MoveAction, AttackAction)�� �ν����Ϳ� ǥ���ϱ� ���� �ʼ�
    public List<GameAction> actionSequence = new List<GameAction>();

    // ���� [�߰�] ī�忡 �ΰ� ȿ��(GameEffect)�� ���� �߰��� �� �ִ� ����Ʈ ����
    [Header("ī�� �ΰ� ȿ��")]
    [Tooltip("�� ī���� '�ֿ� �ൿ'�� ��� ���� ��, �߰��� �ߵ��� '�ΰ� ȿ��(Effect)' ����Դϴ�. (��: �˹�, ����)")]
    [SerializeReference] // �ڽ� Ŭ����(KnockbackEffect ��)�� �ν����Ϳ� ǥ���ϱ� ���� �ʼ�
    public List<GameEffect> appliedEffects = new List<GameEffect>();
    // �����������������������������������������������������������������

    // =================================================================================
    // �ν����� ���� ��� (��Ŭ�� �޴�)
    // =================================================================================

    [ContextMenu("�׼� ������/Move Action �߰�")]
    private void AddMoveAction()
    {
        actionSequence.Add(new MoveAction());
    }

    [ContextMenu("�׼� ������/Attack Action �߰�")]
    private void AddAttackAction()
    {
        actionSequence.Add(new AttackAction());
    }

    // ���� [�߰�] �˹� ����Ʈ�� ���� �߰��� �� �ִ� ��Ŭ�� �޴� ����
    [ContextMenu("�ΰ� ȿ��/Knockback Effect �߰�")]
    private void AddKnockbackEffect()
    {
        appliedEffects.Add(new KnockbackEffect());
    }
    // �����������������������������������������������
}
