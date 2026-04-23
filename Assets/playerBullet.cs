using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class playerBullet : MonoBehaviour
{
    [SerializeField] private Transform point;
    [SerializeField] private GameObject playerBullerPrefad;
    [SerializeField] private GameObject sideBulletPrefad;
    [SerializeField] private float fireRate = 0.1f;
    
    [SerializeField] private  float speed = 10f; //총알 이동속도


    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    private Vector2 xLimit = new Vector2(-2.5f, 2.5f);
    private Vector2 yLimit = new Vector2(-4.5f, 5f);

    private Animator _animator;

    public float bullerSpacing = 0.5f;
    public float sideSpacing = 0.2f;
    public int damage = 10;
    public int power = 1;

    public float _fireTimer;
    [SerializeField] private test lifeManager;
    [SerializeField] private float invincibleDuration = 1f;
    [SerializeField] private float invincibleBlinkInterval = 0.1f;
    [SerializeField] private float blinkAlpha = 0.35f;
    private Rigidbody2D playerRb;
    private Collider2D playerCollider;
    private bool isInvincible;
    private SpriteRenderer[] cachedRenderers;

    void Awake()
    {
        playerRb = GetComponent<Rigidbody2D>();
        if (playerRb == null)
        {
            playerRb = gameObject.AddComponent<Rigidbody2D>();
        }

        playerRb.gravityScale = 0f;
        playerRb.freezeRotation = true;
        playerRb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        playerCollider = GetComponent<Collider2D>();
        if (playerCollider == null)
        {
            playerCollider = gameObject.AddComponent<BoxCollider2D>();
        }

        Debug.Log($"[Player] Physics setup 완료 - Rigidbody2D={playerRb != null}, Collider2D={playerCollider != null}, IsTrigger={playerCollider.isTrigger}");
    }

    void Start()
    {
        _fireTimer = fireRate;
        _animator = GetComponent<Animator>();
        cachedRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        bool connected = ResolveLifeManager();
        Debug.Log("[Player] lifeManager 연결: " + (connected ? "성공" : "실패"));
    }

    // Update is called once per frame
    void Update()
    {
        if (ResolveLifeManager() && lifeManager.IsPlayerControlLocked)
        {
            _animator.SetInteger("dirX", 0);
            return;
        }

        Move();

        _fireTimer += Time.deltaTime;

        if (Input.GetMouseButton(0))
        {
            if (_fireTimer >= fireRate)
            {
                FireTheBullets();
                _fireTimer = 0;
            }
        }
        
    }

    private void Move()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 dir = new Vector3(h, v, 0f).normalized;
        Vector3 newPos = transform.position + dir * moveSpeed * Time.deltaTime;

        newPos.x = Mathf.Clamp(newPos.x, xLimit.x, xLimit.y);
        newPos.y = Mathf.Clamp(newPos.y, yLimit.x, yLimit.y);

        transform.position = newPos;

        _animator.SetInteger("dirX", (int)h);
    }

    private void FireTheBullets()
    {
        switch (power)
        {
            case 1:
                SpawnBullets(sideBulletPrefad, Vector3.zero);
                break;
            
            case 2:
                SpawnBullets(sideBulletPrefad, Vector3.left * sideSpacing);
                SpawnBullets(sideBulletPrefad,Vector3.right * sideSpacing);
                break;
            
            case 3:
                SpawnBullets(playerBullerPrefad, Vector3.zero);
                SpawnBullets(sideBulletPrefad, Vector3.right * sideSpacing);
                SpawnBullets(sideBulletPrefad, Vector3.left * sideSpacing);
                break;
        }
    }

    private void SpawnBullets(GameObject prefab, Vector3 offset)
    {
        Instantiate(prefab, point.position + offset, point.rotation);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[Player] Trigger 충돌 감지: {other.name}, tag={other.tag}");

        HandleEnemyCollision(other.gameObject, "Trigger");
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"[Player] Collision 충돌 감지: {collision.gameObject.name}, tag={collision.gameObject.tag}");

        HandleEnemyCollision(collision.gameObject, "Collision");
    }

    private void HandleEnemyCollision(GameObject enemyObject, string collisionType)
    {
        string enemyTag = enemyObject.tag;
        bool isEnemy = enemyTag == "Enemy" || enemyTag == "enemy";

        if (!isEnemy) return;

        Debug.Log($"[Player] Enemy와 {collisionType} 충돌: {enemyObject.name}");

        bool connected = ResolveLifeManager();
        Debug.Log("[Player] lifeManager 재검색: " + (connected ? "성공" : "실패"));

        TryTakeDamage();

        Destroy(enemyObject);
    }

    public bool TryTakeDamage()
    {
        if (isInvincible)
        {
            Debug.Log("[Player] 무적시간 중이라 피격 무시");
            return false;
        }

        PlayerHpDown();
        return true;
    }

    private bool ResolveLifeManager()
    {
        if (lifeManager != null)
        {
            return true;
        }

        if (test.Instance != null)
        {
            lifeManager = test.Instance;
            return true;
        }

        test[] allManagers = Resources.FindObjectsOfTypeAll<test>();
        for (int i = 0; i < allManagers.Length; i++)
        {
            test candidate = allManagers[i];
            if (candidate == null) continue;
            if (!candidate.gameObject.scene.IsValid()) continue;

            lifeManager = candidate;
            return true;
        }

        return false;
    }

    private void PlayerHpDown()
    {
        bool connected = ResolveLifeManager();
        if (!connected || lifeManager == null || lifeManager.IsGameOver)
        {
            Debug.LogWarning("[Player] HP 감소 실패: lifeManager가 없거나 이미 게임오버 상태");
            return;
        }

        Debug.Log("[Player] HP 1 감소 처리");
        lifeManager.TakeDamage(1);
        StartCoroutine(PlayerInvincibleRoutine());
    }

    private System.Collections.IEnumerator PlayerInvincibleRoutine()
    {
        isInvincible = true;

        float elapsed = 0f;
        bool dim = false;
        float interval = Mathf.Max(0.03f, invincibleBlinkInterval);

        while (elapsed < invincibleDuration)
        {
            dim = !dim;
            SetPlayerAlpha(dim ? blinkAlpha : 1f);

            yield return new WaitForSeconds(interval);
            elapsed += interval;
        }

        SetPlayerAlpha(1f);
        isInvincible = false;
    }

    private void SetPlayerAlpha(float alpha)
    {
        if (cachedRenderers == null || cachedRenderers.Length == 0)
        {
            cachedRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        }

        float targetAlpha = Mathf.Clamp01(alpha);
        for (int i = 0; i < cachedRenderers.Length; i++)
        {
            SpriteRenderer sr = cachedRenderers[i];
            if (sr == null) continue;

            Color color = sr.color;
            color.a = targetAlpha;
            sr.color = color;
        }
    }
}

