using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyBullet : MonoBehaviour
{
    [SerializeField] private float speed = 7f;
    [SerializeField] private int damage = 1;
    [SerializeField] private float lifeTime = 7f;
    [SerializeField] private Vector3 moveDirection = Vector3.down;

    public void SetDirection(Vector3 direction)
    {
        if (direction.sqrMagnitude < 0.0001f)
        {
            moveDirection = Vector3.down;
            return;
        }

        moveDirection = direction.normalized;
    }

    private void Awake()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }

        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
        {
            col = gameObject.AddComponent<CircleCollider2D>();
        }

        col.isTrigger = true;
    }

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        transform.Translate(moveDirection.normalized * speed * Time.deltaTime, Space.World);

        Vector3 position = transform.position;
        if (position.x < -8f || position.x > 8f || position.y < -8f || position.y > 8f)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        playerBullet player = other.GetComponent<playerBullet>();
        bool isPlayer = other.CompareTag("Player") || player != null;
        if (!isPlayer)
        {
            return;
        }

        if (player != null)
        {
            player.TryTakeDamage();
        }
        else
        {
            test lifeManager = test.Instance;
            if (lifeManager != null && !lifeManager.IsGameOver)
            {
                lifeManager.TakeDamage(damage);
            }
        }

        Destroy(gameObject);
    }
}
