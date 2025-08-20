using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ���� ������ ���� �����ϰ� ��ġ�ϴ� ������ �����ϴ� ��ũ��Ʈ�Դϴ�.
/// ���� �����Ϳ� ��õ� ���� �ٸ� �������� �����ϰ�, Sorting Order�� �����մϴ�.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Tooltip("������ ������ �׸��� ��ǥ ����Դϴ�. �� ����� ������� ���� ��ġ�˴ϴ�.")]
    public List<Vector2Int> spawnPositions;

    public List<EnemyController> SpawnEnemiesFromGlobalData()
    {
        List<EnemyController> spawnedEnemies = new List<EnemyController>();

        if (GlobalManager.instance == null)
        {
            Debug.LogError("[EnemySpawner] GlobalManager �ν��Ͻ��� ã�� �� �����ϴ�!");
            return spawnedEnemies;
        }

        List<EnemyDataSO> enemiesToSpawnData = GlobalManager.instance.GetEnemiesForBattle();

        if (enemiesToSpawnData == null || enemiesToSpawnData.Count == 0)
        {
            Debug.LogWarning("[EnemySpawner] GlobalManager�κ��� ���޵� �� �����Ͱ� �����ϴ�. ���� �������� �ʽ��ϴ�.");
            return spawnedEnemies;
        }

        Debug.Log($"[EnemySpawner] {enemiesToSpawnData.Count}���� �� ������ ����. ������ �����մϴ�.");

        for (int i = 0; i < enemiesToSpawnData.Count; i++)
        {
            if (i >= spawnPositions.Count)
            {
                Debug.LogError($"[EnemySpawner] ���� ������ ��ġ�� �����մϴ�! {i + 1}��° ���� ������ �� �����ϴ�.");
                break;
            }

            GameObject prefabToSpawn = enemiesToSpawnData[i].enemyPrefab;

            if (prefabToSpawn == null)
            {
                Debug.LogError($"[EnemySpawner] '{enemiesToSpawnData[i].name}' �����Ϳ� �� �������� �Ҵ���� �ʾҽ��ϴ�!");
                continue;
            }

            Vector2Int spawnPos = spawnPositions[i];
            GameObject targetTile = GridManager.Instance.GetTileAtPosition(spawnPos);

            if (targetTile == null)
            {
                Debug.LogError($"[EnemySpawner] �׸��� ��ǥ {spawnPos}�� �ش��ϴ� Ÿ���� ã�� �� �����ϴ�!");
                continue;
            }

            GameObject enemyObject = Instantiate(prefabToSpawn, targetTile.transform.position, Quaternion.identity);
            EnemyController enemyController = enemyObject.GetComponent<EnemyController>();

            if (enemyController == null)
            {
                Debug.LogError($"[EnemySpawner] ������ '{prefabToSpawn.name}' �����տ� EnemyController ������Ʈ�� �����ϴ�!");
                Destroy(enemyObject);
                continue;
            }

            enemyController.enemyData = enemiesToSpawnData[i];

            // GetComponent ��� GetComponentInChildren�� ����Ͽ�,
            // ��������Ʈ�� �ڽ� ������Ʈ�� �ִ��� Ȯ���ϰ� ã�Ƴ��ϴ�.
            SpriteRenderer renderer = enemyObject.GetComponentInChildren<SpriteRenderer>();
            if (renderer != null)
            {
                // y��ǥ�� �������� Order in Layer ���� �����մϴ�.
                renderer.sortingOrder = 100 - spawnPos.y;
            }
            else
            {
                Debug.LogWarning($"[EnemySpawner] '{enemyObject.name}' ������ �Ǵ� �� �ڽĿ��� SpriteRenderer�� ã�� �� �����ϴ�.");
            }

            enemyController.MoveToTile(targetTile);
            spawnedEnemies.Add(enemyController);
        }

        Debug.Log($"[EnemySpawner] �� {spawnedEnemies.Count}���� �� ������ �Ϸ��߽��ϴ�.");
        return spawnedEnemies;
    }
}
