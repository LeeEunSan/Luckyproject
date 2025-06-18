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
        else Destroy(this);
    }

    // HeroSlot에서 Count==3인 슬롯의 Merge 버튼 클릭 시 이 메서드 호출
    // HeroSlot에서 호출: 해당 슬롯만 병합 처리
    public void MergeSlot(HeroSlot slot)
    {
        // 1. 슬롯에 영웅이 없거나 Count<3 이면 아무것도 안 함
        HeroController oldHero = slot.CurrentHero;
        if (oldHero == null || oldHero.Count < 3)
            return;

        // 2. 기존 컨테이너(3개 모델) 삭제
        slot.ClearHero();

        // 3. 다음 등급 계산
        HeroRarity nextRarity;
        if (oldHero.Rarity == HeroRarity.Legendary)
        {
            // 3-1. Legendary → Mythic 등급은 별도 로직
            SpawnMythic(slot, oldHero.HeroType);
            return;
        }
        else
        {
            // 3-2. Common→Rare→Epic→Legendary 순서로 +1
            nextRarity = (HeroRarity)((int)oldHero.Rarity + 1);
        }

        // 4. 풀에서 같은 heroType & nextRarity 데이터 목록 가져오기
        var candidates = heroDatas
            .Where(d => d.rarity == nextRarity
                     && d.heroType == oldHero.HeroType)
            .ToArray();

        if (candidates.Length == 0)
        {
            Debug.LogWarning($"[{nextRarity}] 등급에 해당하는 HeroData가 없습니다.");
            return;
        }

        // 5. 목록 중 랜덤 선택
        HeroData newData = candidates[Random.Range(0, candidates.Length)];

        // 6. 선택된 프리팹 Instantiate
        GameObject go = Instantiate(
            newData.prefab,
            slot.SpawnPoint.position,
            Quaternion.identity,
            slot.SpawnPoint   // 슬롯 하위로 붙이기
        );

        // 7. HeroController 초기화 (Count=1, child 활성화)
        HeroController newHero = go.GetComponent<HeroController>();
        newHero.Initialize(newData);

        // 8. 슬롯에 새 영웅 세팅 & Merge 버튼 토글
        slot.SetHero(newHero);

        //     var oldHero = slot.CurrentHero;
        //     if (oldHero == null || oldHero.Count < 3) return;

        //     // 1) 삭제
        //     slot.ClearHero();

        //     // 2) 다음 등급 계산
        //     HeroRarity nextRarity;
        //     if (oldHero.Rarity == HeroRarity.Legendary)
        //     {
        //         // TODO: 신화 등급 커스텀 로직 here
        //         SpawnMythic(slot, oldHero.HeroType);
        //         return;
        //     }
        //     else
        //     {
        //         nextRarity = (HeroRarity)((int)oldHero.Rarity + 1);
        //     }

        //     // 3) 풀에서 랜덤 선택 (같은 heroType & nextRarity)
        //     var pool = heroDatas
        //         .Where(d => d.rarity == nextRarity && d.heroType == oldHero.HeroType)
        //         .ToArray();

        //     if (pool.Length == 0)
        //     {
        //         Debug.LogWarning($"[{nextRarity}] 등급 옵션이 없습니다.");
        //         return;
        //     }

        //     var data = pool[Random.Range(0, pool.Length)];

        //     // 4) 생성 및 초기화
        //     var go = Instantiate(data.prefab, slot.SpawnPoint.position, Quaternion.identity);
        //     var ctrl = go.GetComponent<HeroController>();
        //     ctrl.Initialize(data);

        //     // 5) 슬롯에 세팅
        //     slot.SetHero(ctrl);
        // }

        // // 신화 등급 전용 스폰 (예정)
        // private void SpawnMythic(HeroSlot slot, HeroType type)
        // {
        //     // TODO: 타입별 커스텀 로직
        //     Debug.Log($"신화 등급 소환 로직 실행 (타입: {type})");
    }
    
    // Legendary → Mythic 전용 커스텀 소환 로직
    private void SpawnMythic(HeroSlot slot, HeroType type)
    {
        // TODO: 타입별/조합별 고유 신화 영웅 결정 로직 구현
        Debug.Log($"신화 등급 소환 (타입: {type})");
    }
}
