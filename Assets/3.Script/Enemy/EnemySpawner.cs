using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemySpawner : MonoBehaviour
{
    [Header("스폰")]
    public GameObject EnemyPrefabs;
    private float EnemySpawnedCount = 0f;
    private float SpawnMax = 100f;


    [Header("몬스터 스폰 위치")]
    public Transform EnemySpawnPoint;

    [Header("몬스터 스폰 간격(초 단위)")]
    public float SpawnInterval = 2f;

    [Header("Ui 표시")]
    public TextMeshProUGUI SpawnedCountNum;
    public Slider SpawnedCountSlider;

    private float timer = 0f;
    private bool isStopped = false;

    void Awake()
    {
        if (SpawnedCountSlider != null)
        {
            SpawnedCountSlider.maxValue = SpawnMax;
            SpawnedCountSlider.value = 0;
        }
    }

    void Update()
    {
        if (isStopped || GameManager.Instance.IsGameOver()) return;

        timer += Time.deltaTime;
        if (timer >= SpawnInterval) //매 2초 마다 소환
        {
            SpawnEnemy();
            timer = 0f;
        }
    }

    private void SpawnEnemy()
    {
        if (EnemyPrefabs == null && EnemySpawnPoint == null) return;

        var Enemy = Instantiate(EnemyPrefabs, EnemySpawnPoint.position, EnemySpawnPoint.rotation);

        EnemySpawnedCount++;

        if (SpawnedCountNum != null)
        {
            SpawnedCountNum.text = $"{EnemySpawnedCount} / {SpawnMax}";

        }
        if (SpawnedCountSlider != null)
            SpawnedCountSlider.value = EnemySpawnedCount;

        if (EnemySpawnedCount >= SpawnMax)
        {
            isStopped = true;
            GameManager.Instance.GameOver("스폰된 몬스터 100마리 도달");

            Enemy.activeSelf.CompareTo(false);
        }
    }
}
