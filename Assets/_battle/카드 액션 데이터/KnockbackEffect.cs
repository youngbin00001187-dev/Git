using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using System;
using System.Linq;

[System.Serializable]
public class KnockbackEffect : GameEffect
{
    [Header("넉백 설정")]
    public int knockbackDistance = 1;
    public float moveSpeed = 3f;
    public float pushInterval = 0.2f;
    public float firstImpactInterval = 0.4f;
    public float arcHeight = 1.0f;
    public float startDelay = 0f;

    private struct KnockbackPlan
    {
        public UnitController unit;
        public GameObject targetTile;
        public bool isWallHit;
        public Vector2Int direction;
    }

    public override IEnumerator Apply(UnitController user, GameObject targetTile, Action onComplete)
    {
        UnitController initialTarget = GridManager.Instance.GetUnitAtPosition(
            GridManager.Instance.GetGridPositionFromTileObject(targetTile)
        );

        if (initialTarget == null || initialTarget == user)
        {
            Debug.LogWarning("[KnockbackEffect] Apply: 타겟 타일에 유닛이 없거나 자기 자신이어서 효과를 종료합니다.");
            onComplete?.Invoke();
            yield break;
        }

        // 처음 방향 계산 (대각선 포함, 고정)
        Vector2Int rawDir = initialTarget.GetGridPosition() - user.GetGridPosition();
        if (rawDir == Vector2Int.zero) rawDir = Vector2Int.up;
        Vector2Int fixedDirection = new Vector2Int(
            rawDir.x == 0 ? 0 : (int)Mathf.Sign(rawDir.x),
            rawDir.y == 0 ? 0 : (int)Mathf.Sign(rawDir.y)
        );

        Debug.Log($"<color=cyan>[KnockbackEffect] Apply 시작: {user.name} -> {initialTarget.name}, 방향={fixedDirection}, 거리={knockbackDistance}</color>");

        if (startDelay > 0f)
        {
            Debug.Log($"<color=cyan>[KnockbackEffect] {startDelay}초 딜레이 후 넉백을 시작합니다...</color>");
            yield return new WaitForSeconds(startDelay);
        }

        List<KnockbackPlan> knockbackPlans = new List<KnockbackPlan>();

        // 방향 고정 체인 계획
        PlanChainKnockback(initialTarget, fixedDirection, knockbackDistance, knockbackPlans);

        string planLog = string.Join(", ", knockbackPlans.Select(p =>
            $"{p.unit.name} -> {(p.isWallHit ? "WALL" : GridManager.Instance.GetGridPositionFromTileObject(p.targetTile).ToString())}"
        ));
        Debug.Log($"<color=lightblue>[KnockbackEffect] 계획 수립 완료. 총 {knockbackPlans.Count}개 이동 계획: [{planLog}]</color>");

        // 모든 경로가 막혔을 경우 → 리코일만 실행
        if (knockbackPlans.Count == 0 || (knockbackPlans.Count > 0 && knockbackPlans.All(p => p.isWallHit)))
        {
            Debug.LogWarning($"[KnockbackEffect] 모든 경로가 막혀 넉백이 불가능합니다. 첫 타겟({initialTarget.name})의 리코일만 실행합니다.");
            // --- 수정: 콜백 람다 제거 ---
            yield return initialTarget.StartCoroutine(
                initialTarget.RecoilCoroutine(
                    initialTarget.transform.position + (Vector3)(Vector2)fixedDirection * 0.5f
                )
            );
            // 리코일 코루틴이 끝난 뒤에 임팩트 호출
            initialTarget.TakeImpact(0);

            onComplete?.Invoke();
            yield break;
        }

        yield return user.StartCoroutine(ExecuteVisualKnockback(knockbackPlans, onComplete));
        Debug.Log("<color=cyan>[KnockbackEffect] Apply 종료.</color>");
    }

