using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class test : MonoBehaviour
{
    public static test Instance { get; private set; }

    public Image[] images;
    public Image hpImage1;
    public Image hpImage2;
    public Image hpImage3;

    public GameObject gameOverPanel;

    public Button retryButton;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI hpText;
    private int score;

    private int currentLives;
    private const int maxLives = 3;
    private bool isGameOverTriggered;

    public bool IsGameOver => isGameOverTriggered;
    public bool IsPlayerControlLocked => currentLives <= 0;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[HPManager] test 인스턴스가 2개 이상입니다. 마지막으로 로드된 인스턴스를 사용합니다.");
        }

        Instance = this;
    }

    void Start()
    {
        currentLives = maxLives;
        score = 0;
        isGameOverTriggered = false;

        // Canvas에서 UI 요소 자동으로 찾기
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            // 모든 Button과 Text를 이름으로 찾기
            Button[] allButtons = canvas.GetComponentsInChildren<Button>();
            TextMeshProUGUI[] allTexts = canvas.GetComponentsInChildren<TextMeshProUGUI>();

            foreach (Button btn in allButtons)
            {
                if (btn.name == "retryButton") retryButton = btn;
            }

            foreach (TextMeshProUGUI txt in allTexts)
            {
                if (txt.name == "scoreText") scoreText = txt;
                else if (txt.name == "hpText") hpText = txt;
            }

            if (hpImage1 == null)
            {
                Transform t = canvas.transform.Find("hpImage1");
                if (t != null) hpImage1 = t.GetComponent<Image>();
            }
            if (hpImage2 == null)
            {
                Transform t = canvas.transform.Find("hpImage2");
                if (t != null) hpImage2 = t.GetComponent<Image>();
            }
            if (hpImage3 == null)
            {
                Transform t = canvas.transform.Find("hpImage3");
                if (t != null) hpImage3 = t.GetComponent<Image>();
            }

            Debug.Log("scoreText: " + (scoreText != null ? "찾음" : "찾지못함"));
            Debug.Log("hpText: " + (hpText != null ? "찾음" : "찾지못함"));
            Debug.Log("hpImage1: " + (hpImage1 != null ? "찾음" : "찾지못함"));
            Debug.Log("hpImage2: " + (hpImage2 != null ? "찾음" : "찾지못함"));
            Debug.Log("hpImage3: " + (hpImage3 != null ? "찾음" : "찾지못함"));
            Debug.Log("retryButton: " + (retryButton != null ? "찾음" : "찾지못함"));
        }
        else
        {
            Debug.LogError("Canvas를 찾을 수 없음!");
        }

        UpdateScoreText();
        SyncLifeImages();
        UpdateHpText();
        UpdateGameOverUiByHp();

        if (retryButton != null)
        {
            retryButton.onClick.RemoveAllListeners();
            retryButton.onClick.AddListener(OnRetryButtonCick);
        }
    }

    public void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        if (retryButton != null)
        {
            retryButton.onClick.RemoveListener(OnRetryButtonCick);
        }
    }

    public void TakeDamage(int damage)
    {
        if (isGameOverTriggered) return;

        if (currentLives <= 0)
        {
            TriggerGameOver();
            return;
        }

        currentLives -= damage;
        if (currentLives < 0)
        {
            currentLives = 0;
        }

        Debug.Log($"HP 감소: {currentLives}/{maxLives}");
        SyncLifeImages();
        UpdateHpText();
        UpdateGameOverUiByHp();

        // HP가 0이 된 시점에는 버티고, 다음 피격(4번째)에서 게임오버 처리
        if (currentLives == 0)
        {
            Debug.Log("HP가 0입니다. 다음 피격 시 게임오버 처리됩니다.");
        }
    }

    public void OnRetryButtonCick()
    {
        if (isGameOverTriggered)
        {
            Scene currentScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(currentScene.buildIndex);
            return;
        }

        // 목숨 초기화
        currentLives = maxLives;
        SyncLifeImages();
        UpdateHpText();
        UpdateGameOverUiByHp();
        Debug.Log($"HP 회복: {currentLives}/{maxLives}");

    }

    public void AddScore(int amount)
    {
        score += amount;
        Debug.Log("현재 점수: " + score);
        UpdateScoreText();
    }

    private void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = score.ToString("#,##0");
            Debug.Log("스코어 텍스트 업데이트: " + scoreText.text);
        }
        else
        {
            Debug.LogError("scoreText가 null입니다!");
        }
    }

    private void UpdateHpText()
    {
        if (hpText != null)
        {
            hpText.text = $"HP: {currentLives}/{maxLives}";
        }
    }

    private void SyncLifeImages()
    {
        int activeLives = Mathf.Clamp(currentLives, 0, maxLives);
        for (int i = 0; i < maxLives; i++)
        {
            Image lifeImage = GetLifeImageByIndex(i);
            if (lifeImage == null) continue;

            Color color = lifeImage.color;
            color.a = i < activeLives ? 1f : 0f;
            lifeImage.color = color;
        }
    }

    private Image GetLifeImageByIndex(int index)
    {
        if (images != null && images.Length > index && images[index] != null)
        {
            return images[index];
        }

        if (index == 0) return hpImage1;
        if (index == 1) return hpImage2;
        if (index == 2) return hpImage3;

        return null;
    }

    private void TriggerGameOver()
    {
        if (isGameOverTriggered) return;

        isGameOverTriggered = true;
        Debug.Log("게임오버: 4번째 충돌로 게임 종료");
        UpdateGameOverUiByHp();

        StopEnemySpawningAndClearEnemies();
    }

    private void UpdateGameOverUiByHp()
    {
        bool showGameOverUi = currentLives <= 0;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(showGameOverUi);
        }

        if (retryButton != null)
        {
            retryButton.gameObject.SetActive(showGameOverUi);
        }
    }

    private void StopEnemySpawningAndClearEnemies()
    {
        SpawnPoint[] spawnSystems = FindObjectsOfType<SpawnPoint>();
        for (int i = 0; i < spawnSystems.Length; i++)
        {
            SpawnPoint spawner = spawnSystems[i];
            if (spawner == null) continue;

            spawner.CancelInvoke();
            spawner.enabled = false;
        }

        GameManager[] managers = FindObjectsOfType<GameManager>();
        for (int i = 0; i < managers.Length; i++)
        {
            if (managers[i] != null)
            {
                managers[i].enabled = false;
            }
        }

        Enemy[] aliveEnemies = FindObjectsOfType<Enemy>();
        for (int i = 0; i < aliveEnemies.Length; i++)
        {
            if (aliveEnemies[i] != null)
            {
                Destroy(aliveEnemies[i].gameObject);
            }
        }
    }
}
