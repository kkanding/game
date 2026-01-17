using UnityEngine;
using System.Collections.Generic;
using System;

public class PopupManager : MonoBehaviour
{
    public static PopupManager Instance;
	public CardListPopup cardListPopup;
    
    [Header("Prefabs")]
    public GameObject cardListPopupPrefab;
    
    [Header("Canvas")]
    public Canvas canvas;
    
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    
    // 덱 팝업 열기
    public void ShowDeckPopup()
    {
        if (CardManager.Instance == null)
        {
            Debug.LogWarning("CardManager가 없습니다!");
            return;
        }
        
        List<CardData> deckCards = CardManager.Instance.GetDeckList();
        ShowCardListPopup(deckCards, true);
    }
    
    // 버리기 더미 팝업 열기
    public void ShowDiscardPopup()
    {
        if (CardManager.Instance == null)
        {
            Debug.LogWarning("CardManager가 없습니다!");
            return;
        }
        
        List<CardData> discardCards = CardManager.Instance.GetDiscardList();
        ShowCardListPopup(discardCards, false);
    }
    
    // 카드 리스트 팝업 표시
    void ShowCardListPopup(List<CardData> cards, bool isDeck)
    {
        if (cardListPopupPrefab == null || canvas == null)
        {
            Debug.LogWarning("팝업 Prefab 또는 Canvas가 없습니다!");
            return;
        }
        
        // 팝업 생성
        GameObject popupObj = Instantiate(cardListPopupPrefab, canvas.transform);
        CardListPopup popup = popupObj.GetComponent<CardListPopup>();
        
        if (popup != null)
        {
            if (isDeck)
            {
                popup.ShowDeck(cards);
            }
            else
            {
                popup.ShowDiscard(cards);
            }
        }
    }
	
	// ← 업그레이드 팝업
    public void ShowUpgradePopup(List<CardData> cards, System.Action<CardData> onCardSelected)
    {
        if (cardListPopup == null)
        {
            Debug.LogError("CardListPopup이 없습니다!");
            return;
        }
        
        cardListPopup.ShowWithCallback(cards, "업그레이드할 카드 선택", onCardSelected);
    }
    
    // ← 제거 팝업
    public void ShowRemovePopup(List<CardData> cards, System.Action<CardData> onCardSelected)
    {
        if (cardListPopup == null)
        {
            Debug.LogError("CardListPopup이 없습니다!");
            return;
        }
        
        cardListPopup.ShowWithCallback(cards, "제거할 카드 선택", onCardSelected);
    }
}