using UnityEngine;
using System.Collections.Generic; // List�� ����ϱ� ���� �ʿ��մϴ�.
using TMPro; // TextMeshPro�� ����ϱ� ���� �ʿ��մϴ�.

/// <summary>
/// ������ ��� �ð� ȿ��(VFX)�� �߾ӿ��� �����ϰ� ����ϴ� �̱��� �Ŵ����Դϴ�.
/// </summary>
public class VFXManager : MonoBehaviour
{
    public static VFXManager Instance { get; private set; }

    [Header("VFX ������ ��� (������)")]
    [Tooltip("��� VFX �������� ���⿡ ����մϴ�. ����Ʈ�� ������ VFX ID�� �˴ϴ�. (0��, 1��, 2��...)")]
    public List<GameObject> vfxPrefabList;

    [Header("������ ���� ���� (���� ���)")]
    [Tooltip("������ ���ڸ� ǥ���� UI ������")]
    public GameObject damageNumberPrefab;

    [Tooltip("������ ���ڰ� ������ UI ĵ����")]
    public Canvas worldSpaceCanvas;

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    /// <summary>
    /// ������ ��ġ�� ID�� �ش��ϴ� ����Ʈ�� ����մϴ�.
    /// </summary>
    /// <param name="position">����Ʈ�� ������ ���� ��ǥ</param>
    /// <param name="vfxId">vfxPrefabList�� ��ϵ� ����Ʈ�� ID (����Ʈ ����)</param>
    public void PlayHitEffect(Vector3 position, int vfxId)
    {
        // ���� ���⿡ ����� �α� �߰� ����
        Debug.Log($"<color=lightblue>[VFXManager] PlayHitEffect�� ID: {vfxId} �� ȣ��Ǿ����ϴ�.</color>");
        // ����������������������

        // ID�� ��ȿ�� ���� ���� �ִ��� Ȯ���մϴ�. (-1�� 'ȿ�� ����'�� �ǹ�)
        if (vfxId < 0 || vfxId >= vfxPrefabList.Count)
        {
            return; // ��ȿ���� ���� ID�� �ƹ��͵� ���� �ʰ� �Լ��� �����մϴ�.
        }

        // ID�� �̿��� ����Ʈ���� ���� �������� �����ɴϴ�.
        GameObject effectPrefab = vfxPrefabList[vfxId];

        if (effectPrefab != null)
        {
            // �������� �����Ͽ� ����Ʈ�� �����մϴ�.
            Instantiate(effectPrefab, position, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning($"[VFXManager] vfxPrefabList�� {vfxId}�� �ε����� ����ֽ��ϴ�!");
        }
    }

    /// <summary>
    /// ������ ��ġ�� ������ ���ڸ� ǥ���մϴ�. (�� ����� ������� �ʾҽ��ϴ�)
    /// </summary>
    /// <param name="position">���ڰ� ������ ���� ���� ��ǥ</param>
    /// <param name="damage">ǥ���� ������ ��ġ</param>
    public void ShowDamageNumber(Vector3 position, int damage)
    {
        if (damageNumberPrefab != null && worldSpaceCanvas != null)
        {
            GameObject damageTextObj = Instantiate(damageNumberPrefab, worldSpaceCanvas.transform);
            damageTextObj.transform.position = position;

            TextMeshProUGUI tmp = damageTextObj.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.text = damage.ToString();
            }
        }
    }
}