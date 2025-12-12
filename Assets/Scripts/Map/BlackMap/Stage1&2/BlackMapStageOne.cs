using System.Collections;
using UnityEngine;

/// <summary>
/// Black map phase one: continuously spawn BlackMapEnemy along a line at random positions, each falling downward.
/// </summary>
public class BlackMapStageOne : MonoBehaviour
{
    [Header("Spawn Line")]
    [SerializeField] private Transform lineStart;
    [SerializeField] private Transform lineEnd;

    [Header("Enemy")]
    [SerializeField] private GameObject blackMapEnemyPrefab;
    [SerializeField] private int enemyCount = 8;
    [SerializeField] private bool loopSpawn = true;              // true: infinite loop; false: stop after enemyCount
    [SerializeField] private float spawnInterval = 0.25f;
    [SerializeField] private int maxAlive = 12;                  // 阶段内场上存活的怪物上限
    [SerializeField] private float lifetimeSeconds = 10f;        // 每个怪物的存活时间，超时自动销毁
    [SerializeField] private float initialFallSpeed = 6f;
    [SerializeField] private float positionJitter = 0.2f;        // 0-1: random offset applied to Random.value along the line

    [Header("Behaviour")]
    [SerializeField] private bool autoStartOnEnable = true;

    private Coroutine spawnRoutine;
    private readonly System.Collections.Generic.List<GameObject> aliveEnemies = new System.Collections.Generic.List<GameObject>();

    private void OnEnable()
    {
        if (autoStartOnEnable)
        {
            StartStage();
        }
    }

    private void OnDisable()
    {
        StopStage();
    }

    /// <summary>
    /// External entry point to trigger phase one spawning.
    /// </summary>
    public void StartStage()
    {
        if (spawnRoutine != null) StopCoroutine(spawnRoutine);
        spawnRoutine = StartCoroutine(SpawnLoop());
    }

    public void StopStage()
    {
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }
    }

    private IEnumerator SpawnLoop()
    {
        if (blackMapEnemyPrefab == null || lineStart == null || lineEnd == null)
        {
            yield break;
        }

        Vector3 startPos = lineStart.position;
        Vector3 endPos = lineEnd.position;

        int spawned = 0;
        while (loopSpawn || spawned < enemyCount)
        {
            float t = Mathf.Clamp01(Random.value + Random.Range(-positionJitter, positionJitter));
            Vector3 spawnPos = Vector3.Lerp(startPos, endPos, t);

            CleanupDead();
            if (maxAlive > 0 && aliveEnemies.Count >= maxAlive)
            {
                yield return new WaitForSeconds(spawnInterval);
                continue;
            }

            GameObject enemy = Instantiate(blackMapEnemyPrefab, spawnPos, Quaternion.identity);

            if (enemy != null && initialFallSpeed > 0f)
            {
                Rigidbody2D rb = enemy.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.velocity = Vector2.down * initialFallSpeed;
                }
            }
            if (enemy != null)
            {
                if (lifetimeSeconds > 0f)
                {
                    Destroy(enemy, lifetimeSeconds);
                }
                aliveEnemies.Add(enemy);
            }

            spawned++;

            if (spawnInterval > 0f)
            {
                yield return new WaitForSeconds(spawnInterval);
            }
        }

        spawnRoutine = null;
    }

    private void CleanupDead()
    {
        for (int i = aliveEnemies.Count - 1; i >= 0; i--)
        {
            if (aliveEnemies[i] == null || !aliveEnemies[i].activeInHierarchy)
            {
                aliveEnemies.RemoveAt(i);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (lineStart == null || lineEnd == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(lineStart.position, lineEnd.position);
        Gizmos.DrawWireSphere(lineStart.position, 0.1f);
        Gizmos.DrawWireSphere(lineEnd.position, 0.1f);
    }
}
