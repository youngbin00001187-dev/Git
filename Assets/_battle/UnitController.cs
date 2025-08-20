using UnityEngine;
using System.Collections;
using System; // [System.Obsolete] 어트리뷰트를 사용하기 위해 추가

// ▼▼▼ [신규] 유닛의 상태를 정의하는 Enum ▼▼▼
public enum UnitState
{
    Normal, // 통상 상태
    Stun    // 스턴 상태
}
// ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

public abstract class UnitController : MonoBehaviour
{
    public enum UnitType { Player, Enemy }
    public UnitType unitType;

    [Header("공통 능력치")]
    public int maxHealth = 100;
    public int currentHealth;

    // ▼▼▼ [신규] 유닛의 현재 상태를 저장하는 변수와 헤더 추가 ▼▼▼
    [Header("유닛 상태")]
    [Tooltip("유닛의 현재 상태입니다. (예: Normal, Stun)")]
    public UnitState currentState = UnitState.Normal;
    // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

    [Header("UI 연결")]
    public HealthBarUI healthBar;

    [Header("공통 상태")]
    public GameObject currentTile;
    protected bool isActing = false;
    protected Animator animator;

    [Header("피격 시각 효과")]
    protected SpriteRenderer unitSpriteRenderer;
    public Color hitColor = Color.red;
    public float hitColorDuration = 0.15f;
    private Color originalColor;

    public float shakeIntensity = 0.1f;
    public float shakeDuration = 0.2f;

    [Header("캐릭터 원근감 설정")]
    public float unitPerspectiveFactor = 0f;
    public float scaleChangeSpeed = 12f;

    private Vector3 originalScale;

    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
        unitSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        currentHealth = maxHealth;
        UpdateHealthUI();

        if (unitSpriteRenderer != null)
        {
            originalColor = unitSpriteRenderer.color;
        }

