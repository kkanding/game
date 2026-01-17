using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class RestManager : MonoBehaviour
{
    public static RestManager Instance;
    
    [Header("UI References")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public Button smallHealButton;
    public Button fullHealButton;
    
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    
    void Start()
    {
        if (smallHealButton != null)
        {
            smallHealButton.onClick.AddListener(SmallHeal);
        }
        
        if (fullHealButton != null)
        {
            fullHealButton.onClick.AddListener(FullHeal);
        }
        
        UpdateDisplay();
    }
    
    void UpdateDisplay()
    {
        if (GameData.Instance == null || GameData.Instance.raidParty.Count == 0)
            return;
        
        string partyStatus = "\n\n현재 파티 상태:\n";
        
        foreach (var character in GameData.Instance.raidParty)
        {
            partyStatus += $"{character.characterName}: {character.currentHealth}/{character.maxHealth} HP\n";
        }
        
        if (descriptionText != null)
        {
            descriptionText.text = "잠시 휴식을 취합니다. 체력을 회복하세요." + partyStatus;
        }
    }
    
    void SmallHeal()
    {
        if (GameData.Instance == null) return;
        
        Debug.Log("작은 휴식 선택!");
        
        foreach (var character in GameData.Instance.raidParty)
        {
            int healAmount = Mathf.RoundToInt(character.maxHealth * 0.3f);
            character.currentHealth = Mathf.Min(character.currentHealth + healAmount, character.maxHealth);
            
            Debug.Log($"{character.characterName} 체력 회복: +{healAmount} HP");
        }
        
        ReturnToMap();
    }
    
    void FullHeal()
    {
        if (GameData.Instance == null) return;
        
        Debug.Log("긴 휴식 선택!");
        
        foreach (var character in GameData.Instance.raidParty)
        {
            character.maxHealth += 10;
            character.currentHealth = character.maxHealth;
            
            Debug.Log($"{character.characterName} 완전 회복! 최대 체력 +10 (현재: {character.maxHealth})");
        }
        
        ReturnToMap();
    }
    
    void ReturnToMap()
    {
        if (RunData.Instance != null)
        {
            RunData.Instance.AdvanceFloor();
        }
        
        SceneManager.LoadScene("DungeonMapScene");
    }
}