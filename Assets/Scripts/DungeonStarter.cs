using UnityEngine;

public class DungeonStarter : MonoBehaviour
{
    void Start()
    {
        // RunData가 없으면 생성
        if (RunData.Instance == null)
        {
            GameObject runDataObj = new GameObject("RunData");
            runDataObj.AddComponent<RunData>();
        }
        
        // 던전 시작
		if (RunData.Instance != null && !RunData.Instance.isInDungeon)
		{
			RunData.Instance.StartDungeon();
		}
    }
}