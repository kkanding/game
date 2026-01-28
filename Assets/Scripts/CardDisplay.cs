using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CardDisplay : MonoBehaviour, 
    IPointerEnterHandler, 
    IPointerExitHandler,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    [Header("UI References")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI costText;
    public TextMeshProUGUI descriptionText;
    public Image cardImage;
    public Image backgroundImage;
    
    [Header("Upgrade")]
    public GameObject upgradeIndicator;
    
    [Header("Drag Line")]
    private LineRenderer dragLine; // 드래그 선
    private GameObject dragLineObj;
    
    private Card cardData;
    private CardAnimator animator;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private bool isDragging = false;
    
    public bool disableClick = false;
    
    void Awake()
    {
        animator = GetComponent<CardAnimator>();
        canvas = GetComponentInParent<Canvas>();
        
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        rectTransform = GetComponent<RectTransform>();
    }
    
    public void SetupCard(Card card)
    {
        if (card == null)
        {
            Debug.LogError("SetupCard: card가 null입니다!");
            return;
        }
        
        cardData = card;
        
        if (card.cardData == null)
        {
            Debug.LogError($"SetupCard: card.cardData가 null입니다! cardName: {card.cardName}");
            return;
        }
        
        Debug.Log($"SetupCard 호출: {card.cardName}, Type: {card.cardType}");
        
        if (nameText != null)
            nameText.text = card.cardName;
        
        if (costText != null)
            costText.text = card.cost.ToString();
        
        if (descriptionText != null)
            descriptionText.text = card.description;
        
        // 캐릭터별 배경 색상
        if (backgroundImage != null && GameData.Instance != null)
        {
            if (card.characterIndex >= 0 && card.characterIndex < GameData.Instance.raidParty.Count)
            {
                CharacterData character = GameData.Instance.raidParty[card.characterIndex];
                
                switch (character.characterName)
                {
                    case "전사":
                        backgroundImage.color = new Color(1f, 0.3f, 0.3f);
                        break;
                    case "마법사":
                        backgroundImage.color = new Color(0.3f, 0.5f, 1f);
                        break;
                    case "도적":
                        backgroundImage.color = new Color(0.3f, 0.8f, 0.3f);
                        break;
                    default:
                        backgroundImage.color = Color.white;
                        break;
                }
            }
        }
        
        if (upgradeIndicator != null)
        {
            upgradeIndicator.SetActive(card.cardName.EndsWith("+"));
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isDragging && animator != null)
        {
            animator.OnHoverEnter();
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isDragging && animator != null)
        {
            animator.OnHoverExit();
        }
    }
    
    // ======= 드래그 앤 드롭 (궤적만) =======
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (disableClick || BattleManager.Instance == null || cardData == null)
        {
            Debug.Log("드래그 취소: disableClick 또는 null");
            return;
        }
        
        // 에너지 체크
        if (BattleManager.Instance.currentEnergy < cardData.cost)
        {
            Debug.Log($"에너지 부족! 필요: {cardData.cost}, 현재: {BattleManager.Instance.currentEnergy}");
            return;
        }
        
        isDragging = true;
        
        Debug.Log($"{cardData.cardName} 드래그 시작!");
        
        // ← 카드는 반투명만
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0.7f; // 약간 투명하게
        }
        
        // ← 드래그 선 생성
        CreateDragLine();
        
        // 공격 카드만 적 하이라이트
        if (cardData.cardType == CardType.Attack)
        {
            HighlightEnemies(true);
        }
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        
        // ← 드래그 선 업데이트
        UpdateDragLine(eventData.position);
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }
        
        isDragging = false;
        
        // ← 드래그 선 제거
        DestroyDragLine();
        
        // 하이라이트 해제
        HighlightEnemies(false);
        
        if (cardData == null)
        {
            return;
        }
        
        Debug.Log($"카드 드롭: {cardData.cardName}, Type: {cardData.cardType}");
        
        // 공격 카드: 적 찾기
        if (cardData.cardType == CardType.Attack)
        {
            EnemyInstance targetEnemy = GetEnemyUnderPointer(eventData);
            
            if (targetEnemy != null && targetEnemy.currentHealth > 0)
            {
                Debug.Log($"적 발견: {targetEnemy.enemyData.enemyName}");
                UseCardOnEnemy(targetEnemy);
            }
            else
            {
                Debug.Log("적을 찾지 못함");
            }
        }
        // 방어/스킬 카드: 즉시 사용
        else
        {
            UseDefenseCard();
        }
    }
    
    // ← 드래그 선 생성
    void CreateDragLine()
    {
        // Canvas 아래에 선 오브젝트 생성
        dragLineObj = new GameObject("DragLine");
        dragLineObj.transform.SetParent(canvas.transform);
        dragLineObj.transform.SetAsLastSibling();
        
        // LineRenderer 추가 (3D)
        dragLine = dragLineObj.AddComponent<LineRenderer>();
        dragLine.startWidth = 0.05f;  // ← 5에서 0.05로 변경!
        dragLine.endWidth = 0.05f;    // ← 5에서 0.05로 변경!
        dragLine.positionCount = 2;
        dragLine.useWorldSpace = true;
        
        // 색상 (카드 타입에 따라)
        if (cardData.cardType == CardType.Attack)
        {
            dragLine.startColor = new Color(1f, 0.3f, 0.3f, 0.8f); // 빨강
            dragLine.endColor = new Color(1f, 0.3f, 0.3f, 0.8f);
        }
        else
        {
            dragLine.startColor = new Color(0.3f, 1f, 0.3f, 0.8f); // 초록
            dragLine.endColor = new Color(0.3f, 1f, 0.3f, 0.8f);
        }
        
        // Material 설정 (기본 Sprite Material)
        dragLine.material = new Material(Shader.Find("Sprites/Default"));
        
        // 시작점 설정
        Vector3 startPos = Camera.main.ScreenToWorldPoint(new Vector3(rectTransform.position.x, rectTransform.position.y, 0));
        startPos.z = 0;
        dragLine.SetPosition(0, startPos);
        dragLine.SetPosition(1, startPos);
    }
    
    // ← 드래그 선 업데이트
    void UpdateDragLine(Vector2 mousePosition)
    {
        if (dragLine == null) return;
        
        // 시작점: 카드 위치
        Vector3 startPos = Camera.main.ScreenToWorldPoint(new Vector3(rectTransform.position.x, rectTransform.position.y, 0));
        startPos.z = 0;
        
        // 끝점: 마우스 위치
        Vector3 endPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, 0));
        endPos.z = 0;
        
        dragLine.SetPosition(0, startPos);
        dragLine.SetPosition(1, endPos);
    }
    
    // ← 드래그 선 제거
    void DestroyDragLine()
    {
        if (dragLineObj != null)
        {
            Destroy(dragLineObj);
            dragLineObj = null;
            dragLine = null;
        }
    }
    
    // 포인터 아래 적 찾기
    EnemyInstance GetEnemyUnderPointer(PointerEventData eventData)
    {
        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        
        Debug.Log($"Raycast 결과: {results.Count}개");
        
        foreach (var result in results)
        {
            Debug.Log($"  - {result.gameObject.name}");
            
            EnemyInstance enemy = result.gameObject.GetComponent<EnemyInstance>();
            if (enemy != null)
            {
                Debug.Log($"    → EnemyInstance 발견!");
                return enemy;
            }
            
            enemy = result.gameObject.GetComponentInParent<EnemyInstance>();
            if (enemy != null)
            {
                Debug.Log($"    → 부모에서 EnemyInstance 발견!");
                return enemy;
            }
        }
        
        return null;
    }
    
    // 적 하이라이트
    void HighlightEnemies(bool highlight)
    {
        if (BattleManager.Instance == null) return;
        
        foreach (var enemy in BattleManager.Instance.enemies)
        {
            if (enemy != null && enemy.currentHealth > 0)
            {
                enemy.SetTargetable(highlight);
            }
        }
    }
    
    // 적에게 카드 사용
    void UseCardOnEnemy(EnemyInstance target)
    {
        if (cardData == null || BattleManager.Instance == null) return;
        
        BattleManager.Instance.currentEnergy -= cardData.cost;
        
        CharacterData character = null;
        if (GameData.Instance != null && cardData.characterIndex < GameData.Instance.raidParty.Count)
        {
            character = GameData.Instance.raidParty[cardData.characterIndex];
        }
        
        int damage = cardData.value;
        
        if (cardData.damageMultiplier > 0 && character != null)
        {
            damage = cardData.CalculateDamage(character);
        }
        
        BattleManager.Instance.DamageEnemy(target, damage);
        Debug.Log($"{cardData.cardName} 사용! {target.enemyData.enemyName}에게 {damage} 데미지!");
        
        if (cardData.specialEffect == CardEffect.DrawCard && cardData.effectValue > 0)
        {
            CardManager.Instance?.DrawCards(cardData.effectValue);
        }
        
        if (CardManager.Instance != null)
        {
            CardManager.Instance.PlayCard(this);
        }
        
        if (BattleUI.Instance != null)
        {
            BattleUI.Instance.UpdateAllUI();
        }
    }
    
    // 방어/스킬 카드 사용
    void UseDefenseCard()
    {
        if (cardData == null || BattleManager.Instance == null) return;
        
        BattleManager.Instance.currentEnergy -= cardData.cost;
        
        CharacterData character = null;
        if (GameData.Instance != null && cardData.characterIndex < GameData.Instance.raidParty.Count)
        {
            character = GameData.Instance.raidParty[cardData.characterIndex];
        }
        
        if (cardData.cardName == "정신집중" && character != null)
        {
            BattleManager.Instance.UseReviveCard(character);
            Debug.Log($"{cardData.cardName} 사용!");
        }
        else if (cardData.cardName.Contains("명상") && cardData.mentalRestoreAmount > 0 && character != null)
        {
            character.RestoreMentalPower(cardData.mentalRestoreAmount);
            Debug.Log($"{cardData.cardName} 사용! 정신력 {cardData.mentalRestoreAmount} 회복!");
        }
        else
        {
            int block = cardData.value;
            
            if (cardData.defenseMultiplier > 0 && character != null)
            {
                block = cardData.CalculateDefense(character);
            }
            
            BattleManager.Instance.PartyGainBlock(block);
            Debug.Log($"{cardData.cardName} 사용! 방어도 {block} 획득!");
        }
        
        if (cardData.specialEffect == CardEffect.DrawCard && cardData.effectValue > 0)
        {
            CardManager.Instance?.DrawCards(cardData.effectValue);
        }
        
        if (CardManager.Instance != null)
        {
            CardManager.Instance.PlayCard(this);
        }
        
        if (BattleUI.Instance != null)
        {
            BattleUI.Instance.UpdateAllUI();
        }
    }
}