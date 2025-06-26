using UnityEngine;

public enum EnhanceType
{
    CommonRare,
    Hero,
    Legend,
    Probability
}

public class EnhancementManager : MonoBehaviour
{
    public static EnhancementManager Instance { get; private set; }

    [Header("시작 재화")]
    [SerializeField] private int startingCoins = 1000;
    [SerializeField] private int startingDiamonds = 50;
    private int currentCoins;
    private int currentDiamonds;
    public int CurrentCoins => currentCoins;
    public int CurrentDiamonds => currentDiamonds;

    private const int MaxLevel = 12;
    private int commonRareLevel = 1;  // 일반~희귀 공격력
    private int heroLevel = 1;  // 영웅 공격력
    private int legendLevel = 1;  // 전설~신화 공격력
    private int probabilityLevel = 1;  // 소환 확률

    // 비용 테이블 (인덱스 = Lv-1)
    private readonly int[] coinCostsCommonRare = { 30, 60, 90, 120, 150, 180, 210, 240, 270, 300, 330, 360 };
    private readonly int[] coinCostsHero = { 50, 100, 150, 200, 250, 300, 350, 400, 450, 500, 550, 600 };
    private readonly int[] diamondCostsLegend = { 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 };
    private const int coinCostProbability = 100; // 고정

    // 공격력 강화 수치 테이블 (각 레벨에서 추가되는 수치)
    private readonly float[] commonRareEnhancement = { 0.50f, 0.47f, 0.50f, 0.52f, 0.50f, 0.50f, 0.50f, 0.50f, 0.50f, 0.50f, 0.50f };
    private readonly float[] heroEnhancement = { 0.50f, 0.47f, 0.50f, 0.52f, 0.50f, 0.50f, 0.50f, 0.50f, 0.50f, 0.50f, 0.50f };
    private readonly float[] legendEnhancement = { 0.50f, 0.47f, 0.50f, 0.52f, 0.50f, 0.50f, 0.50f, 0.50f, 0.50f, 0.50f, 0.50f };

