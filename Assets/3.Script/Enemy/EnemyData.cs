using System.Collections.Generic;
using CustomInspector;
using SWS;
using UnityEngine;

public enum EnemyTeyp { None, Normal, Boss }

[CreateAssetMenu(menuName = "Data/EnemyData")]
public class EnemyData : ScriptableObject
{
    [HorizontalLine("프로파일", color: FixedColor.Red), HideField] public bool _h0;
    public EnemyTeyp enemyTeyp = EnemyTeyp.Normal;
    public string alias;
    [Preview(Size.medium)] public Sprite portrait;
    [Preview(Size.medium)] public List<GameObject> models;

    [HorizontalLine("기본 속성 창", color: FixedColor.Red), HideField] public bool _l0;
    [Tooltip("체력")] public int health; // 체력.
    [Tooltip("초당 이동 속도(/sec)")] public float movespeed = 0;

    public splineMove SplineMove;

    private void EnemyMoveSpeed()
    {
        movespeed = SplineMove.speed + movespeed;
    }
}
