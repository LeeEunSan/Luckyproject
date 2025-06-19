using Unity.VisualScripting;
using UnityEngine;

public class HeroController : MonoBehaviour
{
    public HeroData Data { get; private set; }
    public HeroType HeroType => Data.heroType;
    public HeroRarity Rarity => Data.rarity;

    public int Count { get; private set; } = 0;  // 1~3 활성화 단계

    public void Update()
    {
        CharacterMove();       
    }

    // 초기 세팅
    public void Initialize(HeroData data)
    {
        Data = data;
        Count = 1;
        UpdateVisuals();
    }

    // 같은 영웅 추가 소환 시 호출
    public void IncreaseCount()
    {
        if (Count >= 3) return;
        Count++;
        UpdateVisuals();
    }

    // 조합 등으로 캐릭터 하나를 소모할 때 호출합니다.
    public void DecreaseCount()
    {
        if (Count <= 0) return;
        Count--;
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

    public void CharacterMove()
    {
        if (Input.GetMouseButtonDown(1))
        {
            
            Debug.Log($"클릭{this}");
        }
    }
}
