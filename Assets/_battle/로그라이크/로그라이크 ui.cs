using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using System; // Action을 사용하기 위해 추가

public class RoguelikeMapUI : MonoBehaviour
{
    public static RoguelikeMapUI instance;

    [Header("UI 설정")]
    public Transform mapNodeParent;
    public GameObject mapNodePrefab;
    public RectTransform playerIcon;

    [Header("맵 레이아웃 설정")]
    public float horizontalSpacing = 150f;
    public float verticalSpacing = 100f;
    public Vector2 randomOffsetRange = new Vector2(20f, 15f);
    [Header("연결선 설정")]
    public Color lineColor = Color.white;
    public float lineWidth = 3f;

    private List<GameObject> connectionLines = new List<GameObject>();
    private List<MapNode> currentMapData = new List<MapNode>();
    private Dictionary<MapNode, RectTransform> nodeTransforms = new Dictionary<MapNode, RectTransform>();

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    /// <summary>
    /// 맵 생성을 시작하는 외부 진입점입니다. 모든 작업 완료 후 onComplete 콜백을 호출합니다.
    /// </summary>
    /// <param name="mapData">생성할 맵의 데이터</param>
    /// <param name="onComplete">맵 UI 생성이 모두 완료되었을 때 호출될 콜백 함수</param>
    public void GenerateMap(List<MapNode> mapData, Action onComplete)
    {
        StartCoroutine(DisplayMapCoroutine(mapData, onComplete));
    }

    private IEnumerator DisplayMapCoroutine(List<MapNode> mapData, Action onComplete)
    {
        // 1. 기존 UI 요소들 정리
        foreach (Transform child in mapNodeParent)
            Destroy(child.gameObject);
        ClearConnectionLines();
        nodeTransforms.Clear();
        currentMapData = new List<MapNode>(mapData);

        if (mapNodePrefab == null)
        {
            Debug.LogError("[RoguelikeMapUI] Map Node Prefab이 할당되지 않았습니다!");
            yield break;
        }

        // 2. 맵 노드(아이콘)들 생성 및 배치
        foreach (var node in mapData)
        {
            GameObject nodeObj = Instantiate(mapNodePrefab, mapNodeParent);
            RectTransform rectTransform = nodeObj.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                // --- ▼▼▼ 이 부분이 수정되었습니다 ▼▼▼ ---
                Vector2 basePosition = new Vector2(node.position.x * horizontalSpacing, node.position.y * verticalSpacing);

                // 시작 노드와 보스 노드는 흔들지 않고 중앙에 고정합니다.
                if (node.type != NodeType.Start && node.type != NodeType.Boss)
                {
                    float randomX = UnityEngine.Random.Range(-randomOffsetRange.x, randomOffsetRange.x);
                    float randomY = UnityEngine.Random.Range(-randomOffsetRange.y, randomOffsetRange.y);
                    rectTransform.anchoredPosition = basePosition + new Vector2(randomX, randomY);
                }
                else
                {
                    rectTransform.anchoredPosition = basePosition;
                }
                // --- ▲▲▲ 수정된 부분 끝 ▲▲▲ ---

                nodeTransforms[node] = rectTransform;
            }

            MapNodeUI nodeUI = nodeObj.GetComponent<MapNodeUI>();
            if (nodeUI != null)
            {
                nodeUI.Setup(node);
                RoguelikeManager.instance.RegisterNodeUI(nodeUI);
            }
        }

        // 3. Unity UI 엔진이 위치 계산을 완료하도록 한 프레임 대기
        yield return new WaitForEndOfFrame();

        // 4. 최종 위치를 기반으로 연결선 생성
        CreateConnectionLines(mapData);

