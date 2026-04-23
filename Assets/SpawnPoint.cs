using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    public GameObject[] enemyPrefabs;
    public Transform[] spawnPoints;
    [SerializeField] private float minSpawnDelay = 4f;
    [SerializeField] private float maxSpawnDelay = 6f;

    private float GetNextSpawnDelay()
    {
        return Random.Range(minSpawnDelay, maxSpawnDelay);
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        for (int i =0; i < spawnPoints.Length; i++)
        {
            if (i == 10 || i == 12) continue;
            
            float delay = GetNextSpawnDelay();
            
            Invoke($"Spawn_{i}", delay);
        }
    }

    void Spawn_0() { SpawnEnemy(0, Vector3.down); }
    void Spawn_1() { SpawnEnemy(1, Vector3.down); }
    void Spawn_2() { SpawnEnemy(2, Vector3.down); }
    void Spawn_3() { SpawnEnemy(3, Vector3.down); }
    void Spawn_4() { SpawnEnemy(4, Vector3.down); }
    
    void Spawn_5() { SpawnEnemy(5, Vector3.left); }
    void Spawn_6() { SpawnEnemy(6, Vector3.left); }
    void Spawn_7() { SpawnEnemy(7, Vector3.right); }
    void Spawn_8() { SpawnEnemy(8, Vector3.right); }
    void Spawn_9() { SpawnEnemyToward(9, 10); }
    void Spawn_11() { SpawnEnemyToward(11, 12); }

    void SpawnEnemy(int pointIndex, Vector3 moveDir)
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0) return;
        if (pointIndex < 0 || pointIndex >= spawnPoints.Length) return;
        if (spawnPoints[pointIndex] == null) return;

        GameObject selectedPrefab = GetRandomPrefabForPoint(pointIndex);
        if (selectedPrefab == null)
        {
            Debug.LogWarning($"[SpawnPoint] pointIndex={pointIndex}에 맞는 적 프리팹이 없습니다.");
            float retryDelay = GetNextSpawnDelay();
            Invoke($"Spawn_{pointIndex}", retryDelay);
            return;
        }

        GameObject enemyGo = Instantiate(selectedPrefab);
        enemyGo.transform.position = spawnPoints[pointIndex].position;

        Enemy enemy = enemyGo.GetComponent<Enemy>();
        if (enemy != null)
            enemy.InitMove(moveDir);
        else
            Debug.LogWarning($"[SpawnPoint] {enemyGo.name}에 Enemy 컴포넌트가 없습니다.");
        
        float delay = GetNextSpawnDelay();
        Invoke($"Spawn_{pointIndex}", delay);
    }

    void SpawnEnemyToward(int fromIndex, int toIndex)
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0) return;
        if (fromIndex < 0 || fromIndex >= spawnPoints.Length) return;
        if (toIndex < 0 || toIndex >= spawnPoints.Length) return;
        if (spawnPoints[fromIndex] == null || spawnPoints[toIndex] == null) return;

        GameObject selectedPrefab = GetRandomPrefabForPoint(fromIndex);
        if (selectedPrefab == null)
        {
            Debug.LogWarning($"[SpawnPoint] fromIndex={fromIndex}에 맞는 적 프리팹이 없습니다.");
            float retryDelay = GetNextSpawnDelay();
            Invoke($"Spawn_{fromIndex}", retryDelay);
            return;
        }

        GameObject enemyGo = Instantiate(selectedPrefab);
        enemyGo.transform.position = spawnPoints[fromIndex].position;
        
        Vector3 direction = spawnPoints[toIndex].position - spawnPoints[fromIndex].position;
        if (direction.sqrMagnitude < 0.0001f)
        {
            Debug.LogWarning($"[SpawnPoint] {fromIndex} -> {toIndex} 방향이 0입니다. spawnPoints 위치를 확인하세요.");
            direction = Vector3.down;
        }

        Enemy enemy = enemyGo.GetComponent<Enemy>();
        if (enemy != null)
            enemy.InitMove(direction);
        else
            Debug.LogWarning($"[SpawnPoint] {enemyGo.name}에 Enemy 컴포넌트가 없습니다.");

        float delay = GetNextSpawnDelay();
        Invoke($"Spawn_{fromIndex}", delay);
    }

    private GameObject GetRandomPrefabForPoint(int pointIndex)
    {
        bool isUpperZone = pointIndex >= 0 && pointIndex <= 4;
        bool isLowerZone = pointIndex >= 5 && pointIndex <= 12;

        if (!isUpperZone && !isLowerZone)
        {
            return null;
        }

        GameObject[] filtered = new GameObject[enemyPrefabs.Length];
        int count = 0;

        for (int i = 0; i < enemyPrefabs.Length; i++)
        {
            GameObject prefab = enemyPrefabs[i];
            if (prefab == null) continue;

            string prefabName = prefab.name.ToLowerInvariant();

            bool isA = prefabName.Contains("enemya") || prefabName.Contains("enemy a");
            bool isB = prefabName.Contains("enemyb") || prefabName.Contains("enemy b");
            bool isC = prefabName.Contains("enemyc") || prefabName.Contains("enemy c");

            if (isUpperZone)
            {
                if (!isA && !isC) continue;
            }
            else
            {
                if (!isB) continue;
            }

            filtered[count] = prefab;
            count++;
        }

        if (count == 0)
        {
            return null;
        }

        int randomIndex = Random.Range(0, count);
        return filtered[randomIndex];
    }

    void OnDrawGizmos()
    {
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if(spawnPoints[i] == null) continue;
            
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(spawnPoints[i].position, 0.2f);
        }

        if (spawnPoints.Length > 10 && spawnPoints[9] != null && spawnPoints[10] != null)
        {
            Vector3 dir9 = spawnPoints[10].position - spawnPoints[9].position;
            DrawArrow.ForGizmo(spawnPoints[9].position, dir9, Color.green);
        }

        if (spawnPoints.Length > 12 && spawnPoints[11]  != null && spawnPoints[12] != null)
        {
            Vector3 dir11 = spawnPoints[12].position - spawnPoints[11].position;
            DrawArrow.ForGizmo(spawnPoints[11].position, dir11, Color.blue);
        }
    }
}
