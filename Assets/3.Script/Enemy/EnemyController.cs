using MoreMountains.Feedbacks;
using SWS;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//[RequireComponent(typeof(EnemyFindWayPoint))]
public class EnemyController : MonoBehaviour
{
    public EnemyData Data { get; private set; }
    public float CurrentHp { get; private set; }
    private Slider healthBar;
    private Animator animator;
    private splineMove mover;

    public TextMeshProUGUI DamageText;
    public MMF_Player Feedback;
    [SerializeField] private float deathAnimLength = 1.0f;

    private bool isDead = false; // 중복 사망 처리 방지

    // 초기 세팅: EnemyData로부터 체력 세팅, 컴포넌트 바인딩
    public void Initialize(EnemyData data)
    {
        Data = data;
        CurrentHp = data.maxHp;
        isDead = false;

        // 컴포넌트 바인딩
        healthBar = GetComponentInChildren<Slider>();
        animator = GetComponentInChildren<Animator>();
        mover = GetComponent<splineMove>();

        // 체력바 초기 세팅
        if (healthBar != null)
        {
            healthBar.maxValue = Data.maxHp;
            healthBar.value = CurrentHp;
        }
    }

    // 히어로 공격 시 호출: 데미지 적용
    public void TakeDamage(float amount)
    {
        if (CurrentHp <= 0 || isDead) return;  // 이미 죽었으면 무시

        // 체력 감소 및 UI 업데이트
        CurrentHp -= amount;
        if (healthBar != null)
            healthBar.value = Mathf.Max(CurrentHp, 0);

        // 데미지 텍스트를 정수로 표시 (소수점 제거)
        DamageText.text = $"{Mathf.RoundToInt(amount)}";
        Feedback.PlayFeedbacks();

        // 피격 애니메이션
        if (animator != null)
            animator.SetTrigger("Hit");

        // 사망 체크
        if (CurrentHp <= 0)
            Die();
    }

    // 내부: 체력 <= 0 시 호출
    private void Die()
    {
        if (isDead) return; // 중복 사망 방지
        isDead = true;

        // WaveManager에 사망 알림
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.OnMonsterDied();
        }
        
        // 이동 정지
        if (mover != null)
            mover.Pause();

        // 사망 애니메이션
        if (animator != null)
            animator.SetTrigger("Die");

        // 지정된 시간 후 오브젝트 제거
        Destroy(gameObject, deathAnimLength);
    }   
}