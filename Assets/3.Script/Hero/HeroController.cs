using UnityEngine;

public class HeroController : MonoBehaviour
{
    public HeroType   HeroType  { get; private set; }
    public HeroRarity Rarity    { get; private set; }
    public int        Count     { get; private set; } = 0;  // 1~3 활성화 단계

    // 초기 세팅
    public void Initialize(HeroData data)
    {
        HeroType = data.heroType;
        Rarity   = data.rarity;
        Count    = 1;
        UpdateVisuals();
    }

    // 같은 영웅 추가 소환 시 호출
    public void IncreaseCount()
    {
        if (Count >= 3) return;
        Count++;
        UpdateVisuals();
    }

    // 자식 오브젝트(모델) 활성화/비활성화 관리
    private void UpdateVisuals()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(i < Count);
        }
    }
}
