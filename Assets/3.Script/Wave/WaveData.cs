using CustomInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/WaveData")]
public class WaveData : ScriptableObject
{
    [HorizontalLine("웨이브 정보", color: FixedColor.Red), HideField] public bool _h0;

    public GameObject enemyPrefab;   // 이 웨이브에서 스폰할 적
    public int count = 20;           // 스폰할 총 수
    public float spawnInterval = 1f; // 몬스터 사이 스폰 텀(초)
}
