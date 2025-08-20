using UnityEngine;
using System.Collections.Generic;

public class IntentManager : MonoBehaviour
{
    public static IntentManager Instance { get; private set; }

    // 각 타일(Key)이 몇 개의 인텐트에 포함되어 있는지(Value) 저장하는 딕셔너리입니다.
    private Dictionary<GameObject, int> _tileReferenceCounts = new Dictionary<GameObject, int>();

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    /// <summary>
    /// 특정 타일 목록에 인텐트 하이라이트를 추가합니다.
    /// </summary>
    public void AddIntentHighlight(List<GameObject> tiles)
    {
        if (TileManager.Instance == null) return;
        foreach (var tile in tiles)
        {
            if (_tileReferenceCounts.ContainsKey(tile))
            {
                // 이미 다른 인텐트에 포함된 타일이면, 카운트만 1 증가시킵니다.
                _tileReferenceCounts[tile]++;
            }
            else
            {
                // 처음으로 인텐트에 포함된 타일이면, 카운트를 1로 설정하고 하이라이트를 켭니다.
                _tileReferenceCounts.Add(tile, 1);
                TileManager.Instance.SetTilesHighlight(TileManager.HighlightType.EnemyIntent, new List<GameObject> { tile }, Color.red);
            }
        }
    }

    /// <summary>
    /// 특정 타일 목록에서 인텐트 하이라이트를 제거합니다.
    /// </summary>
    public void RemoveIntentHighlight(List<GameObject> tiles)
    {
        if (TileManager.Instance == null) return;
        foreach (var tile in tiles)
        {
            if (_tileReferenceCounts.ContainsKey(tile))
            {
                // 카운트를 1 감소시킵니다.
                _tileReferenceCounts[tile]--;
                // 카운트가 0이 되면, 이제 이 타일을 필요로 하는 인텐트가 없다는 의미이므로 하이라이트를 끕니다.
                if (_tileReferenceCounts[tile] <= 0)
                {
                    _tileReferenceCounts.Remove(tile);
                    TileManager.Instance.ClearHighlightForSpecificTiles(new List<GameObject> { tile }, TileManager.HighlightType.EnemyIntent);
                }
            }
        }
    }
}