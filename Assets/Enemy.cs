using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : MonoBehaviour
{
    private enum FirePattern
    {
        Single = 0,
        Burst = 1,
        Spread = 2
    }

    private SpriteRenderer sr;
    [SerializeField] private int scoreValue = -1;
    private bool isDead;

    public int health;
    public Sprite[] Sprites;

    [Header("Movement")]
    public Vector3 moveDirection = Vector3.down;
    public float moveSpeed = 5f; // 적이 내려오는 속도

    [Header("Fire")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform[] bulletPoints;
    [SerializeField] private float fireInterval = 1.2f;
    [SerializeField] private float firstFireDelay = 0.4f;
    [SerializeField] private FirePattern firePattern = FirePattern.Single;
    [SerializeField] private int burstCount = 3;
    [SerializeField] private float burstShotInterval = 0.12f;
    [SerializeField] private float spreadAngle = 15f;
    [SerializeField] private bool aimAtPlayer = true;
    [SerializeField] private Transform playerTarget;

    private float fireTimer;
    private bool isBurstFiring;
    private Camera mainCamera;

    void Awake()
    {
        Rigidbody2D enemyRb = GetComponent<Rigidbody2D>();
        if (enemyRb == null)
        {
            enemyRb = gameObject.AddComponent<Rigidbody2D>();
        }

        enemyRb.gravityScale = 0f;
        enemyRb.freezeRotation = true;
        enemyRb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        Collider2D enemyCollider = GetComponent<Collider2D>();
        if (enemyCollider == null)
        {
            enemyCollider = gameObject.AddComponent<BoxCollider2D>();
        }
    }

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        CacheBulletPointsIfNeeded();
        CachePlayerTargetIfNeeded();
        mainCamera = Camera.main;
        fireTimer = -Mathf.Max(0f, firstFireDelay);
    }

    void Update()
    {
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);
        CheckOutOfScreen();

        if (isDead) return;
        if (!CanFireInCurrentPosition()) return;

        TryFire();
    }

    private bool CanFireInCurrentPosition()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (mainCamera == null)
        {
            return true;
        }

        Vector3 viewportPos = mainCamera.WorldToViewportPoint(transform.position);
        if (viewportPos.z < 0f)
        {
            return false;
        }

        return viewportPos.x >= 0f && viewportPos.x <= 1f && viewportPos.y >= 0f && viewportPos.y <= 1f;
    }

    private void TryFire()
    {
        if (isDead || bulletPrefab == null || fireInterval <= 0f || isBurstFiring)
        {
            return;
        }

        fireTimer += Time.deltaTime;
        if (fireTimer < fireInterval)
        {
            return;
        }

        fireTimer = 0f;

        if (firePattern == FirePattern.Burst)
        {
            StartCoroutine(FireBurst());
            return;
        }

        if (firePattern == FirePattern.Spread)
        {
            FireSpread();
            return;
        }

        FireSingle();
    }

    private IEnumerator FireBurst()
    {
        isBurstFiring = true;

        int shotCount = Mathf.Max(1, burstCount);
        float shotDelay = Mathf.Max(0.01f, burstShotInterval);

        for (int i = 0; i < shotCount; i++)
        {
            if (isDead) break;

            FireSingle();
            if (i < shotCount - 1)
            {
                yield return new WaitForSeconds(shotDelay);
            }
        }

        isBurstFiring = false;
    }

    private void FireSingle()
    {
        FireByPoints(0f);
    }

    private void FireSpread()
    {
        FireByPoints(0f);
        FireByPoints(spreadAngle);
        FireByPoints(-spreadAngle);
    }

    private void FireByPoints(float angleOffset)
    {
        if (bulletPoints == null || bulletPoints.Length == 0)
        {
            Vector3 direction = GetFireDirection(transform.position, angleOffset);
            SpawnBullet(transform.position, direction);
            return;
        }

        for (int i = 0; i < bulletPoints.Length; i++)
        {
            Transform point = bulletPoints[i];
            if (point == null) continue;

            Vector3 direction = GetFireDirection(point.position, angleOffset);
            SpawnBullet(point.position, direction);
        }
    }

    private Vector3 GetFireDirection(Vector3 spawnPosition, float angleOffset)
    {
        Vector3 baseDirection = Vector3.down;

        if (aimAtPlayer)
        {
            CachePlayerTargetIfNeeded();
            if (playerTarget != null)
            {
                Vector3 toPlayer = playerTarget.position - spawnPosition;
                toPlayer.z = 0f;
                if (toPlayer.sqrMagnitude > 0.0001f)
                {
                    baseDirection = toPlayer.normalized;
                }
            }
        }

        if (Mathf.Abs(angleOffset) > 0.001f)
        {
            baseDirection = (Quaternion.Euler(0f, 0f, angleOffset) * baseDirection).normalized;
        }

        return baseDirection;
    }

    private void SpawnBullet(Vector3 spawnPosition, Vector3 direction)
    {
        Object spawnedObject = Instantiate((Object)bulletPrefab, spawnPosition, Quaternion.identity);

        GameObject bullet = spawnedObject as GameObject;
        if (bullet == null && spawnedObject is Component spawnedComponent)
        {
            bullet = spawnedComponent.gameObject;
        }

        if (bullet == null)
        {
            Debug.LogWarning($"[Enemy] bulletPrefab 인스턴스화 실패: {name}, 참조 타입={spawnedObject?.GetType().Name}");
            return;
        }

        EnemyBullet bulletComponent = bullet.GetComponent<EnemyBullet>();
        if (bulletComponent != null)
        {
            bulletComponent.SetDirection(direction);
        }
    }

    private void CacheBulletPointsIfNeeded()
    {
        if (bulletPoints != null && bulletPoints.Length > 0)
        {
            return;
        }

        Transform[] allChildren = GetComponentsInChildren<Transform>(true);
        if (allChildren == null || allChildren.Length == 0)
        {
            return;
        }

        int count = 0;
        for (int i = 0; i < allChildren.Length; i++)
        {
            Transform child = allChildren[i];
            if (child == null || child == transform) continue;
            if (child.name.Contains("EnemyBulletPoint")) count++;
        }

        if (count == 0)
        {
            return;
        }

        bulletPoints = new Transform[count];
        int index = 0;

        for (int i = 0; i < allChildren.Length; i++)
        {
            Transform child = allChildren[i];
            if (child == null || child == transform) continue;
            if (!child.name.Contains("EnemyBulletPoint")) continue;

            bulletPoints[index] = child;
            index++;
        }
    }

    private void CachePlayerTargetIfNeeded()
    {
        if (playerTarget != null)
        {
            return;
        }

        playerBullet player = FindObjectOfType<playerBullet>();
        if (player != null)
        {
            playerTarget = player.transform;
        }
    }

    private void Hit(int damage)
    {
        if (isDead) return;

        health -= damage;
        sr.sprite = Sprites[1];
        Invoke("ReturnDefaultSprite", 0.1f);

        if (health <= 0)
        {
            isDead = true;

            if (test.Instance != null)
            {
                test.Instance.AddScore(GetEnemyScore());
            }

            Destroy(gameObject);
            return;
        }
    }

    private int GetEnemyScore()
    {
        if (scoreValue >= 0)
        {
            return scoreValue;
        }

        string enemyName = gameObject.name;

        if (enemyName.Contains("Enemy A")) return 100;
        if (enemyName.Contains("Enemy B")) return 150;
        if (enemyName.Contains("Enemy C")) return 300;

        return 100;
    }

    public void InitMove(Vector3 direction)
    {
        if (direction == Vector3.zero)
            moveDirection = Vector3.down;
        else
            moveDirection = direction.normalized;
    }

    private void CheckOutOfScreen()
    {
        Vector3 position = transform.position;

        if (position.x < -6f || position.x > 6f || position.y < -7f || position.y > 7f)
        {
            Destroy(gameObject);
        }
    }

    private void ReturnDefaultSprite()
    {
        sr.sprite = Sprites[0];
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("PlayerBullet"))
        {
            BulletSpeed bullet = other.gameObject.GetComponent<BulletSpeed>();

            if (bullet != null)
            {
                Hit(bullet.damage);
            }

            Destroy(other.gameObject);
        }
    }
}