using UnityEngine;
using TMPro;
using UnityEngine.UI;
using MoreMountains.Feedbacks;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI 텍스트")]
    public TextMeshProUGUI waveText; // 현재 웨이브
    public TextMeshProUGUI spawnCountText; //스폰된 수.
    public TextMeshProUGUI waveTime; //웨이브 시간.
    public TextMeshProUGUI NextWaveCountDown; //다음 웨이브 카운트다운
    public Text HeroCount;

    [Header("Info 텍스트")]
    public MMF_Player Feedback_1;
    public MMF_Player Feedback_2;

    [Header("Spawn 카운트 슬라이더")]
    public Slider spawnSlider;

    [Header("Game Over Panel Name (Feel)")]
    public GameObject GameOverPanel;

    [Header("Hero Info Panel")]
    [SerializeField] private GameObject heroInfoPanel;
    [SerializeField] private Image infoSpriteImage; // Sprite 대신 Image 컴포넌트 사용
    [SerializeField] private TextMeshProUGUI infoNameText;
    [SerializeField] private TextMeshProUGUI infoTribeText;
    [SerializeField] private TextMeshProUGUI infoDamageText;
    [SerializeField] private TextMeshProUGUI infoAttackSpeedText;
    [SerializeField] private TextMeshProUGUI infoskillNameText;
    [SerializeField] private TextMeshProUGUI infoskillTypeText;
    [SerializeField] private TextMeshProUGUI infoskillInfoText;

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

    // 게임 오버 UI를 띄웁니다.
    public void ShowGameOver(string reason)
    {
        //Debug.Log($"[UIManager] Game Over: {reason}");
        GameOverPanel.SetActive(true);
    }

    public void HeroMaxCount(int count, int countMax)
    {
        HeroCount.text = $"{count} / {countMax}";
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
}