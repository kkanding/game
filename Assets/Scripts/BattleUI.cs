using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BattleUI : MonoBehaviour
{
    public static BattleUI Instance;
    
   [Header("Party UI")]
	public TextMeshProUGUI[] partyHealthTexts;
	public HealthBarController[] partyHealthBars;

	[Header("Enemy UI")]
	public TextMeshProUGUI enemyNameText;
	public TextMeshProUGUI enemyHealthText;
	public HealthBarController enemyHealthBar;
    
    [Header("Energy & Button")]
    public TextMeshProUGUI energyText;
    public Button endTurnButton;
    
    [Header("Deck & Discard UI")]
    public TextMeshProUGUI deckCountText;
	public TextMeshProUGUI discardCountText;
	public TextMeshProUGUI handCountText;
	public Button deckButton;
	public Button discardButton;
	
	[Header("Block UI")] 
	public GameObject[] partyBlockIcons; 
	public TextMeshProUGUI[] partyBlockTexts; 
	public GameObject enemyBlockIcon; 
	public TextMeshProUGUI enemyBlockText; 
	
	[Header("Enemy Intent")]
	public GameObject enemyIntentPanel;
	public TextMeshProUGUI enemyIntentText;
	public UnityEngine.UI.Image enemyIntentIcon;
    
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
        UpdateEnemyUI();
        UpdateEnergyUI();
        UpdateDeckUI();
    }
    
    public void UpdatePartyUI()
	{
		if (BattleManager.Instance == null || BattleManager.Instance.party == null)
			return;
		
		for (int i = 0; i < BattleManager.Instance.party.Count && i < partyHealthTexts.Length; i++)
		{
			var member = BattleManager.Instance.party[i];
			
			// 체력 텍스트
			if (partyHealthTexts[i] != null)
			{
				partyHealthTexts[i].text = $"체력: {member.currentHealth}/{member.maxHealth}";
			}
			
			// 체력바
			if (partyHealthBars != null && i < partyHealthBars.Length && partyHealthBars[i] != null)
			{
				float healthRatio = (float)member.currentHealth / member.maxHealth;
				partyHealthBars[i].SetHealth(healthRatio);
			}
			
			// 방어도 아이콘 + 텍스트
			bool hasBlock = member.block > 0;
			
			if (partyBlockIcons != null && i < partyBlockIcons.Length && partyBlockIcons[i] != null)
			{
				partyBlockIcons[i].SetActive(hasBlock);
			}
			
			if (partyBlockTexts != null && i < partyBlockTexts.Length && partyBlockTexts[i] != null)
			{
				partyBlockTexts[i].gameObject.SetActive(hasBlock);
				
				if (hasBlock)
				{
					partyBlockTexts[i].text = member.block.ToString();
				}
			}
		}
	}
    
    public void UpdateEnemyUI()
	{
		if (BattleManager.Instance == null || BattleManager.Instance.enemy == null)
			return;
		
		var enemy = BattleManager.Instance.enemy;
		
		// ← 적 이름 추가!
		if (enemyNameText != null)
		{
			enemyNameText.text = enemy.entityName;
		}
		
		// 체력 텍스트
		if (enemyHealthText != null)
		{
			enemyHealthText.text = $"체력: {enemy.currentHealth}/{enemy.maxHealth}";
		}
		
		// 체력바
		if (enemyHealthBar != null)
		{
			float healthRatio = (float)enemy.currentHealth / enemy.maxHealth;
			enemyHealthBar.SetHealth(healthRatio);
		}
		
		// 방어도
		bool hasBlock = enemy.block > 0;
		
		if (enemyBlockIcon != null)
		{
			enemyBlockIcon.SetActive(hasBlock);
		}
		
		if (enemyBlockText != null)
		{
			enemyBlockText.gameObject.SetActive(hasBlock);
			if (hasBlock)
			{
				enemyBlockText.text = enemy.block.ToString();
			}
		}
		
		// 의도 표시
		UpdateEnemyIntent();
	}
    
    public void UpdateEnergyUI()
	{
		if (BattleManager.Instance == null || energyText == null)
			return;
		
		var bm = BattleManager.Instance;
		energyText.text = $"{bm.currentEnergy} / {bm.maxEnergy}";
		
		// 에너지가 0이면 빨간색, 아니면 노란색
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
	
	public void UpdateDeckUI()
	{
		if (CardManager.Instance == null)
        return;
    
		if (deckCountText != null)
			deckCountText.text = CardManager.Instance.GetDeckCount().ToString();
		
		if (discardCountText != null)
			discardCountText.text = CardManager.Instance.GetDiscardCount().ToString();
		
		// ← 이 부분만 추가!
		if (handCountText != null)
			handCountText.text = $"손패: {CardManager.Instance.GetHandCount()}/{CardManager.Instance.maxHandSize}";
	}
	
	// 적 의도 표시
	void UpdateEnemyIntent()
	{
		if (BattleManager.Instance == null || BattleManager.Instance.enemy == null)
			return;
		
		var enemy = BattleManager.Instance.enemy;
		
		if (enemy.nextAction == null)
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
			
			// ← description 사용!
			if (!string.IsNullOrEmpty(enemy.nextAction.description))
			{
				intentStr = $"{enemy.nextAction.description} {enemy.nextAction.value}";
			}
			else
			{
				// description이 없으면 type으로
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
		
		// 아이콘 색상
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
}