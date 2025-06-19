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

        // 1) 병합 전 인구 변경
        SummonManager.Instance.ChangePopulation(-2);

        // 2) 원래 슬롯 비우기
        slot.ClearHero();

        // 3) 다음 등급 결정
        HeroRarity nextRarity;
        if (oldHero.Rarity == HeroRarity.Legendary)
        {
            SpawnMythic(slot, oldHero.HeroType);
            return;
        }
        nextRarity = (HeroRarity)((int)oldHero.Rarity + 1);

        // 4) 후보 풀에서 랜덤 선택
        var candidates = heroDatas
            .Where(d => d.rarity == nextRarity && d.heroType == oldHero.HeroType)
            .ToArray();
        if (candidates.Length == 0)
        {
            Debug.LogWarning($"[{nextRarity}] 매칭되는 HeroData 없음");
            return;
        }
        var newData = candidates[Random.Range(0, candidates.Length)];

        // ★ 5) “같은 newData”가 있는 곳 먼저 찾아 뭉치기
        var existingSlot = slots
            .Select(s => s.GetComponent<HeroSlot>())
            .FirstOrDefault(hs =>
                hs.CurrentHero != null &&
                hs.CurrentHero.Data == newData &&   // 정확히 같은 SO
                hs.CurrentHero.Count < 3
            );
        if (existingSlot != null)
        {
            existingSlot.CurrentHero.IncreaseCount();
            existingSlot.UpdateMergeButtonVisibility();
            return;
        }

        // 6) 없다면 원래 자리(slot)에 새로 생성
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

        // // ── (A) 인구 -2 처리 ───────────────────────
        // SummonManager.Instance.ChangePopulation(-2);

        // // ── (B) 기존 컨테이너(3개 모델) 삭제 ─────────
        // slot.ClearHero();

        // // ── (C) 다음 등급 결정 & 신화 분기 ────────────
        // HeroRarity nextRarity;
        // if (oldHero.Rarity == HeroRarity.Legendary)
        // {
        //     SpawnMythic(slot, oldHero.HeroType);
        //     return;
        // }
        // else
        // {
        //     nextRarity = (HeroRarity)((int)oldHero.Rarity + 1);
        // }

        // // ── (D) 새 등급 랜덤 Instantiate ─────────────
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
