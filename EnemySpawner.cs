using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Cấu hình sinh quái")]
    public GameObject enemyPrefab;
    public float baseSpawnInterval = 1.2f;
    public float minSpawnInterval = 0.22f;
    public float spawnRadius = 11f;

    [Header("Tăng độ khó mạnh theo Level")]
    public float speedPerLevel = 0.55f;
    public float speedMultiplierPerLevel = 1.06f;
    public int hpPerLevel = 1;

    private Transform playerTransform;
    private float timer;
    private int currentLevel = 1;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
    }

    void Update()
    {
        if (enemyPrefab == null || Time.timeScale == 0f) return;

        float currentInterval = Mathf.Max(
            minSpawnInterval,
            baseSpawnInterval * Mathf.Pow(0.88f, currentLevel - 1)
        );

        timer += Time.deltaTime;
        if (timer >= currentInterval)
        {
            timer = 0f;
            SpawnEnemy();
        }
    }

    public void SetLevel(int newLevel)
    {
        currentLevel = Mathf.Max(1, newLevel);
    }

    void SpawnEnemy()
    {
        Vector2 center = playerTransform != null ? (Vector2)playerTransform.position : Vector2.zero;
        Vector2 randomDir = Random.insideUnitCircle.normalized;
        Vector2 spawnPos = center + randomDir * spawnRadius;

        GameObject enemyObj = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        Enemy enemy = enemyObj.GetComponent<Enemy>();
        if (enemy != null)
        {
            float scaledSpeed = (enemy.moveSpeed + (currentLevel - 1) * speedPerLevel)
                                * Mathf.Pow(speedMultiplierPerLevel, currentLevel - 1);
            int scaledHp = enemy.maxHealth + ((currentLevel - 1) * hpPerLevel);

            float roll = Random.value;
            if (currentLevel >= 2 && roll < 0.2f)
            {
                enemy.moveSpeed = scaledSpeed * 1.45f;
                enemy.maxHealth = Mathf.Max(1, scaledHp - 1);
                enemy.scoreValue += 5;
                enemyObj.transform.localScale *= 0.8f;

                SpriteRenderer sr = enemyObj.GetComponent<SpriteRenderer>();
                if (sr != null) sr.color = new Color(1f, 0.35f, 0.1f);
            }
            else if (currentLevel >= 3 && roll > 0.85f)
            {
                enemy.moveSpeed = scaledSpeed * 0.78f;
                enemy.maxHealth = scaledHp + 3;
                enemy.scoreValue += 15;
                enemyObj.transform.localScale *= 1.3f;

                SpriteRenderer sr = enemyObj.GetComponent<SpriteRenderer>();
                if (sr != null) sr.color = new Color(0.65f, 0.1f, 0.85f);
            }
            else
            {
                enemy.moveSpeed = scaledSpeed;
                enemy.maxHealth = scaledHp;
            }
        }
    }
}