using UnityEngine;

public class HeroController : MonoBehaviour
{
    public HeroData Data { get; private set; }
    public HeroType HeroType => Data.heroType;
    public HeroRarity Rarity => Data.rarity;

    private HeroAttackController attackController;

    public int Count { get; private set; } = 0; // 1~3 활성화 단계

    [Header("드래그 & 드롭 설정")]
    [SerializeField, Tooltip("드롭 허용 최대 거리")] private float dropThreshold = 1f; // 드롭 최대 허용 거리
    [SerializeField, Tooltip("Ground 레이어에만 드롭 가능")] private LayerMask groundLayerMask;

    //드래그 앤 드롭 관련 필드 추가
    private bool isDragging = false; // 드래그 중 플래그
    public Camera mainCamera; // 월드 ↔ 스크린 변환용 카메라
    private Vector3 dragOffset; // 클릭 지점 오프셋
    public HeroSlot originalSlot; // 드래그 시작 시 속해 있던 슬롯

    public void Awake()
    {
        mainCamera = Camera.main;
        attackController = GetComponent<HeroAttackController>();

        if (mainCamera == null)
            Debug.LogWarning("MainCamera 태그가 지정된 카메라가 없습니다.");
        if (attackController == null)
            Debug.LogWarning("HeroAttackController 컴포넌트가 없습니다.");
    }

    public void Update()
    {
        CharacterMove();
    }

    // 초기 세팅
    public void Initialize(HeroData data)
    {
        Data = data;
        Count = 1; // 최초 Count 설정 (필요에 따라 변경)
        UpdateVisuals(); // 외형 갱신 메서드
    }

    // 같은 영웅 추가 소환 시 호출
    public void IncreaseCount()
    {
        Count++;
        UpdateVisuals();
    }

    // 조합 등으로 캐릭터 하나를 소모할 때 호출합니다.
    public void DecreaseCount()
    {
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

    // HeroSlot.SetHero(this) 안에서 호출되도록 추가
    public void RecordOriginalSlot(HeroSlot slot)
    {
        originalSlot = slot;
    }

    public void CharacterMove()
    {
        if (mainCamera == null) return;

        // 우클릭 눌렀을 때 히트 테스트 후 드래그 시작
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.GetRayIntersection(ray);

            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                if (originalSlot == null)
                {
                    Debug.LogWarning("RecordOriginalSlot이 호출되지 않아 드래그를 시작할 수 없습니다.");
                    return;
                }

                isDragging = true;

                // 클릭 오프셋 계산
                Vector3 worldMouse = mainCamera.ScreenToWorldPoint(Input.mousePosition);
                worldMouse.z = transform.position.z;
                dragOffset = transform.position - worldMouse;

                // 드래그 중에는 부모 관계 해제
                transform.SetParent(null, true);
            }
        }

