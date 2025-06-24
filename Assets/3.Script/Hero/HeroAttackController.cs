using UnityEngine;
using UnityEngine.UI;

public class HeroAttackController : MonoBehaviour
{
    public HeroData Data { get; private set; }

    // 내부 쿨다운 타이머
    private float attackTimer;
    private Animator animator;

    [Header("UI")]
    [SerializeField] private GameObject uiCanvas; // 캔버스 전체
    [SerializeField] private SpriteRenderer rangeSprite; // 캔버스 하위의 사거리 이미지 (Scale 동기화용)
    //[SerializeField] private Button combineButton; // 합성 버튼
    [SerializeField] private Button deleteButton; // 삭제 버튼

    [Header("Range Sprite 설정")]
    [SerializeField, Tooltip("Range 스프라이트의 기본 반지름 (월드 단위)")]
    private float rangeSpriteBaseRadius = 0f; // 스프라이트가 scale 1일 때의 실제 반지름

    // 외부에서 현재 UI 켜졌는지 확인할 때 쓸 프로퍼티
    public bool IsUIActive => uiCanvas != null && uiCanvas.activeSelf;
    
    public void Initialize(HeroData data)
    {
        Data = data;
        // 컨테이너 루트의 Animator가 아니라…
        animator = GetComponentInChildren<Animator>();
        attackTimer = 0f;

        // 1) 소환 직후: 모든 UI 요소들을 완전히 비활성화
        SetAllUIActive(false);

        // 2) Range Sprite 기본 반지름 자동 계산 (Inspector에서 설정하지 않은 경우)
        if (rangeSpriteBaseRadius <= 0f)
        {
            CalculateRangeSpriteBaseRadius();
        }

        // 버튼 이벤트 연결
        // if (combineButton != null)
        // {
        //     combineButton.onClick.RemoveAllListeners();
        //     combineButton.onClick.AddListener(OnCombine);
        // }

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
        
        // 모든 UI 요소들을 일괄적으로 켜고/끄기
        SetAllUIActive(show);
        
        if (show) 
        {
            UpdateRangeUI();
        }
    }

    // 모든 UI 요소들을 일괄적으로 켜고/끄는 메서드
    private void SetAllUIActive(bool active)
    {
        if (uiCanvas != null)
            uiCanvas.SetActive(active);
            
        if (rangeSprite != null)
            rangeSprite.gameObject.SetActive(active);
            
        if (deleteButton != null)
            deleteButton.gameObject.SetActive(active);
    }

    // 사거리 이미지를 Data.range와 같은 스케일로 설정
    private void UpdateRangeUI()
    {
        if (rangeSprite == null) return;

        float targetRange = Data.range;
        
        // 스프라이트의 기본 반지름을 고려한 스케일 계산
        // 목표 반지름 = 기본 반지름 × 스케일
        // 따라서 스케일 = 목표 반지름 ÷ 기본 반지름
        float scale = targetRange / rangeSpriteBaseRadius;

        // SpriteRenderer은 Transform 스케일로 조절
        rangeSprite.transform.localScale = new Vector3(scale, scale, 1f);
        
        // 디버그 로그 (필요시 주석 해제)
        // Debug.Log($"Range UI 업데이트: 목표범위={targetRange}, 기본반지름={rangeSpriteBaseRadius}, 적용스케일={scale}");
    }

    // Range Sprite의 기본 반지름을 자동으로 계산하는 메서드
    private void CalculateRangeSpriteBaseRadius()
    {
        if (rangeSprite == null) return;

        // 스프라이트의 실제 크기를 가져옴
        Sprite sprite = rangeSprite.sprite;
        if (sprite == null) return;

        // 스프라이트의 월드 단위 크기 계산
        // sprite.bounds.size는 픽셀을 월드 단위로 변환한 크기
        float spriteWidth = sprite.bounds.size.x;
        float spriteHeight = sprite.bounds.size.y;
        
        // 원형 스프라이트라고 가정하고 반지름 계산 (너비와 높이 중 작은 값의 절반)
        rangeSpriteBaseRadius = Mathf.Min(spriteWidth, spriteHeight) / 2f;
        
        Debug.Log($"Range Sprite 기본 반지름 자동 계산: {rangeSpriteBaseRadius}");
    }

    // Inspector에서 Range Sprite 기본 반지름을 수동으로 설정하는 버튼
    [ContextMenu("Calculate Range Sprite Base Radius")]
    private void CalculateRangeSpriteBaseRadiusInEditor()
    {
        CalculateRangeSpriteBaseRadius();
    }

    // Inspector에서 현재 Range UI를 테스트하는 버튼
    [ContextMenu("Test Range UI")]
    private void TestRangeUI()
    {
        if (Data != null)
        {
            UpdateRangeUI();
            Debug.Log($"Range UI 테스트: Data.range={Data.range}, 스케일={rangeSprite.transform.localScale}");
        }
        else
        {
            Debug.LogWarning("HeroData가 설정되지 않아 테스트할 수 없습니다.");
        }
    }

    private void OnCombine()
    {
        // TODO: 합성 매니저 호출
        Debug.Log($"합성 요청: {Data.heroType}");
    }

    private void OnDelete()
    {
        // 1) 현재 슬롯 정보 조회
        var heroCtrl = GetComponent<HeroController>();
        int countToRemove = heroCtrl.Count;

        // 2) 인구 수 차감 (개체 수 기준)
        SummonManager.Instance.ChangePopulation(-countToRemove);

        // 3) 슬롯에서 영웅 완전 제거 (Destroy)
        var slot = heroCtrl.originalSlot;
        if (slot != null)
            slot.ClearHero();      // ClearHero(true) → 게임오브젝트 파괴
        else
            Destroy(gameObject);
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

    // (디버그용) 공격 범위 씬뷰에 가시화
    private void OnDrawGizmosSelected()
    {
        // Data가 없으면 아무것도 그리지 않고 빠져나감
        if (Data == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, Data.range);
        
        // Range Sprite와 비교용으로 다른 색상으로도 그리기
        if (rangeSprite != null)
        {
            Gizmos.color = Color.yellow;
            float currentScale = rangeSprite.transform.localScale.x;
            float visualRadius = rangeSpriteBaseRadius * currentScale;
            Gizmos.DrawWireSphere(transform.position, visualRadius);
        }
    }
}