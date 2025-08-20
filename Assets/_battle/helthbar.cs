using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBarUI : MonoBehaviour
{
    public Slider slider;
    private int _previousHealth;

    [Header("데미지/힐 숫자 표시")]
    public GameObject damageNumberPrefab;
    public GameObject healNumberPrefab;
    public Canvas parentCanvas;

    // ▼▼▼ [신규] 외곽선 두께를 인스펙터에서 조절할 수 있도록 변수 추가 ▼▼▼
    [Header("외곽선 설정")]
    [Tooltip("데미지/힐 숫자에 적용될 외곽선의 두께입니다.")]
    public float numberOutlineWidth = 0.2f;

    void Start()
    {
        UnitController parentUnit = GetComponentInParent<UnitController>();
        if (parentUnit != null)
        {
            _previousHealth = parentUnit.maxHealth;
        }
    }

    public void UpdateHealth(int currentHealth, int maxHealth)
    {
        if (slider == null)
        {
            Debug.LogError("[HealthBarUI] Slider가 연결되지 않았습니다!");
            return;
        }

        int healthChange = currentHealth - _previousHealth;

        // ▼▼▼ [수정] 숫자 표시 함수 호출 시 외곽선 색상 정보를 함께 전달합니다. ▼▼▼
        if (healthChange < 0) // 데미지를 입었을 경우
        {
            ShowFloatingNumber(damageNumberPrefab, Mathf.Abs(healthChange).ToString(), Color.red);
        }
        else if (healthChange > 0) // 체력을 회복했을 경우
        {
            ShowFloatingNumber(healNumberPrefab, "+" + healthChange.ToString(), Color.green);
        }

        float healthPercentage = (float)currentHealth / maxHealth;
        slider.value = healthPercentage;

        _previousHealth = currentHealth;
    }

    /// <summary>
    /// 지정된 프리팹으로 텍스트를 생성하고 외곽선을 설정하는 함수
    /// </summary>
    // ▼▼▼ [수정] Color 파라미터를 추가로 받도록 변경합니다. ▼▼▼
    private void ShowFloatingNumber(GameObject prefab, string textValue, Color outlineColor)
    {
        if (prefab != null && parentCanvas != null)
        {
            GameObject numberObj = Instantiate(prefab, parentCanvas.transform);

            TextMeshProUGUI tmp = numberObj.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.text = textValue;

                // ▼▼▼ [신규] 외곽선 두께와 색상을 스크립트로 설정합니다. ▼▼▼
                tmp.outlineWidth = numberOutlineWidth;
                tmp.outlineColor = outlineColor;
            }
        }
        else
        {
            Debug.LogWarning("[HealthBarUI] 숫자 프리팹 또는 부모 캔버스가 할당되지 않았습니다!");
        }
    }

    private int GetMaxHealthFromParent()
    {
        UnitController parentUnit = GetComponentInParent<UnitController>();
        if (parentUnit != null)
        {
            return parentUnit.maxHealth;
        }
        return 100; // 기본값
    }
}