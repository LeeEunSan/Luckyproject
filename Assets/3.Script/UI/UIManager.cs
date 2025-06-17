using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI 텍스트")]
    public TextMeshProUGUI waveText;
    public TextMeshProUGUI spawnCountText;
    public TextMeshProUGUI WaveInterval;
    public Text HeroCount;

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
    public void UpdateSpawnCount(int current, int max = 100)
    {
        if (spawnCountText != null)
            spawnCountText.text = $"{current} / {max}";
        if (spawnSlider != null)
            spawnSlider.value = current;
    }

    public void UpdateWaveInterval(float current)
    {
        WaveInterval.text = $"{current}";
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
