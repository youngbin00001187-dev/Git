using UnityEngine;
using System.Collections.Generic;

public class IntentManager : MonoBehaviour
{
    public static IntentManager Instance { get; private set; }

    // �� Ÿ��(Key)�� �� ���� ����Ʈ�� ���ԵǾ� �ִ���(Value) �����ϴ� ��ųʸ��Դϴ�.
    private Dictionary<GameObject, int> _tileReferenceCounts = new Dictionary<GameObject, int>();

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    /// <summary>
    /// Ư�� Ÿ�� ��Ͽ� ����Ʈ ���̶���Ʈ�� �߰��մϴ�.
    /// </summary>
    public void AddIntentHighlight(List<GameObject> tiles)
    {
        if (TileManager.Instance == null) return;
        foreach (var tile in tiles)
        {
            if (_tileReferenceCounts.ContainsKey(tile))
            {
                // �̹� �ٸ� ����Ʈ�� ���Ե� Ÿ���̸�, ī��Ʈ�� 1 ������ŵ�ϴ�.
                _tileReferenceCounts[tile]++;
            }
            else
            {
                // ó������ ����Ʈ�� ���Ե� Ÿ���̸�, ī��Ʈ�� 1�� �����ϰ� ���̶���Ʈ�� �մϴ�.
                _tileReferenceCounts.Add(tile, 1);
                TileManager.Instance.SetTilesHighlight(TileManager.HighlightType.EnemyIntent, new List<GameObject> { tile }, Color.red);
            }
        }
    }

    /// <summary>
    /// Ư�� Ÿ�� ��Ͽ��� ����Ʈ ���̶���Ʈ�� �����մϴ�.
    /// </summary>
    public void RemoveIntentHighlight(List<GameObject> tiles)
    {
        if (TileManager.Instance == null) return;
        foreach (var tile in tiles)
        {
            if (_tileReferenceCounts.ContainsKey(tile))
            {
                // ī��Ʈ�� 1 ���ҽ�ŵ�ϴ�.
                _tileReferenceCounts[tile]--;
                // ī��Ʈ�� 0�� �Ǹ�, ���� �� Ÿ���� �ʿ�� �ϴ� ����Ʈ�� ���ٴ� �ǹ��̹Ƿ� ���̶���Ʈ�� ���ϴ�.
                if (_tileReferenceCounts[tile] <= 0)
                {
                    _tileReferenceCounts.Remove(tile);
                    TileManager.Instance.ClearHighlightForSpecificTiles(new List<GameObject> { tile }, TileManager.HighlightType.EnemyIntent);
                }
            }
        }
    }
}