using UnityEngine;
using System.Collections.Generic;

public enum RelicEffectType
{
    BattleStartDraw,
    FirstTurnEnergy,
    GoldOnKill
}

[System.Serializable]
public class RelicDefinition
{
    public string id;
    public string name;
    public string desc;
    public RelicEffectType effect;
    public int value;

    public RelicDefinition(string id, string name, string desc, RelicEffectType effect, int value)
    {
        this.id = id;
        this.name = name;
        this.desc = desc;
        this.effect = effect;
        this.value = value;
    }
}

public class RelicManager : MonoBehaviour
{
    public static RelicManager Instance;

    // MVP 유물 3개
    private readonly List<RelicDefinition> allRelics = new List<RelicDefinition>()
    {
        new RelicDefinition("relic_draw_1", "낡은 부적", "첫 턴에 카드 1장 추가로 뽑는다.", RelicEffectType.BattleStartDraw, 1),
        new RelicDefinition("relic_energy_1", "파열의 구슬", "첫 턴에 에너지 +1", RelicEffectType.FirstTurnEnergy, 1),
        new RelicDefinition("relic_gold_kill_3", "탐욕의 동전", "적 처치 시 골드 +3", RelicEffectType.GoldOnKill, 3),
    };

    private HashSet<string> owned = new HashSet<string>();

    private bool firstTurnPending = false;
    private int pendingExtraDraw = 0;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        SyncFromRunData();
    }

    public void SyncFromRunData()
    {
        owned.Clear();
        if (RunData.Instance != null && RunData.Instance.ownedRelics != null)
        {
            foreach (var id in RunData.Instance.ownedRelics)
                owned.Add(id);
        }
    }

    void SaveToRunData()
    {
        if (RunData.Instance == null) return;

        if (RunData.Instance.ownedRelics == null)
            RunData.Instance.ownedRelics = new List<string>();

        RunData.Instance.ownedRelics.Clear();
        RunData.Instance.ownedRelics.AddRange(owned);
    }

    public RelicDefinition GetRelic(string id) => allRelics.Find(r => r.id == id);

    public RelicDefinition GetRandomRelicNotOwned()
    {
        var pool = new List<RelicDefinition>();
        foreach (var r in allRelics)
            if (!owned.Contains(r.id)) pool.Add(r);

        if (pool.Count == 0) return null;
        return pool[Random.Range(0, pool.Count)];
    }

    public bool AddRelic(string id)
    {
        if (string.IsNullOrEmpty(id)) return false;
        if (owned.Contains(id)) return false;

        owned.Add(id);
        SaveToRunData();

        var r = GetRelic(id);
        Debug.Log(r != null ? $"[Relic] 획득: {r.name} / {r.desc}" : $"[Relic] 획득: {id}");
        return true;
    }

    // ===== 트리거 =====
    public void OnBattleStart()
    {
        firstTurnPending = true;
        pendingExtraDraw = 0;

        foreach (var id in owned)
        {
            var r = GetRelic(id);
            if (r == null) continue;

            if (r.effect == RelicEffectType.BattleStartDraw)
                pendingExtraDraw += r.value;
        }
    }

    // StartPlayerTurn에서 호출(첫 턴만 적용)
    public void OnPlayerTurnStart()
    {
        if (!firstTurnPending) return;
        firstTurnPending = false;

        int extraEnergy = 0;
        foreach (var id in owned)
        {
            var r = GetRelic(id);
            if (r == null) continue;

            if (r.effect == RelicEffectType.FirstTurnEnergy)
                extraEnergy += r.value;
        }

        if (extraEnergy > 0 && BattleManager.Instance != null)
        {
            BattleManager.Instance.currentEnergy += extraEnergy;
            Debug.Log($"[Relic] 첫 턴 에너지 +{extraEnergy}");
        }

        if (pendingExtraDraw > 0 && CardManager.Instance != null)
        {
            CardManager.Instance.DrawCards(pendingExtraDraw);
            Debug.Log($"[Relic] 첫 턴 추가 드로우 +{pendingExtraDraw}");
            pendingExtraDraw = 0;
        }
    }

    public void OnEnemyKilled()
    {
        int goldGain = 0;
        foreach (var id in owned)
        {
            var r = GetRelic(id);
            if (r == null) continue;

            if (r.effect == RelicEffectType.GoldOnKill)
                goldGain += r.value;
        }

        if (goldGain > 0 && GameData.Instance != null)
        {
            GameData.Instance.gold += goldGain;
            Debug.Log($"[Relic] 처치 골드 +{goldGain} (현재 {GameData.Instance.gold})");
        }
    }
}
