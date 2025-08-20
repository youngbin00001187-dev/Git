using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening; // DOTween 사용을 위해 필요

public class CustomerDialogueDisplay : MonoBehaviour
{
    [Header("UI 요소")]
    public GameObject dialogueCanvasGameObject; // 캔버스 전체를 켜고 끌 때 사용
    public Image bubbleImage;                   // 크기가 조절될 배경 이미지 (부모)
    public TextMeshProUGUI dialogueText;      // 대사가 표시될 텍스트 (자식)

    [Header("표시 설정")]
    public float displayDuration = 3f;
    public float typingSpeed = 0.05f;

    [Header("말풍선 자동 크기 설정")]
    public float maxWidth = 280f;
    public float horizontalPadding = 20f;
    public float verticalPadding = 16f;
    public float resizeDuration = 0.15f;

    private Coroutine currentDialogueCoroutine;

    void Awake()
    {
        dialogueCanvasGameObject?.SetActive(false);
        bubbleImage?.gameObject.SetActive(false);

        if (bubbleImage != null)
        {
            bubbleImage.rectTransform.pivot = new Vector2(0, 1);
        }
    }

    /// <summary>
    /// 대사를 표시합니다.
    /// </summary>
    /// <param name="dialogue">표시할 대사 내용</param>
    /// <param name="autoHide">지정된 displayDuration 후 자동으로 숨길지 여부 (기본값 true)</param>
    public void ShowDialogue(string dialogue, bool autoHide = true) // <<=== autoHide 매개변수 추가
    {
        if (bubbleImage == null || dialogueText == null || string.IsNullOrEmpty(dialogue))
        {
            Debug.LogWarning("필수 컴포넌트가 누락되었거나 대사 내용이 비어 있습니다.");
            return;
        }

        if (currentDialogueCoroutine != null)
        {
            StopCoroutine(currentDialogueCoroutine);
        }

        dialogueCanvasGameObject?.SetActive(true);
        bubbleImage.gameObject.SetActive(true);

        dialogueText.text = "";
        currentDialogueCoroutine = StartCoroutine(DisplayDialogueRoutine(dialogue, autoHide)); // <<=== autoHide 전달
    }

    /// <summary>
    /// 대사를 강제로 숨깁니다.
    /// </summary>
    public void ForceHideDialogue() // <<=== 강제 숨김 함수 추가
    {
        HideDialogue(); // 기존 HideDialogue 로직 재활용
    }

    /// <summary>
    /// 대사를 숨깁니다. (내부용)
    /// </summary>
    private void HideDialogue() // <<=== private으로 변경 (외부에서는 ForceHideDialogue 사용)
    {
        if (currentDialogueCoroutine != null)
        {
            StopCoroutine(currentDialogueCoroutine);
            currentDialogueCoroutine = null;
        }

        bubbleImage?.gameObject.SetActive(false);
    }

    private System.Collections.IEnumerator DisplayDialogueRoutine(string fullDialogue, bool autoHide) // <<=== autoHide 매개변수 추가
    {
        // 타이핑 효과
        if (typingSpeed > 0)
        {
            for (int i = 0; i < fullDialogue.Length; i++)
            {
                dialogueText.text += fullDialogue[i];
                ResizeBubbleImmediately();
                yield return new WaitForSeconds(typingSpeed);
            }
        }
        else
        {
            dialogueText.text = fullDialogue;
            ResizeBubbleImmediately();
        }

        // 타이핑 완료 후 부드럽게 최종 크기로 조절
        ResizeBubbleAnimated();

        // === 변경된 부분: autoHide가 true일 때만 타이머 후 숨김 ===
        if (autoHide)
        {
            yield return new WaitForSeconds(displayDuration);
            HideDialogue();
        }
        // ====================================================
    }

    private void ResizeBubbleImmediately()
    {
        Vector2 preferredSize = dialogueText.GetPreferredValues(dialogueText.text, maxWidth - horizontalPadding, Mathf.Infinity);
        float targetWidth = Mathf.Min(preferredSize.x + horizontalPadding, maxWidth);
        float targetHeight = preferredSize.y + verticalPadding;
        bubbleImage.rectTransform.DOKill(true);
        bubbleImage.rectTransform.sizeDelta = new Vector2(targetWidth, targetHeight);
    }

    private void ResizeBubbleAnimated()
    {
        Vector2 preferredSize = dialogueText.GetPreferredValues(dialogueText.text, maxWidth - horizontalPadding, Mathf.Infinity);
        float targetWidth = Mathf.Min(preferredSize.x + horizontalPadding, maxWidth);
        float targetHeight = preferredSize.y + verticalPadding;
        bubbleImage.rectTransform.DOKill(true);
        bubbleImage.rectTransform.DOSizeDelta(new Vector2(targetWidth, targetHeight), resizeDuration).SetEase(Ease.OutQuad);
    }
}