using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class TileManager : MonoBehaviour
{
    // Highlight types and their priorities (lower number = higher priority)
    public enum HighlightType
    {
        PlayerPreview = 0, // Priority 0: Predicted attack range (most important)
        PlayerTarget = 1,  // Priority 1: Player targeting range
        DamageFlash = 2,   // Priority 2: Damage flash
        EnemyIntent = 3    // Priority 3: Enemy action intent (least important)
    }

    // Dictionary to record which highlight types each tile is included in
    private Dictionary<GameObject, List<HighlightType>> highlightedTileInfo = new Dictionary<GameObject, List<HighlightType>>();
    // Dictionary to store the color for each highlight type
    private Dictionary<HighlightType, Color> highlightColors = new Dictionary<HighlightType, Color>();

    public static TileManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    // --- Public Functions ---

    public void ShowPlayerTargeting(List<GameObject> tiles, Color color)
    {
        SetTilesHighlight(HighlightType.PlayerTarget, tiles, color);
    }

    public void ShowPlayerDamagePreview(List<GameObject> tiles)
    {
        SetTilesHighlight(HighlightType.PlayerPreview, tiles, Color.red);
    }

    public void ClearPlayerHighlights()
    {
        ClearHighlight(HighlightType.PlayerTarget);
        ClearHighlight(HighlightType.PlayerPreview);
    }

    public void ShowEnemyIntent(List<GameObject> tiles)
    {
        SetTilesHighlight(HighlightType.EnemyIntent, tiles, Color.red);
    }

    public void ClearEnemyIntent()
    {
        ClearHighlight(HighlightType.EnemyIntent);
    }


    public void ApplyDamageToArea(GameObject originTile, List<Vector2Int> areaOfEffect, int damage, UnitController attacker)
    {
        List<GameObject> hitTiles = new List<GameObject>();
        Vector2Int originPos = GetGridPositionFromTileName(originTile.name);
        Vector2Int attackerPos = attacker.GetGridPosition();

        Vector2Int attackDirection = originPos - attackerPos;
        if (attackDirection == Vector2Int.zero) attackDirection = Vector2Int.up;

        foreach (var baseVector in areaOfEffect)
        {
            Vector2Int rotatedVector = RotateVector(baseVector, attackDirection);
            Vector2Int hitPos = originPos + rotatedVector;
            GameObject hitTile = GridManager.Instance.GetTileAtPosition(hitPos);
            if (hitTile != null) hitTiles.Add(hitTile);
        }

        StartCoroutine(FlashDamageHighlight(hitTiles));

        foreach (var hitTile in hitTiles)
        {
            Tile hitTileComponent = hitTile.GetComponent<Tile>();
            if (hitTileComponent == null) continue;

            UnitController victim = GridManager.Instance.GetUnitAtPosition(hitTileComponent.gridPosition);

            if (victim != null && victim != attacker)
            {
                // --- ▼▼▼ 이 부분이 핵심 수정 내용입니다 ▼▼▼ ---
                int finalDamage = damage; // 기본 데미지로 시작

                // 1. 공격자가 플레이어인지 확인합니다.
                if (attacker is PlayerController)
                {
                    // 2. GlobalManager에서 플레이어의 추가 능력치를 가져옵니다.
                    int equipmentBonus = GlobalManager.instance.playerAttackPower;
                    float simbeopMultiplier = GlobalManager.instance.simbeopDamageMultiplier;

                    // 3. 새로운 공식으로 최종 데미지를 계산합니다.
                    finalDamage = Mathf.RoundToInt((damage + equipmentBonus) * (1f + simbeopMultiplier));
                    Debug.Log($"플레이어 데미지 계산: (기본:{damage} + 장비:{equipmentBonus}) * (1 + 심법:{simbeopMultiplier}) = 최종:{finalDamage}");
                }

                // 4. 최종 계산된 데미지를 입힙니다.
                victim.TakeImpact(finalDamage);
                // --- ▲▲▲ 핵심 수정 내용 끝 ▲▲▲ ---
            }
        }
    }

    public IEnumerator FlashDamageHighlight(List<GameObject> tiles, float duration = 0.2f)
    {
        // Turn on red highlight with DamageFlash type
        TileManager.Instance.SetTilesHighlight(TileManager.HighlightType.DamageFlash, tiles, new Color(1f, 0f, 0f, 1f));

        yield return new WaitForSeconds(duration);

        // Turn off only DamageFlash highlight
        TileManager.Instance.ClearHighlight(TileManager.HighlightType.DamageFlash);
    }

    // --- New Core Logic ---

    public void SetTilesHighlight(HighlightType type, List<GameObject> tiles, Color color)
    {
        // [수정] 이 함수는 이제 기존 하이라이트를 지우지 않고 덮어쓰거나 추가만 합니다.
        // ClearHighlight(type); // 이 줄을 제거하여 다른 타일의 같은 타입 하이라이트를 유지합니다.

        highlightColors[type] = color;
        foreach (var tile in tiles)
        {
            if (tile == null) continue;
            if (!highlightedTileInfo.ContainsKey(tile))
            {
                highlightedTileInfo[tile] = new List<HighlightType>();
            }
            // 중복 추가 방지
            if (!highlightedTileInfo[tile].Contains(type))
            {
                highlightedTileInfo[tile].Add(type);
            }
        }
        UpdateSpecificTileColors(tiles);
    }
    public void ClearHighlightForSpecificTiles(List<GameObject> tilesToClear, HighlightType type)
    {
        if (tilesToClear == null || tilesToClear.Count == 0) return;

        foreach (var tile in tilesToClear)
        {
            if (tile != null && highlightedTileInfo.ContainsKey(tile))
            {
                highlightedTileInfo[tile].Remove(type);
            }
        }
        UpdateSpecificTileColors(tilesToClear);
    }

    public void ClearHighlight(HighlightType type)
    {
        List<GameObject> tilesToUpdate = new List<GameObject>();
        // Iterate through all highlighted tiles to find those with the specified type
        foreach (var pair in highlightedTileInfo)
        {
            if (pair.Value.Contains(type))
            {
                pair.Value.Remove(type); // Remove the highlight type
                tilesToUpdate.Add(pair.Key); // Add to list of tiles to update
            }
        }
        UpdateSpecificTileColors(tilesToUpdate); // Update colors for affected tiles
    }

    private void UpdateSpecificTileColors(List<GameObject> tiles)
    {
        foreach (var tile in tiles)
        {
            if (tile == null) continue;
            SpriteRenderer renderer = tile.GetComponent<SpriteRenderer>();
            if (renderer == null) continue;

            if (highlightedTileInfo.TryGetValue(tile, out List<HighlightType> types) && types.Any())
            {
                HighlightType highestPriorityType = types.Min();
                Color newColor = highlightColors[highestPriorityType];

                // 하이라이트 적용 시에는 원래 타일의 고정된 알파 값을 기준으로 계산
                float originalFixedAlpha = GridManager.Instance.TilePrefab.GetComponent<SpriteRenderer>().color.a;
                float finalAlpha = Mathf.Lerp(originalFixedAlpha, 1.0f, 0.5f);

                renderer.color = new Color(newColor.r, newColor.g, newColor.b, finalAlpha);
            }
            else
            {
                // ▼▼▼ 하이라이트 제거 시 복원 로직 ▼▼▼
                // 원본 프리팹의 알파 값을 가져와서 색상을 완벽하게 복원
                float prefabAlpha = GridManager.Instance.TilePrefab.GetComponent<SpriteRenderer>().color.a;
                renderer.color = new Color(1, 1, 1, prefabAlpha);

                highlightedTileInfo.Remove(tile);
            }
        }
    }

    // --- Helper Functions ---

    public Vector2Int GetGridPositionFromTileName(string name)
    {
        // Tile.cs 컴포넌트가 있다면 우선적으로 사용하도록 GridManager에서 이미 수정됨
        // GridManager.GetGridPositionFromTileObject()를 호출하여 일관성 유지
        return GridManager.Instance.GetGridPositionFromTileObject(GameObject.Find(name));
    }

    public Vector2Int RotateVector(Vector2Int vector, Vector2Int direction)
    {
        // Normalize direction to cardinal (up, down, left, right) if it's not zero
        if (direction.sqrMagnitude > 0)
        {
            // ▼▼▼ 수정된 부분: 대각선 방향을 가장 가까운 주축으로 정규화 ▼▼▼
            if (Mathf.Abs(direction.x) >= Mathf.Abs(direction.y)) // X축 변화량이 Y축 변화량보다 크거나 같으면 수평으로 정규화
            {
                direction = new Vector2Int((int)Mathf.Sign(direction.x), 0);
            }
            else // Y축 변화량이 더 크면 수직으로 정규화
            {
                direction = new Vector2Int(0, (int)Mathf.Sign(direction.y));
            }
            // ▲▲▲ 수정된 부분 ▲▲▲
        }
        else
        {
            direction = Vector2Int.up; // Default direction if zero
        }

        // Apply rotation based on normalized direction
        if (direction == Vector2Int.up)
        {
            return vector;
        }
        if (direction == Vector2Int.right)
        {
            return new Vector2Int(vector.y, -vector.x); // 90 deg clockwise
        }
        if (direction == Vector2Int.down)
        {
            return new Vector2Int(-vector.x, -vector.y); // 180 deg
        }
        if (direction == Vector2Int.left)
        {
            return new Vector2Int(-vector.y, vector.x); // 90 deg counter-clockwise
        }

        return vector; // Should not be reached if directions are handled
    }
}