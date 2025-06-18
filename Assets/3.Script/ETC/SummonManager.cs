using System.Linq;
using UnityEngine;

public class SummonManager : MonoBehaviour
{
    [Header("소환 확률(%)")]
    private float pCommon = 97.45f; //일반등급 소환 확율
    private float pRare = 1.97f; //희귀등급 소환 확율
    private float pEpic = 0.49f; //영웅등급 소환 확율
    private float pLegend = 0.10f; //전설등급 소환 확율

    [Header("영웅 옵션")]
    public HeroData[] heroDatas;

    [Header("3 × 7 슬롯 (Inspector에 모든 21개 할당)")]
    public Transform[] slots;  // 슬롯 부모 Transform 

    [Header("소환 인구 제한")]
    public int maxTotalHeroes = 25;
    private int totalSummoned = 0;

    void Awake()
    {
        // 왼쪽(작은 X) → 오른쪽, 각 컬럼 위(Y 큰) → 아래 정렬
        slots = slots
            .OrderBy(s => s.position.x)
            .ThenByDescending(s => s.position.y)
            .ToArray();
    }

    void Start()
    {
        UIManager.Instance.HeroMaxCount(totalSummoned, maxTotalHeroes);
    }

    public void SummonOnce()
    {
        // 1) 슬롯·총합 제한 체크 (기존 로직 그대로)
        if (slots.All(s => s.childCount > 0)) return;
        if (totalSummoned >= maxTotalHeroes) return;

        // 2) 확률에 따라 HeroData 선택 (기존 로직)
        float r = Random.value * 100f;
        HeroRarity rarity = r <= pCommon ? HeroRarity.Common
                          : r <= pCommon + pRare ? HeroRarity.Rare
                          : r <= pCommon + pRare + pEpic ? HeroRarity.Epic
                          : HeroRarity.Legendary;

        var pool = heroDatas.Where(d => d.rarity == rarity).ToArray();
        if (pool.Length == 0) { Debug.LogWarning($"[{rarity}] 옵션이 없습니다."); return; }
        var data = pool[Random.Range(0, pool.Length)];

        // 3) **중복 검사**: 같은 heroType·rarity 인스턴스가 있고 Count < 3 이면 IncreaseCount 호출
        foreach (var slotTr in slots)
        {
            var heroSlot = slotTr.GetComponent<HeroSlot>();
            var ctrl     = heroSlot.CurrentHero;
            if (ctrl != null
                && ctrl.HeroType  == data.heroType
                && ctrl.Rarity    == data.rarity
                && ctrl.Count     <  3)
            {
                ctrl.IncreaseCount();
                heroSlot.UpdateMergeButtonVisibility();
                
                totalSummoned++;
                UIManager.Instance.HeroMaxCount(totalSummoned, maxTotalHeroes);
                return;
            }
        }

        // 4) 중복 없으면 새로 소환
        var emptySlot = slots.First(s => s.childCount == 0);
        var go        = Instantiate(data.prefab, emptySlot.position, Quaternion.identity, emptySlot);
        var newCtrl   = go.GetComponent<HeroController>();

        // Initialize 호출 (Count=1, HeroType/Rarity 설정, child 활성화)
        newCtrl.Initialize(data);

        // HeroSlot 에 세팅해서 Merge 버튼 토글
        emptySlot.GetComponent<HeroSlot>().SetHero(newCtrl);

        totalSummoned++;
        UIManager.Instance.HeroMaxCount(totalSummoned, maxTotalHeroes);
    }
}