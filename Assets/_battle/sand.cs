using UnityEngine;
using System.Collections;

/// <summary>
/// '캔슬' 성공 시, 대상 스프라이트 주변에 여러 색상의 잔상을 흩뿌리는 이펙트를 관리합니다.
/// </summary>
public class SandevistanGhostSpawner : MonoBehaviour
{
    [Header("타겟 설정")]
    [Tooltip("잔상이 복사할 원본 SpriteRenderer 입니다. (보통 플레이어의 SpriteRenderer)")]
    public SpriteRenderer targetRenderer;

    [Header("이펙트 프리펩")]
    [Tooltip("잔상으로 사용될 프리팹입니다. SpriteRenderer 컴포넌트를 포함해야 합니다.")]
    public GameObject ghostPrefab;

    [Header("잔상 생성 설정")]
    [Tooltip("생성될 잔상의 총 개수입니다.")]
    public int ghostCount = 6;
    [Tooltip("각 잔상이 생성되는 시간 간격(초)입니다.")]
    public float spawnInterval = 0.05f;

    [Header("잔상 표현 설정")]
    [Tooltip("잔상이 흩뿌려지는 최대 범위입니다.")]
    public float scatterRange = 0.4f;
    [Tooltip("잔상이 완전히 사라질 때까지 걸리는 시간(초)입니다.")]
    public float ghostLifetime = 0.4f;
    [Tooltip("잔상에 적용될 무작위 색상 목록입니다.")]
    public Color[] ghostColors;

    // ▼▼▼ 새로 추가할 부분 시작 ▼▼▼
    [Header("이펙트 시작 딜레이")]
    [Tooltip("TriggerGhostEffect 호출 후, 첫 잔상이 생성되기까지 기다리는 시간(초)입니다. Time.timeScale의 영향을 받지 않습니다.")]
    public float startDelay = 0f; // 기본값 0으로 설정하여 즉시 시작
    // ▲▲▲ 새로 추가할 부분 끝 ▲▲▲

    /// <summary>
    /// 외부에서 이 함수를 호출하여 잔상 효과를 시작시킵니다.
    /// </summary>
    public void TriggerGhostEffect()
    {
        if (targetRenderer == null || ghostPrefab == null)
        {
            Debug.LogError("[Sandevistan] Target Renderer 또는 Ghost Prefab이 할당되지 않았습니다!");
            return;
        }

        // ▼▼▼ 이펙트 시작 딜레이 적용 ▼▼▼
        StartCoroutine(SpawnGhostsWithDelay(startDelay));
    }

    // ▼▼▼ 새로 추가할 코루틴 ▼▼▼
    private IEnumerator SpawnGhostsWithDelay(float delay)
    {
        if (delay > 0)
        {
            yield return new WaitForSecondsRealtime(delay); // Time.timeScale 영향을 받지 않는 딜레이
        }
        yield return StartCoroutine(SpawnGhosts()); // 실제 잔상 생성 코루틴 시작
    }

    private IEnumerator SpawnGhosts()
    {
        for (int i = 0; i < ghostCount; i++)
        {
            CreateGhost();
            // Time.timeScale의 영향을 받지 않도록 WaitForSecondsRealtime을 사용합니다.
            yield return new WaitForSecondsRealtime(spawnInterval);
        }
    }

    private void CreateGhost()
    {
        // 1. 현재 시점의 위치와 회전 사용
        Vector3 spawnPosition = targetRenderer.transform.position;
        Quaternion spawnRotation = targetRenderer.transform.rotation;

        GameObject ghost = Instantiate(ghostPrefab, spawnPosition, spawnRotation);
        SpriteRenderer ghostRenderer = ghost.GetComponent<SpriteRenderer>();

        // 2. 현재 스프라이트와 상태 복사
        ghostRenderer.sprite = targetRenderer.sprite;
        ghostRenderer.flipX = targetRenderer.flipX;
        ghostRenderer.sortingLayerID = targetRenderer.sortingLayerID;
        ghostRenderer.sortingOrder = targetRenderer.sortingOrder - 1;

        // 3. 컬러 적용
        if (ghostColors != null && ghostColors.Length > 0)
        {
            ghostRenderer.color = ghostColors[Random.Range(0, ghostColors.Length)];
        }

        // 4. 무작위 위치 오프셋
        Vector3 offset = new Vector3(
            Random.Range(-scatterRange, scatterRange),
            Random.Range(-scatterRange * 0.5f, scatterRange * 0.5f),
            0f
        );
        ghost.transform.position += offset;

        // 5. 사라지는 효과
        StartCoroutine(FadeAndDestroy(ghostRenderer, ghostLifetime));
    }

    private IEnumerator FadeAndDestroy(SpriteRenderer sr, float duration)
    {
        float timer = 0f;
        Color originalColor = sr.color;

        while (timer < duration)
        {
            // Time.timeScale의 영향을 받지 않도록 unscaledDeltaTime을 사용합니다.
            timer += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(originalColor.a, 0f, timer / duration);
            sr.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }
        Destroy(sr.gameObject);
    }
}