        originalScale = transform.localScale;
    }

    // ===================================================================
    //                  ▼▼▼ 핵심 수정 부분 ▼▼▼
    // ===================================================================

    /// <summary>
    /// [신규] 데미지를 포함한 모든 종류의 '충격'에 대한 반응을 처리하는 새로운 중앙 함수입니다.
    /// 앞으로 모든 피격 관련 로직은 이 함수를 통해 시작됩니다.
    /// </summary>
    /// <param name="damage">충격에 동반된 피해량. 순수 효과만 있다면 0이 될 수 있습니다.</param>
    public virtual void TakeImpact(int damage)
    {
        // 1. 모든 충격에 공통적으로 적용될 시각 효과를 먼저 재생합니다.
        PlayHitEffect();

        // 2. "피격 시 에너지 충전" 같은 공통 로-직은 여기에 추가됩니다.

        // 3. 실제 체력 감소는 아래의 내부 함수에게 위임합니다.
        ApplyHealthDamage(damage);
    }

    /// <summary>
    /// [역할 변경] 기존 TakeDamage 함수입니다.
    /// 이제 하위 호환성을 위해 유지되며, 모든 처리를 새로운 TakeImpact 함수로 전달합니다.
    /// </summary>
    [Obsolete("TakeImpact(damage)를 대신 사용하세요. 이 함수는 하위 호환성을 위해 남겨져 있습니다.")]
    public virtual void TakeDamage(int damage)
    {
        // 모든 처리를 새로운 '응급실'인 TakeImpact에게 보냅니다.
        TakeImpact(damage);
    }

    /// <summary>
    /// [신규] 시각 효과만을 담당하는 내부 헬퍼 함수입니다.
    /// </summary>
    private void PlayHitEffect()
    {
        if (animator != null) { StartCoroutine(PlayHitAnimation(99, 0.5f)); }
        if (unitSpriteRenderer != null) { StartCoroutine(FlashColor(hitColor, hitColorDuration)); }
        StartCoroutine(ShakeEffect(shakeDuration, shakeIntensity));
    }

    /// <summary>
    /// [신규] 체력 감소와 사망 처리만을 담당하는 내부 헬퍼 함수입니다.
    /// </summary>
    private void ApplyHealthDamage(int damage)
    {
        if (damage <= 0) return;

        currentHealth -= damage;
        UpdateHealthUI();

        if (currentHealth <= 0) Die();
    }

    // ===================================================================
    //                  ▲▲▲ 핵심 수정 부분 끝 ▲▲▲
    // ===================================================================

    // ▼▼▼ [신규] 상태를 안전하게 변경하기 위한 공용 함수 ▼▼▼
    /// <summary>
    /// 유닛의 상태를 변경하고, 상태 변경에 따른 효과를 처리합니다.
    /// </summary>
    /// <param name="newState">새롭게 적용할 상태</param>
    public void SetState(UnitState newState)
    {
        if (currentState == newState) return;

        currentState = newState;
        Debug.Log($"[UnitController] {gameObject.name}의 상태가 {newState}(으)로 변경되었습니다.");

        // TODO: 여기에 상태 변경에 따른 시각 효과(VFX)나 애니메이션 트리거를 추가할 수 있습니다.
        // 예시: 스턴 아이콘 표시/숨기기 등
    }
    // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

    protected void OnPositionChanged()
    {
        Debug.Log($"[OnPositionChanged] {name} 위치 변경 감지. acting={isActing}");
        if (this is EnemyController enemy)
        {
            enemy.UpdateIntentDisplay();
        }
    }

    public void MoveToTile(GameObject tile)
    {
        if (tile != null)
        {
            if (currentTile != null)
            {
                GridManager.Instance.UnregisterUnitPosition(this, GetGridPosition());
            }

            transform.position = tile.transform.position;
            currentTile = tile;

            GridManager.Instance.RegisterUnitPosition(this, GetGridPosition());

            if (unitSpriteRenderer != null)
            {
                unitSpriteRenderer.sortingOrder = 100 - GetGridPosition().y;
            }

            UpdateScale();
            OnPositionChanged();
        }
    }

    public IEnumerator MoveCoroutine(GameObject targetTile, float moveSpeed)
    {
        if (currentTile != null)
        {
            GridManager.Instance.UnregisterUnitPosition(this, GetGridPosition());
        }

        Vector3 targetPosition = targetTile.transform.position;
        Vector2Int targetGridPos = GridManager.Instance.GetGridPositionFromTileObject(targetTile);

        if (unitSpriteRenderer != null)
        {
            unitSpriteRenderer.sortingOrder = 100 - targetGridPos.y;
        }

        float perspective = 1f + ((GridManager.Instance.height - 1) - targetGridPos.y) * unitPerspectiveFactor;
        Vector3 targetScale = originalScale * perspective;

        while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, scaleChangeSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetPosition;
        transform.localScale = targetScale;

        currentTile = targetTile;
        GridManager.Instance.RegisterUnitPosition(this, GetGridPosition());

        OnPositionChanged();
    }
    // UnitController.cs 의 FlyArcCoroutine을 아래 내용으로 완전히 교체하고 실행해주세요.

    /// <summary>
    /// [문제 진단용] 이동 시간 계산 과정을 확인하기 위한 디버그 코드가 추가되었습니다.
    /// </summary>
    public IEnumerator FlyArcCoroutine(GameObject targetTile, float moveSpeed, float arcHeight = 1.5f)
    {
        // --- 1. 애니메이션에 필요한 모든 좌표를 '실시간'으로 미리 계산합니다. ---
        Vector3 startPos = transform.position;
        Vector3 endPos = targetTile.transform.position;
        Vector2Int targetGridPos = GridManager.Instance.GetGridPositionFromTileObject(targetTile);
        Vector3 controlPoint = (startPos + endPos) * 0.5f + Vector3.up * arcHeight;
        float perspective = 1f + ((GridManager.Instance.height - 1) - targetGridPos.y) * unitPerspectiveFactor;
        Vector3 targetScale = originalScale * perspective;
        float distance = Vector3.Distance(startPos, endPos);
        float duration = (moveSpeed > 0) ? distance / moveSpeed : 0;
        float elapsed = 0f;

        // --- 2. 애니메이션을 시작하기 전에, 그리드에서 자신의 '현재' 위치만 제거합니다. ---
        if (currentTile != null)
        {
            GridManager.Instance.UnregisterUnitPosition(this, GetGridPosition());
        }

        // --- 3. 순수한 '시각적 연출'(애니메이션)을 실행합니다. ---
        if (unitSpriteRenderer != null)
        {
            unitSpriteRenderer.sortingOrder = 100 - targetGridPos.y;
        }

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            Vector3 a = Vector3.Lerp(startPos, controlPoint, t);
            Vector3 b = Vector3.Lerp(controlPoint, endPos, t);
            transform.position = Vector3.Lerp(a, b, t);
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, scaleChangeSpeed * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // --- 4. 모든 애니메이션이 끝난 후, '논리적 데이터'를 한 번에 갱신합니다. ---
        transform.position = endPos;
        transform.localScale = targetScale;
        currentTile = targetTile;
        GridManager.Instance.RegisterUnitPosition(this, GetGridPosition());
        OnPositionChanged();
    }
    public void UpdateScale()
    {
        if (currentTile == null) return;
        Vector2Int gridPos = GetGridPosition();
        if (gridPos.y == -1) return;
        float perspectiveScale = 1f + ((GridManager.Instance.height - 1) - gridPos.y) * unitPerspectiveFactor;
        transform.localScale = originalScale * perspectiveScale;
    }

    protected void UpdateHealthUI()
    {
        if (healthBar != null) { healthBar.UpdateHealth(currentHealth, maxHealth); }
    }

    public Vector2Int GetGridPosition()
    {
        if (currentTile == null) { return new Vector2Int(-1, -1); }
        Tile tileComponent = currentTile.GetComponent<Tile>();
        if (tileComponent != null) { return tileComponent.gridPosition; }
        string[] parts = currentTile.name.Split('_');
        if (parts.Length == 3) { if (int.TryParse(parts[1], out int x) && int.TryParse(parts[2], out int y)) return new Vector2Int(x, y); }
        return new Vector2Int(-1, -1);
    }

    protected virtual void Die()
    {
        Debug.Log($"<color=red>{gameObject.name}이(가) 사망했습니다.</color>");

        if (animator != null)
        {
            StartCoroutine(PlayHitAnimation(99, 0.5f));
        }
        if (unitSpriteRenderer != null)
        {
            StartCoroutine(FlashColor(hitColor, hitColorDuration));
        }
        StartCoroutine(ShakeEffect(shakeDuration, shakeIntensity));

        if (currentTile != null)
        {
            GridManager.Instance.UnregisterUnitPosition(this, GetGridPosition());
        }

        gameObject.SetActive(false);
        Destroy(gameObject, 1.5f);
    }

    private IEnumerator FlashColor(Color flashColor, float duration)
    {
        if (unitSpriteRenderer == null) yield break;
        unitSpriteRenderer.color = flashColor;
        yield return new WaitForSeconds(duration);
        unitSpriteRenderer.color = originalColor;
    }
    public IEnumerator RecoilCoroutine(Vector3 targetPosition, float recoilSpeed = 10f)
    {
        Vector3 startPosition = transform.position;
        // 목표 지점의 절반까지만 이동하여 '부딪히는' 느낌을 줍니다.
        Vector3 bumpPosition = Vector3.Lerp(startPosition, targetPosition, 0.4f);
        float duration = Vector3.Distance(startPosition, bumpPosition) / recoilSpeed;
        float elapsedTime = 0f;

        // --- 1. 목표를 향해 날아가는 단계 ---
        while (elapsedTime < duration)
        {
            transform.position = Vector3.Lerp(startPosition, bumpPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.position = bumpPosition;

        elapsedTime = 0f; // 타이머 리셋

        // --- 2. 원래 자리로 튕겨 나오는 단계 ---
        while (elapsedTime < duration)
        {
            transform.position = Vector3.Lerp(bumpPosition, startPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.position = startPosition; // 위치 보정

        // ▼▼▼ [신규] 리코일 후 스턴 상태로 변경하는 로직 ▼▼▼
    }
    private IEnumerator ShakeEffect(float duration, float intensity)
    {
        Vector3 startWorldPosition = transform.position;
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            // ▼▼▼ 이 부분이 수정되었습니다 ▼▼▼
            transform.position = startWorldPosition + (Vector3)UnityEngine.Random.insideUnitCircle * intensity;
            // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.position = startWorldPosition;
    }

    private IEnumerator PlayHitAnimation(int motionIdToPlay, float animationDuration)
    {
        if (animator == null)
        {
            Debug.LogWarning("Animator가 없어서 PlayHitAnimation을 실행할 수 없습니다.");
            yield break;
        }

        animator.SetInteger("motionID", motionIdToPlay);
        yield return new WaitForSeconds(animationDuration);
        animator.SetInteger("motionID", 0);
    }
}