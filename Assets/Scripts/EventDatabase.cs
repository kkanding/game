using UnityEngine;
using System.Collections.Generic;

public class EventDatabase : MonoBehaviour
{
    public static EventDatabase Instance;
    
    private List<EventData> events = new List<EventData>();
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeEvents();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void InitializeEvents()
    {
        events.Clear();
        
        // ===== 이벤트 1: 떠돌이 상인 =====
        EventData merchant = new EventData();
        merchant.eventTitle = "떠돌이 상인";
        merchant.eventDescription = "길을 가던 중 떠돌이 상인을 만났다.\n\"좋은 물건이 있소. 관심 있으시오?\"";
        
        EventChoice buyCard = new EventChoice();
        buyCard.choiceText = "카드를 구매한다 (-50 골드)";
        buyCard.resultText = "상인에게서 카드를 구매했다.";
        buyCard.goldChange = -50;
        buyCard.addRandomCard = true;
        merchant.choices.Add(buyCard);
        
        EventChoice decline = new EventChoice();
        decline.choiceText = "거절한다";
        decline.resultText = "상인과 헤어졌다.";
        merchant.choices.Add(decline);
        
        events.Add(merchant);
        
        // ===== 이벤트 2: 신비한 샘물 =====
        EventData fountain = new EventData();
        fountain.eventTitle = "신비한 샘물";
        fountain.eventDescription = "맑은 샘물이 흐르고 있다.\n물에서 신비한 기운이 느껴진다.";
        
        EventChoice drink = new EventChoice();
        drink.choiceText = "물을 마신다";
        drink.resultText = "상쾌한 기분이 든다! 체력이 회복되었다.";
        drink.healthChangePercent = 30;
        fountain.choices.Add(drink);
        
        EventChoice bless = new EventChoice();
        bless.choiceText = "기도를 올린다";
        bless.resultText = "신비한 힘이 깃들었다! 최대 체력이 증가했다.";
        bless.maxHealthChange = 5;
        fountain.choices.Add(bless);
        
        EventChoice leave = new EventChoice();
        leave.choiceText = "그냥 지나간다";
        leave.resultText = "샘물을 뒤로하고 길을 계속 간다.";
        fountain.choices.Add(leave);
        
        events.Add(fountain);
        
        // ===== 이벤트 3: 도박꾼 =====
        EventData gambler = new EventData();
        gambler.eventTitle = "도박꾼";
        gambler.eventDescription = "수상한 도박꾼이 제안을 한다.\n\"동전 던지기 한판 어때? 이기면 두 배!\"";
        
        EventChoice gambleBig = new EventChoice();
        gambleBig.choiceText = "큰 돈을 건다 (-50 골드)";
        gambleBig.resultText = "";
        gambleBig.goldChange = Random.value > 0.5f ? 100 : -50;
        gambler.choices.Add(gambleBig);
        
        EventChoice gambleSmall = new EventChoice();
        gambleSmall.choiceText = "작은 돈을 건다 (-25 골드)";
        gambleSmall.resultText = "";
        gambleSmall.goldChange = Random.value > 0.5f ? 50 : -25;
        gambler.choices.Add(gambleSmall);
        
        EventChoice refuseGamble = new EventChoice();
        refuseGamble.choiceText = "거절한다";
        refuseGamble.resultText = "도박꾼은 아쉬워하며 떠났다.";
        gambler.choices.Add(refuseGamble);
        
        events.Add(gambler);
        
        // ===== 이벤트 4: 버려진 보물 상자 =====
        EventData chest = new EventData();
        chest.eventTitle = "버려진 보물 상자";
        chest.eventDescription = "먼지 쌓인 보물 상자를 발견했다.\n함정일 수도 있지만 보물이 있을지도...";
        
        EventChoice openChest = new EventChoice();
        openChest.choiceText = "조심스럽게 연다";
        openChest.resultText = Random.value > 0.6f ? "금화를 발견했다!" : "함정이었다! 부상을 입었다.";
        openChest.goldChange = Random.value > 0.6f ? 75 : 0;
        openChest.healthChangePercent = Random.value > 0.6f ? 0 : -15;
        chest.choices.Add(openChest);
        
        EventChoice breakChest = new EventChoice();
        breakChest.choiceText = "힘으로 부순다";
        breakChest.resultText = "보물 상자를 부쉈다. 약간의 금화를 얻었다.";
        breakChest.goldChange = 35;
        chest.choices.Add(breakChest);
        
        EventChoice ignoreChest = new EventChoice();
        ignoreChest.choiceText = "무시한다";
        ignoreChest.resultText = "위험을 무릅쓰지 않기로 했다.";
        chest.choices.Add(ignoreChest);
        
        events.Add(chest);
        
        // ===== 이벤트 5: 떠돌이 대장장이 =====
        EventData blacksmith = new EventData();
        blacksmith.eventTitle = "떠돌이 대장장이";
        blacksmith.eventDescription = "대장간을 차린 대장장이를 만났다.\n\"카드를 강화해 드릴 수 있소!\"";
        
        EventChoice upgrade = new EventChoice();
        upgrade.choiceText = "카드를 강화한다 (-75 골드)";
        upgrade.resultText = "카드가 강화되었다!";
        upgrade.goldChange = -75;
        upgrade.transformCard = true;
        blacksmith.choices.Add(upgrade);
        
        EventChoice declineSmith = new EventChoice();
        declineSmith.choiceText = "거절한다";
        declineSmith.resultText = "대장장이와 헤어졌다.";
        blacksmith.choices.Add(declineSmith);
        
        events.Add(blacksmith);
        
        // ===== 이벤트 6: 의문의 약초 =====
        EventData herbs = new EventData();
        herbs.eventTitle = "의문의 약초";
        herbs.eventDescription = "길가에 이상한 약초가 자라고 있다.\n독초일 수도, 약초일 수도...";
        
        EventChoice eatHerb = new EventChoice();
        eatHerb.choiceText = "먹어본다";
        eatHerb.resultText = Random.value > 0.5f ? "몸 상태가 좋아졌다!" : "배탈이 났다...";
        eatHerb.healthChangePercent = Random.value > 0.5f ? 20 : -10;
        eatHerb.maxHealthChange = Random.value > 0.5f ? 8 : 0;
        herbs.choices.Add(eatHerb);
        
        EventChoice ignoreHerb = new EventChoice();
        ignoreHerb.choiceText = "무시한다";
        ignoreHerb.resultText = "약초를 그냥 두고 갔다.";
        herbs.choices.Add(ignoreHerb);
        
        events.Add(herbs);
        
        // ===== 이벤트 7: 수상한 상인 =====
        EventData shadyMerchant = new EventData();
        shadyMerchant.eventTitle = "수상한 상인";
        shadyMerchant.eventDescription = "수상한 상인이 제안을 한다.\n\"필요 없는 카드를 금화로 바꿔드리죠!\"";
        
        EventChoice sellCard = new EventChoice();
        sellCard.choiceText = "카드를 판다 (+30 골드)";
        sellCard.resultText = "카드를 팔고 금화를 받았다.";
        sellCard.goldChange = 30;
        sellCard.removeRandomCard = true;
        shadyMerchant.choices.Add(sellCard);
        
        EventChoice declineSale = new EventChoice();
        declineSale.choiceText = "거절한다";
        declineSale.resultText = "상인은 실망하며 떠났다.";
        shadyMerchant.choices.Add(declineSale);
        
        events.Add(shadyMerchant);
        
        Debug.Log($"이벤트 {events.Count}개 초기화 완료!");
    }
    
    public EventData GetRandomEvent()
    {
        if (events.Count == 0)
        {
            Debug.LogError("이벤트가 없습니다!");
            return null;
        }
        
        return events[Random.Range(0, events.Count)];
    }
}