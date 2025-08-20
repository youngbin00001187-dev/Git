using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using System; // Action�� ����ϱ� ���� �߰�

public class RoguelikeMapUI : MonoBehaviour
{
    public static RoguelikeMapUI instance;

    [Header("UI ����")]
    public Transform mapNodeParent;
    public GameObject mapNodePrefab;
    public RectTransform playerIcon;

    [Header("�� ���̾ƿ� ����")]
    public float horizontalSpacing = 150f;
    public float verticalSpacing = 100f;
    public Vector2 randomOffsetRange = new Vector2(20f, 15f);
    [Header("���ἱ ����")]
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
    /// �� ������ �����ϴ� �ܺ� �������Դϴ�. ��� �۾� �Ϸ� �� onComplete �ݹ��� ȣ���մϴ�.
    /// </summary>
    /// <param name="mapData">������ ���� ������</param>
    /// <param name="onComplete">�� UI ������ ��� �Ϸ�Ǿ��� �� ȣ��� �ݹ� �Լ�</param>
    public void GenerateMap(List<MapNode> mapData, Action onComplete)
    {
        StartCoroutine(DisplayMapCoroutine(mapData, onComplete));
    }

    private IEnumerator DisplayMapCoroutine(List<MapNode> mapData, Action onComplete)
    {
        // 1. ���� UI ��ҵ� ����
        foreach (Transform child in mapNodeParent)
            Destroy(child.gameObject);
        ClearConnectionLines();
        nodeTransforms.Clear();
        currentMapData = new List<MapNode>(mapData);

        if (mapNodePrefab == null)
        {
            Debug.LogError("[RoguelikeMapUI] Map Node Prefab�� �Ҵ���� �ʾҽ��ϴ�!");
            yield break;
        }

        // 2. �� ���(������)�� ���� �� ��ġ
        foreach (var node in mapData)
        {
            GameObject nodeObj = Instantiate(mapNodePrefab, mapNodeParent);
            RectTransform rectTransform = nodeObj.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                // --- ���� �� �κ��� �����Ǿ����ϴ� ���� ---
                Vector2 basePosition = new Vector2(node.position.x * horizontalSpacing, node.position.y * verticalSpacing);

                // ���� ���� ���� ���� ����� �ʰ� �߾ӿ� �����մϴ�.
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
                // --- ���� ������ �κ� �� ���� ---

                nodeTransforms[node] = rectTransform;
            }

            MapNodeUI nodeUI = nodeObj.GetComponent<MapNodeUI>();
            if (nodeUI != null)
            {
                nodeUI.Setup(node);
                RoguelikeManager.instance.RegisterNodeUI(nodeUI);
            }
        }

        // 3. Unity UI ������ ��ġ ����� �Ϸ��ϵ��� �� ������ ���
        yield return new WaitForEndOfFrame();

        // 4. ���� ��ġ�� ������� ���ἱ ����
        CreateConnectionLines(mapData);

        // 5. ��� �۾��� �������� ���� ��û�ߴ� ��(RoguelikeManager)�� �˸�
        Debug.Log("�� UI ���� �Ϸ�. RoguelikeManager�� �ݹ��� �����ϴ�.");
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
            Debug.LogWarning("��� Transform�� ã�� �� �����ϴ�.");
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
        // �ణ ���� �������� �ָ� �� ���� ����
        targetLocalPos.x += 300;
        mapNodeParent.GetComponent<RectTransform>().DOAnchorPos(targetLocalPos, 0.5f).SetEase(Ease.InOutSine);
    }

    public void MovePlayerIconTo(Vector3 targetPos, float duration, System.Action onComplete = null)
    {
        if (playerIcon == null)
        {
            Debug.LogWarning("[RoguelikeMapUI] playerIcon�� �Ҵ���� �ʾҽ��ϴ�.");
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
            Debug.LogWarning("[RoguelikeMapUI] playerIcon�� �Ҵ���� �ʾҽ��ϴ�.");
    }

    // ���ἱ ��Ÿ�� ������Ʈ
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