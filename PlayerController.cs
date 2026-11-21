using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Di chuyển & Ngắm")]
    public float moveSpeed = 6f;
    public float speedBonusPerLevel = 0.4f;
    public Camera mainCamera;
    public Transform visualTransform; // Kéo object Visual vào đây

    [Header("Bắn đạn")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRadius = 0.6f; // Khoảng cách từ tâm người tới điểm bắn
    public float fireCooldown = 0.2f;
    public float minFireCooldown = 0.07f;
    public float cooldownReducePerLevel = 0.02f;

    [Header("Số đường đạn theo Level")]
    public int bulletCount = 1;
    public float spreadAngle = 12f;

    [Header("Máu (HP)")]
    public int maxHealth = 100;
    public int healPerLevel = 20;
    public float invulnerableTime = 0.5f;

    private int currentHealth;
    private float nextInvulnerableTime;
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 mouseWorldPos;
    private float nextFireTime;
    private float aimAngle;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        currentHealth = maxHealth;
    }

    void Start()
    {
        if (GameUI.Instance != null)
        {
            GameUI.Instance.UpdateHP(currentHealth, maxHealth);
        }
    }

    void Update()
    {
        if (Time.timeScale == 0f) return;

        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        moveInput = moveInput.normalized;

        if (mainCamera != null)
        {
            mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        }

        // Giữ Player luôn đứng thẳng
        rb.rotation = 0f;

        Vector2 aimDir = (mouseWorldPos - rb.position).normalized;
        if (aimDir.sqrMagnitude > 0.001f)
        {
            aimAngle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg - 90f;

            // Lật nhân vật quay trái / phải theo hướng chuột
            if (visualTransform != null)
            {
                Vector3 scale = visualTransform.localScale;
                if (aimDir.x < -0.05f)
                    scale.x = -Mathf.Abs(scale.x);
                else if (aimDir.x > 0.05f)
                    scale.x = Mathf.Abs(scale.x);
                visualTransform.localScale = scale;
            }

            // Cho FirePoint xoay quanh người nhân vật theo hướng chuột
            if (firePoint != null)
            {
                firePoint.position = rb.position + aimDir * fireRadius;
                firePoint.rotation = Quaternion.Euler(0f, 0f, aimAngle);
            }
        }

        if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireCooldown;
        }
    }

    void FixedUpdate()
    {
        if (Time.timeScale == 0f) return;

        rb.MovePosition(rb.position + moveInput * moveSpeed * Time.fixedDeltaTime);
    }

    void Shoot()
    {
        if (bulletPrefab == null || firePoint == null) return;

        int count = Mathf.Max(1, bulletCount);
        float totalSpread = spreadAngle * (count - 1);
        float startOffset = -totalSpread / 2f;

        for (int i = 0; i < count; i++)
        {
            float offsetAngle = startOffset + spreadAngle * i;
            Quaternion rot = firePoint.rotation * Quaternion.Euler(0f, 0f, offsetAngle);
            Instantiate(bulletPrefab, firePoint.position, rot);
        }
    }

    public void OnLevelUp(int newLevel)
    {
        moveSpeed += speedBonusPerLevel;
        fireCooldown = Mathf.Max(minFireCooldown, fireCooldown - cooldownReducePerLevel);
        bulletCount += 1;

        currentHealth = Mathf.Min(maxHealth, currentHealth + healPerLevel);
        if (GameUI.Instance != null)
        {
            GameUI.Instance.UpdateHP(currentHealth, maxHealth);
        }
    }

    public void TakeDamage(int damage)
    {
        if (Time.time < nextInvulnerableTime || currentHealth <= 0) return;

        currentHealth -= damage;
        nextInvulnerableTime = Time.time + invulnerableTime;

        if (GameUI.Instance != null)
        {
            GameUI.Instance.UpdateHP(currentHealth, maxHealth);
        }

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            if (GameUI.Instance != null)
            {
                GameUI.Instance.ShowGameOver();
            }
            else
            {
                Time.timeScale = 0f;
            }
        }
    }
}