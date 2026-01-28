using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public enum NodeType
{
    Battle,      // 일반 전투
    Elite,       // 엘리트 전투
    Rest,        // 휴식
    Shop,        // 상점
    Event,       // 이벤트
    Boss         // 보스
}

[System.Serializable]
public class NodeData
{
    public NodeType nodeType;
    // row: 층(가로 진행, 0층 -> 1층 -> ...)
    // column: 같은 층 내 세로 위치(0,1,2...)
    public int row;
    public int column;
    public Vector2 position;  // UI 위치
    public List<NodeData> connectedNodes = new List<NodeData>(); // 연결된 다음 노드들
    public bool isVisited = false;
    public bool isAccessible = false; // 현재 갈 수 있는지
    
    public NodeData(NodeType type, int row, int column)
    {
        nodeType = type;
        this.row = row;
        this.column = column;
    }
}