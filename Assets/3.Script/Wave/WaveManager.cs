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
        UIManager.Instance.InitializeSpecialSpawnButton();
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
        //Debug.Log("[WaveManager] 모든 웨이브 완료!");
        OnAllWavesComplete();
    }

    private IEnumerator RunSingleWave()
    {
        WaveData wave = waves[currentWaveIndex];

        // 웨이브 시작 알림
        int waveNum = currentWaveIndex + 1;
        UIManager.Instance.UpdateWave(waveNum);
        UIManager.Instance.ShowWaveStartBanner(waveNum, 3f);

        float waveStartTime = Time.time;
        
        // 특별 몬스터 버튼 토글
        bool isSpecialWave = System.Array.IndexOf(specialWaveNumbers, waveNum) >= 0;

        if (isSpecialWave)
            UIManager.Instance.ShowSpecialSpawnButton();
        else
            UIManager.Instance.HideSpecialSpawnButton();

        if (wave.isBossWave)
        {
            // 1) 보스 한 마리만 소환
            SpawnOneEnemy(wave);
            aliveMonsterCount++; // 보스 소환 시 카운트 증가
            UIManager.Instance.UpdateAliveCount(aliveMonsterCount);

            // 2) 60초 타임어택 시작
            float remaining = 60f;
            bool warned = false;
            while (remaining > 0f && aliveMonsterCount > 0)
            {
                remaining -= Time.deltaTime;
                // 남은 시간 표시
                UIManager.Instance.WaveTime(Mathf.CeilToInt(remaining));
                // 마지막 5초엔 NextWaveCD로 카운트다운
                if (remaining <= 5f)
                {
                    UIManager.Instance.NextWaveCD(Mathf.CeilToInt(remaining));
                    if (!warned)
                    {
                        // 경고 이펙트가 필요하면 여기에 추가
                        CountImage?.PlayFeedbacks();
                        warned = true;
                    }
                }
                yield return null;
            }

            // 3) 실패 처리: 60초 내 미처치
            if (aliveMonsterCount > 0)
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("RewardScene");
                yield break;
            }

            // 4) 성공 처리: freeSummonPanel(보상) 띄우기
            UIManager.Instance.ShowFreeSummonPanel(5);
            // 5) 그 뒤 5초 NextWaveCD 카운트다운
            UIManager.Instance.NextWaveCD(5);
            yield return new WaitForSeconds(5f);
        }
        else
        {
            // 일반 웨이브 로직
            int waveSpawnCount = 0; // 현재 웨이브에서 스폰된 수
            float nextSpawnTime = Time.time;
            float spawnInterval = wave.spawnInterval;

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
                        CountImage?.PlayFeedbacks();
                        isWarningShown = true;
                    }
                }

                // 몬스터 스폰 타이밍 체크 (20마리 미만일 때만)
                if (!allMonstersSpawned && Time.time >= nextSpawnTime && waveSpawnCount < maxSpawnPerWave)
                {
                    SpawnOneEnemy(wave);
                    waveSpawnCount++;
                    aliveMonsterCount++; // 살아있는 몬스터 수 증가

                    // UI 업데이트 (전체 누적 카운트)
                    UIManager.Instance.UpdateAliveCount(aliveMonsterCount);

                    // 20마리 다 스폰되었는지 체크
                    if (waveSpawnCount >= maxSpawnPerWave)
                    {
                        allMonstersSpawned = true;
                        UIManager.Instance.ShowWaveStartBanner(currentWaveIndex,5);
                        Debug.Log($"[WaveManager] 웨이브 {currentWaveIndex + 1} - 20마리 스폰 완료. 웨이브 시간 종료 대기 중...");
                    }
                    else
                    {
                        // 다음 스폰 시간 설정
                        nextSpawnTime = Time.time + spawnInterval;
                    }
                }

                yield return null; // 매 프레임마다 실행
            }

            // 일반 웨이브는 시간이 끝나면 바로 다음 웨이브로
            // 살아있는 몬스터가 있어도 웨이브는 진행됨
            //Debug.Log($"[WaveManager] 웨이브 {currentWaveIndex + 1} 완료 - 스폰: {waveSpawnCount}마리, 총 누적: {aliveMonsterCount}마리");
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
            // 웨이브 번호 기반 체력 배율: 예를 들어 매 웨이브마다 5%씩 증가
            int waveNum = currentWaveIndex + 1;
            float hpMultiplier = 1f + (waveNum - 1) * 0.05f;
            ctrl.Initialize(wave.enemyData, hpMultiplier);
        }
        else
        {
            Debug.LogError("[WaveManager] EnemyController가 없습니다!");
        }

        // 보스/일반 태그 설정
        go.tag = wave.isBossWave ? "BossEnemy" : "Enemy";
    }

    public void OnMonsterDied()
    {
        aliveMonsterCount--;
        if (aliveMonsterCount < 0) aliveMonsterCount = 0; // 안전장치

        // UI 업데이트
        UIManager.Instance.UpdateAliveCount(aliveMonsterCount);

//        Debug.Log($"[WaveManager] 몬스터 사망 - 현재 생존: {aliveMonsterCount}마리");
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
    
    // UIManager의 특별 소환 버튼이 눌렸을 때 호출됩니다.
    // specialWaveNumbers에 등록된 순서대로 스폰.
    public void SpawnSpecialEnemy()
    {
        int waveNum = currentWaveIndex + 1;
        int idx = System.Array.IndexOf(specialWaveNumbers, waveNum);
        if (idx < 0 || idx >= specialWaves.Length) return;

        var wave = specialWaves[idx];
        var go = Instantiate(wave.enemyPrefab, spawnPoint.position, Quaternion.identity);
        go.tag = "SpecialEnemy"; //특별 몬스터 태그
        var ctrl = go.GetComponent<EnemyController>();
        if (ctrl != null) ctrl.Initialize(wave.enemyData);

        aliveMonsterCount++;
        UIManager.Instance.UpdateAliveCount(aliveMonsterCount);

        // 한 번만 소환 가능
        UIManager.Instance.HideSpecialSpawnButton();
    }
}