    // 방향 고정 버전
    private void PlanChainKnockback(UnitController currentUnit, Vector2Int fixedDirection, int distanceRemaining, List<KnockbackPlan> plans)
    {
        if (distanceRemaining <= 0) return;

        Vector2Int currentPos = currentUnit.GetGridPosition();
        Vector2Int destinationPos = currentPos + fixedDirection;
        GameObject destinationTile = GridManager.Instance.GetTileAtPosition(destinationPos);

        Debug.Log($"[Plan] {currentUnit.name} 계획 수립: {currentPos} -> {destinationPos} (고정방향: {fixedDirection}, 남은거리: {distanceRemaining})");

        // 벽 체크
        if (destinationTile == null || !destinationTile.GetComponent<Tile>().isWalkable)
        {
            Debug.Log($"[Plan] {currentUnit.name}의 목적지 {destinationPos}는 벽입니다. 리코일 처리.");
            plans.Add(new KnockbackPlan { unit = currentUnit, isWallHit = true, direction = fixedDirection });
            return;
        }

        // 유닛 충돌 체크
        UnitController occupant = GridManager.Instance.GetUnitAtPosition(destinationPos);
        if (occupant != null && occupant != currentUnit)
        {
            Debug.Log($"[Plan] {currentUnit.name}의 목적지 {destinationPos}에 {occupant.name}이(가) 있습니다. 연쇄 계획 시작.");
            PlanChainKnockback(occupant, fixedDirection, distanceRemaining, plans);

            bool occupantBlocked = false;
            for (int i = plans.Count - 1; i >= 0; i--)
            {
                if (plans[i].unit == occupant)
                {
                    occupantBlocked = plans[i].isWallHit;
                    break;
                }
            }

            if (occupantBlocked)
            {
                Debug.Log($"[Plan] {occupant.name}이(가) 움직일 수 없으므로, {currentUnit.name}도 막혔습니다.");
                plans.Add(new KnockbackPlan { unit = currentUnit, isWallHit = true, direction = fixedDirection });
                return;
            }
        }

        // 이동 계획 추가
        Debug.Log($"[Plan] {currentUnit.name}을(를) {destinationPos}로 이동 계획 추가.");
        plans.Add(new KnockbackPlan { unit = currentUnit, targetTile = destinationTile, isWallHit = false, direction = fixedDirection });

        if (distanceRemaining > 1)
        {
            PlanChainKnockback(currentUnit, fixedDirection, distanceRemaining - 1, plans);
        }
    }

    private IEnumerator ExecuteVisualKnockback(List<KnockbackPlan> plans, Action onComplete)
    {
        Debug.Log("<color=lime>[VisualKnockback] 시각 연출을 시작합니다...</color>");
        plans.Reverse();
        var sequence = DOTween.Sequence();
        float delay = 0f;
        bool isFirstInChain = true;

        // 이동 전 좌표 해제
        foreach (var plan in plans)
        {
            if (!plan.isWallHit)
                GridManager.Instance.UnregisterUnitPosition(plan.unit, plan.unit.GetGridPosition());
        }

        // 각 유닛 애니메이션
        foreach (var plan in plans)
        {
            if (plan.isWallHit)
            {
                // 리코일
                Debug.Log($"[VisualKnockback] {plan.unit.name} 리코일 애니메이션 {delay}초 뒤 시작.");
                Vector3 recoilTargetPos = plan.unit.transform.position + (Vector3)(Vector2)plan.direction * 0.4f;
                var recoilTween = DOTween.Sequence()
                    .Append(plan.unit.transform.DOMove(recoilTargetPos, 0.15f))
                    .Append(plan.unit.transform.DOMove(plan.unit.transform.position, 0.15f));

                // 콜백 등록 (Tween 자체에 등록)
                recoilTween.OnComplete(() => plan.unit.TakeImpact(0));

                sequence.Insert(delay, recoilTween);
            }
            else
            {
                // 점프 이동
                Debug.Log($"[VisualKnockback] {plan.unit.name} 이동 애니메이션 {delay}초 뒤 시작. -> {GridManager.Instance.GetGridPositionFromTileObject(plan.targetTile)}");
                float distance = Vector3.Distance(plan.unit.transform.position, plan.targetTile.transform.position);
                float duration = Mathf.Max(distance / moveSpeed, 0.5f);

                var moveTween = plan.unit.transform.DOJump(
                    plan.targetTile.transform.position,
                    arcHeight,
                    1,
                    duration
                )
                .SetEase(Ease.OutQuad);

                // 콜백 등록
                moveTween.OnComplete(() => plan.unit.TakeImpact(0));

                sequence.Insert(delay, moveTween);
            }

            delay += isFirstInChain ? firstImpactInterval : pushInterval;
            isFirstInChain = false;
        }

        yield return sequence.WaitForCompletion();

        // 좌표 갱신
        foreach (var plan in plans)
        {
            if (!plan.isWallHit)
            {
                plan.unit.MoveToTile(plan.targetTile);
            }
        }

        onComplete?.Invoke();
    }
}