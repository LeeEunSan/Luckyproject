using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    private bool isGameOver = false;

    void Awake()
    {
        // 싱글턴 설정
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    public void GameOver(string reason)
    {
        if (isGameOver) return;
        isGameOver = true;

        //Show("GameOverPanel");
    }

    public bool IsGameOver()
    {
        return isGameOver;
    }

    public void MainSceneToInGameScene()
    {
        // 게임 상태 완전 리셋
        ResetGameState();
        
        // 씬 로드
        SceneManager.LoadScene("InGameScene", LoadSceneMode.Single);
    }

    public void InGameSceneToMainScene()
    {
        // 메인으로 돌아갈 때도 리셋
        ResetGameState();
        
        SceneManager.LoadScene("MainScene", LoadSceneMode.Single);
    }

    // 게임 상태를 완전히 리셋하는 메서드
    private void ResetGameState()
    {
        // 1. GameManager 상태 리셋
        isGameOver = false;

        // 2. 다른 싱글턴 매니저들 리셋 (존재하는 경우에만)
        
        // EnhancementManager 리셋
        if (EnhancementManager.Instance != null)
        {
            // 강화 레벨 초기화
            EnhancementManager.Instance.ResetEnhancements();
            
            // 재화 초기값으로 리셋 (필요시 메서드 추가)
            // EnhancementManager.Instance.ResetCurrency();
        }

        // SummonManager 리셋
        if (SummonManager.Instance != null)
        {
            // 영웅 슬롯 모두 비우기
            ClearAllHeroSlots();
            
            // 다이아몬드 초기값으로 리셋 (필요시)
            // SummonManager.Instance.ResetDiamonds();
        }

        // WaveManager 리셋 (인스턴스 파괴)
        if (WaveManager.Instance != null)
        {
            Destroy(WaveManager.Instance.gameObject);
        }

        // UIManager 리셋
        if (UIManager.Instance != null)
        {
            // UI 초기 상태로
            UIManager.Instance.HideHeroInfo();
            UIManager.Instance.HideSpecialSpawnButton();
            UIManager.Instance.HideFreeSummonPanel();
        }

        // 3. 모든 적과 영웅 제거
        ClearAllEnemies();
        ClearAllProjectiles();

        // 4. Time Scale 정상화 (혹시 일시정지 상태일 수 있으므로)
        Time.timeScale = 1f;

        // 5. 기타 static 변수들 리셋
        // 필요한 경우 추가

        Debug.Log("게임 상태가 완전히 리셋되었습니다.");
    }

    // 모든 영웅 슬롯 비우기
    private void ClearAllHeroSlots()
    {
        if (SummonManager.Instance == null || SummonManager.Instance.slots == null) return;

        foreach (var slot in SummonManager.Instance.slots)
        {
            if (slot != null)
            {
                var heroSlot = slot.GetComponent<HeroSlot>();
                if (heroSlot != null && heroSlot.CurrentHero != null)
                {
                    heroSlot.ClearHero(true); // 영웅 오브젝트 파괴
                }
            }
        }
    }

    // 모든 적 제거
    private void ClearAllEnemies()
    {
        // Enemy 태그를 가진 모든 오브젝트 제거
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (var enemy in enemies)
        {
            Destroy(enemy);
        }

        // BossEnemy 태그를 가진 모든 오브젝트 제거
        GameObject[] bosses = GameObject.FindGameObjectsWithTag("BossEnemy");
        foreach (var boss in bosses)
        {
            Destroy(boss);
        }

        // SpecialEnemy 태그를 가진 모든 오브젝트 제거
        GameObject[] specials = GameObject.FindGameObjectsWithTag("SpecialEnemy");
        foreach (var special in specials)
        {
            Destroy(special);
        }
    }

    // 모든 투사체 제거 (필요한 경우)
    private void ClearAllProjectiles()
    {
        // 투사체 태그가 있다면 여기서 제거
        // GameObject[] projectiles = GameObject.FindGameObjectsWithTag("Projectile");
        // foreach (var proj in projectiles)
        // {
        //     Destroy(proj);
        // }
    }

    // 디버그용: 수동 리셋 메서드
    [ContextMenu("Force Reset Game State")]
    public void ForceResetGameState()
    {
        ResetGameState();
    }
}