    // 소환 확률 테이블 [Lv-1][Common, Rare, Epic, Legendary]
    private readonly float[][] probabilityTable = new float[][] {
        new[] {97.45f, 1.97f, 0.49f, 0.10f},
        new[] {97.45f, 1.97f, 0.49f, 0.10f},
        new[] {90.07f, 7.23f, 2.25f, 0.45f},
        new[] {83.73f,11.74f, 3.77f, 0.75f},
        new[] {78.23f,15.66f, 5.09f, 1.02f},
        new[] {69.14f,22.14f, 7.26f, 1.45f},
        new[] {69.14f,22.14f, 7.26f, 1.45f},
        new[] {65.35f,24.85f, 8.17f, 1.63f},
        new[] {61.94f,27.27f, 8.98f, 1.80f},
        new[] {58.88f,29.46f, 9.72f, 1.94f},
        new[] {56.11f,31.44f,10.38f, 2.08f},
        new[] {53.58f,33.24f,10.98f, 2.20f},
    };

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            currentCoins = startingCoins;
            currentDiamonds = startingDiamonds;
        }
        else Destroy(gameObject);
    }

    // 재화 소모
    public bool TrySpendCoins(int amount)
    {
        if (currentCoins < amount)
            return false;

        currentCoins -= amount;

        UIManager.Instance.UpdateCoinUI(currentCoins);
        UIManager.Instance.UpdateCoinUI_1(currentCoins);

        return true;
    }

    public bool TrySpendDiamonds(int amount)
    {
        if (currentDiamonds < amount)
            return false;

        currentDiamonds -= amount;

        UIManager.Instance.UpdateDiamondUI(currentDiamonds);

        return true;
    }

    // 레벨 / 비용 조회
    public int GetEnhanceLevel(EnhanceType type)
    {
        return type switch
        {
            EnhanceType.CommonRare => commonRareLevel,
            EnhanceType.Hero => heroLevel,
            EnhanceType.Legend => legendLevel,
            EnhanceType.Probability => probabilityLevel,
            _ => 1
        };
    }

    public int GetNextCost(EnhanceType type)
    {
        int lvl = GetEnhanceLevel(type);

        if (lvl >= MaxLevel) return -1;
        return type switch
        {
            EnhanceType.CommonRare => coinCostsCommonRare[lvl - 1],
            EnhanceType.Hero => coinCostsHero[lvl - 1],
            EnhanceType.Legend => diamondCostsLegend[lvl - 1],
            EnhanceType.Probability => coinCostProbability,
            _ => 0
        };
    }

    // 강 화 실행
    // EnhancementManager.cs의 Enhance 메서드를 찾아서 이렇게 수정하세요:

    public void Enhance(EnhanceType type)
    {
        int lvl = GetEnhanceLevel(type);

        // 이미 최대 레벨인 경우
        if (lvl >= MaxLevel)
        {
            // 최대 레벨 사운드 재생
            if (SoundManager.Instance != null)
                SoundManager.Instance.PlayEnhanceMax();

            Debug.LogWarning($"{type} 강화는 이미 최대 레벨입니다.");
            return;
        }

        // 재화 소모 시도
        bool ok = type switch
        {
            EnhanceType.CommonRare => TrySpendCoins(GetNextCost(type)),
            EnhanceType.Hero => TrySpendCoins(GetNextCost(type)),
            EnhanceType.Legend => TrySpendDiamonds(GetNextCost(type)),
            EnhanceType.Probability => TrySpendCoins(GetNextCost(type)),
            _ => false
        };

        if (!ok)
        {
            Debug.LogWarning("강화에 필요한 재화가 부족합니다.");
            return;
        }

        // 레벨업
        switch (type)
        {
            case EnhanceType.CommonRare: commonRareLevel++; break;
            case EnhanceType.Hero: heroLevel++; break;
            case EnhanceType.Legend: legendLevel++; break;
            case EnhanceType.Probability: probabilityLevel++; break;
        }

        // 강화 성공 사운드 재생
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayEnhanceSuccess();

        // 디버그 로그 추가
        Debug.Log($"강화 완료: {type}, 새 레벨: {GetEnhanceLevel(type)}");

        // UI 갱신
        UIManager.Instance.UpdateEnhancementUI();

        // 강화 후 최대 레벨에 도달했는지 확인
        if (GetEnhanceLevel(type) >= MaxLevel)
        {
            // 최대 레벨 도달 시 특별 사운드 재생
            if (SoundManager.Instance != null)
                SoundManager.Instance.PlayEnhanceMax();

            Debug.Log($"{type} 강화가 최대 레벨에 도달했습니다!");
        }
    }

    // 누적 강화 데미지 배율 계산
    public float GetDamageMultiplier(HeroRarity rarity)
    {
        float[] enhancementArray;
        int level;

        // 등급에 따른 강화 배열과 레벨 선택
        switch (rarity)
        {
            case HeroRarity.Common:
            case HeroRarity.Rare:
                enhancementArray = commonRareEnhancement;
                level = commonRareLevel;
                break;
            case HeroRarity.Epic:
                enhancementArray = heroEnhancement;
                level = heroLevel;
                break;
            case HeroRarity.Legendary:
            case HeroRarity.Mythic:
                enhancementArray = legendEnhancement;
                level = legendLevel;
                break;
            default:
                return 1f;
        }

        // 기본 배율 1.0에서 시작하여 누적 계산
        float totalMultiplier = 1f;

        // 현재 레벨-1까지의 모든 강화 수치를 누적
        for (int i = 0; i < level - 1 && i < enhancementArray.Length; i++)
        {
            totalMultiplier += enhancementArray[i];
        }

        // 디버그 로그 추가
        //Debug.Log($"누적 데미지 배율 계산: {rarity} Lv{level} = {totalMultiplier:F2}배");

        return totalMultiplier;
    }

    public float[] GetCurrentProbability()
        => probabilityTable[probabilityLevel - 1];

    // 강화 레벨 초기화 (게임 재시작 시 호출)
    public void ResetEnhancements()
    {
        commonRareLevel = 1;
        heroLevel = 1;
        legendLevel = 1;
        probabilityLevel = 1;
        UIManager.Instance.UpdateEnhancementUI();
        //Debug.Log("강화 레벨이 초기화되었습니다.");
    }

    // 코인 보상용
    public void AddCoins(int amount)
    {
        currentCoins += amount;
        // 코인 UI 두 곳 모두 업데이트
        UIManager.Instance.UpdateCoinUI(currentCoins);
        UIManager.Instance.UpdateCoinUI_1(currentCoins);
    }

    // 재화를 초기값으로 리셋
    public void ResetCurrency()
    {
        currentCoins = startingCoins;
        currentDiamonds = startingDiamonds;

        // UI 업데이트
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateCoinUI(currentCoins);
            UIManager.Instance.UpdateCoinUI_1(currentCoins);
            UIManager.Instance.UpdateDiamondUI(currentDiamonds);
        }

        Debug.Log($"재화 리셋 완료 - 코인: {currentCoins}, 다이아: {currentDiamonds}");
    }

    // 게임 완전 리셋 (강화 레벨 + 재화)
    public void CompleteReset()
    {
        ResetEnhancements();
        ResetCurrency();
    }
}