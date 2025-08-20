using UnityEngine;
using TMPro; // TextMeshPro를 사용하기 위해 추가
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
            // 위로 이동
            transform.position += Vector3.up * moveSpeed * Time.deltaTime;

            // 서서히 투명하게
            float alpha = Mathf.Lerp(originalColor.a, 0f, timer / fadeOutTime);
            textMesh.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);

            timer += Time.deltaTime;
            yield return null;
        }

        // 애니메이션이 끝나면 오브젝트 파괴
        Destroy(gameObject);
    }
} // <--- 이 닫는 괄호 '}'가 누락되어 있었습니다.