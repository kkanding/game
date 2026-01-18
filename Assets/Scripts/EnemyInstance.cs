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
    
    public void Initialize(EnemyData data)
    {
        enemyData = data;
        currentHealth = data.maxHealth;
        UpdateUI();
        
        if (nameText != null)
            nameText.text = data.enemyName;
            
        ShowNextAction();
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