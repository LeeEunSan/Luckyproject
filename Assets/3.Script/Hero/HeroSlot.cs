using UnityEngine;
using UnityEngine.UI;

public class HeroSlot : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;
    public Transform SpawnPoint => spawnPoint;

    public HeroController CurrentHero { get; private set; }
    [SerializeField] private Button mergeButton;
    [SerializeField] private Canvas canvas;

    //SummonManager가 Instantiate한 뒤 호출
    public void SetHero(HeroController hero)
    {
        CurrentHero = hero;
        
        // 안전한 위치 설정
        SetHeroPosition(hero);

        // HeroController에 원래 슬롯 기록
        hero.RecordOriginalSlot(this);

        // 컨테이너(Prefab) 내부에서 버튼 찾아오기
        mergeButton = hero.GetComponentInChildren<Button>(true);
        canvas = hero.GetComponentInChildren<Canvas>(true);

        // 최초 상태 갱신
        UpdateMergeButtonVisibility();

        mergeButton?.onClick.RemoveAllListeners();
        mergeButton?.onClick.AddListener(OnMergeButtonClicked);
        mergeButton?.gameObject.SetActive(hero.Count == 3);
    }

    // 영웅을 올바른 위치에 배치하는 메서드
    private void SetHeroPosition(HeroController hero)
    {
        if (hero == null || spawnPoint == null) return;

        // 목표 월드 좌표 저장
        Vector3 targetWorldPos = spawnPoint.position;
        
        // 부모 설정 (worldPositionStays를 false로 설정)
        hero.transform.SetParent(spawnPoint, false);
        
        // 로컬 좌표를 명확히 0으로 설정
        hero.transform.localPosition = Vector3.zero;
        hero.transform.localRotation = Quaternion.identity;
        hero.transform.localScale = Vector3.one;
        
        // 위치 검증 및 보정 (허용 오차 0.1f)
        if (Vector3.Distance(hero.transform.position, targetWorldPos) > 0.1f)
        {
//            Debug.LogWarning($"영웅 위치 보정: {hero.name} in {gameObject.name}");
            hero.transform.position = targetWorldPos;
        }
        
        // 추가 검증용 로그
//        Debug.Log($"영웅 배치 완료: {hero.name} -> {gameObject.name} (World: {hero.transform.position}, Local: {hero.transform.localPosition})");
    }

    // 현재 Count에 따라 Merge 버튼 보이기/숨기기
    public void UpdateMergeButtonVisibility()
    {
        // if (canvas != null)
        //     canvas.gameObject.SetActive(true);

        // if (mergeButton != null)
        //     mergeButton.gameObject.SetActive(CurrentHero != null && CurrentHero.Count >= 3);

        // 캔버스는 HeroAttackController.ToggleUI() 만으로 제어하니 여기서는 제거
        if (mergeButton != null)
            mergeButton.gameObject.SetActive(CurrentHero != null && CurrentHero.Count >= 3);
    }

    // 이동·교환 시 오브젝트 파괴 여부 선택
    public void ClearHero(bool destroyHero)
    {
        if (CurrentHero != null)
        {
            if (destroyHero)
                Destroy(CurrentHero.gameObject);
            else
                CurrentHero.transform.SetParent(null);
            CurrentHero = null;
        }
        mergeButton = null;
    }

    // 기존 파괴 방식 유지용
    public void ClearHero()
    {
        ClearHero(true);
    }

    private void OnMergeButtonClicked()
    {
        if (CurrentHero != null && CurrentHero.Count == 3)
            MergeManager.Instance.MergeSlot(this);
    }
}
