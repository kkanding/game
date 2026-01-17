using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance;
    
    [Header("UI References")]
    public TextMeshProUGUI eventTitleText;
    public TextMeshProUGUI eventDescriptionText;
    public Transform choicesPanel;
    public GameObject choiceButtonPrefab;
    
    [Header("Result Panel")]
    public GameObject resultPanel;
    public TextMeshProUGUI resultText;
    public Button continueButton;
    
    private EventData currentEvent;
    
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    
    void Start()
    {
        // 결과 패널 숨김
        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
        }
        
        // 계속 버튼
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(ContinueAfterEvent);
        }
        
        // 랜덤 이벤트 로드
        LoadRandomEvent();
    }
    
    void LoadRandomEvent()
    {
        if (EventDatabase.Instance == null)
        {
            Debug.LogError("EventDatabase가 없습니다!");
            return;
        }
        
        currentEvent = EventDatabase.Instance.GetRandomEvent();
        
        if (currentEvent == null)
        {
            Debug.LogError("이벤트를 불러올 수 없습니다!");
            return;
        }
        
        DisplayEvent();
    }
    
    void DisplayEvent()
    {
        // 타이틀 & 설명
        if (eventTitleText != null)
        {
            eventTitleText.text = currentEvent.eventTitle;
        }
        
        if (eventDescriptionText != null)
        {
            eventDescriptionText.text = currentEvent.eventDescription;
        }
        
        // 기존 선택지 제거
        foreach (Transform child in choicesPanel)
        {
            Destroy(child.gameObject);
        }
        
        // 선택지 버튼 생성
        foreach (EventChoice choice in currentEvent.choices)
        {
            CreateChoiceButton(choice);
        }
    }
    
    void CreateChoiceButton(EventChoice choice)
	{
		// 버튼 생성
		GameObject buttonObj = new GameObject("ChoiceButton");
		buttonObj.transform.SetParent(choicesPanel, false);
		
		// RectTransform 설정
		RectTransform rect = buttonObj.AddComponent<RectTransform>();
		rect.sizeDelta = new Vector2(1300, 100);
		
		// Button 추가
		Button button = buttonObj.AddComponent<Button>();
		
		// Image 추가
		Image image = buttonObj.AddComponent<Image>();
		image.color = new Color(0.3f, 0.3f, 0.4f);
		
		// ← Navigation 설정
		Navigation nav = button.navigation;
		nav.mode = Navigation.Mode.None;
		button.navigation = nav;
		
		// Text 추가
		GameObject textObj = new GameObject("Text");
		textObj.transform.SetParent(buttonObj.transform, false);
		
		TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
		text.text = choice.choiceText;
		text.fontSize = 32;
		text.color = Color.white;
		text.alignment = TextAlignmentOptions.Center;
		text.enableAutoSizing = false; // ← 추가!
		text.fontStyle = FontStyles.Normal; // ← 추가!
		
		// ← 한글 폰트 설정!
		// EventTitleText에서 폰트 가져오기
		if (eventTitleText != null && eventTitleText.font != null)
		{
			text.font = eventTitleText.font;
		}
		
		RectTransform textRect = textObj.GetComponent<RectTransform>();
		textRect.anchorMin = Vector2.zero;
		textRect.anchorMax = Vector2.one;
		textRect.sizeDelta = Vector2.zero;
		textRect.offsetMin = new Vector2(20, 10); // ← 여백 추가
		textRect.offsetMax = new Vector2(-20, -10);
		
		// 클릭 이벤트
		button.onClick.AddListener(() => OnChoiceSelected(choice));
		
		// 골드 부족 시 비활성화
		if (choice.goldChange < 0 && GameData.Instance != null)
		{
			if (GameData.Instance.gold < Mathf.Abs(choice.goldChange))
			{
				button.interactable = false;
				text.text += " (골드 부족)";
				text.color = new Color(0.5f, 0.5f, 0.5f); // ← 회색으로
			}
		}
	}
    
    void OnChoiceSelected(EventChoice choice)
    {
        Debug.Log($"선택: {choice.choiceText}");
        
        // 선택지 패널 숨기기
        if (choicesPanel != null)
        {
            choicesPanel.gameObject.SetActive(false);
        }
        
        // 효과 적용
        ApplyChoice(choice);
        
        // 결과 표시
        ShowResult(choice);
    }
    
    void ApplyChoice(EventChoice choice)
    {
        if (GameData.Instance == null) return;
        
        // 골드 변화
        if (choice.goldChange != 0)
        {
            GameData.Instance.gold += choice.goldChange;
            Debug.Log($"골드 변화: {choice.goldChange} (현재: {GameData.Instance.gold})");
        }
        
        // 체력 변화 (%)
        if (choice.healthChangePercent != 0)
        {
            foreach (var character in GameData.Instance.raidParty)
            {
                int healthChange = Mathf.RoundToInt(character.maxHealth * choice.healthChangePercent / 100f);
                character.currentHealth = Mathf.Clamp(character.currentHealth + healthChange, 0, character.maxHealth);
            }
            Debug.Log($"체력 {choice.healthChangePercent}% 변화");
        }
        
        // 최대 체력 변화
        if (choice.maxHealthChange != 0)
        {
            foreach (var character in GameData.Instance.raidParty)
            {
                character.maxHealth += choice.maxHealthChange;
                character.currentHealth += choice.maxHealthChange;
            }
            Debug.Log($"최대 체력 +{choice.maxHealthChange}");
        }
        
        // 랜덤 카드 추가
        if (choice.addRandomCard)
        {
            AddRandomCard();
        }
        
        // 랜덤 카드 제거
        if (choice.removeRandomCard)
        {
            RemoveRandomCard();
        }
        
        // 카드 변환 (업그레이드)
        if (choice.transformCard)
        {
            UpgradeRandomCard();
        }
    }
    
    void AddRandomCard()
    {
        if (GameData.Instance == null || GameData.Instance.raidParty.Count == 0) return;
        
        int characterIndex = Random.Range(0, GameData.Instance.raidParty.Count);
        CharacterData character = GameData.Instance.raidParty[characterIndex];
        
        List<string> availableCards = GetAllCardsForCharacter(character.characterName);
        
        if (availableCards.Count > 0)
        {
            string randomCard = availableCards[Random.Range(0, availableCards.Count)];
            character.cardList.Add(randomCard);
            Debug.Log($"{character.characterName}에게 {randomCard} 추가!");
        }
    }
    
    void RemoveRandomCard()
    {
        if (GameData.Instance == null || GameData.Instance.raidParty.Count == 0) return;
        
        CharacterData character = GameData.Instance.raidParty[Random.Range(0, GameData.Instance.raidParty.Count)];
        
        if (character.cardList.Count > 0)
        {
            string removedCard = character.cardList[Random.Range(0, character.cardList.Count)];
            character.cardList.Remove(removedCard);
            Debug.Log($"{character.characterName}의 {removedCard} 제거!");
        }
    }
    
    void UpgradeRandomCard()
    {
        if (GameData.Instance == null || GameData.Instance.raidParty.Count == 0) return;
        
        // 업그레이드 가능한 카드 찾기
        List<int> upgradeableCandidates = new List<int>();
        CharacterData selectedCharacter = null;
        
        foreach (var character in GameData.Instance.raidParty)
        {
            for (int i = 0; i < character.cardList.Count; i++)
            {
                if (!character.cardList[i].EndsWith("+"))
                {
                    upgradeableCandidates.Add(i);
                    selectedCharacter = character;
                    break;
                }
            }
            
            if (upgradeableCandidates.Count > 0) break;
        }
        
        // 업그레이드
        if (upgradeableCandidates.Count > 0 && selectedCharacter != null)
        {
            int index = upgradeableCandidates[Random.Range(0, upgradeableCandidates.Count)];
            string oldCard = selectedCharacter.cardList[index];
            selectedCharacter.cardList[index] = oldCard + "+";
            Debug.Log($"{oldCard} → {oldCard}+ 업그레이드!");
        }
    }
    
    List<string> GetAllCardsForCharacter(string className)
    {
        List<string> cards = new List<string>();
        
        switch (className)
        {
            case "전사":
                cards.AddRange(new[] { "타격", "방어", "강타", "철벽", "분노", "광전사" });
                break;
            case "마법사":
                cards.AddRange(new[] { "화염구", "방어막", "번개", "얼음 창", "마나 실드", "집중" });
                break;
            case "도적":
                cards.AddRange(new[] { "암습", "회피", "독칼", "연막탄", "그림자 숨기", "급소 찌르기" });
                break;
        }
        
        return cards;
    }
    
    void ShowResult(EventChoice choice)
    {
        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
        }
        
        if (resultText != null)
        {
            string result = choice.resultText;
            
            // 추가 정보 표시
            if (choice.goldChange > 0)
            {
                result += $"\n\n골드 +{choice.goldChange}";
            }
            else if (choice.goldChange < 0)
            {
                result += $"\n\n골드 {choice.goldChange}";
            }
            
            if (choice.healthChangePercent > 0)
            {
                result += $"\n체력 +{choice.healthChangePercent}%";
            }
            else if (choice.healthChangePercent < 0)
            {
                result += $"\n체력 {choice.healthChangePercent}%";
            }
            
            if (choice.maxHealthChange > 0)
            {
                result += $"\n최대 체력 +{choice.maxHealthChange}";
            }
            
            if (choice.addRandomCard)
            {
                result += "\n랜덤 카드 획득!";
            }
            
            if (choice.removeRandomCard)
            {
                result += "\n카드 1장 제거됨";
            }
            
            if (choice.transformCard)
            {
                result += "\n카드 1장 업그레이드!";
            }
            
            resultText.text = result;
        }
    }
    
    void ContinueAfterEvent()
    {
        Debug.Log("이벤트 종료!");
        
        // 다음 층으로 진행
        if (RunData.Instance != null)
        {
            RunData.Instance.AdvanceFloor();
        }
        
        SceneManager.LoadScene("DungeonMapScene");
    }
}