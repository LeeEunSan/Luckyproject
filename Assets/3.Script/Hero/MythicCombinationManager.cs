using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MythicCombinationManager : MonoBehaviour
{
    [Header("1) 조합 레시피 목록")]
    public MythicRecipe[] recipes;

    [Header("2) UI 버튼이 배치될 부모 Canvas")]
    public Transform uiParent;

    [Header("3) 소환 슬롯들 (SummonManager.slots와 동일)")]
    public Transform[] slots;

    private Dictionary<MythicRecipe, Button> recipeButtonMap = new Dictionary<MythicRecipe, Button>();

    private void Start()
    {
        foreach (var recipe in recipes)
        {
            var btn = Instantiate(recipe.recipeButtonPrefab, uiParent);
            btn.onClick.AddListener(() => TryCombine(recipe));

            // 생성한 버튼을 'recipe' 키로 매핑
            recipeButtonMap[recipe] = btn;

            btn.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        foreach (var recipe in recipes)
        {
            // 여기서 placeholder 대신 실제 딕셔너리에서 꺼냅니다
            var btn = recipeButtonMap[recipe];  
            bool can = CanCombine(recipe);
            btn.gameObject.SetActive(can);
        }
    }

    /// <summary>
    /// 이 레시피로 조합이 가능한지 검사
    /// (필드에 inputHeroes 각각 1개 이상 존재하면 true)
    /// </summary>
    private bool CanCombine(MythicRecipe recipe)
    {
        return recipe.inputHeroes.All(input =>
            slots.Any(s =>
            {
                var hc = s.GetComponent<HeroSlot>().CurrentHero;
                return hc != null && hc.Data == input;
            })
        );
    }

    /// 조합 버튼 클릭 시 실제 조합 진행
    public void TryCombine(MythicRecipe recipe)
    {
        if (!CanCombine(recipe)) return;

        // 1) 사용된 영웅들 인구 및 비활성화
        foreach (var input in recipe.inputHeroes)
        {
            var hs = slots
                .Select(s => s.GetComponent<HeroSlot>())
                .First(h => h.CurrentHero != null && h.CurrentHero.Data == input);

            hs.CurrentHero.DecreaseCount();
            SummonManager.Instance.ChangePopulation(-1);

            if (hs.CurrentHero.Count == 0)
                hs.ClearHero(); // Count 0 → 슬롯 비우기
        }

        // 2) 빈 슬롯 찾아 Mythic 소환
        var empty = slots.First(s => s.childCount == 0);
        var go    = Instantiate(recipe.outputHero.prefab, empty.position, Quaternion.identity, empty);
        var ctrl  = go.GetComponent<HeroController>();
        ctrl.Initialize(recipe.outputHero);
        empty.GetComponent<HeroSlot>().SetHero(ctrl);
        SummonManager.Instance.ChangePopulation(+1);
    }
}
