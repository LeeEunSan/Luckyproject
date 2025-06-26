using System.Collections;
using MoreMountains.Feedbacks;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [Header("웨이브 데이터")]
    public WaveData[] waves;
    public Transform spawnPoint;

    [Header("특별 몬스터 설정")]
    [Tooltip("6,11,16 웨이브에 대응하는 SO를 순서대로 넣으세요")]
    public WaveData[] specialWaves;
    [Tooltip("specialWaves[i]를 스폰할 웨이브 번호 (1-based)")]
    public int[] specialWaveNumbers;

    [Header("웨이브 설정")]
    public int maxSpawnPerWave = 20;
    public float waveDuration = 20f;
    public float nextWaveWarningTime = 5f;
    
    [Header("게임 시작 설정")]
    [SerializeField] private float gameStartDelay = 5f; // 게임 시작 대기 시간

    private int currentWaveIndex = 0;
    private int aliveMonsterCount = 0;

    public MMF_Player CountImage;

    public static WaveManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // 초기 데이터 검증
        if (waves == null || waves.Length == 0)
        {
            Debug.LogError("[WaveManager] 웨이브 데이터가 없습니다!");
            return;
        }

        if (UIManager.Instance == null)
        {
            Debug.LogError("[WaveManager] UIManager가 없습니다!");
            return;
        }

        // 게임 시작 카운트다운 코루틴 시작
        StartCoroutine(GameStartCountdown());
    }

    // 게임 시작 카운트다운
    private IEnumerator GameStartCountdown()
    {
        // UI 초기화
        UIManager.Instance.InitializeSpecialSpawnButton();
        UIManager.Instance.UpdateWave(1); // 첫 번째 웨이브 표시
        UIManager.Instance.UpdateAliveCount(0);
        
        // 카운트다운 표시
        float remainingTime = gameStartDelay;
        
        // 카운트 이펙트 재생
        CountImage?.PlayFeedbacks();
        
        while (remainingTime > 0)
        {
            int seconds = Mathf.CeilToInt(remainingTime);
            
            // 카운트다운 UI 표시
            UIManager.Instance.NextWaveCD(seconds);
            
            remainingTime -= Time.deltaTime;
            yield return null;
        }
        
        // 카운트다운 UI 초기화
        UIManager.Instance.NextWaveCD(0);
        
        // 첫 웨이브 시작
        StartCoroutine(RunWaves());
    }

    private IEnumerator RunWaves()
    {
        while (currentWaveIndex < waves.Length)
        {
            yield return StartCoroutine(RunSingleWave());
            currentWaveIndex++;
        }

        // 모든 웨이브 완료 - 게임 클리어 처리
        OnAllWavesComplete();
    }

    private IEnumerator RunSingleWave()
    {
        WaveData wave = waves[currentWaveIndex];

        // 웨이브 시작 알림
        int waveNum = currentWaveIndex + 1;
        UIManager.Instance.UpdateWave(waveNum);
        UIManager.Instance.ShowWaveStartBanner(waveNum);

        float waveStartTime = Time.time;
        
        // 특별 몬스터 버튼 토글
        bool isSpecialWave = System.Array.IndexOf(specialWaveNumbers, waveNum) >= 0;

        if (isSpecialWave)
            UIManager.Instance.ShowSpecialSpawnButton();
        else
            UIManager.Instance.HideSpecialSpawnButton();

        if (wave.isBossWave)
        {
            // 보스 웨이브 로직 (기존과 동일)
            SpawnOneEnemy(wave);
            aliveMonsterCount++;
            UIManager.Instance.UpdateAliveCount(aliveMonsterCount);
            SoundManager.Instance.PlayBossBGM();

            float remaining = 60f;
            bool warned = false;
            while (remaining > 0f && aliveMonsterCount > 0)
            {
                remaining -= Time.deltaTime;
                UIManager.Instance.WaveTime(Mathf.CeilToInt(remaining));
                
                if (remaining <= 5f)
                {
                    UIManager.Instance.NextWaveCD(Mathf.CeilToInt(remaining));
                    if (!warned)
                    {
                        CountImage?.PlayFeedbacks();
                        warned = true;
                    }
                }
                yield return null;
            }

            if (aliveMonsterCount > 0)
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("RewardScene");
                yield break;
            }

            UIManager.Instance.ShowFreeSummonPanel(5);
            UIManager.Instance.NextWaveCD(5);
            yield return new WaitForSeconds(5f);
        }
        else
        {
            // 일반 웨이브 로직 (기존과 동일)
            int waveSpawnCount = 0;
            float nextSpawnTime = Time.time;
            float spawnInterval = wave.spawnInterval;

            bool isWarningShown = false;
            bool allMonstersSpawned = false;

            while (Time.time - waveStartTime < waveDuration)
            {
                float elapsedTime = Time.time - waveStartTime;
                float remainingTime = waveDuration - elapsedTime;
                int remainingSec = Mathf.CeilToInt(remainingTime);

                UIManager.Instance.WaveTime(remainingSec);

                if (remainingTime <= nextWaveWarningTime)
                {
                    UIManager.Instance.NextWaveCD(remainingSec);

                    if (!isWarningShown)
                    {
                        CountImage?.PlayFeedbacks();
                        isWarningShown = true;
                    }
                }

                if (!allMonstersSpawned && Time.time >= nextSpawnTime && waveSpawnCount < maxSpawnPerWave)
                {
                    SpawnOneEnemy(wave);
                    waveSpawnCount++;
                    aliveMonsterCount++;

                    UIManager.Instance.UpdateAliveCount(aliveMonsterCount);

                    if (waveSpawnCount >= maxSpawnPerWave)
                    {
                        allMonstersSpawned = true;
                        UIManager.Instance.ShowWaveStartBanner(currentWaveIndex);
                        Debug.Log($"[WaveManager] 웨이브 {currentWaveIndex + 1} - 20마리 스폰 완료. 웨이브 시간 종료 대기 중...");
                    }
                    else
                    {
                        nextSpawnTime = Time.time + spawnInterval;
                    }
                }

                yield return null;
            }
        }
    }

    private void SpawnOneEnemy(WaveData wave)
    {
        if (wave.enemyPrefab == null)
        {
            Debug.LogError($"[WaveManager] 웨이브 {currentWaveIndex + 1}의 enemyPrefab이 null입니다!");
            return;
        }

        var go = Instantiate(wave.enemyPrefab, spawnPoint.position, Quaternion.identity);

        var ctrl = go.GetComponent<EnemyController>();
        if (ctrl != null)
        {
            int waveNum = currentWaveIndex + 1;
            float hpMultiplier = 1f + (waveNum - 1) * 0.05f;
            ctrl.Initialize(wave.enemyData, hpMultiplier);
        }
        else
        {
            Debug.LogError("[WaveManager] EnemyController가 없습니다!");
        }

        go.tag = wave.isBossWave ? "BossEnemy" : "Enemy";
    }

    public void OnMonsterDied()
    {
        aliveMonsterCount--;
        if (aliveMonsterCount < 0) aliveMonsterCount = 0;

        UIManager.Instance.UpdateAliveCount(aliveMonsterCount);
    }

    private void OnAllWavesComplete()
    {
        Debug.Log("게임 클리어!");
        // 필요 시 추가 처리
    }

    private void OnGUI()
    {
        if (Application.isPlaying)
        {
            GUI.Label(new Rect(10, 10, 200, 20), $"현재 웨이브: {currentWaveIndex + 1}/{waves.Length}");
            GUI.Label(new Rect(10, 50, 200, 20), $"현재 생존: {aliveMonsterCount}");
        }
    }

    public int GetCurrentWave() => currentWaveIndex + 1;
    public int GetAliveMonsterCount() => aliveMonsterCount;
    public bool IsAllWavesComplete() => currentWaveIndex >= waves.Length;

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
    
    public void SpawnSpecialEnemy()
    {
        SoundManager.Instance.PlaySpecialMonsterSpawn();

        int waveNum = currentWaveIndex + 1;
        int idx = System.Array.IndexOf(specialWaveNumbers, waveNum);

        if (idx < 0 || idx >= specialWaves.Length) return;

        var wave = specialWaves[idx];
        var go = Instantiate(wave.enemyPrefab, spawnPoint.position, Quaternion.identity);
        go.tag = "SpecialEnemy";
        var ctrl = go.GetComponent<EnemyController>();
        
        if (ctrl != null) ctrl.Initialize(wave.enemyData);

        aliveMonsterCount++;
        UIManager.Instance.UpdateAliveCount(aliveMonsterCount);

        UIManager.Instance.HideSpecialSpawnButton();
    }
}