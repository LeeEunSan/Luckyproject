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

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        isGameOver = false;
    }

    public void MainSceneToInGameScene()
    {
        SceneManager.LoadScene("TestScene", LoadSceneMode.Single);
    }

    public void InGameSceneToMainScene()
    {
        SceneManager.LoadScene("MainScene", LoadSceneMode.Single);
    }

}

