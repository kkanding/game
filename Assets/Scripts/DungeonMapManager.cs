using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class DungeonMapManager : MonoBehaviour
{
    public static DungeonMapManager Instance;
    
    [Header("UI References")]
    public Transform mapContainer;
    public GameObject nodeButtonPrefab;
    public TextMeshProUGUI titleText;
    public GameObject lineRendererPrefab; // 연결선 Prefab
    
    [Header("Map Settings")]
    public int totalColumns = 12; // 총 층(가로)
    public int minNodesPerColumn = 1; // 층당 최소 노드
    public int maxNodesPerColumn = 4; // 층당 최대 노드
    public float horizontalSpacing = 150f; // 가로 간격
    public float verticalSpacing = 120f; // 세로 간격
    
    [Header("Current Progress")]
    public int currentColumn = 0; // 현재 층
    
    // 맵 데이터: [층][노드인덱스]
    private List<List<NodeData>> mapNodes = new List<List<NodeData>>();
    private List<GameObject> nodeButtons = new List<GameObject>();
    private List<GameObject> connectionLines = new List<GameObject>();
    private bool isMapGenerated = false;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        if (!isMapGenerated)
        {
            GenerateMap();
            isMapGenerated = true;
        }
        
        RefreshMap();
    }
    
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		if (scene.name == "DungeonMapScene")
		{
			Debug.Log("DungeonMapScene 로드됨! 맵 갱신");
			
			// Canvas 다시 찾기
			Canvas canvas = FindFirstObjectByType<Canvas>();
			if (canvas != null)
			{
				// ← MapScrollView → Viewport → Content → MapContainer 경로!
				Transform scrollView = canvas.transform.Find("MapScrollView");
				if (scrollView != null)
				{
					Transform viewport = scrollView.Find("Viewport");
					if (viewport != null)
					{
						Transform content = viewport.Find("Content");
						if (content != null)
						{
							Transform container = content.Find("MapContainer");
							if (container != null)
							{
								mapContainer = container;
								Debug.Log("MapContainer 찾음!");
							}
							else
							{
								Debug.LogError("MapContainer를 찾을 수 없습니다!");
							}
						}
					}
				}
				
				Transform title = canvas.transform.Find("TitleText");
				if (title != null)
				{
					titleText = title.GetComponent<TextMeshProUGUI>();
				}
			}
			
			// DungeonMapManager.cs - OnSceneLoaded() 안, RefreshMap() 호출 직전에 추가
			if (RunData.Instance != null && RunData.Instance.isInDungeon && RunData.Instance.lastCompletedNode != null)
			{
				// lastCompletedNode.row = 방금 방문한 층(0부터)
				// currentFloor = "다음에 선택할 층"이 되어야 함
				int expectedNextFloor = RunData.Instance.lastCompletedNode.row + 1;

				// 맵 범위 밖 방지
				if (mapNodes != null && mapNodes.Count > 0)
					expectedNextFloor = Mathf.Clamp(expectedNextFloor, 0, mapNodes.Count - 1);

				// 아직 같은 층에 머물러 있으면(=상점/이벤트에서 전진 호출이 누락된 케이스) 강제 보정
				if (RunData.Instance.currentFloor <= RunData.Instance.lastCompletedNode.row)
				{
					Debug.Log($"[Map AutoFix] currentFloor {RunData.Instance.currentFloor} -> {expectedNextFloor}");
					RunData.Instance.currentFloor = expectedNextFloor;
				}
			}
			
			RefreshMap();
		}
	}
    
    void RefreshMap()
	{
		if (mapContainer == null)
		{
			Debug.LogError("MapContainer가 없습니다!");
			return;
		}

		// ★ 먼저 접근가능 노드 계산 -> 그 다음 UI를 그려야 버튼/색이 맞음
		UpdateAccessibleNodes();
		DisplayMap();

		// 현재 층으로 스크롤
		ScrollToCurrentColumn();
	}

	
	void ScrollToCurrentColumn()
	{
		if (mapContainer == null) return;
		
		ScrollRect scrollRect = mapContainer.GetComponentInParent<ScrollRect>();
		if (scrollRect == null) return;
		
		// 현재 층의 X 위치 계산
		float targetX = -1000f + currentColumn * horizontalSpacing;
		
		// Content의 전체 너비
		RectTransform content = scrollRect.content;
		RectTransform viewport = scrollRect.viewport;
		
		if (content == null || viewport == null) return;
		
		// 정규화된 스크롤 위치 계산
		float contentWidth = content.rect.width;
		float viewportWidth = viewport.rect.width;
		float scrollableWidth = contentWidth - viewportWidth;
		
		if (scrollableWidth <= 0) return;
		
		// 타겟 위치를 0~1로 정규화
		float normalizedX = Mathf.Clamp01((targetX + contentWidth / 2f) / scrollableWidth);
		
		// ← 실행 중인 코루틴 먼저 정지!
		StopAllCoroutines();
		
		// 부드럽게 스크롤
		StartCoroutine(SmoothScrollTo(scrollRect, normalizedX));
	}
    
    // ← 여기에 SmoothScrollTo() 추가!
    System.Collections.IEnumerator SmoothScrollTo(ScrollRect scrollRect, float targetX)
	{
		// ← null 체크 추가!
		if (scrollRect == null) yield break;
		
		float duration = 0.5f;
		float elapsed = 0f;
		float startX = scrollRect.horizontalNormalizedPosition;
		
		while (elapsed < duration)
		{
			// ← 매 프레임마다 null 체크!
			if (scrollRect == null) yield break;
			
			elapsed += Time.deltaTime;
			float t = elapsed / duration;
			
			t = 1f - Mathf.Pow(1f - t, 3f);
			
			scrollRect.horizontalNormalizedPosition = Mathf.Lerp(startX, targetX, t);
			
			yield return null;
		}
		
		// ← 마지막에도 null 체크!
		if (scrollRect != null)
		{
			scrollRect.horizontalNormalizedPosition = targetX;
		}
	}
    
    // 맵 생성
    void GenerateMap()
	{
		Debug.Log("던전 맵 생성 시작!");
		
		mapNodes.Clear();
		
		int previousNodeCount = 0; // 이전 층의 노드 개수
		
		// 각 층별로 노드 생성
		for (int col = 0; col < totalColumns; col++)
		{
			List<NodeData> columnNodes = new List<NodeData>();
			
			int nodeCount;
			
			// 첫 층: 3개 또는 4개
			if (col == 0)
			{
				nodeCount = Random.Range(3, 5); // 3 또는 4
			}
			// 마지막 층: 보스 1개
			else if (col == totalColumns - 1)
			{
				nodeCount = 1;
			}
			// 중간 층: 2~4개 (연속 제약)
			else
			{
				// 이전 층이 2개였으면 다음 층은 3 또는 4
				if (previousNodeCount == 2)
				{
					nodeCount = Random.Range(3, 5); // 3 또는 4
				}
				else
				{
					nodeCount = Random.Range(2, 5); // 2, 3, 또는 4
				}
			}
			
			// 노드 생성
			for (int row = 0; row < nodeCount; row++)
			{
				NodeType type = DetermineNodeType(col, nodeCount);
				NodeData node = new NodeData(type, col, row); // col=층, row=세로위치
				columnNodes.Add(node);
			}
			
			// Y 좌표 정렬 (row 기준, 내림차순)
			columnNodes.Sort((a, b) => b.column.CompareTo(a.column));
			
			mapNodes.Add(columnNodes);
			
			// 다음 층을 위해 현재 노드 개수 저장
			previousNodeCount = nodeCount;
			
			Debug.Log($"{col}층: {nodeCount}개 노드 생성");
		}
		
		// 노드 연결 (선 교차 방지)
		ConnectNodesNoIntersection();
		
		// 시작 노드들 접근 가능
		foreach (var node in mapNodes[0])
		{
			node.isAccessible = true;
		}
		
		Debug.Log($"맵 생성 완료! {totalColumns}층, 총 {GetTotalNodeCount()}개 노드");
	}
    
    // 전체 노드 개수
    int GetTotalNodeCount()
    {
        int count = 0;
        foreach (var column in mapNodes)
        {
            count += column.Count;
        }
        return count;
    }
    
    // 노드 타입 결정
    NodeType DetermineNodeType(int column, int nodeCount)
	{
		// 첫 층은 무조건 전투!
		if (column == 0)
		{
			return NodeType.Battle;
		}
		
		// 마지막 층은 보스
		if (column == totalColumns - 1)
		{
			return NodeType.Boss;
		}
		
		// 7층마다 엘리트 확률 증가
		if (column > 0 && column % 7 == 0)
		{
			return Random.value < 0.5f ? NodeType.Elite : NodeType.Battle;
		}
		
		// 랜덤 노드 타입
		float rand = Random.value;
		
		if (rand < 0.65f)
			return NodeType.Battle;
		else if (rand < 0.8f)
			return NodeType.Rest;
		else if (rand < 0.92f)
			return NodeType.Shop;
		else
			return NodeType.Event;
	}
    
	// 노드 연결 (엄격한 선 교차 방지 + 분기 생성)
	void ConnectNodesNoIntersection()
	{
		for (int col = 0; col < totalColumns - 1; col++)
		{
			List<NodeData> currentColumn = mapNodes[col];
			List<NodeData> nextColumn = mapNodes[col + 1];
			
			// 각 층의 노드를 Y 좌표 순으로 정렬 (위→아래)
			currentColumn.Sort((a, b) => b.column.CompareTo(a.column));
			nextColumn.Sort((a, b) => b.column.CompareTo(a.column));
			
			foreach (var currentNode in currentColumn)
			{
				int currentRow = currentNode.column; // Y 좌표
				
				// 연결 가능한 다음 층 노드 찾기 (교차 방지)
				List<NodeData> possibleConnections = new List<NodeData>();
				
				foreach (var nextNode in nextColumn)
				{
					int nextRow = nextNode.column; // Y 좌표
					
					// ← 교차 방지 규칙!
					// 현재 노드와 같은 높이 또는 아래쪽만 연결 가능
					// (위쪽 노드는 위쪽으로만, 아래쪽 노드는 아래쪽으로만)
					
					int distance = Mathf.Abs(nextRow - currentRow);
					
					// 같은 높이 또는 ±1 범위만
					if (distance <= 1)
					{
						possibleConnections.Add(nextNode);
					}
				}
				
				// 연결 개수: 1~2개
				if (possibleConnections.Count > 0)
				{
					// 최대 2개 연결
					int connectionCount = Mathf.Min(Random.Range(1, 3), possibleConnections.Count);
					
					// 가까운 노드 우선 정렬
					possibleConnections.Sort((a, b) => {
						int distA = Mathf.Abs(a.column - currentRow);
						int distB = Mathf.Abs(b.column - currentRow);
						return distA.CompareTo(distB);
					});
					
					// 연결
					for (int i = 0; i < connectionCount && i < possibleConnections.Count; i++)
					{
						if (!currentNode.connectedNodes.Contains(possibleConnections[i]))
						{
							currentNode.connectedNodes.Add(possibleConnections[i]);
						}
					}
				}
				// 연결 가능한 노드가 없으면 가장 가까운 노드 강제 연결
				else if (nextColumn.Count > 0)
				{
					NodeData closest = FindClosestNode(currentNode, nextColumn);
					if (closest != null)
					{
						currentNode.connectedNodes.Add(closest);
					}
				}
			}
		}
		
		// 모든 다음 층 노드가 최소 1개 이상 연결되었는지 확인
		EnsureAllNodesConnected();
		
		// 양방향 연결 보장 (중간 층)
		EnsureBidirectionalConnections();
		
		// 마지막 층 연결 보장
		EnsureBossConnection();
		
		Debug.Log("노드 연결 완료 (엄격한 교차 방지)!");
	}

	// 모든 노드가 최소 1개 이상 연결되었는지 확인 (교차 방지)
	void EnsureAllNodesConnected()
	{
		for (int col = 1; col < totalColumns; col++)
		{
			List<NodeData> currentColumn = mapNodes[col];
			List<NodeData> prevColumn = mapNodes[col - 1];
			
			foreach (var node in currentColumn)
			{
				// 이 노드로 들어오는 연결이 있는지 확인
				bool hasIncomingConnection = false;
				
				foreach (var prevNode in prevColumn)
				{
					if (prevNode.connectedNodes.Contains(node))
					{
						hasIncomingConnection = true;
						break;
					}
				}
				
				// 연결 없으면 가장 가까운 이전 노드에서 연결
				if (!hasIncomingConnection)
				{
					// ← 교차하지 않는 가장 가까운 노드 찾기
					NodeData closest = null;
					int minDistance = int.MaxValue;
					
					foreach (var prevNode in prevColumn)
					{
						int distance = Mathf.Abs(prevNode.column - node.column);
						
						// 거리 1 이내만 (교차 방지)
						if (distance <= 1 && distance < minDistance)
						{
							closest = prevNode;
							minDistance = distance;
						}
					}
					
					// 거리 1 이내 없으면 어쩔 수 없이 가장 가까운 노드
					if (closest == null)
					{
						closest = FindClosestNode(node, prevColumn);
					}
					
					if (closest != null && !closest.connectedNodes.Contains(node))
					{
						closest.connectedNodes.Add(node);
						Debug.Log($"고립 노드 연결: {closest.row}층 → {node.row}층");
					}
				}
			}
		}
	}

	// 양방향 연결 보장 (교차 방지)
	void EnsureBidirectionalConnections()
	{
		for (int col = 1; col < totalColumns - 1; col++)
		{
			List<NodeData> currentColumn = mapNodes[col];
			List<NodeData> prevColumn = mapNodes[col - 1];
			
			foreach (var node in currentColumn)
			{
				// 이전 층 연결 확인
				bool hasBackConnection = false;
				foreach (var prevNode in prevColumn)
				{
					if (prevNode.connectedNodes.Contains(node))
					{
						hasBackConnection = true;
						break;
					}
				}
				
				// 이전 층 연결 없으면 강제 생성 (교차 방지)
				if (!hasBackConnection)
				{
					NodeData closest = null;
					int minDistance = int.MaxValue;
					
					foreach (var prevNode in prevColumn)
					{
						int distance = Mathf.Abs(prevNode.column - node.column);
						if (distance <= 1 && distance < minDistance)
						{
							closest = prevNode;
							minDistance = distance;
						}
					}
					
					if (closest == null)
					{
						closest = FindClosestNode(node, prevColumn);
					}
					
					if (closest != null && !closest.connectedNodes.Contains(node))
					{
						closest.connectedNodes.Add(node);
					}
				}
				
				// 다음 층 연결 확인
				if (node.connectedNodes.Count == 0)
				{
					List<NodeData> nextColumn = mapNodes[col + 1];
					
					NodeData closest = null;
					int minDistance = int.MaxValue;
					
					foreach (var nextNode in nextColumn)
					{
						int distance = Mathf.Abs(nextNode.column - node.column);
						if (distance <= 1 && distance < minDistance)
						{
							closest = nextNode;
							minDistance = distance;
						}
					}
					
					if (closest == null)
					{
						closest = FindClosestNode(node, nextColumn);
					}
					
					if (closest != null)
					{
						node.connectedNodes.Add(closest);
					}
				}
			}
		}
	}

	// 보스 연결 보장
	void EnsureBossConnection()
	{
		List<NodeData> lastColumn = mapNodes[totalColumns - 1];
		List<NodeData> beforeLastColumn = mapNodes[totalColumns - 2];
		
		foreach (var bossNode in lastColumn)
		{
			bool hasConnection = false;
			foreach (var prevNode in beforeLastColumn)
			{
				if (prevNode.connectedNodes.Contains(bossNode))
				{
					hasConnection = true;
					break;
				}
			}
			
			if (!hasConnection)
			{
				// 모든 이전 층 노드에서 보스로 연결
				foreach (var prevNode in beforeLastColumn)
				{
					if (!prevNode.connectedNodes.Contains(bossNode))
					{
						prevNode.connectedNodes.Add(bossNode);
					}
				}
			}
		}
	}

	// 가장 가까운 노드 찾기
	NodeData FindClosestNode(NodeData sourceNode, List<NodeData> targetNodes)
	{
		if (targetNodes.Count == 0) return null;
		
		NodeData closest = targetNodes[0];
		float minDistance = Mathf.Abs(sourceNode.column - closest.column);
		
		foreach (var node in targetNodes)
		{
			float distance = Mathf.Abs(sourceNode.column - node.column);
			if (distance < minDistance)
			{
				minDistance = distance;
				closest = node;
			}
		}
		
		return closest;
	}
    
    // 맵 UI 표시
    void DisplayMap()
    {
        // 기존 버튼 및 선 제거
        foreach (var btn in nodeButtons)
        {
            if (btn != null) Destroy(btn);
        }
        nodeButtons.Clear();
        
        foreach (var line in connectionLines)
        {
            if (line != null) Destroy(line);
        }
        connectionLines.Clear();
        
        // 노드 버튼 생성
        for (int col = 0; col < mapNodes.Count; col++)
        {
            for (int row = 0; row < mapNodes[col].Count; row++)
            {
                NodeData node = mapNodes[col][row];
                
                // 노드 버튼 생성
                GameObject nodeBtn = Instantiate(nodeButtonPrefab, mapContainer);
                RectTransform rect = nodeBtn.GetComponent<RectTransform>();
                
                // 위치 계산 (가로: 왼쪽→오른쪽, 세로: 중앙 정렬)
                float xPos = -1600f + col * horizontalSpacing; // 왼쪽 시작
                float yPos = CalculateYPosition(row, mapNodes[col].Count);
                
                rect.anchoredPosition = new Vector2(xPos, yPos);
                node.position = new Vector2(xPos, yPos);
                
                // 노드 설정
                SetupNodeButton(nodeBtn, node);
                
                nodeButtons.Add(nodeBtn);
            }
        }
        
        // 연결선 그리기
        DrawConnections();
    }
    
    // Y 위치 계산 (세로 중앙 정렬)
    float CalculateYPosition(int row, int totalNodesInColumn)
    {
        float totalHeight = (totalNodesInColumn - 1) * verticalSpacing;
        float startY = totalHeight / 2f;
        return startY - row * verticalSpacing;
    }
    
    // 연결선 그리기
	void DrawConnections()
	{
		for (int col = 0; col < mapNodes.Count; col++)
		{
			foreach (var node in mapNodes[col])
			{
				foreach (var connectedNode in node.connectedNodes)
				{
					// 둘 다 방문했으면 방문한 경로
					bool isVisitedPath = node.isVisited && connectedNode.isVisited;
					DrawLine(node.position, connectedNode.position, isVisitedPath);
				}
			}
		}
	}
    
    // 선 그리기 (방문 여부에 따라 색상 변경)
	void DrawLine(Vector2 start, Vector2 end, bool isVisited = false)
	{
		GameObject lineObj = new GameObject("ConnectionLine");
		lineObj.transform.SetParent(mapContainer, false);
		
		Image lineImage = lineObj.AddComponent<Image>();
		
		// 방문한 경로는 밝은 색, 미방문은 어두운 색
		if (isVisited)
		{
			lineImage.color = new Color(1f, 0.9f, 0.5f, 1f); // 밝은 노란색
		}
		else
		{
			lineImage.color = new Color(0.4f, 0.4f, 0.4f, 0.6f); // 어두운 회색
		}
		
		RectTransform rect = lineObj.GetComponent<RectTransform>();
		
		// 선의 중심점
		Vector2 dir = end - start;
		float distance = dir.magnitude;
		
		// 위치와 회전 설정
		rect.anchoredPosition = start + dir / 2f;
		rect.sizeDelta = new Vector2(distance, isVisited ? 5f : 3f); // 방문한 경로는 두껍게
		
		float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
		rect.localRotation = Quaternion.Euler(0, 0, angle);
		
		// 맨 뒤로 보내기
		rect.SetAsFirstSibling();
		
		connectionLines.Add(lineObj);
	}
    
    // 노드 버튼 설정
	void SetupNodeButton(GameObject nodeBtn, NodeData node)
	{
		Button btn = nodeBtn.GetComponent<Button>();
		TextMeshProUGUI text = nodeBtn.GetComponentInChildren<TextMeshProUGUI>();
		Image image = nodeBtn.GetComponent<Image>();
		
		// 기본 색상 설정
		Color baseColor = Color.white;
		
		switch (node.nodeType)
		{
			case NodeType.Battle:
				text.text = "전투";
				baseColor = new Color(1f, 0.3f, 0.3f);
				break;
			case NodeType.Elite:
				text.text = "엘리트";
				baseColor = new Color(0.8f, 0.2f, 0.8f);
				break;
			case NodeType.Rest:
				text.text = "휴식";
				baseColor = new Color(0.3f, 0.8f, 0.3f);
				break;
			case NodeType.Shop:
				text.text = "상점";
				baseColor = new Color(1f, 0.8f, 0.3f);
				break;
			case NodeType.Event:
				text.text = "?";
				baseColor = new Color(0.3f, 0.7f, 1f);
				break;
			case NodeType.Boss:
				text.text = "보스";
				baseColor = new Color(1f, 0.5f, 0f);
				break;
		}
		
		// 방문 상태에 따라 색상 조정
		if (node.isVisited)
		{
			image.color = Color.gray; // 방문함
		}
		else if (node.isAccessible)
		{
			image.color = baseColor; // 접근 가능 - 밝게
			
			// 현재 층이면 테두리 추가
			if (node.row == currentColumn)
			{
				// Outline 추가
				Outline outline = nodeBtn.GetComponent<Outline>();
				if (outline == null)
				{
					outline = nodeBtn.AddComponent<Outline>();
				}
				outline.effectColor = Color.yellow;
				outline.effectDistance = new Vector2(3, 3);
			}
		}
		else
		{
			image.color = baseColor * 0.5f; // 접근 불가 - 어둡게
			image.color = new Color(image.color.r, image.color.g, image.color.b, 1f); // 알파는 유지
		}
		
		// 클릭 이벤트
		btn.onClick.RemoveAllListeners();
		btn.onClick.AddListener(() => OnNodeClick(node));
		
		// 접근 가능 여부
		btn.interactable = node.isAccessible;
	}
    
	// 접근 가능한 노드 업데이트
	void UpdateAccessibleNodes()
	{
		// RunData에서 현재 층 가져오기 + "다음에 선택할 층" 자동 보정
		if (mapNodes == null || mapNodes.Count == 0) return;

		if (RunData.Instance != null)
		{
			int nextFloor = RunData.Instance.currentFloor;

			// 마지막으로 진행(선택)했던 노드 기준으로 다음 층을 강제 보정
			if (RunData.Instance.lastCompletedNode != null)
			{
				nextFloor = Mathf.Max(nextFloor, RunData.Instance.lastCompletedNode.row + 1);
			}

			// 범위 보정 (마지막층 넘어가서 막히는 거 방지)
			nextFloor = Mathf.Clamp(nextFloor, 0, mapNodes.Count - 1);

			if (nextFloor != RunData.Instance.currentFloor)
			{
				Debug.Log($"[UpdateAccessibleNodes] currentFloor 보정: {RunData.Instance.currentFloor} -> {nextFloor}");
				RunData.Instance.currentFloor = nextFloor;
			}

			currentColumn = RunData.Instance.currentFloor;

			Debug.Log($"[UpdateAccessibleNodes] RunData.currentFloor: {RunData.Instance.currentFloor}");
			Debug.Log($"[UpdateAccessibleNodes] currentColumn 설정: {currentColumn}");
		}
		else
		{
			Debug.LogError("[UpdateAccessibleNodes] RunData.Instance가 null입니다!");
		}

		
		// 모든 노드 비활성화
		foreach (var column in mapNodes)
		{
			foreach (var node in column)
			{
				node.isAccessible = false;
			}
		}
		
		Debug.Log($"[UpdateAccessibleNodes] 현재 층: {currentColumn}, 총 층 수: {mapNodes.Count}");
		
		// 현재 층이 0이면 첫 층 모든 노드 활성화
		if (currentColumn == 0)
		{
			Debug.Log("[UpdateAccessibleNodes] 0층 노드들 활성화");
			foreach (var node in mapNodes[0])
			{
				if (!node.isVisited)
				{
					node.isAccessible = true;
				}
			}
		}
		else
		{
			Debug.Log($"[UpdateAccessibleNodes] {currentColumn}층 노드 활성화 시작");
			
			// 이전 층에서 방문한 노드들과 연결된 다음 노드만 활성화
			if (currentColumn > 0 && currentColumn < mapNodes.Count)
			{
				List<NodeData> prevColumn = mapNodes[currentColumn - 1];
				List<NodeData> currentColumnNodes = mapNodes[currentColumn];
				
				Debug.Log($"[UpdateAccessibleNodes] 이전 층({currentColumn - 1}층) 노드 수: {prevColumn.Count}");
				Debug.Log($"[UpdateAccessibleNodes] 현재 층({currentColumn}층) 노드 수: {currentColumnNodes.Count}");
				
				int visitedCount = 0;
				foreach (var prevNode in prevColumn)
				{
					if (prevNode.isVisited)
					{
						visitedCount++;
						Debug.Log($"[UpdateAccessibleNodes] {currentColumn - 1}층 방문한 노드 발견! 연결된 노드 수: {prevNode.connectedNodes.Count}");
						
						// 이 노드와 연결된 다음 층 노드 활성화
						foreach (var connectedNode in prevNode.connectedNodes)
						{
							if (currentColumnNodes.Contains(connectedNode) && !connectedNode.isVisited)
							{
								connectedNode.isAccessible = true;
								Debug.Log($"[UpdateAccessibleNodes] {currentColumn}층 노드 활성화!");
							}
						}
					}
				}
				
				Debug.Log($"[UpdateAccessibleNodes] 이전 층 방문한 노드 수: {visitedCount}");
			}
			else
			{
				Debug.LogWarning($"[UpdateAccessibleNodes] 층 인덱스 범위 초과! currentColumn: {currentColumn}, mapNodes.Count: {mapNodes.Count}");
			}
		}
		
		// 접근 가능한 노드 개수 로그
		int accessibleCount = 0;
		if (currentColumn < mapNodes.Count)
		{
			foreach (var node in mapNodes[currentColumn])
			{
				if (node.isAccessible) accessibleCount++;
			}
		}
		
		Debug.Log($"[UpdateAccessibleNodes] {currentColumn}층에서 접근 가능한 노드: {accessibleCount}개");
		
		// ... UI 업데이트 코드는 그대로 ...
	}

	// 노드 타입별 기본 색상 반환
	Color GetNodeBaseColor(NodeType nodeType)
	{
		switch (nodeType)
		{
			case NodeType.Battle:
				return new Color(1f, 0.3f, 0.3f);
			case NodeType.Elite:
				return new Color(0.8f, 0.2f, 0.8f);
			case NodeType.Rest:
				return new Color(0.3f, 0.8f, 0.3f);
			case NodeType.Shop:
				return new Color(1f, 0.8f, 0.3f);
			case NodeType.Event:
				return new Color(0.3f, 0.7f, 1f);
			case NodeType.Boss:
				return new Color(1f, 0.5f, 0f);
			default:
				return Color.white;
		}
	}
    
    // 버튼으로 노드 찾기
    NodeData FindNodeByButton(GameObject button)
    {
        RectTransform rect = button.GetComponent<RectTransform>();
        Vector2 pos = rect.anchoredPosition;
        
        foreach (var column in mapNodes)
        {
            foreach (var node in column)
            {
                if (Vector2.Distance(node.position, pos) < 1f)
                {
                    return node;
                }
            }
        }
        return null;
    }
    
	// 노드 클릭
	void OnNodeClick(NodeData node)
	{
		Debug.Log($"===== 노드 클릭됨! =====");
		Debug.Log($"노드 타입: {node.nodeType}");
		Debug.Log($"층(row): {node.row}, 위치(column/y): {node.column}");

		if (!node.isAccessible)
		{
			Debug.LogWarning("이 노드는 접근할 수 없습니다!");
			return;
		}

		node.isVisited = true;

		if (RunData.Instance != null)
		{
			RunData.Instance.CompleteNode(node);

			// ★ 노드 선택 순간에 '다음 층'을 확정해둔다 (전투/상점/이벤트 모두 공통으로 안정화)
			int nextFloor = node.row + 1;
			if (mapNodes != null && mapNodes.Count > 0)
				nextFloor = Mathf.Clamp(nextFloor, 0, mapNodes.Count - 1);

			RunData.Instance.currentFloor = nextFloor;
			Debug.Log($"[OnNodeClick] nextFloor 확정: {RunData.Instance.currentFloor}");
		}

		// currentColumn도 '다음 층' 기준으로 맞춘다
		currentColumn = (RunData.Instance != null) ? RunData.Instance.currentFloor : (node.row + 1);

		StopAllCoroutines();
		
		switch (node.nodeType)
		{
			case NodeType.Battle:
			case NodeType.Elite:
			case NodeType.Boss:
				Debug.Log("전투 Scene으로 이동!");
				SceneManager.LoadScene("BattleScene");
				break;
				
			case NodeType.Rest:
				Debug.Log("휴식 Scene으로 이동!");
				SceneManager.LoadScene("RestScene");
				break;
				
			case NodeType.Shop:
				Debug.Log("상점 Scene으로 이동!");
				SceneManager.LoadScene("ShopScene");
				break;
				
			case NodeType.Event:
				Debug.Log("이벤트 Scene으로 이동!");
				SceneManager.LoadScene("EventScene");
				break;
		}
	}
}