using System.Linq;
using UnityEngine;

public class MergeManager : MonoBehaviour
{
    public static MergeManager Instance { get; private set; }

    [Header("영웅 데이터")]
    public HeroData[] heroDatas;

    [Header("소환 좌표 Transforms (SummonManager와 동일)")]
    public Transform[] slots;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // HeroSlot의 MergeButton 클릭 시 호출
    public void MergeSlot(HeroSlot slot)
    {
        var oldHero = slot.CurrentHero;
    if (oldHero == null || oldHero.Count < 3)
        return;

    // ── (A) 인구 -2 처리 ───────────────────────
    SummonManager.Instance.ChangePopulation(-2);

    // ── (B) 기존 컨테이너(3개 모델) 삭제 ─────────
    slot.ClearHero();

    // ── (C) 다음 등급 결정 & 신화 분기 ────────────
    HeroRarity nextRarity;
    if (oldHero.Rarity == HeroRarity.Legendary)
    {
        SpawnMythic(slot, oldHero.HeroType);
        return;
    }
    else
    {
        nextRarity = (HeroRarity)((int)oldHero.Rarity + 1);
    }

    // ── (D) 새 등급 랜덤 Instantiate ─────────────
    var candidates = heroDatas
        .Where(d => d.rarity == nextRarity && d.heroType == oldHero.HeroType)
        .ToArray();

    var newData = candidates[Random.Range(0, candidates.Length)];
    var go = Instantiate(
        newData.prefab,
        slot.SpawnPoint.position,
        Quaternion.identity,
        slot.SpawnPoint
    );

    var newHero = go.GetComponent<HeroController>();
    newHero.Initialize(newData);
    slot.SetHero(newHero);

        // var oldHero = slot.CurrentHero;
        // if (oldHero == null || oldHero.Count < 3)
        //     return;

        // // 1) 기존 컨테이너 삭제
        // slot.ClearHero();

        // // 2) 다음 등급 결정
        // HeroRarity nextRarity;
        // if (oldHero.Rarity == HeroRarity.Legendary)
        // {
        //     SpawnMythic(slot, oldHero.HeroType);
        //     return;
        // }
        // nextRarity = (HeroRarity)((int)oldHero.Rarity + 1);

        // // 3) 같은 heroType & nextRarity 인스턴스 중 Count<3 인 게 있는지 검색
        // foreach (var s in slots)
        // {
        //     var hs = s.GetComponent<HeroSlot>();
        //     var hc = hs.CurrentHero;
        //     if (hc != null
        //         && hc.HeroType == oldHero.HeroType
        //         && hc.Rarity == nextRarity
        //         && hc.Count < 3)
        //     {
        //         // 이미 존재하는 컨테이너에 뭉치기
        //         hc.IncreaseCount();
        //         hs.UpdateMergeButtonVisibility();
        //         return;
        //     }
        // }

        // // 4) 없으면 새로 소환 (기존 코드)
        // var candidates = heroDatas
        //     .Where(d => d.rarity == nextRarity && d.heroType == oldHero.HeroType)
        //     .ToArray();

        // var newData = candidates[Random.Range(0, candidates.Length)];
        // var go = Instantiate(
        //     newData.prefab,
        //     slot.SpawnPoint.position,
        //     Quaternion.identity,
        //     slot.SpawnPoint
        // );

        // var newHero = go.GetComponent<HeroController>();
        // newHero.Initialize(newData);
        // slot.SetHero(newHero);
    }

    // Legendary → Mythic 전용 커스텀 소환 로직
    private void SpawnMythic(HeroSlot slot, HeroType type)
    {
        // TODO: 타입별/조합별 고유 신화 영웅 결정 로직 구현
        Debug.Log($"신화 등급 소환 (타입: {type})");
    }
}
