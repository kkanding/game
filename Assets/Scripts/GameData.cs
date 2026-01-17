using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class CharacterData
{
    public string characterName;
    public string characterClass;
    public int maxHealth;
	public int currentHealth;
    public Sprite characterImage;
    public List<string> cardList;
    
    public CharacterData(string name, string charClass, int hp)
    {
        characterName = name;
        characterClass = charClass;
        maxHealth = hp;
		currentHealth = hp;
        cardList = new List<string>();
    }
}

public class GameData : MonoBehaviour
{
    public static GameData Instance;
    
    [Header("All Characters")]
    public List<CharacterData> allCharacters = new List<CharacterData>();
    
    [Header("Raid Party (Selected 3)")]
    public List<CharacterData> raidParty = new List<CharacterData>();
    
    [Header("Dungeon Data")]
    public int currentDungeonSeed; // 시드 저장
    public bool isInDungeon = false; // 던전 진행 중인지
	
	private bool isInitialized = false; // ← 추가!
    public int gold = 100; // ← 시작 골드
	
    void Awake()
	{
		Debug.Log("GameData Awake 호출!");
		
		if (Instance == null)
		{
			Debug.Log("새 GameData Instance 생성");
			Instance = this;
			DontDestroyOnLoad(gameObject);
			
			// 한 번만 초기화!
			if (!isInitialized)
			{
				Debug.Log("최초 초기화!");
				InitializeCharacters();
				isInitialized = true;
			}
		}
		else
		{
			Debug.Log("기존 GameData Instance 있음 - 삭제");
			Destroy(gameObject);
		}
	}
    
    void InitializeCharacters()
	{
		Debug.Log("===== InitializeCharacters 호출! =====");
		Debug.Log($"현재 raidParty 개수: {raidParty.Count}");
		
		// 기존 파티 클리어
		raidParty.Clear();
		
		// ===== 전사 =====
		CharacterData warrior = new CharacterData("전사", "전사", 80);
		warrior.cardList.Add("타격");
		warrior.cardList.Add("타격");      // 타격 2장
		warrior.cardList.Add("방어");
		warrior.cardList.Add("방어");      // 방어 2장
		warrior.cardList.Add("강타");      // 🆕 강타 추가
		warrior.cardList.Add("철벽");      // 🆕 철벽 추가
		
		// ===== 마법사 =====
		CharacterData mage = new CharacterData("마법사", "마법사", 60);
		mage.cardList.Add("화염구");
		mage.cardList.Add("번개");         // 🆕 번개 추가
		mage.cardList.Add("방어막");
		mage.cardList.Add("방어막");       // 방어막 2장
		mage.cardList.Add("마나 실드");    // 🆕 마나 실드 추가
		mage.cardList.Add("집중");         // 🆕 집중 추가
		
		// ===== 도적 =====
		CharacterData rogue = new CharacterData("도적", "도적", 70);
		rogue.cardList.Add("암습");
		rogue.cardList.Add("암습");        // 암습 2장
		rogue.cardList.Add("회피");
		rogue.cardList.Add("회피");        // 회피 2장
		rogue.cardList.Add("연막탄");      // 🆕 연막탄 추가
		rogue.cardList.Add("독칼");        // 🆕 독칼 추가
		
		allCharacters.Add(warrior);
		allCharacters.Add(mage);
		allCharacters.Add(rogue);
		
		raidParty.Add(warrior);
		raidParty.Add(mage);
		raidParty.Add(rogue);
		
		Debug.Log($"캐릭터 초기화 완료! raidParty 개수: {raidParty.Count}");
	}
    
    // 던전 시작
    public void StartDungeon()
    {
        currentDungeonSeed = Random.Range(0, 999999999);
        isInDungeon = true;
        Debug.Log($"던전 시작! 시드: {currentDungeonSeed}");
    }
    
    // 던전 종료
    public void EndDungeon()
    {
        isInDungeon = false;
        Debug.Log("던전 종료!");
    }
	
	// 덱 초기화 (던전 종료 시)
	public void ResetDecks()
	{
		if (raidParty == null || raidParty.Count == 0)
			return;
		
		Debug.Log("덱 초기화 시작!");
		
		// ← 골드 초기화
		gold = 100;
		
		foreach (var character in raidParty)
		{
			// 체력 초기화
			switch (character.characterName)
			{
				case "전사":
					character.maxHealth = 80;
					character.currentHealth = 80; // ← 추가!
					break;
				case "마법사":
					character.maxHealth = 60;
					character.currentHealth = 60; // ← 추가!
					break;
				case "도적":
					character.maxHealth = 70;
					character.currentHealth = 70; // ← 추가!
					break;
			}
			
			// 덱 초기화
			character.cardList.Clear();
			
			switch (character.characterName)
			{
				case "전사":
					for (int i = 0; i < 5; i++) character.cardList.Add("타격");
					for (int i = 0; i < 4; i++) character.cardList.Add("방어");
					character.cardList.Add("강타");
					break;
					
				case "마법사":
					for (int i = 0; i < 4; i++) character.cardList.Add("화염구");
					for (int i = 0; i < 4; i++) character.cardList.Add("방어막");
					character.cardList.Add("번개");
					character.cardList.Add("집중");
					break;
					
				case "도적":
					for (int i = 0; i < 5; i++) character.cardList.Add("암습");
					for (int i = 0; i < 4; i++) character.cardList.Add("회피");
					character.cardList.Add("독칼");
					break;
			}
			
			Debug.Log($"{character.characterName} 덱 초기화: {character.cardList.Count}장");
		}
	}
}