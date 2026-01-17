using UnityEngine;
using TMPro;

public class RosterManager : MonoBehaviour
{
    [Header("Character Cards")]
    public CharacterCardUI card1;
    public CharacterCardUI card2;
    public CharacterCardUI card3;
    
    [Header("Raid Display")]
    public TextMeshProUGUI raidMembersText;
    public TextMeshProUGUI raidCountText;
    
    void Start()
    {
        if (GameData.Instance != null && GameData.Instance.allCharacters.Count >= 3)
        {
            if (card1 != null)
                card1.SetCharacterData(GameData.Instance.allCharacters[0]);
            
            if (card2 != null)
                card2.SetCharacterData(GameData.Instance.allCharacters[1]);
            
            if (card3 != null)
                card3.SetCharacterData(GameData.Instance.allCharacters[2]);
        }
        
        UpdateRaidDisplay();
    }
    
    public void UpdateRaidDisplay()
    {
        if (GameData.Instance == null) return;
        
        string members = "";
        for (int i = 0; i < GameData.Instance.raidParty.Count; i++)
        {
            members += GameData.Instance.raidParty[i].characterName;
            if (i < GameData.Instance.raidParty.Count - 1)
                members += ", ";
        }
        
        if (raidMembersText != null)
        {
            string display = $"현재 공격대 ({GameData.Instance.raidParty.Count}/3)\n";
            display += members;
            raidMembersText.text = display;
        }
    }
}