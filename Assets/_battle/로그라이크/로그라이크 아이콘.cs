using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 맵의 각 노드를 나타내는 버튼의 UI와 기능을 담당합니다.
/// </summary>
public class MapNodeUI : MonoBehaviour
{
    [Header("UI Elements")]
    public Image nodeIcon;
    public Button button;
    public TextMeshProUGUI nodeTypeText; // 디버그용

    [Header("아이콘 스프라이트")]
    public Sprite battleIcon;
    public Sprite eliteIcon;
    public Sprite shopIcon;
    public Sprite restIcon;
    public Sprite bossIcon;

    private MapNode myNodeData;

    /// <summary>
    /// 이 버튼의 내용을 설정합니다.
    /// </summary>
    public void Setup(MapNode nodeData)
    {
        myNodeData = nodeData;
        button.onClick.AddListener(OnNodeClicked);

        // 노드 타입에 맞는 아이콘을 설정합니다.
        switch (nodeData.type)
        {
            case NodeType.Battle:
                nodeIcon.sprite = battleIcon;
                break;
            case NodeType.EliteBattle:
                nodeIcon.sprite = eliteIcon;
                break;
            case NodeType.Shop:
                nodeIcon.sprite = shopIcon;
                break;
            case NodeType.Rest:
                nodeIcon.sprite = restIcon;
                break;
            case NodeType.Boss:
                nodeIcon.sprite = bossIcon;
                break;
        }

        if (nodeTypeText != null)
        {
            nodeTypeText.text = nodeData.type.ToString();
        }
    }

    public void SetInteractable(bool interactable)
    {
        button.interactable = interactable;

        Color color = interactable ? Color.white : Color.gray;
        nodeIcon.color = color;

        if (nodeTypeText != null)
            nodeTypeText.color = color;
    }

    private void OnNodeClicked()
    {
        Debug.Log($"[MapNodeUI] {myNodeData.type} 노드가 클릭되었습니다.");
        // '두뇌'인 RoguelikeManager에게 내가 클릭되었음을 알립니다.
        RoguelikeManager.instance.OnNodeSelected(myNodeData);
    }

    // ▼▼▼ 이 함수를 추가합니다 ▼▼▼
    /// <summary>
    /// 이 UI가 담고 있는 MapNode 데이터를 반환합니다.
    /// </summary>
    public MapNode GetNodeData()
    {
        return myNodeData;
    }
    // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
}