using System.Collections;
using MoreMountains.Feedbacks;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [Header("웨이브 데이터")]
    public WaveData[] waves;
    public Transform spawnPoint;

    [Header("웨이브 설정")]
    public int maxSpawnPerWave = 20;
    public float waveDuration = 20f;
    public float nextWaveWarningTime = 5f;

    private int currentWaveIndex = 0;
    private int aliveMonsterCount = 0; // 현재 살아있는 몬스터 수

    public MMF_Player CountImage;

    // 싱글톤 패턴 (EnemyController에서 접근하기 위해)
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

        // 첫 웨이브 UI 초기화
        UIManager.Instance.UpdateWave(currentWaveIndex + 1);
        UIManager.Instance.UpdateAliveCount(aliveMonsterCount);
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
        Debug.Log("[WaveManager] 모든 웨이브 완료!");
        OnAllWavesComplete();
    }

    private IEnumerator RunSingleWave()
    {
        // 웨이브 시작 설정
        UIManager.Instance.UpdateWave(currentWaveIndex + 1);

        float waveStartTime = Time.time;
        int waveSpawnCount = 0; // 현재 웨이브에서 스폰된 수
        float nextSpawnTime = Time.time;
        float spawnInterval = waves[currentWaveIndex].spawnInterval;

        bool isWarningShown = false;
        bool allMonstersSpawned = false;

        // 웨이브는 무조건 20초 동안 진행
        while (Time.time - waveStartTime < waveDuration)
        {
            float elapsedTime = Time.time - waveStartTime;
            float remainingTime = waveDuration - elapsedTime;
            int remainingSec = Mathf.CeilToInt(remainingTime);

            // 남은 시간 UI 업데이트 (매 프레임)
            UIManager.Instance.WaveTime(remainingSec);

            // 5초 이하일 때 카운트다운 표시
            if (remainingTime <= nextWaveWarningTime)
            {
                UIManager.Instance.NextWaveCD(remainingSec);

                // 경고 효과는 한 번만 실행
                if (!isWarningShown)
                {
                    CountImage.PlayFeedbacks(); // 주석 해제 시 사용
                    isWarningShown = true;
                }
            }

            // 몬스터 스폰 타이밍 체크 (20마리 미만일 때만)
            if (!allMonstersSpawned && Time.time >= nextSpawnTime && waveSpawnCount < maxSpawnPerWave)
            {
                SpawnOneEnemy(waves[currentWaveIndex]);
                waveSpawnCount++;
                aliveMonsterCount++; // 살아있는 몬스터 수 증가

                // UI 업데이트 (전체 누적 카운트)
                UIManager.Instance.UpdateAliveCount(aliveMonsterCount);

                // 20마리 다 스폰되었는지 체크
                if (waveSpawnCount >= maxSpawnPerWave)
                {
                    allMonstersSpawned = true;
//                    Debug.Log($"[WaveManager] 웨이브 {currentWaveIndex + 1} - 20마리 스폰 완료. 웨이브 시간 종료 대기 중...");
                }
                else
                {
                    // 다음 스폰 시간 설정
                    nextSpawnTime = Time.time + spawnInterval;
                }
            }

            yield return null; // 매 프레임마다 실행
        }

        // 웨이브 완료 로그
//        Debug.Log($"[WaveManager] 웨이브 {currentWaveIndex + 1} 완료 - 스폰: {waveSpawnCount}마리, 총 누적: {aliveMonsterCount}마리");
    }

    private void SpawnOneEnemy(WaveData wave)
    {
        if (wave.enemyPrefab == null)
        {
            Debug.LogError($"[WaveManager] 웨이브 {currentWaveIndex + 1}의 enemyPrefab이 null입니다!");
            return;
        }

        var go = Instantiate(wave.enemyPrefab, spawnPoint.position, Quaternion.identity);
        go.tag = "Enemy";

        var ctrl = go.GetComponent<EnemyController>();
        if (ctrl != null)
        {
            ctrl.Initialize(wave.enemyData);
        }
        else
        {
            Debug.LogError("[WaveManager] EnemyController가 없습니다!");
        }
    }

    public void OnMonsterDied()
    {
        aliveMonsterCount--;
        if (aliveMonsterCount < 0) aliveMonsterCount = 0; // 안전장치

        // UI 업데이트
        UIManager.Instance.UpdateAliveCount(aliveMonsterCount);

        Debug.Log($"[WaveManager] 몬스터 사망 - 현재 생존: {aliveMonsterCount}마리");
    }

    private void OnAllWavesComplete()
    {
        // 게임 클리어 처리
        // 예: 승리 UI 표시, 보상 지급 등
        Debug.Log("게임 클리어!");

        // 필요 시 추가 처리
        // UIManager.Instance.ShowGameClearUI();
        // GameManager.Instance.OnGameClear();
    }

    // 디버그용 메서드들
    private void OnGUI()
    {
        if (Application.isPlaying)
        {
            GUI.Label(new Rect(10, 10, 200, 20), $"현재 웨이브: {currentWaveIndex + 1}/{waves.Length}");
            GUI.Label(new Rect(10, 50, 200, 20), $"현재 생존: {aliveMonsterCount}");
        }
    }

    // 외부에서 현재 상태 확인용
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
}