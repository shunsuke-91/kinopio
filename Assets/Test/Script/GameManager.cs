using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// ゲーム全体を管理
/// </summary>

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    [SerializeField] GameObject clearPanel;
    [SerializeField] GameObject gameOverPanel;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public bool isGame = false;

    void Start()
    {
        isGame = true;
        clearPanel.SetActive(false);
        gameOverPanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void GameClear()
    {
        isGame = false;
        clearPanel.SetActive(true);
    }

    public void GameOver()
    {
        isGame = false;
        gameOverPanel.SetActive(true);
    }

    public void Restart()
    {
        SceneManager.LoadScene("Sample");
    }

}
