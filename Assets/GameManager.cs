using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject[] enemies;
    public Transform[] spawnPoints;

    private float delta = 0;
    private int span = 0;

    public int sideIndex = 5;
    // Update is called once per frame
    void Update()
    {
    //     delta += Time.deltaTime;
    //
    //     if (delta > span)
    //     {
    //         CreatEnemy();
    //         delta = 0;
    //
    //         span = Random.Range(3, 5);
    //     }
        if (Input.GetMouseButtonDown(0))
        {
            if (enemies == null || enemies.Length == 0)
            {
                Debug.LogWarning("[GameManager] enemies 배열이 비어 있습니다.");
                return;
            }

            if (spawnPoints == null || spawnPoints.Length <= 9)
            {
                Debug.LogWarning("[GameManager] spawnPoints 배열에 최소 10개의 위치가 필요합니다.");
                return;
            }

            if (spawnPoints[5] == null || spawnPoints[9] == null)
            {
                Debug.LogWarning("[GameManager] spawnPoints[5] 또는 spawnPoints[9]가 비어 있습니다.");
                return;
            }

            GameObject prefab = enemies[Random.Range(0, enemies.Length)];
            GameObject enemyGo = Instantiate(prefab);

            var A = spawnPoints[5].position;
            var B = spawnPoints[9].position;

            var C = B - A;
            DrawArrow.ForDebug(A, C, 10f, Color.yellow);

            enemyGo.transform.position = A;

            var enemy = enemyGo.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.InitMove(C.normalized);
            }
        }
    }

    // private void CreatEnemy()
    // {
    //     GameObject prefab = enemies[Random.Range(0, enemies.Length)];
    //     Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
    //     GameObject enemyGo = Instantiate(prefab);
    //     enemyGo.transform.position = spawnPoint.position;
    // }
}
