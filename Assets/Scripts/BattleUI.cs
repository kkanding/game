using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BattleUI : MonoBehaviour
{
    public static BattleUI Instance;
    
    [Header("Party HP UI (공유 체력)")]
    public TextMeshProUGUI partyHealthText;
    public HealthBarController partyHealthBar;
    public TextMeshProUGUI partyBlockText;
    public GameObject partyBlockIcon;
    
    [Header("Character Mental Power UI (개별 정신력)")]
    public TextMeshProUGUI[] characterMentalTexts; // 3명 정신력 텍스트
    public HealthBarController[] characterMentalBars; // 3명 정신력 바
    public TextMeshProUGUI[] characterNameTexts; // 3명 이름
    public GameObject[] characterDefeatedIcons; // 쓰러짐 표시
    
    [Header("Enemy UI")]
    public TextMeshProUGUI enemyNameText;
    public TextMeshProUGUI enemyHealthText;
    public HealthBarController enemyHealthBar;
    public GameObject enemyBlockIcon;
    public TextMeshProUGUI enemyBlockText;
    
    [Header("Energy & Button")]
    public TextMeshProUGUI energyText;
    public Button endTurnButton;
    
    [Header("Deck & Discard UI")]
    public TextMeshProUGUI deckCountText;
    public TextMeshProUGUI discardCountText;
    public TextMeshProUGUI handCountText;
    public Button deckButton;
    public Button discardButton;
    
    [Header("Enemy Intent")]
    public GameObject enemyIntentPanel;
    public TextMeshProUGUI enemyIntentText;
    public Image enemyIntentIcon;
	
	[Header("Character Panel (Optional)")]
	public CanvasGroup[] characterPanelGroups; // CharPanel_0/1/2에 CanvasGroup 붙여서 연결

    
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    
    void Start()
    {
        if (endTurnButton != null)
        {
            endTurnButton.onClick.AddListener(OnEndTurnClicked);
        }
        
        if (deckButton != null)
        {
            deckButton.onClick.AddListener(OnDeckClicked);
        }
        
        if (discardButton != null)
        {
            discardButton.onClick.AddListener(OnDiscardClicked);
        }
        
        UpdateAllUI();
    }
    
    public void UpdateAllUI()
    {
        UpdatePartyUI();
        UpdateCharacterUI();
        UpdateEnemyUI();
        UpdateEnergyUI();
        UpdateDeckUI();
    }
    
    // 파티 전체 체력 UI 업데이트
    public void UpdatePartyUI()
    {
        if (BattleManager.Instance == null)
            return;
        
        var bm = BattleManager.Instance;
        
        // 파티 체력
        if (partyHealthText != null)
        {
            partyHealthText.text = $"{bm.partyCurrentHealth} / {bm.partyMaxHealth}";
        }
        
        // 파티 체력바
        if (partyHealthBar != null && bm.partyMaxHealth > 0)
        {
            float healthRatio = (float)bm.partyCurrentHealth / bm.partyMaxHealth;
            partyHealthBar.SetHealth(healthRatio);
        }
        
        // 파티 방어도
        bool hasBlock = bm.partyCurrentBlock > 0;
        
        if (partyBlockIcon != null)
        {
            partyBlockIcon.SetActive(hasBlock);
        }
        
        if (partyBlockText != null)
        {
            partyBlockText.gameObject.SetActive(hasBlock);
            if (hasBlock)
            {
                partyBlockText.text = bm.partyCurrentBlock.ToString();
            }
        }
    }
    
    // 캐릭터 개별 정신력 UI 업데이트
    public void UpdateCharacterUI()
    {
        if (BattleManager.Instance == null || BattleManager.Instance.partyCharacters == null)
            return;
        
        var characters = BattleManager.Instance.partyCharacters;
        
        for (int i = 0; i < characters.Count && i < 3; i++)
        {
            var character = characters[i];
            
            // 캐릭터 이름
            if (characterNameTexts != null && i < characterNameTexts.Length && characterNameTexts[i] != null)
            {
                characterNameTexts[i].text = character.characterName;
            }
			
			bool isDown = character.isDefeated || character.currentMentalPower <= 0;
            
            // 정신력 텍스트
            if (characterMentalTexts != null && i < characterMentalTexts.Length && characterMentalTexts[i] != null)
            {
                // ← 쓰러진 경우 부활 진행도 표시
                if (isDown)
                {
                    characterMentalTexts[i].text = $"부활 {character.reviveCardUseCount}/{BattleManager.Instance.revivedRequiredCount}";
                }
                else
                {
                    characterMentalTexts[i].text = $"{character.currentMentalPower}/{character.maxMentalPower}";
                }
            }
            
            // 정신력 바
            if (characterMentalBars != null && i < characterMentalBars.Length && characterMentalBars[i] != null)
            {
                if (isDown)
                {
                    // 부활 진행도 표시
                    float reviveProgress = (float)character.reviveCardUseCount / BattleManager.Instance.revivedRequiredCount;
                    characterMentalBars[i].SetHealth(reviveProgress);
                }
                else
                {
                    float mentalRatio = (float)character.currentMentalPower / character.maxMentalPower;
                    characterMentalBars[i].SetHealth(mentalRatio);
                }
            }
            
            // 쓰러짐 표시
            if (characterDefeatedIcons != null && i < characterDefeatedIcons.Length && characterDefeatedIcons[i] != null)
            {
                characterDefeatedIcons[i].SetActive(isDown);
            }
        }
    }
    
    // 적 UI 업데이트
    public void UpdateEnemyUI()
    {
        if (BattleManager.Instance == null || BattleManager.Instance.enemies == null)
            return;
        
        // 첫 번째 적만 표시 (나중에 여러 적 지원 시 수정)
        if (BattleManager.Instance.enemies.Count > 0)
        {
            var enemy = BattleManager.Instance.enemies[0];
            
            if (enemy == null) return;
            
            // 적 이름
            if (enemyNameText != null)
            {
                enemyNameText.text = enemy.enemyData.enemyName;
            }
            
            // 적 체력
            if (enemyHealthText != null)
            {
                enemyHealthText.text = $"{enemy.currentHealth}/{enemy.enemyData.maxHealth}";
            }
            
            // 적 체력바
            if (enemyHealthBar != null)
            {
                float healthRatio = (float)enemy.currentHealth / enemy.enemyData.maxHealth;
                enemyHealthBar.SetHealth(healthRatio);
            }
            
            // 적 방어도
            bool hasBlock = enemy.currentDefense > 0;
            
            if (enemyBlockIcon != null)
            {
                enemyBlockIcon.SetActive(hasBlock);
            }
            
            if (enemyBlockText != null)
            {
                enemyBlockText.gameObject.SetActive(hasBlock);
                if (hasBlock)
                {
                    enemyBlockText.text = enemy.currentDefense.ToString();
                }
            }
            
            // 적 의도
            UpdateEnemyIntent(enemy);
        }
    }
    
    // 적 의도 표시
    void UpdateEnemyIntent(EnemyInstance enemy)
    {
        if (enemy == null || enemy.nextAction == null)
        {
            if (enemyIntentPanel != null)
            {
                enemyIntentPanel.SetActive(false);
            }
            return;
        }
        
        if (enemyIntentPanel != null)
        {
            enemyIntentPanel.SetActive(true);
        }
        
        if (enemyIntentText != null)
        {
            string intentStr = "";
            
            if (!string.IsNullOrEmpty(enemy.nextAction.description))
            {
                intentStr = $"{enemy.nextAction.description}";
                
                // 공격이면 데미지 표시
                if (enemy.nextAction.type == EnemyAction.ActionType.Attack)
                {
                    intentStr += $" {enemy.nextAction.value}";
                    
                    // 정신공격력도 표시
                    if (enemy.nextAction.mentalAttackValue > 0)
                    {
                        intentStr += $" (정신 -{enemy.nextAction.mentalAttackValue})";
                    }
                }
                else if (enemy.nextAction.type == EnemyAction.ActionType.Defend)
                {
                    intentStr += $" {enemy.nextAction.value}";
                }
            }
            else
            {
                switch (enemy.nextAction.type)
                {
                    case EnemyAction.ActionType.Attack:
                        intentStr = $"공격 {enemy.nextAction.value}";
                        break;
                    case EnemyAction.ActionType.Defend:
                        intentStr = $"방어 {enemy.nextAction.value}";
                        break;
                    case EnemyAction.ActionType.Buff:
                        intentStr = "버프";
                        break;
                    case EnemyAction.ActionType.Debuff:
                        intentStr = "디버프";
                        break;
                }
            }
            
            enemyIntentText.text = intentStr;
        }
        
        // 의도 아이콘 색상
        if (enemyIntentIcon != null)
        {
            switch (enemy.nextAction.type)
            {
                case EnemyAction.ActionType.Attack:
                    enemyIntentIcon.color = new Color(1f, 0.3f, 0.3f); // 빨강
                    break;
                case EnemyAction.ActionType.Defend:
                    enemyIntentIcon.color = new Color(0.3f, 0.7f, 1f); // 파랑
                    break;
                default:
                    enemyIntentIcon.color = new Color(0.8f, 0.8f, 0.3f); // 노랑
                    break;
            }
        }
    }
    
    public void UpdateEnergyUI()
    {
        if (BattleManager.Instance == null || energyText == null)
            return;
        
        var bm = BattleManager.Instance;
        energyText.text = $"{bm.currentEnergy} / {bm.maxEnergy}";
        
        // 에너지 색상
        if (bm.currentEnergy == 0)
        {
            energyText.color = new Color(1f, 0.3f, 0.3f); // 빨강
        }
        else if (bm.currentEnergy < bm.maxEnergy)
        {
            energyText.color = new Color(1f, 0.8f, 0.3f); // 주황
        }
        else
        {
            energyText.color = new Color(1f, 1f, 0.3f); // 노랑
        }
    }
    
    public void UpdateDeckUI()
    {
        if (CardManager.Instance == null)
            return;
        
        if (deckCountText != null)
            deckCountText.text = CardManager.Instance.GetDeckCount().ToString();
        
        if (discardCountText != null)
            discardCountText.text = CardManager.Instance.GetDiscardCount().ToString();
        
        if (handCountText != null)
            handCountText.text = $"손패: {CardManager.Instance.GetHandCount()}/{CardManager.Instance.maxHandSize}";
    }
    
    void OnEndTurnClicked()
    {
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.EndPlayerTurn();
        }
    }
    
    void OnDeckClicked()
    {
        if (CardManager.Instance != null)
        {
            CardManager.Instance.ShowDeckView();
        }
    }
    
    void OnDiscardClicked()
    {
        if (CardManager.Instance != null)
        {
            CardManager.Instance.ShowDiscardView();
        }
    }
}