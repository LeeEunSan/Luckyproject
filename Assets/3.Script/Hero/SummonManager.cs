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
    public int maxTotalHeroes = 21;
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
        // 1) 슬롯이 모두 찼거나 총합 한도 초과 시
        if (slots.All(s => s.childCount > 0))
        {
            //UIManager.Instance.ShowMessage("모든 슬롯이 가득 찼습니다.");
            return;
        }
        if (totalSummoned >= maxTotalHeroes)
        {
            //UIManager.Instance.ShowMessage($"총 {maxTotalHeroes}개의 영웅을 모두 소환했습니다.");
            return;
        }

        // 2) 확률 결정
        float r = Random.value * 100f;
        HeroRarity rarity = r <= pCommon ? HeroRarity.Common : r <= pCommon + pRare ? HeroRarity.Rare
                : r <= pCommon + pRare + pEpic ? HeroRarity.Epic : HeroRarity.Legendary;

        // 3) 해당 등급 풀에서 랜덤 선택
        var pool = heroDatas.Where(t => t.rarity == rarity).ToArray();
        if (pool.Length == 0)
        {
            Debug.LogWarning($"[{rarity}] 등급 옵션이 없습니다.");
            return;
        }
        var data = pool[Random.Range(0, pool.Length)];

        // 4) 빈 슬롯 중 컬럼 우선으로 첫 번째 빈 슬롯에 배치
        var slot = slots.First(s => s.childCount == 0);
        Instantiate(data.prefab, slot.position, Quaternion.identity, slot);

        totalSummoned++;
        UIManager.Instance.HeroMaxCount(totalSummoned, maxTotalHeroes);
    }
}

