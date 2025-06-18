using UnityEngine;
using UnityEngine.UI;

public class HeroSlot : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;
    public Transform SpawnPoint => spawnPoint;

    public HeroController CurrentHero { get; private set; }
    public Button mergeButton;
    public Canvas canvas;

    //SummonManager가 Instantiate한 뒤 호출
    public void SetHero(HeroController hero)
    {
        CurrentHero = hero;
        hero.transform.SetParent(spawnPoint, false);

        // 컨테이너(Prefab) 내부에서 버튼 찾아오기
        mergeButton = hero.GetComponentInChildren<Button>(true);
        canvas = hero.GetComponentInChildren<Canvas>(true);
        if (mergeButton != null)
        {
            mergeButton.onClick.RemoveAllListeners();
            mergeButton.onClick.AddListener(OnMergeButtonClicked);
            mergeButton.gameObject.SetActive(false);
        }

        UpdateMergeButtonVisibility();
    }

    // Count가 3일 때만 버튼 보이기
    public void UpdateMergeButtonVisibility()
    {
        bool canMerge = (CurrentHero != null && CurrentHero.Count == 3);
        canvas.gameObject.SetActive(true);
        //canMerge = true;
        if (mergeButton != null)
            mergeButton.gameObject.SetActive(canMerge);
    }

    // 병합 후 컨테이너 전체 삭제 및 버튼 레퍼런스 클리어
    public void ClearHero()
    {
        if (CurrentHero != null)
        {
            Destroy(CurrentHero.gameObject);
            CurrentHero = null;
        }
        mergeButton = null;
    }

    // 버튼 클릭 시 병합 실행
    private void OnMergeButtonClicked()
    {
        if (CurrentHero != null && CurrentHero.Count == 3)
            MergeManager.Instance.MergeSlot(this);
    }
}
