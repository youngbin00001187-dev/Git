using UnityEngine;
using System.Collections.Generic; // List를 사용하기 위해 추가

public class Tile : MonoBehaviour
{
    [Header("그리드 좌표")]
    [Tooltip("이 타일의 그리드 상의 X, Y 좌표입니다.")]
    public Vector2Int gridPosition;

    [Header("타일 속성 및 효과")]
    [Tooltip("이 타일이 이동 가능한지 여부를 나타냅니다. (예: 벽, 함정 등)")]
    public bool isWalkable = true; // 이동 가능 여부 (기본값: true)

    [Tooltip("이 타일에 적용된 특수 효과 목록입니다. (예: 독 장판, 버프 존 등)")]
    public List<TileEffect> activeEffects = new List<TileEffect>(); // 타일에 적용된 특수 효과 목록

    // 여기에 타일의 추가적인 속성이나 상태를 정의할 수 있습니다.
    // 예: public E_TileType tileType; (타일 종류 enum)
    // 예: public int defenseBonus; (방어 보너스)

    /// <summary>
    /// 타일에 새로운 효과를 추가합니다.
    /// </summary>
    /// <param name="effect">추가할 TileEffect 객체</param>
    public void AddEffect(TileEffect effect)
    {
        if (effect == null) return;
        activeEffects.Add(effect);
        Debug.Log($"[Tile] Tile {gridPosition}에 {effect.effectName} 효과가 추가되었습니다.");
        // TODO: 효과 적용 로직 (예: 유닛이 밟았을 때 데미지, 버프 등)
    }

    /// <summary>
    /// 타일에서 특정 효과를 제거합니다.
    /// </summary>
    /// <param name="effectName">제거할 효과의 이름</param>
    public void RemoveEffect(string effectName)
    {
        int removedCount = activeEffects.RemoveAll(e => e.effectName == effectName);
        if (removedCount > 0)
        {
            Debug.Log($"[Tile] Tile {gridPosition}에서 {effectName} 효과 {removedCount}개가 제거되었습니다.");
        }
    }

    /// <summary>
    /// 타일에 특정 효과가 적용되어 있는지 확인합니다.
    /// </summary>
    /// <param name="effectName">확인할 효과의 이름</param>
    /// <returns>효과가 존재하면 true, 아니면 false</returns>
    public bool HasEffect(string effectName)
    {
        return activeEffects.Exists(e => e.effectName == effectName);
    }
}

// TileEffect 클래스는 Tile 스크립트와 함께 사용될 수 있는 예시입니다.
// 이 클래스는 ScriptableObject로 만들거나, GameAction처럼 [System.Serializable]로 만들 수 있습니다.
[System.Serializable]
public class TileEffect
{
    public string effectName; // 효과 이름 (예: "독 장판", "방어 강화")
    public int duration;      // 효과 지속 턴 (0이면 영구)
    public int value;         // 효과 값 (예: 독 데미지, 방어력 증가량)
    // TODO: 효과 타입 (E_TileEffectType enum), 효과 아이콘 등 추가 가능
}
