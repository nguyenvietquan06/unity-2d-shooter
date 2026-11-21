using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameUI : MonoBehaviour
{
    public static GameUI Instance;

    [Header("Hiển thị chữ (TMP)")]
    public TMP_Text scoreText;
    public TMP_Text hpText;
    public TMP_Text levelText;
    public TMP_Text levelUpBannerText;

    [Header("Màn hình Game Over")]
    public GameObject gameOverPanel;
    public TMP_Text gameOverSummaryText;
    public Button restartButton;

    [Header("Cấu hình Level tăng dần")]
    public int baseLevelUpScore = 150;
    public float scoreStepMultiplier = 1.7f;
    public float bannerDuration = 1.2f;
    public EnemySpawner enemySpawner;

    [Header("Thanh máu dài (Slider 0..1)")]
    public Slider healthBarSlider;
    public Image healthFillImage;
    public float smoothSpeed = 8f;

    private int score = 0;
    private int currentLevel = 1;
    private int nextLevelScore;
    private float bannerTimer = 0f;
    private float targetFill = 1f;
    private bool isGameOver = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        Time.timeScale = 1f;
        nextLevelScore = Mathf.Max(10, baseLevelUpScore);
    }

    void Start()
    {
        UpdateScoreUI();
        UpdateLevelUI();

        if (levelUpBannerText != null)
        {
            levelUpBannerText.gameObject.SetActive(false);
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(RestartGame);
        }

        if (enemySpawner == null)
        {
            enemySpawner = FindFirstObjectByType<EnemySpawner>();
        }
        if (enemySpawner != null)
        {
            enemySpawner.SetLevel(currentLevel);
        }

        if (healthBarSlider != null)
        {
            healthBarSlider.minValue = 0f;
            healthBarSlider.maxValue = 1f;
            healthBarSlider.value = 1f;
        }
    }

    void Update()
    {
        if (levelUpBannerText != null && levelUpBannerText.gameObject.activeSelf)
        {
            bannerTimer -= Time.unscaledDeltaTime;
            if (bannerTimer <= 0f)
            {
                levelUpBannerText.gameObject.SetActive(false);
            }
        }

        if (healthBarSlider != null && !Mathf.Approximately(healthBarSlider.value, targetFill))
        {
            healthBarSlider.value = Mathf.Lerp(
                healthBarSlider.value,
                targetFill,
                Time.unscaledDeltaTime * smoothSpeed
            );

            if (healthFillImage != null)
            {
                if (healthBarSlider.value > 0.5f)
                {
                    healthFillImage.color = Color.Lerp(
                        Color.yellow,
                        Color.green,
                        (healthBarSlider.value - 0.5f) * 2f
                    );
                }
                else
                {
                    healthFillImage.color = Color.Lerp(
                        Color.red,
                        Color.yellow,
                        healthBarSlider.value * 2f
                    );
                }
            }
        }
    }

    public void AddScore(int amount)
    {
        if (isGameOver) return;

        score += amount;
        UpdateScoreUI();
        CheckLevelUp();
    }

    void CheckLevelUp()
    {
        while (score >= nextLevelScore)
        {
            currentLevel++;
            int step = Mathf.RoundToInt(baseLevelUpScore * Mathf.Pow(scoreStepMultiplier, currentLevel - 1));
            nextLevelScore += Mathf.Max(50, step);

            UpdateLevelUI();
            ShowLevelUpBanner();

            if (enemySpawner != null)
            {
                enemySpawner.SetLevel(currentLevel);
            }

            PlayerController player = FindFirstObjectByType<PlayerController>();
            if (player != null)
            {
                player.OnLevelUp(currentLevel);
            }
        }
    }

    void ShowLevelUpBanner()
    {
        if (levelUpBannerText != null)
        {
            levelUpBannerText.text = "LEVEL " + currentLevel + "!";
            levelUpBannerText.gameObject.SetActive(true);
            bannerTimer = bannerDuration;
        }
    }

    public void UpdateHP(int currentHp, int maxHp)
    {
        currentHp = Mathf.Clamp(currentHp, 0, maxHp);

        if (hpText != null)
        {
            hpText.text = "HP: " + currentHp + " / " + maxHp;
        }

        targetFill = maxHp > 0 ? (float)currentHp / maxHp : 0f;
    }

    public void ShowGameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        if (levelUpBannerText != null)
        {
            levelUpBannerText.gameObject.SetActive(false);
        }

        if (gameOverSummaryText != null)
        {
            gameOverSummaryText.text = "Score: " + score + "  |  Level: " + currentLevel;
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        Scene activeScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(activeScene.buildIndex);
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score;
        }
    }

    void UpdateLevelUI()
    {
        if (levelText != null)
        {
            levelText.text = "Level: " + currentLevel;
        }
    }
}
