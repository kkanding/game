using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class EnemyInstance : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("데이터")]
    public EnemyData enemyData;
    public int currentHealth;
    public int currentDefense = 0;
    public int currentActionIndex = 0;
	public EnemyAction nextAction;
    
    [Header("UI")]
    public Slider healthBar;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI nextActionText;
    public Image defenseIcon;
    public TextMeshProUGUI defenseText;
    
    [Header("타겟팅 표시")]
    public GameObject targetableHighlight; // 초록 테두리
    public GameObject dropIndicator;       // 드롭 표시
    
    private bool isTargetable = false;
	
	private bool hasDied = false;
    
    public void Initialize(EnemyData data)
    {
        enemyData = data;
        currentHealth = data.maxHealth;
        UpdateUI();
        
        if (nameText != null)
            nameText.text = data.enemyName;
        
        // 첫 행동 결정
        DecideNextAction();
        ShowNextAction();
    }
    
    // ← 다음 행동 결정
    public void DecideNextAction()
    {
        if (enemyData == null || enemyData.actionPattern.Count == 0)
        {
            // 기본 공격
            nextAction = new EnemyAction(EnemyAction.ActionType.Attack, Random.Range(5, 11), "공격");
            nextAction.mentalAttackValue = 5; // 기본 정신공격력
            return;
        }
        
        nextAction = enemyData.actionPattern[currentActionIndex];
        currentActionIndex = (currentActionIndex + 1) % enemyData.actionPattern.Count;
        
        Debug.Log($"{enemyData.enemyName}의 다음 행동: {nextAction.description} ({nextAction.value})");
    }
    
    // ← UI 자동 생성
    void CreateUI()
    {
        // 배경 이미지 (이미 있으면 사용)
        UnityEngine.UI.Image bgImage = GetComponent<UnityEngine.UI.Image>();
        if (bgImage != null)
        {
            bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f); // 어두운 회색
        }
        
        // 이름 텍스트
        GameObject nameObj = new GameObject("NameText");
        nameObj.transform.SetParent(transform);
        RectTransform nameRect = nameObj.AddComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0, 1);
        nameRect.anchorMax = new Vector2(1, 1);
        nameRect.pivot = new Vector2(0.5f, 1);
        nameRect.anchoredPosition = new Vector2(0, -10);
        nameRect.sizeDelta = new Vector2(-20, 30);
        
        nameText = nameObj.AddComponent<TMPro.TextMeshProUGUI>();
        nameText.text = enemyData.enemyName;
        nameText.fontSize = 20;
        nameText.alignment = TMPro.TextAlignmentOptions.Center;
        nameText.color = Color.white;
        
        // 체력바 배경
        GameObject hpBgObj = new GameObject("HealthBarBG");
        hpBgObj.transform.SetParent(transform);
        RectTransform hpBgRect = hpBgObj.AddComponent<RectTransform>();
        hpBgRect.anchorMin = new Vector2(0, 0);
        hpBgRect.anchorMax = new Vector2(1, 0);
        hpBgRect.pivot = new Vector2(0.5f, 0);
        hpBgRect.anchoredPosition = new Vector2(0, 50);
        hpBgRect.sizeDelta = new Vector2(-40, 20);
        
        UnityEngine.UI.Image hpBgImage = hpBgObj.AddComponent<UnityEngine.UI.Image>();
        hpBgImage.color = new Color(0.3f, 0.3f, 0.3f);
        
        // 체력바
        GameObject hpBarObj = new GameObject("HealthBar");
        hpBarObj.transform.SetParent(hpBgObj.transform);
        RectTransform hpBarRect = hpBarObj.AddComponent<RectTransform>();
        hpBarRect.anchorMin = new Vector2(0, 0);
        hpBarRect.anchorMax = new Vector2(1, 1);
        hpBarRect.pivot = new Vector2(0, 0.5f);
        hpBarRect.anchoredPosition = Vector2.zero;
        hpBarRect.sizeDelta = Vector2.zero;
        
        UnityEngine.UI.Image hpBarImage = hpBarObj.AddComponent<UnityEngine.UI.Image>();
        hpBarImage.color = new Color(0f, 1f, 0f); // 초록색
        hpBarImage.type = UnityEngine.UI.Image.Type.Filled;
        hpBarImage.fillMethod = UnityEngine.UI.Image.FillMethod.Horizontal;
        hpBarImage.fillOrigin = (int)UnityEngine.UI.Image.OriginHorizontal.Left;
        
        // Slider 컴포넌트 추가
        healthBar = hpBgObj.AddComponent<UnityEngine.UI.Slider>();
        healthBar.targetGraphic = hpBarImage;
        healthBar.fillRect = hpBarRect;
        healthBar.minValue = 0;
        healthBar.maxValue = 1;
        healthBar.value = 1;
        healthBar.interactable = false;
        
        // 체력 텍스트
        GameObject hpTextObj = new GameObject("HealthText");
        hpTextObj.transform.SetParent(transform);
        RectTransform hpTextRect = hpTextObj.AddComponent<RectTransform>();
        hpTextRect.anchorMin = new Vector2(0, 0);
        hpTextRect.anchorMax = new Vector2(1, 0);
        hpTextRect.pivot = new Vector2(0.5f, 0);
        hpTextRect.anchoredPosition = new Vector2(0, 20);
        hpTextRect.sizeDelta = new Vector2(-20, 25);
        
        healthText = hpTextObj.AddComponent<TMPro.TextMeshProUGUI>();
        healthText.fontSize = 16;
        healthText.alignment = TMPro.TextAlignmentOptions.Center;
        healthText.color = Color.white;
        
        Debug.Log($"적 UI 생성 완료: {enemyData.enemyName}");
    }
    
    // 데미지 받기
    public void TakeDamage(int damage)
    {
        int actualDamage = Mathf.Max(0, damage - currentDefense);
        currentHealth -= actualDamage;
        
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
        
        UpdateUI();
        
        Debug.Log($"{enemyData.enemyName}이(가) {actualDamage} 데미지를 받았습니다! ({currentHealth}/{enemyData.maxHealth})");
        
        // ← 데미지 팝업 (간단 버전)
        ShowDamagePopup(actualDamage);
    }
    
    // ← 간단한 데미지 팝업
    void ShowDamagePopup(int damage)
    {
        GameObject popupObj = new GameObject("DamagePopup");
        popupObj.transform.SetParent(transform);
        
        RectTransform popupRect = popupObj.AddComponent<RectTransform>();
        popupRect.anchoredPosition = new Vector2(0, 50);
        popupRect.sizeDelta = new Vector2(100, 50);
        
        TMPro.TextMeshProUGUI popupText = popupObj.AddComponent<TMPro.TextMeshProUGUI>();
        popupText.text = $"-{damage}";
        popupText.fontSize = 36;
        popupText.alignment = TMPro.TextAlignmentOptions.Center;
        popupText.color = Color.red;
        popupText.fontStyle = TMPro.FontStyles.Bold;
        
        // 애니메이션 (위로 올라가며 사라짐)
        StartCoroutine(AnimateDamagePopup(popupObj));
    }
    
    System.Collections.IEnumerator AnimateDamagePopup(GameObject popup)
    {
        RectTransform rect = popup.GetComponent<RectTransform>();
        TMPro.TextMeshProUGUI text = popup.GetComponent<TMPro.TextMeshProUGUI>();
        
        float duration = 1f;
        float elapsed = 0f;
        Vector2 startPos = rect.anchoredPosition;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            
            // 위로 이동
            rect.anchoredPosition = startPos + Vector2.up * (progress * 100);
            
            // 페이드 아웃
            Color color = text.color;
            color.a = 1f - progress;
            text.color = color;
            
            yield return null;
        }
        
        Destroy(popup);
    }
    
    // 방어도 획득
    public void GainDefense(int defense)
    {
        currentDefense += defense;
        UpdateUI();
    }
    
    // 다음 행동 표시
    void ShowNextAction()
    {
        if (enemyData.actionPattern.Count == 0) return;
        
        EnemyAction nextAction = enemyData.actionPattern[currentActionIndex];
        
        if (nextActionText != null)
        {
            string actionIcon = nextAction.type == EnemyAction.ActionType.Attack ? "⚔️" : "🛡️";
            nextActionText.text = $"{actionIcon} {nextAction.description} ({nextAction.value})";
        }
    }
    
    // 적 턴 행동
    public EnemyAction PerformAction()
    {
        if (enemyData.actionPattern.Count == 0) return null;
        
        EnemyAction action = enemyData.actionPattern[currentActionIndex];
        
        // 다음 행동으로 이동
        currentActionIndex = (currentActionIndex + 1) % enemyData.actionPattern.Count;
        ShowNextAction();
        
        return action;
    }
    
    void Die()
    {
		if (hasDied) return;
		hasDied = true;

		RelicManager.Instance?.OnEnemyKilled();
		
        Debug.Log($"{enemyData.enemyName} 처치!");
        // 사망 애니메이션 후 제거
        Destroy(gameObject, 1f);
    }
    
    void UpdateUI()
    {
        if (healthBar != null)
        {
            healthBar.value = (float)currentHealth / enemyData.maxHealth;
        }
        
        if (healthText != null)
        {
            healthText.text = $"{currentHealth}/{enemyData.maxHealth}";
        }
        
        if (defenseIcon != null && defenseText != null)
        {
            if (currentDefense > 0)
            {
                defenseIcon.gameObject.SetActive(true);
                defenseText.text = currentDefense.ToString();
            }
            else
            {
                defenseIcon.gameObject.SetActive(false);
            }
        }
    }
    
    // 타겟 가능 표시
    public void SetTargetable(bool targetable)
    {
        isTargetable = targetable;
        
        if (targetableHighlight != null)
        {
            targetableHighlight.SetActive(targetable);
        }
    }
    
    // 마우스 호버 시
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isTargetable && dropIndicator != null)
        {
            dropIndicator.SetActive(true);
            transform.localScale = Vector3.one * 1.1f;
        }
    }
    
    // 마우스 벗어날 시
    public void OnPointerExit(PointerEventData eventData)
    {
        if (dropIndicator != null)
        {
            dropIndicator.SetActive(false);
        }
        transform.localScale = Vector3.one;
    }
}