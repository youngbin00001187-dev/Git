using UnityEngine;
using TMPro; // TextMeshPro�� ����ϱ� ���� �ʿ��մϴ�.

public class VFXManager : MonoBehaviour
{
    public static VFXManager Instance { get; private set; }

    [Header("VFX ������ ����")]
    [Tooltip("�⺻ �ǰ� �� ����� ��ƼŬ ����Ʈ ������")]
    public GameObject hitEffectPrefab;

    [Tooltip("������ ���ڸ� ǥ���� UI ������")]
    public GameObject damageNumberPrefab;

    [Header("ĵ���� ����")]
    [Tooltip("������ ���ڰ� ������ UI ĵ����")]
    public Canvas worldSpaceCanvas;

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    /// <summary>
    /// ������ ��ġ�� �ǰ� ����Ʈ�� ����մϴ�.
    /// </summary>
    /// <param name="position">����Ʈ�� ������ ���� ��ǥ</param>
    public void PlayHitEffect(Vector3 position)
    {
        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, position, Quaternion.identity);
        }
    }

    /// <summary>
    /// ������ ��ġ�� ������ ���ڸ� ǥ���մϴ�.
    /// </summary>
    /// <param name="position">���ڰ� ������ ���� ���� ��ǥ</param>
    /// <param name="damage">ǥ���� ������ ��ġ</param>
    public void ShowDamageNumber(Vector3 position, int damage)
    {
        if (damageNumberPrefab != null && worldSpaceCanvas != null)
        {
            GameObject damageTextObj = Instantiate(damageNumberPrefab, worldSpaceCanvas.transform);
            damageTextObj.transform.position = position;

            // TextMeshPro ������Ʈ�� ã�� ������ �ؽ�Ʈ�� �����մϴ�.
            TextMeshProUGUI tmp = damageTextObj.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.text = damage.ToString();
            }

            // ������ ���� �ִϸ��̼� ��ũ��Ʈ�� �ִٸ� �ڵ����� �����̰� ������ϴ�.
            // (3�ܰ迡�� ����)
        }
    }
}