using UnityEngine;

public class GameManager : MonoBehaviour
{
    public CardDisplay cardDisplayPrefab;
    public Transform handTransform;
    
    void Start()
    {
        Debug.Log("GameManager 시작!");
        Invoke("CreateTestCard", 0.5f); // 약간의 지연 후 생성
    }
    
    void CreateTestCard()
    {
        if (cardDisplayPrefab == null)
        {
            Debug.LogError("Card Prefab이 연결되지 않았습니다!");
            return;
        }
        
        if (handTransform == null)
        {
            Debug.LogError("Hand Transform이 연결되지 않았습니다!");
            return;
        }
        
        Debug.Log("카드 생성 시작...");
        
        // 카드 UI 생성
        CardDisplay cardDisplay = Instantiate(cardDisplayPrefab, handTransform);
        
        // 바로 Card 컴포넌트 추가
        Card cardData = cardDisplay.gameObject.AddComponent<Card>();
        
        if (cardData == null)
        {
            Debug.LogError("Card 컴포넌트 추가 실패!");
            return;
        }
        
        Debug.Log("Card 컴포넌트 추가 성공!");
        
        // 데이터 설정
        cardData.cardName = "타격";
        cardData.cardType = CardType.Attack;
        cardData.cost = 1;
        cardData.value = 6;
        cardData.description = "적에게 6의 피해를 준다.";
        
        // UI 업데이트 - 다음 프레임에서
        StartCoroutine(SetupCardNextFrame(cardDisplay, cardData));
    }
    
    System.Collections.IEnumerator SetupCardNextFrame(CardDisplay display, Card data)
    {
        yield return null; // 한 프레임 대기
        
        Debug.Log($"SetupCard 호출 전 - Card 존재: {display.GetComponent<Card>() != null}");
        display.SetupCard(data);
        Debug.Log("테스트 카드 생성 완료!");
    }
}