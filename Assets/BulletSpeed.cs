using UnityEngine;

public class BulletSpeed : MonoBehaviour
{
    public int damage = 10;
    public float speed = 10f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.up * speed * Time.deltaTime);

        if (transform.position.y > 5f)
        {
            Destroy(gameObject);
        }
    }
}