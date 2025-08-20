using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// �������� ����ĥ ������ �׷�(����)�� �����ϴ� ScriptableObject�Դϴ�.
/// </summary>
[CreateAssetMenu(fileName = "New Encounter", menuName = "�������� �ܻ��� ����/Encounter")]
public class EncounterSO : ScriptableObject
{
    [Header("���� ����")]
    [Tooltip("�� ������ �̸��Դϴ�. (��: ���� ����, ��ǳä ����)")]
    public string encounterName;

    [Header("���� �� ���")]
    [Tooltip("�� ���쿡�� ������ ������ EnemyDataSO ����Դϴ�.")]
    public List<EnemyDataSO> enemies;
}
