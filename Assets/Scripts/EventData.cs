using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class EventChoice
{
    public string choiceText;        // 선택지 텍스트
    public string resultText;        // 결과 텍스트
    public int goldChange;           // 골드 변화 (+/-)
    public int healthChangePercent;  // 체력 변화 (%)
    public int maxHealthChange;      // 최대 체력 변화
    public bool addRandomCard;       // 랜덤 카드 추가
    public bool removeRandomCard;    // 랜덤 카드 제거
    public bool transformCard;       // 카드 변환
}

[System.Serializable]
public class EventData
{
    public string eventTitle;
    public string eventDescription;
    public List<EventChoice> choices = new List<EventChoice>();
}