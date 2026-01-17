using UnityEngine;
using UnityEngine.EventSystems;

public class RewardCardSelector : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    private CardDisplay cardDisplay;
    private Card card;
    private Vector3 originalScale;
    private bool isHovering = false;
    
    void Start()
    {
        cardDisplay = GetComponent<CardDisplay>();
        card = GetComponent<Card>();
        originalScale = transform.localScale;
    }
    
    void Update()
    {
        // 호버 효과
        if (isHovering)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, originalScale * 1.1f, Time.deltaTime * 10f);
        }
        else
        {
            transform.localScale = Vector3.Lerp(transform.localScale, originalScale, Time.deltaTime * 10f);
        }
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (card != null && RewardManager.Instance != null)
        {
            // 카드 데이터 생성
            CardData cardData = new CardData(
                card.cardName,
                card.cardType,
                card.cost,
                card.value,
                card.description,
                card.characterIndex
            );
            cardData.specialEffect = card.specialEffect;
            cardData.effectValue = card.effectValue;
            
            RewardManager.Instance.SelectCard(cardData);
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
    }
}