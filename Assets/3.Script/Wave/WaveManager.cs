using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class WaveManager : MonoBehaviour
{
    [Header("웨이브 목록")]
    public WaveData[] waves;        // ScriptableObject로 만든 웨이브들
    private int currentWaveIndex = 0;
    private int NextWaveInterval = 0;
    private int NextWave = 1;
    private int currentSpawned;

    [Header("스폰 위치 참조")]
    public Transform spawnPoint;    // EnemySpawner 대신 WaveManager에서 직접 스폰

    void Start()
    {
        if (waves == null || waves.Length == 0)
        {
            Debug.LogError("웨이브 데이터가 설정되지 않았습니다!");
            return;
        }
        // 첫 웨이브 시작
        StartCoroutine(SpawnWave(waves[currentWaveIndex]));
        UIManager.Instance.UpdateWave(NextWave);
        //UIManager.Instance.CoinInfo(Coin);
    }

    private IEnumerator SpawnWave(WaveData wave)
    {        
        for (int i = 0; i < wave.count; i++)
        {
            // 스폰
            Instantiate(wave.enemyPrefab, spawnPoint.position, spawnPoint.rotation);
            // Feel UI로 스폰 카운트 갱신
            //Feel.UI.SetText("SpawnCountText", $"{i+1}/{wave.count}");
            currentSpawned++;
            UIManager.Instance.UpdateSpawnCount(currentSpawned, 100);
            yield return new WaitForSeconds(wave.spawnInterval);
        }

        // 이번 웨이브 끝
        currentWaveIndex++;

        if (currentWaveIndex < waves.Length)
        {
            // 다음 웨이브로 이어서 스폰
            yield return new WaitForSeconds(2f); // 짧은 텀
            StartCoroutine(SpawnWave(waves[currentWaveIndex]));
            NextWave++;
            UIManager.Instance.UpdateWave(NextWave);
        }
        else
        {
            // 모든 웨이브 클리어
            GameManager.Instance.GameOver("모든 웨이브 클리어");
        }
    }
}


