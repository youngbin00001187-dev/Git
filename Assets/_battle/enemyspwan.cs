using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 전투 씬에서 적을 생성하고 배치하는 역할을 전담하는 스크립트입니다.
/// 이제 데이터에 명시된 각기 다른 프리팹을 생성하고, Sorting Order를 설정합니다.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Tooltip("적들이 생성될 그리드 좌표 목록입니다. 이 목록의 순서대로 적이 배치됩니다.")]
    public List<Vector2Int> spawnPositions;

    public List<EnemyController> SpawnEnemiesFromGlobalData()
    {
        List<EnemyController> spawnedEnemies = new List<EnemyController>();

        if (GlobalManager.instance == null)
        {
            Debug.LogError("[EnemySpawner] GlobalManager 인스턴스를 찾을 수 없습니다!");
            return spawnedEnemies;
        }

        List<EnemyDataSO> enemiesToSpawnData = GlobalManager.instance.GetEnemiesForBattle();

        if (enemiesToSpawnData == null || enemiesToSpawnData.Count == 0)
        {
            Debug.LogWarning("[EnemySpawner] GlobalManager로부터 전달된 적 데이터가 없습니다. 적을 생성하지 않습니다.");
            return spawnedEnemies;
        }

        Debug.Log($"[EnemySpawner] {enemiesToSpawnData.Count}명의 적 데이터 수신. 생성을 시작합니다.");

        for (int i = 0; i < enemiesToSpawnData.Count; i++)
        {
            if (i >= spawnPositions.Count)
            {
                Debug.LogError($"[EnemySpawner] 적을 생성할 위치가 부족합니다! {i + 1}번째 적을 생성할 수 없습니다.");
                break;
            }

            GameObject prefabToSpawn = enemiesToSpawnData[i].enemyPrefab;

            if (prefabToSpawn == null)
            {
                Debug.LogError($"[EnemySpawner] '{enemiesToSpawnData[i].name}' 데이터에 적 프리팹이 할당되지 않았습니다!");
                continue;
            }

            Vector2Int spawnPos = spawnPositions[i];
            GameObject targetTile = GridManager.Instance.GetTileAtPosition(spawnPos);

            if (targetTile == null)
            {
                Debug.LogError($"[EnemySpawner] 그리드 좌표 {spawnPos}에 해당하는 타일을 찾을 수 없습니다!");
                continue;
            }

            GameObject enemyObject = Instantiate(prefabToSpawn, targetTile.transform.position, Quaternion.identity);
            EnemyController enemyController = enemyObject.GetComponent<EnemyController>();

            if (enemyController == null)
            {
                Debug.LogError($"[EnemySpawner] 생성된 '{prefabToSpawn.name}' 프리팹에 EnemyController 컴포넌트가 없습니다!");
                Destroy(enemyObject);
                continue;
            }

            enemyController.enemyData = enemiesToSpawnData[i];

            // GetComponent 대신 GetComponentInChildren을 사용하여,
            // 스프라이트가 자식 오브젝트에 있더라도 확실하게 찾아냅니다.
            SpriteRenderer renderer = enemyObject.GetComponentInChildren<SpriteRenderer>();
            if (renderer != null)
            {
                // y좌표를 기준으로 Order in Layer 값을 설정합니다.
                renderer.sortingOrder = 100 - spawnPos.y;
            }
            else
            {
                Debug.LogWarning($"[EnemySpawner] '{enemyObject.name}' 프리팹 또는 그 자식에서 SpriteRenderer를 찾을 수 없습니다.");
            }

            enemyController.MoveToTile(targetTile);
            spawnedEnemies.Add(enemyController);
        }

        Debug.Log($"[EnemySpawner] 총 {spawnedEnemies.Count}명의 적 생성을 완료했습니다.");
        return spawnedEnemies;
    }
}
