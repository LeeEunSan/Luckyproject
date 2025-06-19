using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "Game/Mythic Recipe", fileName = "NewMythicRecipe")]
public class MythicRecipe : ScriptableObject
{
    [Header("1) 조합에 필요한 영웅들 (각 1개씩)")]
    public HeroData[] inputHeroes; // 재료 영웅.

    [Header("2) 결과로 소환될 Mythic 영웅")]
    public HeroData outputHero; // 소환될 영웅.

    [Header("3) 이 조합을 눌러줄 UI 버튼 프리팹")]
    public Button recipeButtonPrefab;
}
