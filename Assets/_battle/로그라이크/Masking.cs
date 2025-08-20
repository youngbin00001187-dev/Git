using UnityEngine;
using System.Collections;

public class UnfurlScroll : MonoBehaviour
{
    public float targetWidth = 500f; // 최종적으로 펼쳐질 너비
    public float duration = 2.0f;    // 펼쳐지는 데 걸리는 시간

    private RectTransform rectTransform;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        StartCoroutine(Unfurl());
    }

    IEnumerator Unfurl()
    {
        float elapsedTime = 0f;
        float startWidth = rectTransform.sizeDelta.x; // 시작 시점의 너비

        while (elapsedTime < duration)
        {
            // 시간에 따라 시작 너비에서 목표 너비까지 점진적으로 값을 계산
            float newWidth = Mathf.Lerp(startWidth, targetWidth, elapsedTime / duration);

            // RectTransform의 너비(sizeDelta.x)를 변경
            rectTransform.sizeDelta = new Vector2(newWidth, rectTransform.sizeDelta.y);

            elapsedTime += Time.deltaTime;
            yield return null; // 다음 프레임까지 대기
        }

        // 애니메이션 종료 후 정확한 목표값으로 설정
        rectTransform.sizeDelta = new Vector2(targetWidth, rectTransform.sizeDelta.y);
    }
}