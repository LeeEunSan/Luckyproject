using CustomInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/WaveData")]
public class WaveData : ScriptableObject
{
    [HorizontalLine("웨이브 정보", color: FixedColor.Red), HideField] public bool _h0;

    public string waveName;
    public GameObject enemyPrefab;   // 몬스터 컨테이너 Prefab (모델+Slider+Controller 포함)
    public EnemyData enemyData;     // 스탯만 담긴 SO
    //public int count = 20;           // 스폰할 총 수
    public float spawnInterval = 1f; // 몬스터 사이 스폰 텀(초)
}
