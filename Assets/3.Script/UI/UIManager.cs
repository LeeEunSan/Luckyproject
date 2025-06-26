using UnityEngine;
using TMPro;
using UnityEngine.UI;
using MoreMountains.Feedbacks;
using System.Collections.Generic;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI 텍스트")]
    public TextMeshProUGUI waveText; // 현재 웨이브
    public TextMeshProUGUI spawnCountText; //스폰된 수.
    public TextMeshProUGUI waveTime; //웨이브 시간.
    public TextMeshProUGUI NextWaveCountDown; //다음 웨이브 카운트다운
    public TextMeshProUGUI HeroCount;
    public TextMeshProUGUI HeroCount1;

    [Header("Info 텍스트")]
    public MMF_Player Feedback_1;
    public MMF_Player Feedback_2;

    [Header("Spawn 카운트 슬라이더")]
    public Slider spawnSlider;

    [Header("영웅 정보 창")]
    [SerializeField] private GameObject heroInfoPanel;
    [SerializeField] private Image infoSpriteImage; // Sprite 대신 Image 컴포넌트 사용
    [SerializeField] private TextMeshProUGUI infoNameText;
    [SerializeField] private TextMeshProUGUI infoTribeText;
    [SerializeField] private TextMeshProUGUI infoDamageText;
    [SerializeField] private TextMeshProUGUI infoAttackSpeedText;
    [SerializeField] private TextMeshProUGUI infoskillNameText;
    [SerializeField] private TextMeshProUGUI infoskillTypeText;
    [SerializeField] private TextMeshProUGUI infoskillInfoText;
    [SerializeField] private TextMeshProUGUI infoEnhanceText;

    [Header("재화 UI")]
    [SerializeField] private TextMeshProUGUI diamondText; // 다이아 잔량 표시
    [SerializeField] private TextMeshProUGUI diamondText_1;
    [SerializeField] private TextMeshProUGUI diamondText_2;
    [SerializeField] private TextMeshProUGUI coinText;
    [SerializeField] private TextMeshProUGUI coinText_1;

    [Header("가챠 버튼")]
    [SerializeField] private Button rareBtn1;
    [SerializeField] private Button rareBtn2;
    [SerializeField] private Button epicBtn1;
    [SerializeField] private Button epicBtn2;
    [SerializeField] private Button legendaryBtn1;
    [SerializeField] private Button legendaryBtn2;

    [Header("특별 몬스터 소환")]
    [SerializeField] private Button specialSpawnBtn;

    [SerializeField] private TextMeshProUGUI waveStartText; // 새로 연결할 텍스트 오브젝트
    [SerializeField] private MMF_Player WaveStartFB;

    [Header("강화 버튼")]
    [SerializeField] private Button commonRareBtn;
    [SerializeField] private Button heroBtn;
    [SerializeField] private Button legendBtn;
    [SerializeField] private Button probBtn;

    [Header("레벨 & 비용")]
    [SerializeField] private TextMeshProUGUI commonRareLvText;
    [SerializeField] private TextMeshProUGUI commonRareCostText;
    [SerializeField] private TextMeshProUGUI heroLvText;
    [SerializeField] private TextMeshProUGUI heroCostText;
    [SerializeField] private TextMeshProUGUI legendLvText;
    [SerializeField] private TextMeshProUGUI legendCostText;
    [SerializeField] private TextMeshProUGUI probLvText;
    [SerializeField] private TextMeshProUGUI probCostText;

    [Header("확률 값")]
    [SerializeField] private TextMeshProUGUI probCommonText;
    [SerializeField] private TextMeshProUGUI probRareText;
    [SerializeField] private TextMeshProUGUI probEpicText;
    [SerializeField] private TextMeshProUGUI probLegendText;

    [Header("보스 무료 가차 패널")]
    [SerializeField] private GameObject freeSummonPanel;
    [SerializeField] private TextMeshProUGUI freeCountdownText;
    [SerializeField] private Button freeRareBtn, freeEpicBtn, freeLegendBtn;

    private bool freeSummonSelected = false;
    private Coroutine freeSummonRoutine;

    // 현재 화면에 띄운 데이터 체크용
    private HeroData currentInfoData;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);

        // 초기 UI 리셋
        if (spawnSlider != null) spawnSlider.value = 0;

        // 초기에는 Hero Info Panel 및 이미지 비활성화
        if (heroInfoPanel != null) heroInfoPanel.SetActive(false);
        if (infoSpriteImage != null) infoSpriteImage.gameObject.SetActive(false);

        // 다이아 초기 표시
        if (diamondText != null)
            diamondText.text = SummonManager.Instance.CurrentDiamonds.ToString();
        if (diamondText_1 != null)
            diamondText_1.text = SummonManager.Instance.CurrentDiamonds.ToString();
        if (diamondText_2 != null)
            diamondText_2.text = SummonManager.Instance.CurrentDiamonds.ToString();

        // 뽑기 버튼 리스너 연결
        if (rareBtn1 != null) rareBtn1.onClick.AddListener(() => SummonManager.Instance.SummonRare());
        if (rareBtn2 != null) rareBtn2.onClick.AddListener(() => SummonManager.Instance.SummonRare());
        if (epicBtn1 != null) epicBtn1.onClick.AddListener(() => SummonManager.Instance.SummonEpic());
        if (epicBtn2 != null) epicBtn2.onClick.AddListener(() => SummonManager.Instance.SummonEpic());
        if (legendaryBtn1 != null) legendaryBtn1.onClick.AddListener(() => SummonManager.Instance.SummonLegendary());
        if (legendaryBtn2 != null) legendaryBtn2.onClick.AddListener(() => SummonManager.Instance.SummonLegendary());

        // 초기 화폐 UI
        UpdateCoinUI(EnhancementManager.Instance.CurrentCoins);
        UpdateCoinUI_1(EnhancementManager.Instance.CurrentCoins);
        UpdateDiamondUI(EnhancementManager.Instance.CurrentDiamonds);

        // 강화 버튼들
        commonRareBtn.onClick.AddListener(() => EnhancementManager.Instance.Enhance(EnhanceType.CommonRare));
        heroBtn.onClick.AddListener(() => EnhancementManager.Instance.Enhance(EnhanceType.Hero));
        legendBtn.onClick.AddListener(() => EnhancementManager.Instance.Enhance(EnhanceType.Legend));
        probBtn.onClick.AddListener(() => EnhancementManager.Instance.Enhance(EnhanceType.Probability));

        // 초기 강화 UI 렌더
        UpdateEnhancementUI();

        // 특별 소환 버튼 초기 리스너 & 숨김
        if (specialSpawnBtn != null)
        {
            specialSpawnBtn.onClick.AddListener(() => WaveManager.Instance.SpawnSpecialEnemy());
            specialSpawnBtn.gameObject.SetActive(false);
        }
    }

    // 현재 웨이브를 보여줍니다.
    public void UpdateWave(int current)
    {
        if (waveText != null)
            waveText.text = $"WAVE {current}";
    }

    // 스폰 카운트를 업데이트합니다.
    public void UpdateAliveCount(int aliveCount)
    {
        int max = 100;
        if (spawnCountText != null)
            spawnCountText.text = $"{aliveCount} / {max}";
        if (spawnSlider != null)
            spawnSlider.value = aliveCount;
    }

    // 현재 웨이브 시간
    public void WaveTime(int totalSeconds)
    {
        if (waveTime == null) return;

        int m = totalSeconds / 60;
        int s = totalSeconds % 60;

        waveTime.text = $"{m:00}:{s:00}";
    }

    // 웨이브 카운트
    public void NextWaveCD(int seconds)
    {
        if (NextWaveCountDown == null) return;
        NextWaveCountDown.text = $"{seconds}";
    }

    //최대 스폰 가능 25개를 넘으면 소폰제한 알림.
    public void SpawnedMax()
    {
        Feedback_1.PlayFeedbacks();
    }

    //이후 이후 사용할 목적.
    public void SpawnedMax_1()
    {
        Feedback_2.PlayFeedbacks();
    }

    public void UpdateCoinUI(int c) => coinText.text = c.ToString();
    public void UpdateCoinUI_1(int c) => coinText_1.text = c.ToString();

    // 우클릭 시 호출: 히어로 데이터로 UI 갱신 후 보여주기
    public void ShowHeroInfo(HeroData data)
    {
        currentInfoData = data;

        if (heroInfoPanel == null) return;

        heroInfoPanel.SetActive(true);

        if (infoSpriteImage != null)
            infoSpriteImage.gameObject.SetActive(true);

        // 이미지 설정 (Image 컴포넌트에 Sprite 할당)
        if (infoSpriteImage != null && data.Hero_Image != null)
        {
            infoSpriteImage.sprite = data.Hero_Image;
            infoSpriteImage.SetNativeSize(); // 필요시 원본 크기로
        }

        // 텍스트 정보 설정
        if (infoNameText != null)
            infoNameText.text = data.Hero_Name.ToString();
        if (infoTribeText != null)
            infoTribeText.text = data.Tribe.ToString();
        if (infoskillNameText != null)
            infoskillNameText.text = $"{data.Skill_Name}";
        if (infoskillInfoText != null)
            infoskillInfoText.text = $"{data.Skill_Info}";
        if (infoskillTypeText != null)
            infoskillTypeText.text = $"{data.hero_SkillType}";
        if (infoDamageText != null)
            infoDamageText.text = $"{data.damage}";
        if (infoAttackSpeedText != null)
            infoAttackSpeedText.text = $"{data.attackSpeed}";

        float mult = EnhancementManager.Instance.GetDamageMultiplier(data.rarity);

        int bonus = Mathf.FloorToInt(data.damage * (mult - 1f));

        if (infoEnhanceText != null)
            infoEnhanceText.text = $"+{bonus}";
    }

    // 필요 시 숨기기
    public void HideHeroInfo()
    {
        if (heroInfoPanel == null) return;

        heroInfoPanel.SetActive(false);

        if (infoSpriteImage != null)
            infoSpriteImage.gameObject.SetActive(false);

        currentInfoData = null;
    }

    public void HeroMaxCount(int count, int countMax)
    {
        HeroCount.text = $"{count} / {countMax}";
        HeroCount1.text = $"{count} / {countMax}";
    }

    // 토글 메서드는 HeroController에서 직접 처리하도록 변경했으므로 제거하거나 단순화
    public void ToggleHeroInfo(HeroData data)
    {
        if (heroInfoPanel == null) return;
        if (heroInfoPanel.activeSelf && currentInfoData == data)
            HideHeroInfo();
        else
            ShowHeroInfo(data);
    }

    //다이아 UI 갱신
    public void UpdateDiamondUI(int current)
    {
        if (diamondText != null) diamondText.text = current.ToString();
        if (diamondText_1 != null) diamondText_1.text = current.ToString();
        if (diamondText_2 != null) diamondText_2.text = current.ToString();
    }

    //뽑기 실패 피드백 (선택사항)
    public void ShowSummonFailed()
    {
        // TODO: 페이드/토스트 등 사용자 피드백
        Debug.Log("뽑기 실패: 확률에 걸리지 않았습니다.");
    }

    // 강화 레벨, 비용, 소환 확률을 모두 갱신
    public void UpdateEnhancementUI()
    {
        // 레벨 (Max 처리 추가)
        int crLv = EnhancementManager.Instance.GetEnhanceLevel(EnhanceType.CommonRare);
        int hLv = EnhancementManager.Instance.GetEnhanceLevel(EnhanceType.Hero);
        int lLv = EnhancementManager.Instance.GetEnhanceLevel(EnhanceType.Legend);
        int pLv = EnhancementManager.Instance.GetEnhanceLevel(EnhanceType.Probability);

        // 레벨 텍스트 (Max 레벨일 때 "Max" 표시)
        const int MaxLevel = 12; // EnhancementManager의 MaxLevel과 동일하게 설정

        commonRareLvText.text = crLv >= MaxLevel ? "Max" : $"Lv.{crLv}";
        heroLvText.text = hLv >= MaxLevel ? "Max" : $"Lv.{hLv}";
        legendLvText.text = lLv >= MaxLevel ? "Max" : $"Lv.{lLv}";
        probLvText.text = pLv >= MaxLevel ? "Max" : $"Lv.{pLv}";

        // 비용 (Max 처리)
        int cost;
        cost = EnhancementManager.Instance.GetNextCost(EnhanceType.CommonRare);
        commonRareCostText.text = cost < 0 ? "Max" : cost.ToString();
        cost = EnhancementManager.Instance.GetNextCost(EnhanceType.Hero);
        heroCostText.text = cost < 0 ? "Max" : cost.ToString();
        cost = EnhancementManager.Instance.GetNextCost(EnhanceType.Legend);
        legendCostText.text = cost < 0 ? "Max" : cost.ToString();
        cost = EnhancementManager.Instance.GetNextCost(EnhanceType.Probability);
        probCostText.text = cost < 0 ? "Max" : cost.ToString();

        // 소환 확률 표시
        var probs = EnhancementManager.Instance.GetCurrentProbability();
        probCommonText.text = $"{probs[0]:0.##}%";
        probRareText.text = $"{probs[1]:0.##}%";
        probEpicText.text = $"{probs[2]:0.##}%";
        probLegendText.text = $"{probs[3]:0.##}%";
    }

    // 6,11,16 웨이브에만 보여 줍니다.
    public void ShowSpecialSpawnButton()
    {
        if (specialSpawnBtn != null)
            specialSpawnBtn.gameObject.SetActive(true);
    }

    // 그 외 웨이브엔 숨깁니다.
    public void HideSpecialSpawnButton()
    {
        if (specialSpawnBtn != null)
            specialSpawnBtn.gameObject.SetActive(false);
    }

    // WaveManager.Start()에서 호출: 특별 소환 버튼의 초기 상태를 세팅합니다.
    public void InitializeSpecialSpawnButton()
    {
        //명시적으로 한 번 더 초기화
        HideSpecialSpawnButton();
    }

    public void ShowWaveStartBanner(int waveNum, float duration)
    {
        if (waveStartText == null) return;
        WaveStartFB.PlayFeedbacks();
        StopCoroutine("WaveStartCoroutine");
        StartCoroutine(WaveStartCoroutine(waveNum, duration));
    }

    private IEnumerator WaveStartCoroutine(int waveNum, float duration)
    {
        WaveStartFB.PlayFeedbacks();
        waveStartText.text = $"WAVE {waveNum}";
        waveStartText.gameObject.SetActive(true);
        yield return new WaitForSeconds(duration);
        waveStartText.gameObject.SetActive(false);
    }
    
    public void ShowFreeSummonPanel(int seconds)
{
    if (freeSummonPanel == null) return;
    freeSummonPanel.SetActive(true);
    freeSummonSelected = false;
    // 버튼 리스너 (한 번만)
    freeRareBtn.onClick.RemoveAllListeners();
    freeEpicBtn.onClick.RemoveAllListeners();
    freeLegendBtn.onClick.RemoveAllListeners();
    freeRareBtn.onClick.AddListener(() => OnFreeSummonChosen(HeroRarity.Rare));
    freeEpicBtn.onClick.AddListener(() => OnFreeSummonChosen(HeroRarity.Epic));
    freeLegendBtn.onClick.AddListener(() => OnFreeSummonChosen(HeroRarity.Legendary));
    // 카운트다운 시작
    if (freeSummonRoutine != null) StopCoroutine(freeSummonRoutine);
    freeSummonRoutine = StartCoroutine(FreeSummonCountdown(seconds));
}

    private IEnumerator FreeSummonCountdown(int seconds)
    {
        int remaining = seconds;
        while (remaining > 0 && !freeSummonSelected)
        {
            freeCountdownText.text = $"{remaining--}s";
            yield return new WaitForSeconds(1f);
        }
        if (!freeSummonSelected)
            OnFreeSummonChosen(HeroRarity.Rare);
    }

    private void OnFreeSummonChosen(HeroRarity rarity)
    {
        if (freeSummonSelected) return;
        freeSummonSelected = true;
        SummonManager.Instance.FreeSummonByRarity(rarity);
        HideFreeSummonPanel();
    }

    public void HideFreeSummonPanel()
    {
        if (freeSummonPanel != null)
            freeSummonPanel.SetActive(false);
        if (freeSummonRoutine != null)
            StopCoroutine(freeSummonRoutine);
    }
}