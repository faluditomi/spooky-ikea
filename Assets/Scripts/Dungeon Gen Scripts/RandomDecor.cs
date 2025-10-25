using UnityEngine;

//Class that spawns random tile decoration sets from a predefined list.
public class RandomDecor : MonoBehaviour
{
    [SerializeField] GameObject[] decorPrefabs;

    DungeonGenerator myGenerator;

    private GuardSpawner guardSpawner;

    bool isCompleted;

    private void Awake()
    {
        guardSpawner = FindFirstObjectByType<GuardSpawner>();
    }

    private void Start()
    {
        myGenerator = GameObject.Find("Generator").GetComponent<DungeonGenerator>();
    }

    private void Update()
    {
        if(!isCompleted && myGenerator.dungeonState == DungeonState.completed)
        {
            isCompleted = true;

            int decorIndex = Random.Range(0, decorPrefabs.Length);

            GameObject goDecor = Instantiate(decorPrefabs[decorIndex], transform.position, transform.rotation, transform) as GameObject;

            goDecor.name = decorPrefabs[decorIndex].name;

            guardSpawner.InitialiseGuardSpawner();
        }
    }
}
