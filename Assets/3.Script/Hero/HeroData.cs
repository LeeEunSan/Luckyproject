using UnityEngine;

public enum HeroRarity
{
    Common,
    Rare,
    Epic,
    Legendary
}

public enum HeroType
{
    Melee,   // 근거리
    Ranged   // 원거리
}

[CreateAssetMenu(menuName = "Hero System/Hero Data", fileName = "NewHeroData")]
public class HeroData : ScriptableObject
{
    public string     HeroName;
    public HeroRarity rarity;
    public HeroType   heroType;      // ← 추가

    [Header("스탯")]
    public float damage;
    public float attackSpeed;
    public float range;
    public string passive;
    public string Skill;

    [Header("프리팹")]
    // 이 프리팹은 “컨테이너” 형식이어야 합니다.
    // 내부에 child 3개(각 hero 모델)가 있고, 활성화 단계는 HeroController가 관리
    public GameObject prefab;
}