        // 드래그 중: 오브젝트 위치를 마우스에 고정
        if (isDragging)
        {
            Vector3 worldMouse = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            worldMouse.z = transform.position.z;
            transform.position = worldMouse + dragOffset;

            // 우클릭 해제 시: 드롭 처리
            if (Input.GetMouseButtonUp(1))
            {
                isDragging = false;
                HandleDrop();
            }
        }
    }

    private void HandleDrop()
    {
        // 1) Ground 레이어 위인지 체크 (가장 중요한 조건)
        Collider2D groundCollider = Physics2D.OverlapPoint(transform.position, groundLayerMask);
        if (groundCollider == null)
        {
            Debug.Log("Ground 레이어가 아닌 곳에 드롭 시도 - 원위치로 복귀");
            ReturnToOriginal();
            return;
        }

        // 2) 유효한 HeroSlot들만 필터링 (Ground 레이어 위에 있는 슬롯들만)
        HeroSlot[] validSlots = GetValidSlotsOnGround();
        if (validSlots.Length == 0)
        {
            Debug.Log("유효한 슬롯이 없음 - 원위치로 복귀");
            ReturnToOriginal();
            return;
        }

        // 3) 가장 가까운 유효한 슬롯 찾기
        HeroSlot closestSlot = FindClosestSlot(validSlots);
        if (closestSlot == null)
        {
            Debug.Log("가까운 슬롯이 없음 - 원위치로 복귀");
            ReturnToOriginal();
            return;
        }

        float distance = Vector2.Distance(transform.position, closestSlot.SpawnPoint.position);

        // 4) 허용 거리 내인지 확인
        if (distance > dropThreshold)
        {
            Debug.Log($"허용 거리 초과 ({distance:F2} > {dropThreshold}) - 원위치로 복귀");
            ReturnToOriginal();
            return;
        }

        // 5) 슬롯 처리 (이동 또는 교환)
        ProcessSlotPlacement(closestSlot);
    }

    private HeroSlot[] GetValidSlotsOnGround()
    {
        Transform[] allSlots = SummonManager.Instance.slots;
        var validSlots = new System.Collections.Generic.List<HeroSlot>();

        foreach (Transform slotTransform in allSlots)
        {
            // 슬롯이 Ground 레이어 위에 있는지 확인
            Collider2D groundCheck = Physics2D.OverlapPoint(slotTransform.position, groundLayerMask);
            if (groundCheck != null)
            {
                HeroSlot heroSlot = slotTransform.GetComponent<HeroSlot>();
                if (heroSlot != null)
                {
                    validSlots.Add(heroSlot);
                }
            }
        }

        return validSlots.ToArray();
    }

    private HeroSlot FindClosestSlot(HeroSlot[] validSlots)
    {
        HeroSlot closestSlot = null;
        float minDistance = float.MaxValue;

        foreach (HeroSlot slot in validSlots)
        {
            float distance = Vector2.Distance(transform.position, slot.SpawnPoint.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestSlot = slot;
            }
        }

        return closestSlot;
    }

    private void ProcessSlotPlacement(HeroSlot targetSlot)
    {
        // 슬롯이 비어 있으면 단순 이동
        if (targetSlot.CurrentHero == null)
        {
            Debug.Log($"빈 슬롯으로 이동: {targetSlot.name}");
            originalSlot.ClearHero(false);

            // 안전한 위치 설정
            SetToSlotPosition(targetSlot);
            targetSlot.SetHero(this);
            originalSlot = targetSlot;
        }
        // 슬롯이 다르고 영웅이 있으면 자리 교환
        else if (targetSlot != originalSlot)
        {
            Debug.Log($"영웅 교환: {originalSlot.name} ↔ {targetSlot.name}");
            HeroController otherHero = targetSlot.CurrentHero;
            HeroSlot fromSlot = originalSlot;
            HeroSlot toSlot = targetSlot;

            // 양쪽 슬롯 클리어 (오브젝트는 파괴하지 않음)
            fromSlot.ClearHero(false);
            toSlot.ClearHero(false);

            // 영웅들을 올바른 위치로 설정
            SetHeroToSlotPosition(otherHero, fromSlot);
            SetToSlotPosition(toSlot);

            // 슬롯에 영웅 설정
            fromSlot.SetHero(otherHero);
            toSlot.SetHero(this);

            // 현재 영웅의 원래 슬롯 업데이트
            originalSlot = toSlot;
        }
        else
        {
            // 같은 슬롯이면 원위치
            Debug.Log("같은 슬롯 - 원위치로 복귀");
            ReturnToOriginal();
        }
    }

    // 초기 슬롯으로 복귀
    private void ReturnToOriginal()
    {
        if (originalSlot != null && originalSlot.SpawnPoint != null)
        {
            SetToSlotPosition(originalSlot);
            Debug.Log($"원위치 복귀: {originalSlot.name}");
        }
        else
        {
            Debug.LogError("원래 슬롯 정보가 없어 복귀할 수 없습니다!");
        }
    }

    // 영웅을 특정 슬롯 위치로 안전하게 이동시키는 메서드
    private void SetToSlotPosition(HeroSlot slot)
    {
        if (slot == null || slot.SpawnPoint == null) return;

        // 부모 설정 전에 월드 좌표 저장
        Vector3 targetWorldPos = slot.SpawnPoint.position;

        // 부모 설정
        transform.SetParent(slot.SpawnPoint, false);

        // 로컬 좌표를 0으로 설정 (SpawnPoint 기준)
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        // 만약 여전히 위치가 맞지 않으면 강제로 월드 좌표 설정
        if (Vector3.Distance(transform.position, targetWorldPos) > 0.1f)
        {
            transform.position = targetWorldPos;
        }
    }

    // 다른 영웅을 특정 슬롯 위치로 이동시키는 메서드
    private void SetHeroToSlotPosition(HeroController hero, HeroSlot slot)
    {
        if (hero == null || slot == null || slot.SpawnPoint == null) return;

        // 부모 설정 전에 월드 좌표 저장
        Vector3 targetWorldPos = slot.SpawnPoint.position;

        // 부모 설정
        hero.transform.SetParent(slot.SpawnPoint, false);

        // 로컬 좌표를 0으로 설정
        hero.transform.localPosition = Vector3.zero;
        hero.transform.localRotation = Quaternion.identity;

        // 위치 검증 및 보정
        if (Vector3.Distance(hero.transform.position, targetWorldPos) > 0.1f)
        {
            hero.transform.position = targetWorldPos;
        }
    }

    // 왼쪽 클릭 시 호출됩니다.
    private void OnMouseDown()
    {
        // Collider2D가 있어야 동작합니다.
        attackController?.ToggleUI();
    }
}
