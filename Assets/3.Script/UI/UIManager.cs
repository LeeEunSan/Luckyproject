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
    }

    // 현재 웨이브를 보여줍니다.
    public void UpdateWave(int current)
    {
        if (waveText != null)
            waveText.text = $"WAVE {current}";
    }

    // 스폰 카운트를 업데이트합니다.
    public void UpdateSpawnCount(int current)
    {
        int max = 100;
        if (spawnCountText != null)
            spawnCountText.text = $"{current} / {max}";
        if (spawnSlider != null)
            spawnSlider.value = current;
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
}
