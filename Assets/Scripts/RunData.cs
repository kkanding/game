using UnityEngine;
using System.Collections.Generic;

public class RunData : MonoBehaviour
{
    public static RunData Instance;
    
    [Header("Run Progress")]
    public int currentFloor = 0;
    public bool isInDungeon = false;
    public int battlesWon = 0;  // ← 승리한 전투 수
    public NodeData lastCompletedNode;  // ← 마지막으로 완료한 노드
	
	[Header("Relics")]
	public List<string> ownedRelics = new List<string>();

    
    void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			DontDestroyOnLoad(gameObject);

			// ✅ 여기 추가 (딱 이 위치!)
			if (RelicManager.Instance == null)
			{
				gameObject.AddComponent<RelicManager>();
			}
		}
		else
		{
			Destroy(gameObject);
		}
	}

    
    // 던전 시작
    public void StartDungeon()
    {
        isInDungeon = true;
        currentFloor = 0;
        battlesWon = 0;
        lastCompletedNode = null;
        Debug.Log("던전 시작!");
    }
    
    // 던전 종료 (로비로)
    public void EndDungeon()
    {
        isInDungeon = false;
        currentFloor = 0;
        battlesWon = 0;
        lastCompletedNode = null;
        
        // GameData 초기화 (덱 초기화)
        if (GameData.Instance != null)
        {
            GameData.Instance.ResetDecks();
        }
        
        Debug.Log("던전 종료! 덱 초기화됨");
    }
    
    // 다음 층으로
    public void AdvanceFloor()
	{
		// 기본은 +1 이지만,
		// lastCompletedNode가 있으면 "그 노드의 다음 층"이 정답
		int expectedNext = lastCompletedNode != null ? lastCompletedNode.row + 1 : currentFloor + 1;

		if (expectedNext > currentFloor)
			currentFloor = expectedNext;

		Debug.Log($"다음 층: {currentFloor}");
	}
    
    // ← 노드 완료
    public void CompleteNode(NodeData node)
	{
		lastCompletedNode = node;
		Debug.Log($"노드 완료: {node.nodeType}, 층: {node.row}");
	}
}