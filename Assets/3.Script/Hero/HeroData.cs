using UnityEngine;

public enum HeroRarity
{
    Common,
    Rare,
    Epic,
    Legendary
}

[CreateAssetMenu(menuName = "Hero System/Hero Data", fileName = "NewHeroData")]
public class HeroData : ScriptableObject
{
    public string HeroName;
    public HeroRarity rarity;

    [Header("스탯")]
    public float damage; // 데미지
    public float attackSpeed; // 공격 스피드 
    public float range; // 사거리
    public string passive; //패시브
    public string Skill; // 스킬

    [Header("프리팹")]
    public GameObject prefab; //영웅모델
}
