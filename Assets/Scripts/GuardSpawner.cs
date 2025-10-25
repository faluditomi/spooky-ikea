using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
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
        
        for(int i = 0; i < numberOfEnemies; i++)
        {
            if(spawnPoints.Count <= 0) break;
            
            int randomNumber = Random.Range(0, spawnPoints.Count);
            Transform spawnPoint = spawnPoints[randomNumber];
            Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity, transform);
            spawnPoints.Remove(spawnPoint);
        }
    }
}
