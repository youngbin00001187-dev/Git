using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }
    public GameObject TilePrefab => tilePrefab;

    [Header("맵 크기 설정")]
    public int width = 8;
    public int height = 5;

    [Header("원근감 및 위치 설정")]
    public float perspectiveFactor = 0.15f;
    public float verticalSpacingMultiplier = 0.7f;
    public float horizontalSpacingMultiplier = 1.0f;
    public float shearFactor = -0.5f;
    public float gridXOffset = 0f;
    public float gridYOffset = 0f;

    [Header("오브젝트 설정")]
    [SerializeField]
    private GameObject tilePrefab;

    [Header("펼침 효과 설정")]
    public float tileRevealDelay = 0.01f;
    public float tileAnimationDuration = 0.2f;

    private Dictionary<Vector2Int, GameObject> grid = new Dictionary<Vector2Int, GameObject>();
    private Dictionary<Vector2Int, UnitController> unitsOnGrid = new Dictionary<Vector2Int, UnitController>();

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    public void GenerateGrid()
    {
        DestroyGrid(); // 기존 그리드를 먼저 파괴

        float nominalTileHeight = tilePrefab.transform.localScale.y;
        float totalHeight = height * nominalTileHeight * verticalSpacingMultiplier;
        transform.position = new Vector3(gridXOffset, gridYOffset - (totalHeight / 2f), 0);

        for (int y = 0; y < height; y++)
        {
            float scale = 1f + ((height - 1) - y) * perspectiveFactor;
            float currentTileWidth = tilePrefab.transform.localScale.x * scale;
            float rowWidth = width * (currentTileWidth * horizontalSpacingMultiplier);
            float startX = -rowWidth / 2f + (currentTileWidth * horizontalSpacingMultiplier) / 2f;

            for (int x = 0; x < width; x++)
            {
                GameObject newTile = Instantiate(tilePrefab, transform);
                newTile.transform.localScale = tilePrefab.transform.localScale * scale;
                float currentTileHeight = tilePrefab.transform.localScale.y * scale;
                float posY = y * (currentTileHeight * verticalSpacingMultiplier);
                float shearOffset = posY * shearFactor;
                float posX = startX + (x * (currentTileWidth * horizontalSpacingMultiplier)) + shearOffset;
                newTile.transform.localPosition = new Vector3(posX, posY, 0);
                newTile.name = $"Tile_{x}_{y}";
                newTile.tag = "Tile";
                Tile tileComponent = newTile.AddComponent<Tile>();
                tileComponent.gridPosition = new Vector2Int(x, y);
                if (newTile.GetComponent<BoxCollider2D>() == null)
                {
                    newTile.AddComponent<BoxCollider2D>();
                }
                grid[new Vector2Int(x, y)] = newTile;
            }
        }
    }

    public IEnumerator GenerateGridCoroutine()
    {
        DestroyGrid(); // 기존 그리드를 먼저 파괴

        float nominalTileHeight = tilePrefab.transform.localScale.y;
        float totalHeight = height * nominalTileHeight * verticalSpacingMultiplier;
        transform.position = new Vector3(gridXOffset, gridYOffset - (totalHeight / 2f), 0);

        for (int y = 0; y < height; y++)
        {
            float scale = 1f + ((height - 1) - y) * perspectiveFactor;
            float currentTileWidth = tilePrefab.transform.localScale.x * scale;
            float rowWidth = width * (currentTileWidth * horizontalSpacingMultiplier);
            float startX = -rowWidth / 2f + (currentTileWidth * horizontalSpacingMultiplier) / 2f;

            for (int x = 0; x < width; x++)
            {
                GameObject newTile = Instantiate(tilePrefab, transform);
                Vector3 finalScale = tilePrefab.transform.localScale * scale;
                newTile.transform.localScale = Vector3.zero;
                float currentTileHeight = tilePrefab.transform.localScale.y * scale;
                float posY = y * (currentTileHeight * verticalSpacingMultiplier);
                float shearOffset = posY * shearFactor;
                float posX = startX + (x * (currentTileWidth * horizontalSpacingMultiplier)) + shearOffset;
                newTile.transform.localPosition = new Vector3(posX, posY, 0);
                newTile.name = $"Tile_{x}_{y}";
                newTile.tag = "Tile";
                Tile tileComponent = newTile.AddComponent<Tile>();
                tileComponent.gridPosition = new Vector2Int(x, y);
                if (newTile.GetComponent<BoxCollider2D>() == null)
                {
                    newTile.AddComponent<BoxCollider2D>();
                }
                grid[new Vector2Int(x, y)] = newTile;
                StartCoroutine(AnimateTileAppearance(newTile, finalScale));
                yield return new WaitForSeconds(tileRevealDelay);
            }
        }
        yield return new WaitForSeconds(tileAnimationDuration);
    }

    private IEnumerator AnimateTileAppearance(GameObject tile, Vector3 targetScale)
    {
        float timer = 0f;
        while (timer < tileAnimationDuration)
        {
            tile.transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, timer / tileAnimationDuration);
            timer += Time.deltaTime;
            yield return null;
        }
        tile.transform.localScale = targetScale;
    }

    // --- ▼▼▼ 이 부분이 핵심 수정 내용입니다 ▼▼▼ ---
    /// <summary>
    /// [신규] 현재 생성된 모든 타일 오브젝트를 파괴하고 관련 데이터를 초기화합니다.
    /// </summary>
    public void DestroyGrid()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        grid.Clear();
        unitsOnGrid.Clear();
    }
    // --- ▲▲▲ 핵심 수정 내용 끝 ▲▲▲ ---

    public GameObject GetTileAtPosition(Vector2Int position)
    {
        grid.TryGetValue(position, out GameObject tile);
        return tile;
    }

    public void RegisterUnitPosition(UnitController unit, Vector2Int position)
    {
        if (unitsOnGrid.ContainsKey(position))
        {
            unitsOnGrid[position] = unit;
        }
        else
        {
            unitsOnGrid.Add(position, unit);
        }
    }

    public void UnregisterUnitPosition(UnitController unit, Vector2Int position)
    {
        if (unitsOnGrid.ContainsKey(position) && unitsOnGrid[position] == unit)
        {
            unitsOnGrid.Remove(position);
        }
    }

    public UnitController GetUnitAtPosition(Vector2Int position)
    {
        unitsOnGrid.TryGetValue(position, out UnitController unit);
        return unit;
    }

    public bool IsTileOccupied(Vector2Int position)
    {
        return unitsOnGrid.ContainsKey(position);
    }

    public bool IsTileWalkable(Vector2Int position)
    {
        if (grid.TryGetValue(position, out GameObject tileObject))
        {
            Tile tileComponent = tileObject.GetComponent<Tile>();
            return tileComponent != null && tileComponent.isWalkable && !IsTileOccupied(position);
        }
        return false;
    }

    public Vector2Int GetGridPositionFromTileObject(GameObject tileObject)
    {
        if (tileObject == null) return new Vector2Int(-1, -1);
        Tile tileComponent = tileObject.GetComponent<Tile>();
        if (tileComponent != null)
        {
            return tileComponent.gridPosition;
        }
        string[] parts = tileObject.name.Split('_');
        if (parts.Length == 3 && int.TryParse(parts[1], out int x) && int.TryParse(parts[2], out int y))
            return new Vector2Int(x, y);
        return new Vector2Int(-1, -1);
    }
}