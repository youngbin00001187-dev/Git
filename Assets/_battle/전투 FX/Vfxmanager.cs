using UnityEngine;
using TMPro; // TextMeshPro를 사용하기 위해 필요합니다.

public class VFXManager : MonoBehaviour
{
    public static VFXManager Instance { get; private set; }

    [Header("VFX 프리팹 연결")]
    [Tooltip("기본 피격 시 재생될 파티클 이펙트 프리팹")]
    public GameObject hitEffectPrefab;

    [Tooltip("데미지 숫자를 표시할 UI 프리팹")]
    public GameObject damageNumberPrefab;

    [Header("캔버스 설정")]
    [Tooltip("데미지 숫자가 생성될 UI 캔버스")]
    public Canvas worldSpaceCanvas;

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    /// <summary>
    /// 지정된 위치에 피격 이펙트를 재생합니다.
    /// </summary>
    /// <param name="position">이펙트가 생성될 월드 좌표</param>
    public void PlayHitEffect(Vector3 position)
    {
        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, position, Quaternion.identity);
        }
    }

    /// <summary>
    /// 지정된 위치에 데미지 숫자를 표시합니다.
    /// </summary>
    /// <param name="position">숫자가 생성될 기준 월드 좌표</param>
    /// <param name="damage">표시할 데미지 수치</param>
    public void ShowDamageNumber(Vector3 position, int damage)
    {
        if (damageNumberPrefab != null && worldSpaceCanvas != null)
        {
            GameObject damageTextObj = Instantiate(damageNumberPrefab, worldSpaceCanvas.transform);
            damageTextObj.transform.position = position;

            // TextMeshPro 컴포넌트를 찾아 데미지 텍스트를 설정합니다.
            TextMeshProUGUI tmp = damageTextObj.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.text = damage.ToString();
            }

            // 데미지 숫자 애니메이션 스크립트가 있다면 자동으로 움직이고 사라집니다.
            // (3단계에서 설명)
        }
    }
}