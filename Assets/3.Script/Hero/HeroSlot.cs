using UnityEngine;
using UnityEngine.UI;

public class HeroSlot : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;   
    [SerializeField] private Button mergeButton;  

    public Transform SpawnPoint  => spawnPoint;
    public HeroController CurrentHero { get; private set; }

    private void Awake()
    {
        mergeButton.onClick.AddListener(OnMergeButtonClicked);
        mergeButton.gameObject.SetActive(false);
    }

    // SummonManager가 Instantiate한 뒤, 이걸 호출하세요.
    public void SetHero(HeroController hero)
    {
        CurrentHero = hero;
        hero.transform.SetParent(spawnPoint, false);
        UpdateMergeButtonVisibility();
    }

    // 병합 후 기존 객체 삭제
    public void ClearHero()
    {
        if (CurrentHero != null)
        {
            Destroy(CurrentHero.gameObject);
            CurrentHero = null;
        }
        mergeButton.gameObject.SetActive(false);
    }

    public void UpdateMergeButtonVisibility()
    {
        bool canMerge = (CurrentHero != null && CurrentHero.Count == 3);
        mergeButton.gameObject.SetActive(canMerge);
    }

    public void OnMergeButtonClicked()
    {
        if (CurrentHero != null && CurrentHero.Count == 3)
            MergeManager.Instance.MergeSlot(this);
    }
}
