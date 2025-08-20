using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 전투에서 마주칠 적들의 그룹(조우)을 정의하는 ScriptableObject입니다.
/// </summary>
[CreateAssetMenu(fileName = "New Encounter", menuName = "무림에는 외상이 없다/Encounter")]
public class EncounterSO : ScriptableObject
{
    [Header("조우 정보")]
    [Tooltip("이 조우의 이름입니다. (예: 산적 초입, 흑풍채 정예)")]
    public string encounterName;

    [Header("등장 적 목록")]
    [Tooltip("이 조우에서 등장할 적들의 EnemyDataSO 목록입니다.")]
    public List<EnemyDataSO> enemies;
}
