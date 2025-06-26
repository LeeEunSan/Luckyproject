using System.Linq;
using UnityEngine;

public class SummonManager : MonoBehaviour
{
    public static SummonManager Instance { get; private set; }

    [Header("Player Currency")]
    [SerializeField] private int startingDiamonds = 10;  // 인스펙터에서 초기값 설정
    private int currentDiamonds;
    public int CurrentDiamonds => currentDiamonds;

    [Header("영웅 데이터 (ScriptableObjects)")]
    public HeroData[] heroDatas;

    [Header("소환 슬롯 (Transform[])")]
    public Transform[] slots;

    [Header("소환 제한")]
    public int maxTotalHeroes = 25;
    private int totalSummoned;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // 다이아 초기화
        currentDiamonds = startingDiamonds;
    }

    public void SummonOnce()
    {
        // 0) 슬롯·총합 제한 검사
        if (slots.All(s => s.childCount > 0)) return;
        if (totalSummoned >= maxTotalHeroes)
        {
            UIManager.Instance.SpawnedMax();
            return;
        }

        // 1) EnhancementManager에서 현재 확률 가져오기
        float[] currentProb = EnhancementManager.Instance.GetCurrentProbability();
        float pCommon = currentProb[0];  // Common 확률
        float pRare = currentProb[1];    // Rare 확률  
        float pEpic = currentProb[2];    // Epic 확률
        float pLegendary = currentProb[3]; // Legendary 확률

        // 2) 랜덤 등급 결정 (누적 확률 방식)
        float r = Random.value * 100f;
        HeroRarity chosenRarity;

        if (r <= pCommon)
            chosenRarity = HeroRarity.Common;
        else if (r <= pCommon + pRare)
            chosenRarity = HeroRarity.Rare;
        else if (r <= pCommon + pRare + pEpic)
            chosenRarity = HeroRarity.Epic;
        else
            chosenRarity = HeroRarity.Legendary;

        //Debug.Log($"소환 확률 적용: {r:F2}% → {chosenRarity} (C:{pCommon:F2}% R:{pRare:F2}% E:{pEpic:F2}% L:{pLegendary:F2}%)");

        // 3) 해당 등급 HeroData 풀에서 랜덤 선택
        var pool = heroDatas.Where(d => d.rarity == chosenRarity).ToArray();
        if (pool.Length == 0)
        {
            Debug.LogWarning($"[{chosenRarity}] 등급 데이터 없음");
            return;
        }
        var data = pool[Random.Range(0, pool.Length)];

        // 4) 뭉치기 검사: 같은 타입·등급 컨테이너 찾기
        var matchingSlot = slots
        .Select(s => s.GetComponent<HeroSlot>())
        .FirstOrDefault(hs =>
            hs.CurrentHero != null &&
            hs.CurrentHero.Data == data &&      // ← HeroData 동일성 비교
            hs.CurrentHero.Count < 3
        );

        if (matchingSlot != null)
        {
            // 있으면 IncreaseCount → 버튼 갱신 → 리턴
            matchingSlot.CurrentHero.IncreaseCount();
            matchingSlot.UpdateMergeButtonVisibility();
            totalSummoned++;
            UIManager.Instance.HeroMaxCount(totalSummoned, maxTotalHeroes);
            return;
        }

        // 5) 빈 슬롯 찾아 새 컨테이너 소환
        var empty = slots.First(s => s.childCount == 0);
        var go = Instantiate(
            data.prefab,
            empty.position,
            Quaternion.identity,
            empty
        );

        var attackCtrl = go.GetComponent<HeroAttackController>();
        attackCtrl.Initialize(data);

        // 초기화 후 슬롯에 세팅
        var ctrl = go.GetComponent<HeroController>();
        ctrl.Initialize(data);
        empty.GetComponent<HeroSlot>().SetHero(ctrl);

        totalSummoned++;
        UIManager.Instance.HeroMaxCount(totalSummoned, maxTotalHeroes);
    }

    // 인구(현재 필드에 남아 있는 영웅 수)를 delta만큼 조정합니다.
    public void ChangePopulation(int delta)
    {
        totalSummoned = Mathf.Clamp(totalSummoned + delta, 0, maxTotalHeroes);
        // UI 업데이트 호출 (필요하다면)
        UIManager.Instance.HeroMaxCount(totalSummoned, maxTotalHeroes);
    }

    // 다이아가 충분하면 소모하고 true, 아니면 false 반환
    private bool SpendDiamonds(int amount)
    {
        if (currentDiamonds < amount) return false;
        currentDiamonds -= amount;
        UIManager.Instance.UpdateDiamondUI(currentDiamonds);
        return true;
    }

    // 공통 뽑기 로직: 확률(chance%)로 성공 시 해당 등급 뽑기
    private void TryGacha(HeroRarity rarity, float chance, int cost)
    {
        // 1) 재화 확인
        if (!SpendDiamonds(cost))
        {
            Debug.LogWarning("다이아가 부족합니다.");
            return;
        }

        // 2) 확률 판정
        if (Random.value * 100f > chance)
        {
            UIManager.Instance.ShowSummonFailed();
            return;
        }

        // 3) 성공하면 실제 소환 (총합·슬롯 제한 등 내부 로직 재사용)
        SummonByRarity(rarity);
    }

    // 성공 판정 후 호출: 지정 등급으로 실제 소환 처리
    public void SummonByRarity(HeroRarity rarity)
    {
        // 기존 SummonOnce의 "등급 결정"만 대체하고 나머지 동일하게 재사용
        if (slots.All(s => s.childCount > 0) || totalSummoned >= maxTotalHeroes)
        {
            UIManager.Instance.SpawnedMax();
            return;
        }

        var pool = heroDatas.Where(d => d.rarity == rarity).ToArray();
        if (pool.Length == 0)
        {
            Debug.LogWarning($"[{rarity}] 등급 데이터 없음");
            return;
        }
        var data = pool[Random.Range(0, pool.Length)];

        // 뭉치기(merge) 검사
        var matchingSlot = slots
            .Select(s => s.GetComponent<HeroSlot>())
            .FirstOrDefault(hs => hs.CurrentHero != null
                && hs.CurrentHero.Data == data
                && hs.CurrentHero.Count < 3);

        if (matchingSlot != null)
        {
            matchingSlot.CurrentHero.IncreaseCount();
            matchingSlot.UpdateMergeButtonVisibility();
        }
        else
        {
            var empty = slots.First(s => s.childCount == 0);
            var go = Instantiate(data.prefab, empty.position, Quaternion.identity, empty);
            var attackCtrl = go.GetComponent<HeroAttackController>();
            attackCtrl.Initialize(data);
            var ctrl = go.GetComponent<HeroController>();
            ctrl.Initialize(data);
            empty.GetComponent<HeroSlot>().SetHero(ctrl);
        }

        // 4) 인구(총 소환수) 증가
        ChangePopulation(+1);
    }

    // 버튼에서 직접 호출할 메서드들
    public void SummonRare() => TryGacha(HeroRarity.Rare, 60f, 1);
    public void SummonEpic() => TryGacha(HeroRarity.Epic, 20f, 1);
    public void SummonLegendary() => TryGacha(HeroRarity.Legendary, 10f, 2);

    // 무료 가차: 비용 없이 60%/20%/10% 확률로 소환
    public void FreeSummon()
    {
        float roll = Random.value * 100f;
        if (roll < 60f) SummonByRarity(HeroRarity.Rare);
        else if (roll < 80f) SummonByRarity(HeroRarity.Epic);
        else if (roll < 90f) SummonByRarity(HeroRarity.Legendary);
        else UIManager.Instance.ShowSummonFailed();
    }

    // 보석 보상용
    public void AddDiamonds(int amount)
    {
        currentDiamonds += amount;
        UIManager.Instance.UpdateDiamondUI(currentDiamonds);
    }
    
    // 특정 등급으로 무료 소환
    public void FreeSummonByRarity(HeroRarity rarity)
    {
        SummonByRarity(rarity);
    }
}