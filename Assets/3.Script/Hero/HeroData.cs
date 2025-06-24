using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;

public enum HeroRarity
{
    Common,
    Rare,
    Epic,
    Legendary,
    Mythic
}

public enum HeroType
{
    Melee,   // 근거리
    Ranged   // 원거리
}

public enum HeroSkillType
{
    Passive,
    Skill
}

[CreateAssetMenu(menuName = "Hero System/Hero Data", fileName = "NewHeroData")]
public class HeroData : ScriptableObject
{
    [Header("영웅 정보")]
    public Sprite Hero_Image;
    public string Hero_Name;
    public string Tribe;
    public string Skill_Name;
    public HeroSkillType hero_SkillType;
    public string Skill_Info;

    public HeroRarity rarity;
    public HeroType heroType;      // ← 추가

    [Header("스탯")]
    public float damage;
    public float attackSpeed;
    public float range;

    [Header("프리팹")]
    public GameObject prefab;
}
