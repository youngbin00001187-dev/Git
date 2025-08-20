using UnityEngine;
using System.Collections.Generic; // List�� ����ϱ� ���� �߰�

public class Tile : MonoBehaviour
{
    [Header("�׸��� ��ǥ")]
    [Tooltip("�� Ÿ���� �׸��� ���� X, Y ��ǥ�Դϴ�.")]
    public Vector2Int gridPosition;

    [Header("Ÿ�� �Ӽ� �� ȿ��")]
    [Tooltip("�� Ÿ���� �̵� �������� ���θ� ��Ÿ���ϴ�. (��: ��, ���� ��)")]
    public bool isWalkable = true; // �̵� ���� ���� (�⺻��: true)

    [Tooltip("�� Ÿ�Ͽ� ����� Ư�� ȿ�� ����Դϴ�. (��: �� ����, ���� �� ��)")]
    public List<TileEffect> activeEffects = new List<TileEffect>(); // Ÿ�Ͽ� ����� Ư�� ȿ�� ���

    // ���⿡ Ÿ���� �߰����� �Ӽ��̳� ���¸� ������ �� �ֽ��ϴ�.
    // ��: public E_TileType tileType; (Ÿ�� ���� enum)
    // ��: public int defenseBonus; (��� ���ʽ�)

    /// <summary>
    /// Ÿ�Ͽ� ���ο� ȿ���� �߰��մϴ�.
    /// </summary>
    /// <param name="effect">�߰��� TileEffect ��ü</param>
    public void AddEffect(TileEffect effect)
    {
        if (effect == null) return;
        activeEffects.Add(effect);
        Debug.Log($"[Tile] Tile {gridPosition}�� {effect.effectName} ȿ���� �߰��Ǿ����ϴ�.");
        // TODO: ȿ�� ���� ���� (��: ������ ����� �� ������, ���� ��)
    }

    /// <summary>
    /// Ÿ�Ͽ��� Ư�� ȿ���� �����մϴ�.
    /// </summary>
    /// <param name="effectName">������ ȿ���� �̸�</param>
    public void RemoveEffect(string effectName)
    {
        int removedCount = activeEffects.RemoveAll(e => e.effectName == effectName);
        if (removedCount > 0)
        {
            Debug.Log($"[Tile] Tile {gridPosition}���� {effectName} ȿ�� {removedCount}���� ���ŵǾ����ϴ�.");
        }
    }

    /// <summary>
    /// Ÿ�Ͽ� Ư�� ȿ���� ����Ǿ� �ִ��� Ȯ���մϴ�.
    /// </summary>
    /// <param name="effectName">Ȯ���� ȿ���� �̸�</param>
    /// <returns>ȿ���� �����ϸ� true, �ƴϸ� false</returns>
    public bool HasEffect(string effectName)
    {
        return activeEffects.Exists(e => e.effectName == effectName);
    }
}

// TileEffect Ŭ������ Tile ��ũ��Ʈ�� �Բ� ���� �� �ִ� �����Դϴ�.
// �� Ŭ������ ScriptableObject�� ����ų�, GameActionó�� [System.Serializable]�� ���� �� �ֽ��ϴ�.
[System.Serializable]
public class TileEffect
{
    public string effectName; // ȿ�� �̸� (��: "�� ����", "��� ��ȭ")
    public int duration;      // ȿ�� ���� �� (0�̸� ����)
    public int value;         // ȿ�� �� (��: �� ������, ���� ������)
    // TODO: ȿ�� Ÿ�� (E_TileEffectType enum), ȿ�� ������ �� �߰� ����
}
