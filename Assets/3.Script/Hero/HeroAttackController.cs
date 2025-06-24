using UnityEngine;
using UnityEngine.UI;

public class HeroAttackController : MonoBehaviour
{
    public HeroData Data { get; private set; }

    // 내부 쿨다운 타이머
    private float attackTimer;
    private Animator animator;

    [Header("UI")]
    [SerializeField] private GameObject uiCanvas;       // 캔버스 전체
    [SerializeField] private SpriteRenderer rangeSprite;           // 캔버스 하위의 사거리 이미지 (Scale 동기화용)
    [SerializeField] private Button combineButton;       // 합성 버튼
    [SerializeField] private Button deleteButton;        // 삭제 버튼

    // 소환 직후, SummonManager 또는 HeroSlot.SetHero()에서 호출하세요.
    public void Initialize(HeroData data)
    {
        Data = data;
        // 컨테이너 루트의 Animator가 아니라…
        animator = GetComponentInChildren<Animator>();
        attackTimer = 0f;

        // UI는 기본 꺼두기
        if (uiCanvas != null)
            uiCanvas.SetActive(false);

        // 버튼 이벤트 연결
        if (combineButton != null)
        {
            combineButton.onClick.RemoveAllListeners();
            combineButton.onClick.AddListener(OnCombine);
        }
        if (deleteButton != null)
        {
            deleteButton.onClick.RemoveAllListeners();
            deleteButton.onClick.AddListener(OnDelete);
        }
    }

    private void Update()
    {
        // 1) 타이머 감소
        attackTimer -= Time.deltaTime;
        // 2) 쿨다운 끝나면 공격 시도
        if (attackTimer <= 0f)
        {
            TryAttack();
            // 다음 공격까지 대기시간 = 1 / attackSpeed
            attackTimer = 1f / Data.attackSpeed;
        }
    }

    // 클릭 시 호출: 캔버스 전체 토글
    public void ToggleUI()
    {
        if (uiCanvas == null) return;
        bool show = !uiCanvas.activeSelf;
        uiCanvas.SetActive(show);
        if (show)
            UpdateRangeUI();
    }

    // 사거리 이미지를 Data.range와 같은 스케일로 설정
    private void UpdateRangeUI()
    {
        if (rangeSprite == null) return;

        float r = Data.range;
        
        // SpriteRenderer은 Transform 스케일로 조절
        rangeSprite.transform.localScale = new Vector3(r, r, 1f);
    }

    private void OnCombine()
    {
        // TODO: 합성 매니저 호출
        Debug.Log($"합성 요청: {Data.heroType}");
    }

    private void OnDelete()
    {
        // TODO: 삭제 로직 호출 (예: UIManager.Instance.DeleteHero(this); )
        Debug.Log($"삭제 요청: {Data.heroType}");
    }

    /// 범위 내 태그 "Enemy" 몬스터 중 가장 가까운 대상을 찾아 공격합니다.
    private void TryAttack()
    {
        // 1. 범위 내 모든 Collider2D 검색
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, Data.range);

        // 2. "Enemy" 태그만 필터, 가장 가까운 대상 찾기
        Transform closest = null;
        float minDist = float.MaxValue;
        Collider2D closestHit = null;

        foreach (var c in hits)
        {
            if (!c.CompareTag("Enemy")) continue;
            float d = (c.transform.position - transform.position).sqrMagnitude;
            if (d < minDist)
            {
                minDist = d;
                closest = c.transform;
                closestHit = c;
            }
        }

        if (closestHit == null) return;

        // 부모 쪽으로 EnemyController를 찾아야 올바르게 참조됩니다
        var enemy = closestHit.GetComponentInParent<EnemyController>();
        if (enemy != null)
            PerformAttack(enemy);
    }

    private void PerformAttack(EnemyController target)
    {
        // 1) 실제 데미지 적용
        target.TakeDamage(Data.damage);

        // 2) 활성화된 모든 child 모델 Animator에 트리거
        var animators = GetComponentsInChildren<Animator>(true);

        foreach (var anim in animators)
        {
            if (anim.gameObject.activeInHierarchy)
                anim.SetTrigger("Attack");
        }
    }

    // // 클릭 토글용 퍼블릭 메서드
    // public void ToggleRangeIndicator()
    // {
    //     if (rangeIndicatorInstance == null) return;

    //     isRangeVisible = !isRangeVisible;
    //     rangeIndicatorInstance.SetActive(isRangeVisible);
    //     if (isRangeVisible)
    //     {
    //         UpdateRangeIndicatorScale();
    //         lastRange = Data.range;
    //     }
    // }

    // // 스케일 갱신
    // private void UpdateRangeIndicatorScale()
    // {
    //     // 스프라이트가 반경 1 유닛일 때,
    //     // 지름 = range * 2
    //     float diameter = Data.range * 2f;
    //     rangeIndicatorInstance.transform.localScale = new Vector3(diameter, diameter, 1f);
    // }

    // (디버그용) 공격 범위 씬뷰에 가시화
    private void OnDrawGizmosSelected()
    {
        // Data가 없으면 아무것도 그리지 않고 빠져나감
        if (Data == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, Data.range);
    }
}
