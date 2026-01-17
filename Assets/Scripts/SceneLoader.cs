using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadScene(string sceneName)
    {
        Debug.Log($"씬 로드: {sceneName}");
        SceneManager.LoadScene(sceneName);
    }
    
    public void LoadLobby()
    {
        SceneManager.LoadScene("LobbyScene");
    }
    
    public void LoadRoster()
    {
        SceneManager.LoadScene("RosterScene");
    }
    
    public void LoadDungeonSelect()
    {
        SceneManager.LoadScene("DungeonSelectScene");
    }
    
    public void LoadBattle()
    {
        SceneManager.LoadScene("BattleScene");
    }
	
	public void LoadDungeonMap()
	{
		SceneManager.LoadScene("DungeonMapScene");
	}
    
    public void QuitGame()
    {
        Debug.Log("게임 종료!");
        Application.Quit();
        
        // 에디터에서 테스트할 때
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}