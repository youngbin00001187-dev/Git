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

    [Header("실행할 카드")]
    [Tooltip("이 행동이 실행할 카드 에셋을 여기에 끌어다 놓으세요.")]
    public CardDataSO referenceCard;
}

[System.Serializable]
public class RewardDrop<T>
{
    [Tooltip("드랍될 아이템 (카드 또는 심법 등)")]
    public T rewardItem;

    [Range(0f, 1f)]
    [Tooltip("드랍 확률 (0~1 사이)")]
    public float dropChance;
}

[CreateAssetMenu(fileName = "New Enemy Data", menuName = "Enemy Data")]
public class EnemyDataSO : ScriptableObject
{
    [Header("기본 정보")]
    public string enemyName;
    public GameObject enemyPrefab;
    public int maxHealth;

    [Header("행동 정보")]
    public int actionsPerTurn = 1;
    public List<EnemyAction> actionPattern;

    [Header("금화/명성 보상 (처치 시)")]
    public int goldMin = 5;
    public int goldMax = 10;

    public int fameMin = 1;
    public int fameMax = 3;

    [Header("확률 드랍 보상")]
    public List<RewardDrop<CardDataSO>> cardDrops;
    public List<RewardDrop<SimbeopDataSO>> xinfaDrops; // 심법
}