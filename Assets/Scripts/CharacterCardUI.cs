using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CharacterCardUI : MonoBehaviour
{
    [Header("Character Info")]
    public CharacterData characterData;
    
    [Header("UI Elements")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI classText;
    public Image characterImage;
    public Button selectButton;
    public TextMeshProUGUI buttonText;
    
    [Header("References")]
    public RosterManager rosterManager;
    
    private bool isSelected = true; // 시작 시 모두 선택됨
    
    void Start()
    {
        // 버튼 클릭 이벤트 연결
        if (selectButton != null)
        {
            selectButton.onClick.AddListener(ToggleSelection);
        }
        
        UpdateUI();
    }
    
    public void SetCharacterData(CharacterData data)
    {
        characterData = data;
        UpdateUI();
    }
    
    void UpdateUI()
    {
        if (characterData == null) return;
        
        if (nameText != null)
            nameText.text = characterData.characterName;
        
        if (hpText != null)
            hpText.text = $"체력: {characterData.maxHealth}";
        
        if (classText != null)
            classText.text = $"클래스: {characterData.characterClass}";
        
        UpdateButtonUI();
    }
    
    void UpdateButtonUI()
    {
        if (selectButton != null && buttonText != null)
        {
            if (isSelected)
            {
                selectButton.GetComponent<Image>().color = Color.green;
                buttonText.text = "선택됨";
            }
            else
            {
                selectButton.GetComponent<Image>().color = Color.gray;
                buttonText.text = "선택";
            }
        }
    }
    
    public void ToggleSelection()
    {
        if (GameData.Instance == null) return;
        
        if (isSelected)
        {
            // 선택 해제
            GameData.Instance.raidParty.Remove(characterData);
            isSelected = false;
            Debug.Log($"{characterData.characterName} 선택 해제!");
        }
        else
        {
            // 선택
            if (GameData.Instance.raidParty.Count < 3)
            {
                GameData.Instance.raidParty.Add(characterData);
                isSelected = true;
                Debug.Log($"{characterData.characterName} 선택!");
            }
            else
            {
                Debug.Log("공격대는 최대 3명까지만 가능합니다!");
            }
        }
        
        UpdateButtonUI();
        
        // RosterManager 업데이트
        if (rosterManager != null)
        {
            rosterManager.UpdateRaidDisplay();
        }
    }
}