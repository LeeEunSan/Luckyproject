using System.Linq;
using UnityEngine;

public class SummonManager : MonoBehaviour
{
    public static SummonManager Instance { get; private set; }

    [Header("영웅 데이터 (ScriptableObjects)")]
    public HeroData[] heroDatas;

    [Header("소환 슬롯 (Transform[])")]
    public Transform[] slots;

    [Header("소환 확률 (%)")]
    [Range(0, 100)] public float pCommon = 50f;
    [Range(0, 100)] public float pRare = 30f;
    [Range(0, 100)] public float pEpic = 15f;
    // Legendary 확률 = 100 - (pCommon + pRare + pEpic)

    [Header("소환 제한")]
    public int maxTotalHeroes = 25;
    private int totalSummoned;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // 기존 초기화 로직이 있으면 그 아래에
    }

    public void SummonOnce()
    {
        // 0) 슬롯·총합 제한 검사
        // 빈 칸(slot.childCount == 0)이 하나도 없거나
        // 이미 총 소환 횟수가 max를 넘으면 중단
        if (slots.All(s => s.childCount > 0)) return;
        if (totalSummoned >= maxTotalHeroes)
        {
            UIManager.Instance.SpawnedMax();
            //UIManager.Instance.SpawnedMax_1();
            return;
        }

        // 1) 랜덤 등급 결정
        float r = Random.value * 100f;
        HeroRarity chosenRarity =
            r <= pCommon ? HeroRarity.Common :
            r <= pCommon + pRare ? HeroRarity.Rare :
            r <= pCommon + pRare + pEpic ? HeroRarity.Epic :
            HeroRarity.Legendary;

              

        // 2) 해당 등급 HeroData 풀에서 랜덤 선택
        var pool = heroDatas.Where(d => d.rarity == chosenRarity).ToArray();
        if (pool.Length == 0)
        {
            Debug.LogWarning($"[{chosenRarity}] 등급 데이터 없음");
            return;
        }
        var data = pool[Random.Range(0, pool.Length)];

        // 3) 뭉치기 검사: 같은 타입·등급 컨테이너 찾기
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

        // 4) 빈 슬롯 찾아 새 컨테이너 소환
        var empty = slots.First(s => s.childCount == 0);
        var go = Instantiate(
            data.prefab,
            empty.position,
            Quaternion.identity,
            empty
        );

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
}
