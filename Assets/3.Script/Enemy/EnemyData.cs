using System.Collections.Generic;
using CustomInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/EnemyData")]
public class EnemyData : ScriptableObject
{
    [HorizontalLine("기본 속성 창", color: FixedColor.Red), HideField] public bool _l0;
    [Tooltip("최대 체력")] public int maxHp; // 체력.
}
