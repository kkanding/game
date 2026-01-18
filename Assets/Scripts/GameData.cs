using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class CharacterData
{
    public string characterName;
    public string characterClass;
    
    // 기존 스탯
    public int maxHealth;
    public int currentHealth;
    
    // ← 새로운 스탯 추가!
    public int attackPower;
    public int defensePower;
    public int maxMentalPower;
    public int currentMentalPower;
    public bool isDefeated = false;
    public int reviveCardUseCount = 0;
    
    public Sprite characterImage;
    public List<string> cardList;
    
    public CharacterData(string name, string charClass, int hp, int atk, int def, int mp)
    {
        characterName = name;
        characterClass = charClass;
        maxHealth = hp;
        currentHealth = hp;
        attackPower = atk;
        defensePower = def;
        maxMentalPower = mp;
        currentMentalPower = mp;
        cardList = new List<string>();
    }
    
    // ← 정신력 관련 메서드 추가
    public void TakeMentalDamage(int amount)
    {
        currentMentalPower -= amount;
        if (currentMentalPower <= 0)
        {
            currentMentalPower = 0;
            Defeat();
        }
    }
    
    public void Defeat()
    {
        isDefeated = true;
        reviveCardUseCount = 0;
        Debug.Log($"{characterName} 쓰러짐!");
    }
    
    public void Revive()
    {
        isDefeated = false;
        currentMentalPower = maxMentalPower;
        reviveCardUseCount = 0;
        
        // 체력 1/3 회복
        int healthRestore = maxHealth / 3;
        currentHealth += healthRestore;
        
        Debug.Log($"{characterName} 부활! HP +{healthRestore}");
    }
    
    public void RestoreMentalPower(int amount)
    {
        if (isDefeated) return;
        
        currentMentalPower += amount;
        if (currentMentalPower > maxMentalPower)
        {
            currentMentalPower = maxMentalPower;
        }
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
    public int currentDungeonSeed;
    public bool isInDungeon = false;
    
    private bool isInitialized = false;
    public int gold = 100;
    
    void Awake()
    {
        Debug.Log("GameData Awake 호출!");
        
        if (Instance == null)
        {
            Debug.Log("새 GameData Instance 생성");
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
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
        
        raidParty.Clear();
        allCharacters.Clear();
        
        // ===== 전사 ===== (HP, ATK, DEF, MP)
        CharacterData warrior = new CharacterData("전사", "전사", 500, 100, 50, 120);
        warrior.cardList.Add("타격");
        warrior.cardList.Add("타격");
        warrior.cardList.Add("방어");
        
        // ===== 마법사 =====
        CharacterData mage = new CharacterData("마법사", "마법사", 400, 130, 30, 80);
        mage.cardList.Add("화염구");
        mage.cardList.Add("마법 방벽");
        mage.cardList.Add("명상");
        
        // ===== 도적 =====
        CharacterData rogue = new CharacterData("도적", "도적", 450, 120, 30, 90);
        rogue.cardList.Add("암습");
        rogue.cardList.Add("암습");
        rogue.cardList.Add("회피");
        
        allCharacters.Add(warrior);
        allCharacters.Add(mage);
        allCharacters.Add(rogue);
        
        raidParty.Add(warrior);
        raidParty.Add(mage);
        raidParty.Add(rogue);
        
        Debug.Log($"캐릭터 초기화 완료! 파티 체력 합계: {GetTotalPartyHealth()}");
    }
    
    // ← 파티 전체 체력 계산
    public int GetTotalPartyHealth()
    {
        int total = 0;
        foreach (var character in raidParty)
        {
            total += character.maxHealth;
        }
        return total;
    }
    
    // ← 파티 전체 방어력 계산
    public int GetTotalPartyDefense()
    {
        int total = 0;
        foreach (var character in raidParty)
        {
            if (!character.isDefeated)
            {
                total += character.defensePower;
            }
        }
        return total;
    }
    
    public void StartDungeon()
    {
        currentDungeonSeed = Random.Range(0, 999999999);
        isInDungeon = true;
        
        // 던전 시작 시 모든 캐릭터 상태 초기화
        foreach (var character in raidParty)
        {
            character.currentHealth = character.maxHealth;
            character.currentMentalPower = character.maxMentalPower;
            character.isDefeated = false;
            character.reviveCardUseCount = 0;
        }
        
        Debug.Log($"던전 시작! 시드: {currentDungeonSeed}");
    }
    
    public void EndDungeon()
    {
        isInDungeon = false;
        Debug.Log("던전 종료!");
    }
    
    public void ResetDecks()
    {
        if (raidParty == null || raidParty.Count == 0)
            return;
        
        Debug.Log("덱 초기화 시작!");
        
        gold = 100;
        
        foreach (var character in raidParty)
        {
            // 스탯 초기화
            switch (character.characterName)
            {
                case "전사":
                    character.maxHealth = 500;
                    character.currentHealth = 500;
                    character.attackPower = 100;
                    character.defensePower = 50;
                    character.maxMentalPower = 120;
                    character.currentMentalPower = 120;
                    break;
                    
                case "마법사":
                    character.maxHealth = 400;
                    character.currentHealth = 400;
                    character.attackPower = 130;
                    character.defensePower = 30;
                    character.maxMentalPower = 80;
                    character.currentMentalPower = 80;
                    break;
                    
                case "도적":
                    character.maxHealth = 450;
                    character.currentHealth = 450;
                    character.attackPower = 120;
                    character.defensePower = 30;
                    character.maxMentalPower = 90;
                    character.currentMentalPower = 90;
                    break;
            }
            
            character.isDefeated = false;
            character.reviveCardUseCount = 0;
            
            // 덱 초기화
            character.cardList.Clear();
            
            switch (character.characterName)
            {
                case "전사":
                    character.cardList.Add("타격");
                    character.cardList.Add("타격");
                    character.cardList.Add("방어");
                    break;
                    
                case "마법사":
                    character.cardList.Add("화염구");
                    character.cardList.Add("마법 방벽");
                    character.cardList.Add("명상");
                    break;
                    
                case "도적":
                    character.cardList.Add("암습");
                    character.cardList.Add("암습");
                    character.cardList.Add("회피");
                    break;
            }
            
            Debug.Log($"{character.characterName} 덱 초기화: {character.cardList.Count}장");
        }
    }
}