        // 5. 모든 작업이 끝났음을 원래 요청했던 곳(RoguelikeManager)에 알림
        Debug.Log("맵 UI 생성 완료. RoguelikeManager에 콜백을 보냅니다.");
        onComplete?.Invoke();
    }

    private void CreateConnectionLines(List<MapNode> mapData)
    {
        for (int i = 0; i < mapData.Count; i++)
        {
            var currentNode = mapData[i];
            foreach (int nextNodeIndex in currentNode.nextNodeIndices)
            {
                if (nextNodeIndex >= 0 && nextNodeIndex < mapData.Count)
                {
                    var nextNode = mapData[nextNodeIndex];
                    if (nextNode.position.x == currentNode.position.x + 1)
                    {
                        CreateConnectionLine(currentNode, nextNode);
                    }
                }
            }
        }
    }

    private void CreateConnectionLine(MapNode fromNode, MapNode toNode)
    {
        if (!nodeTransforms.ContainsKey(fromNode) || !nodeTransforms.ContainsKey(toNode))
        {
            Debug.LogWarning("노드 Transform을 찾을 수 없습니다.");
            return;
        }

        GameObject lineObj = new GameObject($"ConnectionLine_{fromNode.position}_{toNode.position}");
        lineObj.transform.SetParent(mapNodeParent, false);

        Image lineImage = lineObj.AddComponent<Image>();
        lineImage.color = lineColor;
        lineImage.raycastTarget = false;

        RectTransform lineRect = lineObj.GetComponent<RectTransform>();
        Vector2 startPos = nodeTransforms[fromNode].anchoredPosition;
        Vector2 endPos = nodeTransforms[toNode].anchoredPosition;
        Vector2 direction = endPos - startPos;
        float distance = direction.magnitude;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        lineRect.anchorMin = new Vector2(0.5f, 0.5f);
        lineRect.anchorMax = new Vector2(0.5f, 0.5f);
        lineRect.pivot = new Vector2(0.5f, 0.5f);

        lineRect.sizeDelta = new Vector2(distance, lineWidth);
        lineRect.anchoredPosition = (startPos + endPos) / 2;
        lineRect.localRotation = Quaternion.Euler(0, 0, angle);

        lineRect.SetAsFirstSibling();
        connectionLines.Add(lineObj);
    }

    private void ClearConnectionLines()
    {
        foreach (var line in connectionLines)
        {
            if (line != null)
                Destroy(line);
        }
        connectionLines.Clear();
    }

    public void FocusMapOnNode(RectTransform nodeRect)
    {
        if (nodeRect == null || mapNodeParent == null)
            return;

        RectTransform parentRect = mapNodeParent.GetComponent<RectTransform>();
        Vector2 targetLocalPos = -nodeRect.anchoredPosition;
        // 약간 왼쪽 오프셋을 주면 더 보기 좋음
        targetLocalPos.x += 300;
        mapNodeParent.GetComponent<RectTransform>().DOAnchorPos(targetLocalPos, 0.5f).SetEase(Ease.InOutSine);
    }

    public void MovePlayerIconTo(Vector3 targetPos, float duration, System.Action onComplete = null)
    {
        if (playerIcon == null)
        {
            Debug.LogWarning("[RoguelikeMapUI] playerIcon이 할당되지 않았습니다.");
            onComplete?.Invoke();
            return;
        }

        playerIcon.DOMove(targetPos, duration).SetEase(Ease.InOutSine).OnComplete(() =>
        {
            onComplete?.Invoke();
        });
    }

    public void SetPlayerIconPosition(Vector3 newPos)
    {
        if (playerIcon != null)
            playerIcon.position = newPos;
        else
            Debug.LogWarning("[RoguelikeMapUI] playerIcon이 할당되지 않았습니다.");
    }

    // 연결선 스타일 업데이트
    public void UpdateConnectionLineStyle(Color newColor, float newWidth)
    {
        lineColor = newColor;
        lineWidth = newWidth;

        foreach (var lineObj in connectionLines)
        {
            if (lineObj != null)
            {
                Image lineImage = lineObj.GetComponent<Image>();
                if (lineImage != null)
                    lineImage.color = newColor;

                RectTransform lineRect = lineObj.GetComponent<RectTransform>();
                if (lineRect != null)
                {
                    Vector2 currentSize = lineRect.sizeDelta;
                    lineRect.sizeDelta = new Vector2(currentSize.x, newWidth);
                }
            }
        }
    }

    void OnDestroy()
    {
        ClearConnectionLines();
    }
}