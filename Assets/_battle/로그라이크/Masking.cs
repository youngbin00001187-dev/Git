using UnityEngine;
using System.Collections;

public class UnfurlScroll : MonoBehaviour
{
    public float targetWidth = 500f; // ���������� ������ �ʺ�
    public float duration = 2.0f;    // �������� �� �ɸ��� �ð�

    private RectTransform rectTransform;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        StartCoroutine(Unfurl());
    }

    IEnumerator Unfurl()
    {
        float elapsedTime = 0f;
        float startWidth = rectTransform.sizeDelta.x; // ���� ������ �ʺ�

        while (elapsedTime < duration)
        {
            // �ð��� ���� ���� �ʺ񿡼� ��ǥ �ʺ���� ���������� ���� ���
            float newWidth = Mathf.Lerp(startWidth, targetWidth, elapsedTime / duration);

            // RectTransform�� �ʺ�(sizeDelta.x)�� ����
            rectTransform.sizeDelta = new Vector2(newWidth, rectTransform.sizeDelta.y);

            elapsedTime += Time.deltaTime;
            yield return null; // ���� �����ӱ��� ���
        }

        // �ִϸ��̼� ���� �� ��Ȯ�� ��ǥ������ ����
        rectTransform.sizeDelta = new Vector2(targetWidth, rectTransform.sizeDelta.y);
    }
}