using UnityEngine;

public class RunData : MonoBehaviour
{
    public static RunData Instance;
    
    [Header("Run Progress")]
    public int currentFloor = 0;
    public bool isInDungeon = false; // 던전 진행 중인지
    
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
    
    // 던전 시작
    public void StartDungeon()
    {
        isInDungeon = true;
        currentFloor = 0;
        Debug.Log("던전 시작!");
    }
    
    // 던전 종료 (로비로)
    public void EndDungeon()
    {
        isInDungeon = false;
        currentFloor = 0;
        
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
        currentFloor++;
        Debug.Log($"다음 층: {currentFloor}");
    }
}