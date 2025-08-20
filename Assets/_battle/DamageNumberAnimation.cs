using UnityEngine;
using TMPro; // TextMeshPro�� ����ϱ� ���� �߰�
using System.Collections;

public class DamageNumberAnimation : MonoBehaviour
{
    public float moveSpeed = 1.0f;
    public float fadeOutTime = 0.8f;
    private TextMeshProUGUI textMesh;
    private Color originalColor;

    void Start()
    {
        textMesh = GetComponentInChildren<TextMeshProUGUI>();
        originalColor = textMesh.color;
        StartCoroutine(Animate());
    }

    private IEnumerator Animate()
    {
        float timer = 0f;
        while (timer < fadeOutTime)
        {
            // ���� �̵�
            transform.position += Vector3.up * moveSpeed * Time.deltaTime;

            // ������ �����ϰ�
            float alpha = Mathf.Lerp(originalColor.a, 0f, timer / fadeOutTime);
            textMesh.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);

            timer += Time.deltaTime;
            yield return null;
        }

        // �ִϸ��̼��� ������ ������Ʈ �ı�
        Destroy(gameObject);
    }
} // <--- �� �ݴ� ��ȣ '}'�� �����Ǿ� �־����ϴ�.