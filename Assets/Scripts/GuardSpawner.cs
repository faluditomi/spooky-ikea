using System.Collections.Generic;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEngine;

public class GuardSpawner : MonoBehaviour
{
    private List<Transform> spawnPoints;

    private GameObject enemyPrefab;

    private NavMeshSurface navMeshSurface;

    public int numberOfEnemies = 10;

    private void Awake()
    {
        enemyPrefab = Resources.Load<GameObject>("Prefabs/Guard/Guard");

        navMeshSurface = FindFirstObjectByType<NavMeshSurface>();
    }

    public void InitialiseGuardSpawner()
    {
        navMeshSurface.BuildNavMesh();

        spawnPoints = new List<Transform>();

        foreach(GameObject gameObject in GameObject.FindGameObjectsWithTag("EnemySpawn"))
        {
            spawnPoints.Add(gameObject.transform);
        }
        
        List<Transform> waypoints = new List<Transform>(spawnPoints);
        
        for(int i = 0; i < numberOfEnemies; i++)
        {
            if(spawnPoints.Count <= 0) break;
            
            int randomNumber = Random.Range(0, spawnPoints.Count);
            Transform spawnPoint = spawnPoints[randomNumber];
            GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity, transform);

            List<Transform> potentialEndWaypoints = new List<Transform>(waypoints);
            potentialEndWaypoints.Remove(spawnPoint);
            randomNumber = Random.Range(0, potentialEndWaypoints.Count);
            Transform endWaypoint = potentialEndWaypoints[randomNumber];
            enemy.GetComponent<GuardPatrolBehaviour>().SetTwoWaypoints(spawnPoint, endWaypoint);

            spawnPoints.Remove(spawnPoint);
        }
    }
}
