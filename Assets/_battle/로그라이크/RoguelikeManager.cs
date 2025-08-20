using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;

public enum NodeType
{
    Start,
    Battle,
    EliteBattle,
    Shop,
    Rest,
    Boss
}

[System.Serializable]
public class MapNode
{
    public NodeType type;
    public Vector2Int position;
    public bool isCompleted = false;
    public EncounterSO encounter;
    public List<int> nextNodeIndices = new List<int>();
}

public class RoguelikeManager : MonoBehaviour
{
    public static RoguelikeManager instance;

    [Header("맵 생성 설정")]
    public int mapLength = 10;
    public int minNodesPerLayer = 2;
    public int maxNodesPerLayer = 3;

    [Header("맵 생성 규칙")]
    public List<int> restNodeLayers;

    [Header("조우(Encounter) 목록")]
    public List<EncounterSO> normalEncounters;
    public List<EncounterSO> eliteEncounters;
    public EncounterSO bossEncounter;

    [Header("UI 연결")]
    public GameObject deckBuildingPanel;
    public RectTransform mapPanelRect;

    private List<MapNode> generatedMap = new List<MapNode>();
    private List<MapNodeUI> nodeUIList = new List<MapNodeUI>();
    private MapNode currentNode;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (GlobalManager.instance != null && GlobalManager.instance.currentGameMode == GameMode.RoguelikeBattle)
        {
            GenerateNewMap();
            if (RoguelikeMapUI.instance != null)
            {
                RoguelikeMapUI.instance.GenerateMap(generatedMap, OnMapGenerationComplete);
            }
        }
    }

    public void OnMapGenerationComplete()
    {
        Debug.Log("RoguelikeMapUI로부터 맵 생성 완료 신호를 받았습니다. 후속 작업을 시작합니다.");
        PlacePlayerIconAtStart();
        SetNodeInteractableStates();
    }

    public void RegisterNodeUI(MapNodeUI nodeUI)
    {
        if (!nodeUIList.Contains(nodeUI)) nodeUIList.Add(nodeUI);
    }

    private void SetNodeInteractableStates()
    {
        foreach (var nodeUI in nodeUIList)
        {
            nodeUI.SetInteractable(false);
        }

        if (currentNode != null)
        {
            foreach (int nextIndex in currentNode.nextNodeIndices)
            {
                // ★★★ associatedNode 없이, 인덱스(순서)를 기반으로 찾는 방식으로 되돌렸습니다. ★★★
                if (nextIndex >= 0 && nextIndex < nodeUIList.Count)
                {
                    nodeUIList[nextIndex].SetInteractable(true);
                }
            }
        }
    }

    private void GenerateNewMap()
    {
        generatedMap.Clear();
        nodeUIList.Clear();
        var layers = new List<List<MapNode>>();

        var startLayer = new List<MapNode> { new MapNode { type = NodeType.Start, position = new Vector2Int(0, 0) } };
        layers.Add(startLayer);

        for (int i = 1; i < mapLength; i++)
        {
            var newLayer = new List<MapNode>();
            int nodesInLayer = GetNodesForLayer(i);
            bool isRestLayer = restNodeLayers.Contains(i);
            bool restNodePlacedThisLayer = false;

            for (int j = 0; j < nodesInLayer; j++)
            {
                int centeredY;
                if (nodesInLayer % 2 == 1) { centeredY = j - (nodesInLayer / 2); }
                else { centeredY = j - (nodesInLayer / 2) + (j >= nodesInLayer / 2 ? 1 : 0); }

                MapNode newNode = new MapNode { position = new Vector2Int(i, centeredY) };

                if (isRestLayer && !restNodePlacedThisLayer) { newNode.type = NodeType.Rest; restNodePlacedThisLayer = true; }
                else if (i == mapLength - 1) { newNode.type = NodeType.Boss; newNode.encounter = bossEncounter; }
                else if (IsEliteLayer(i)) { newNode.type = NodeType.EliteBattle; if (eliteEncounters.Any()) newNode.encounter = eliteEncounters[Random.Range(0, eliteEncounters.Count)]; }
                else { newNode.type = NodeType.Battle; if (normalEncounters.Any()) newNode.encounter = normalEncounters[Random.Range(0, normalEncounters.Count)]; }

                newLayer.Add(newNode);
            }
            layers.Add(newLayer);
        }

        generatedMap = layers.SelectMany(l => l).ToList();
        ConnectNodes(layers);
    }

    private int GetNodesForLayer(int layerIndex)
    {
        int totalLayers = mapLength - 1;
        if (layerIndex == totalLayers) return 1;

        int expandPhase = totalLayers / 3;
        int maxPhase = totalLayers * 2 / 3;

        if (layerIndex <= expandPhase) { return 2 + (layerIndex * 2 / expandPhase); }
        else if (layerIndex <= maxPhase) { return Random.Range(3, 5); }
        else { int remainingLayers = totalLayers - maxPhase; int layersIntoShrink = layerIndex - maxPhase; return Mathf.Max(2, 4 - (int)((float)layersIntoShrink / remainingLayers * 2f)); }
    }

    private bool IsEliteLayer(int layerIndex)
    {
        int totalLayers = mapLength - 1;
        return layerIndex == totalLayers / 3 || layerIndex == totalLayers * 2 / 3;
    }

    private void ConnectNodes(List<List<MapNode>> layers)
    {
        for (int i = 0; i < layers.Count - 1; i++)
        {
            var currentLayerNodes = layers[i];
            var nextLayerNodes = layers[i + 1];

            foreach (var node in currentLayerNodes)
            {
                int connectionsToMake = Random.Range(1, Mathf.Min(3, nextLayerNodes.Count + 1));
                var availableTargets = new List<MapNode>(nextLayerNodes);

                for (int c = 0; c < connectionsToMake && availableTargets.Count > 0; c++)
                {
                    var targetNode = availableTargets[Random.Range(0, availableTargets.Count)];
                    node.nextNodeIndices.Add(generatedMap.IndexOf(targetNode));
                    availableTargets.Remove(targetNode);
                }
            }

            foreach (var nextNode in nextLayerNodes)
            {
                int nextNodeIndex = generatedMap.IndexOf(nextNode);
                if (!currentLayerNodes.Any(n => n.nextNodeIndices.Contains(nextNodeIndex)))
                {
                    var sourceNode = currentLayerNodes[Random.Range(0, currentLayerNodes.Count)];
                    sourceNode.nextNodeIndices.Add(nextNodeIndex);
                }
            }
        }
    }

    private void PlacePlayerIconAtStart()
    {
        currentNode = generatedMap.FirstOrDefault(n => n.type == NodeType.Start);
        if (currentNode == null) return;

        if (RoguelikeMapUI.instance != null && nodeUIList.Any())
        {
            int startIndex = generatedMap.IndexOf(currentNode);
            // ★★★ associatedNode 없이, 인덱스(순서)를 기반으로 찾는 방식으로 되돌렸습니다. ★★★
            if (startIndex >= 0 && startIndex < nodeUIList.Count)
            {
                RoguelikeMapUI.instance.SetPlayerIconPosition(nodeUIList[startIndex].transform.position);
            }
        }
    }

    public void OnNodeSelected(MapNode selectedNode)
    {
        int selectedIndex = generatedMap.IndexOf(selectedNode);
        if (selectedIndex < 0) return;

        bool isValid = (currentNode == null && selectedNode.type == NodeType.Start) ||
                       (currentNode != null && currentNode.nextNodeIndices.Contains(selectedIndex));

        if (!isValid) return;

        StartCoroutine(MovePlayerIconAndHandleNode(selectedNode));
    }

    private IEnumerator MovePlayerIconAndHandleNode(MapNode nodeToHandle)
    {
        foreach (var nodeUI in nodeUIList) nodeUI.SetInteractable(false);

        int targetIndex = generatedMap.IndexOf(nodeToHandle);
        // ★★★ associatedNode 없이, 인덱스(순서)를 기반으로 찾는 방식으로 되돌렸습니다. ★★★
        if (targetIndex >= 0 && targetIndex < nodeUIList.Count)
        {
            bool iconMoveComplete = false;
            Vector3 targetPos = nodeUIList[targetIndex].transform.position;

            if (RoguelikeMapUI.instance != null)
            {
                RoguelikeMapUI.instance.MovePlayerIconTo(targetPos, 0.5f, () => { iconMoveComplete = true; });
            }
            else { iconMoveComplete = true; }

            yield return new WaitUntil(() => iconMoveComplete);
        }

        currentNode = nodeToHandle;
        currentNode.isCompleted = true;

        SetNodeInteractableStates();

        switch (nodeToHandle.type)
        {
            case NodeType.Battle:
            case NodeType.EliteBattle:
            case NodeType.Boss:
                GlobalManager.instance.SetEnemiesForBattle(nodeToHandle.encounter.enemies);
                if (BattleUIManager.instance != null) BattleUIManager.instance.ShowBattleUI();
                if (CoreEventManager.instance != null) CoreEventManager.instance.RaiseCombatStartRequested();
                break;
            case NodeType.Rest:
                if (deckBuildingPanel != null)
                {
                    deckBuildingPanel.SetActive(true);
                    if (DeckBuildingUIManager.instance != null) DeckBuildingUIManager.instance.RefreshAllUI();
                }
                break;
            case NodeType.Start:
                break;
        }
    }

    public void CompleteRestAndProceed()
    {
        if (currentNode != null) currentNode.isCompleted = true;
        if (deckBuildingPanel != null) deckBuildingPanel.SetActive(false);
        SetNodeInteractableStates();
    }
}