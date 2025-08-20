using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBarUI : MonoBehaviour
{
    public Slider slider;
    private int _previousHealth;

    [Header("������/�� ���� ǥ��")]
    public GameObject damageNumberPrefab;
    public GameObject healNumberPrefab;
    public Canvas parentCanvas;

    // ���� [�ű�] �ܰ��� �β��� �ν����Ϳ��� ������ �� �ֵ��� ���� �߰� ����
    [Header("�ܰ��� ����")]
    [Tooltip("������/�� ���ڿ� ����� �ܰ����� �β��Դϴ�.")]
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
            Debug.LogError("[HealthBarUI] Slider�� ������� �ʾҽ��ϴ�!");
            return;
        }

        int healthChange = currentHealth - _previousHealth;

        // ���� [����] ���� ǥ�� �Լ� ȣ�� �� �ܰ��� ���� ������ �Բ� �����մϴ�. ����
        if (healthChange < 0) // �������� �Ծ��� ���
        {
            ShowFloatingNumber(damageNumberPrefab, Mathf.Abs(healthChange).ToString(), Color.red);
        }
        else if (healthChange > 0) // ü���� ȸ������ ���
        {
            ShowFloatingNumber(healNumberPrefab, "+" + healthChange.ToString(), Color.green);
        }

        float healthPercentage = (float)currentHealth / maxHealth;
        slider.value = healthPercentage;

        _previousHealth = currentHealth;
    }

    /// <summary>
    /// ������ ���������� �ؽ�Ʈ�� �����ϰ� �ܰ����� �����ϴ� �Լ�
    /// </summary>
    // ���� [����] Color �Ķ���͸� �߰��� �޵��� �����մϴ�. ����
    private void ShowFloatingNumber(GameObject prefab, string textValue, Color outlineColor)
    {
        if (prefab != null && parentCanvas != null)
        {
            GameObject numberObj = Instantiate(prefab, parentCanvas.transform);

            TextMeshProUGUI tmp = numberObj.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.text = textValue;

                // ���� [�ű�] �ܰ��� �β��� ������ ��ũ��Ʈ�� �����մϴ�. ����
                tmp.outlineWidth = numberOutlineWidth;
                tmp.outlineColor = outlineColor;
            }
        }
        else
        {
            Debug.LogWarning("[HealthBarUI] ���� ������ �Ǵ� �θ� ĵ������ �Ҵ���� �ʾҽ��ϴ�!");
        }
    }

    private int GetMaxHealthFromParent()
    {
        UnitController parentUnit = GetComponentInParent<UnitController>();
        if (parentUnit != null)
        {
            return parentUnit.maxHealth;
        }
        return 100; // �⺻��
    }
}