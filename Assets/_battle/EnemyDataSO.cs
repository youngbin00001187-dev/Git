using UnityEngine;
using System.Collections.Generic;

public enum E_ActionType { Move, Attack, Defend }
public enum E_MoveType { Fixed, ChasePlayer }

[System.Serializable]
public class EnemyAction
{
    public string actionName;
    public E_ActionType actionType;
    public E_MoveType movementType;
    public int value;

    [Header("������ ī��")]
    [Tooltip("�� �ൿ�� ������ ī�� ������ ���⿡ ����� ��������.")]
    public CardDataSO referenceCard;
}

[System.Serializable]
public class RewardDrop<T>
{
    [Tooltip("����� ������ (ī�� �Ǵ� �ɹ� ��)")]
    public T rewardItem;

    [Range(0f, 1f)]
    [Tooltip("��� Ȯ�� (0~1 ����)")]
    public float dropChance;
}

[CreateAssetMenu(fileName = "New Enemy Data", menuName = "Enemy Data")]
public class EnemyDataSO : ScriptableObject
{
    [Header("�⺻ ����")]
    public string enemyName;
    public GameObject enemyPrefab;
    public int maxHealth;

    [Header("�ൿ ����")]
    public int actionsPerTurn = 1;
    public List<EnemyAction> actionPattern;

    [Header("��ȭ/�� ���� (óġ ��)")]
    public int goldMin = 5;
    public int goldMax = 10;

    public int fameMin = 1;
    public int fameMax = 3;

    [Header("Ȯ�� ��� ����")]
    public List<RewardDrop<CardDataSO>> cardDrops;
    public List<RewardDrop<SimbeopDataSO>> xinfaDrops; // �ɹ�
}