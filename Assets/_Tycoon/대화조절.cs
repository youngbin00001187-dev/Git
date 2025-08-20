using TMPro;
using UnityEngine;
using DG.Tweening;

public static class BubbleUtils
{
    /// <summary>
    /// 타이핑 중 즉시 크기 변경 (애니메이션 없이 바로 적용)
    /// </summary>
    public static void ResizeBubbleImmediately(
        TextMeshProUGUI text,
        RectTransform rect,
        Vector2 padding,    // 좌우, 상하 합산 패딩 (예: 좌+우=20, 상+하=16)
        Vector2 baseSize,   // 최소 크기
        float maxWidth)     // 최대 너비 제한
    {
        if (text == null || rect == null) return;

        text.enableWordWrapping = true;

        Vector2 preferredSize = text.GetPreferredValues(text.text, maxWidth - padding.x, Mathf.Infinity);

        float targetWidth = Mathf.Clamp(preferredSize.x + padding.x, baseSize.x, maxWidth);
        float targetHeight = Mathf.Max(baseSize.y, preferredSize.y + padding.y);

        rect.DOKill(true);
        rect.sizeDelta = new Vector2(targetWidth, targetHeight);
    }

    /// <summary>
    /// 타이핑 완료 후 부드러운 애니메이션으로 크기 변경
    /// </summary>
    public static void ResizeBubbleToFitText(
        TextMeshProUGUI text,
        RectTransform rect,
        Vector2 padding,
        Vector2 baseSize,
        float maxWidth,
        float duration = 0.15f)
    {
        if (text == null || rect == null) return;

        text.enableWordWrapping = true;

        Vector2 preferredSize = text.GetPreferredValues(text.text, maxWidth - padding.x, Mathf.Infinity);

        float targetWidth = Mathf.Clamp(preferredSize.x + padding.x, baseSize.x, maxWidth);
        float targetHeight = Mathf.Max(baseSize.y, preferredSize.y + padding.y);

        rect.DOKill(true);
        rect.DOSizeDelta(new Vector2(targetWidth, targetHeight), duration)
            .SetEase(Ease.OutQuad);
    }
}