using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 오행(五行) 속성을 정의하는 구조체입니다.
/// </summary>
[System.Serializable]
public struct FiveElementsStats
{
    public int metal;     // 금(金)
    public int wood;      // 목(木)
    public int water;     // 수(水)
    public int fire;      // 화(火)
    public int earth;     // 토(土)
}

/// <summary>
/// 심법(心法) 하나의 데이터를 정의하는 ScriptableObject입니다.
/// </summary>
[CreateAssetMenu(fileName = "New Simbeop", menuName = "무림에는 외상이 없다/Simbeop")]
public class SimbeopDataSO : ScriptableObject
{
    [Header("심법 정보")]
    public string simbeopName; // 심법 이름 (예: 삼재공)
    [TextArea(3, 5)]
    public string description; // 심법 설명
    public Sprite icon;        // 심법 아이콘

    [Header("무공 종류")]
    [Tooltip("이 심법이 속한 무공의 종류를 나타내는 ID입니다. (예: 태극권=1, 소림권=2)")]
    public int martialArtID = 0; // 기본값은 0 (특정 무공 없음)으로 설정

    [Header("제공 능력치")]
    [Tooltip("이 심법을 장착했을 때 플레이어에게 제공되는 오행 능력치입니다.")]
    public FiveElementsStats providedStats;

    [Header("전투 보너스 및 페널티")]
    [Tooltip("이 심법의 최대 핸드 수입니다.")]
    public int maxHandSize = 7;
    [Tooltip("턴마다 주어지는 멀리건(패 다시 뽑기) 횟수입니다.")]
    public int mulliganPerTurn = 1;
}
