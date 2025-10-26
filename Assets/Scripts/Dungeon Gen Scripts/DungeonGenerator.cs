using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public enum DungeonState { inactive, generatingMain, generatingBranches, cleanup, completed }

public class DungeonGenerator : MonoBehaviour
{
    [Tooltip("List of normal tiles.")]
    [SerializeField] GameObject[] tilePrefabs;
    [Tooltip("List of starting tiles.")]
    [SerializeField] GameObject[] startPrefabs;
    [Tooltip("List of exit tiles.")]
    [SerializeField] GameObject[] exitPrefabs;
    [Tooltip("List of blocked path panels.")]
    [SerializeField] GameObject[] blockedPrefabs;
    [Tooltip("List of doorways.")]
    [SerializeField] GameObject[] doorwayPrefabs;

    [Header("Debugging Options")]
    [Tooltip("Uses box colliders to check for tile overlaps. Disable for complex tiles with non-box shapes.")]
    [SerializeField] bool useBoxColliders;
    [Tooltip("Color codes room lights to differentiate pathways: start(blue), main path(yellow), branch path(green), exit(magenta)")]
    [SerializeField] bool useLightsForDebugging;
    [Tooltip("Restores the room lights back to their normal color after the dungeon is generated.")]
    [SerializeField] bool restoreLightsAfterDebugging;
    [SerializeField] private GameObject loadingScreen;

    [Header("Generation Limits")]
    [Tooltip("Length of the main path.")]
    [Range(2, 100)][SerializeField] int mainLength = 10;
    [Tooltip("Length of the branch path.")]
    [Range(0, 50)][SerializeField] int branchLength = 5;
    [Tooltip("Number of branches.")]
    [Range(0, 25)][SerializeField] int numBranches = 10;
    [Tooltip("Percentage of a doorway to spawn in between tiles.")]
    [Range(0, 100)][SerializeField] int doorwayPercent = 25;
    [Tooltip("Delay for dungeon generation for dramatic effect.")]
    [Range(0, 1f)][SerializeField] float constructionDelay;

    [Header("Available at Runtime")]
    [HideInInspector] public DungeonState dungeonState = DungeonState.inactive;

    public List<Tile> generatedTiles = new List<Tile>();
    private List<Connector> availableConnectors = new List<Connector>();

    [SerializeField] private PlayerController playerController;

    private Transform tileFrom, tileTo, tileRoot;
    private Transform container;

    int attempts;
    int maxAttempts = 50;

    private Color startLightColor = Color.white;

    private void Start()
    {
        LoadingScreen();
        StartCoroutine(DungeonBuildCoroutine());
    }

    private IEnumerator DungeonBuildCoroutine()
    {
        GameObject goContainer = new GameObject("Main Path");

        container = goContainer.transform;
        container.SetParent(transform);
        tileRoot = CreateStartTile();

        DebugRoomLighting(tileRoot, Color.cyan);

        tileTo = tileRoot;
        dungeonState = DungeonState.generatingMain;

        while (generatedTiles.Count < mainLength)
        {
            yield return new WaitForSeconds(constructionDelay);

            tileFrom = tileTo;

            if (generatedTiles.Count == mainLength - 1)
            {
                tileTo = CreateExitTile();

                DebugRoomLighting(tileTo, Color.magenta);
            }
            else
            {
                tileTo = CreateTile();

                DebugRoomLighting(tileTo, Color.yellow);
            }

            ConnectTiles();
            CollisionCheck();
        }

        foreach(Connector connector in container.GetComponentsInChildren<Connector>())
        {
            if(!connector.isConnected)
            {
                if(!availableConnectors.Contains(connector))
                {
                    availableConnectors.Add(connector);
                }
            }
        }

        dungeonState = DungeonState.generatingBranches;

        for(int b = 0; b < numBranches; b++)
        {
            if(availableConnectors.Count > 0)
            {
                goContainer = new GameObject("Branch " + (b + 1));

                container = goContainer.transform;
                container.SetParent(transform);

                int availIndex = Random.Range(0, availableConnectors.Count);

                tileRoot = availableConnectors[availIndex].transform.parent.parent;
                availableConnectors.RemoveAt(availIndex);
                tileTo = tileRoot;

                for (int i = 0; i < branchLength - 1; i++)
                {
                    yield return new WaitForSeconds(constructionDelay);

                    tileFrom = tileTo;
                    tileTo = CreateTile();

                    DebugRoomLighting(tileTo, Color.green);
                    ConnectTiles();
                    CollisionCheck();

                    if (attempts >= maxAttempts) break;
                }
            }
            else
            {
                break;
            }
        }

        int validBranchCount = 0;

        foreach (Transform child in transform)
        {
            if (child.name.Contains("Branch") && child.childCount > 0)
            {
                validBranchCount++;
            }
        }

        if (numBranches > 2 && validBranchCount <= 2)
        {
            SceneManager.LoadScene("Game too");

            yield break;
        }

        dungeonState = DungeonState.cleanup;

        LightRestoration();
        CleanupBoxes();
        BlockedPassages();

        dungeonState = DungeonState.completed;

        LoadingScreenDone();

        yield return null;
    }

    private void LoadingScreen()
    {
        loadingScreen.SetActive(true);
        playerController.LockMovement(true);

        //load sounds here?
    }

    private void LoadingScreenDone()
    {
        loadingScreen.SetActive(false);
        playerController.UnlockMovement();
    }

    private void BlockedPassages()
    {
        foreach(Connector connector in transform.GetComponentsInChildren<Connector>())
        {
            if(!connector.isConnected)
            {
                Vector3 pos = connector.transform.position;

                int wallIndex = Random.Range(0, blockedPrefabs.Length);

                GameObject goWall = Instantiate(blockedPrefabs[wallIndex], pos, connector.transform.rotation, connector.transform) as GameObject;

                goWall.name = blockedPrefabs[wallIndex].name;
            }
        }
    }

    private void CollisionCheck()
    {
        BoxCollider box = tileTo.GetComponent<BoxCollider>();

        if(box == null)
        {
            box = tileTo.gameObject.AddComponent<BoxCollider>();
            box.isTrigger = true;
        }

        Vector3 offset = (tileTo.right * box.center.x) + (tileTo.up * box.center.y) + (tileTo.forward * box.center.z);
        Vector3 halfExtents = box.bounds.extents;

        List<Collider> hits = Physics.OverlapBox(tileTo.position + offset, halfExtents, Quaternion.identity, LayerMask.GetMask("Tile")).ToList();

        if(hits.Count > 0)
        {
            if(hits.Exists(x => x.transform != tileFrom && x.transform != tileTo))
            {
                attempts++
                    ;
                int toIndex = generatedTiles.FindIndex(x => x.tile == tileTo);

                if(generatedTiles[toIndex].connector != null)
                {
                    generatedTiles[toIndex].connector.isConnected = false;
                }

                generatedTiles.RemoveAt(toIndex);

                DestroyImmediate(tileTo.gameObject);

                if(attempts >= maxAttempts)
                {
                    int fromIndex = generatedTiles.FindIndex(x => x.tile == tileFrom);

                    Tile myTileFrom = generatedTiles[fromIndex];

                    if(tileFrom != tileRoot)
                    {
                        if(myTileFrom.connector != null)
                        {
                            myTileFrom.connector.isConnected = false;
                        }

                        availableConnectors.RemoveAll(x => x.transform.parent.parent == tileFrom);
                        generatedTiles.RemoveAt(fromIndex);

                        DestroyImmediate(tileFrom.gameObject);

                        if(myTileFrom.origin != tileRoot)
                        {
                            tileFrom = myTileFrom.origin;
                        }
                        else if(container.name.Contains("Main"))
                        {
                            if(myTileFrom.origin != null)
                            {
                                tileRoot = myTileFrom.origin;
                                tileFrom = tileRoot;
                            }
                        }
                        else if(availableConnectors.Count > 0)
                        {
                            int availIndex = Random.Range(0, availableConnectors.Count);

                            tileRoot = availableConnectors[availIndex].transform.parent.parent;
                            availableConnectors.RemoveAt(availIndex);
                            tileFrom = tileRoot;
                        }
                        else
                        {
                            return;
                        }

                    }
                    else if(container.name.Contains("Main"))
                    {
                        if(myTileFrom.origin != null)
                        {
                            tileRoot = myTileFrom.origin;
                            tileFrom = tileRoot;
                        }
                    }
                    else if(availableConnectors.Count > 0)
                    {
                        int availIndex = Random.Range(0, availableConnectors.Count);

                        tileRoot = availableConnectors[availIndex].transform.parent.parent;
                        availableConnectors.RemoveAt(availIndex);
                        tileFrom = tileRoot;
                    }
                    else
                    {
                        return;
                    }
                }

                if(tileFrom != null)
                {
                    if(generatedTiles.Count == mainLength - 1)
                    {
                        tileTo = CreateExitTile();

                        DebugRoomLighting(tileTo, Color.magenta);
                    }
                    else
                    {
                        tileTo = CreateTile();

                        Color retryColor = container.name.Contains("Branch") ? Color.green : Color.yellow;

                        DebugRoomLighting(tileTo, retryColor * 2f);
                    }

                    ConnectTiles();
                    CollisionCheck();
                }
            }
            else
            {
                attempts = 0;
            }
        }
    }

    private void LightRestoration()
    {
        if(useLightsForDebugging && restoreLightsAfterDebugging && Application.isEditor)
        {
            Light[] lights = transform.GetComponentsInChildren<Light>();

            foreach(Light light in lights)
            {
                light.color = startLightColor;
            }
        }
    }

    private void CleanupBoxes()
    {
        if(!useBoxColliders)
        {
            foreach(Tile myTile in generatedTiles)
            {
                BoxCollider box = myTile.tile.GetComponent<BoxCollider>();

                if(box != null)
                {
                    Destroy(box);
                }
            }
        }
    }

    private void DebugRoomLighting(Transform tile, Color lightColor)
    {
        if(useLightsForDebugging && Application.isEditor)
        {
            Light[] lights = tile.GetComponentsInChildren<Light>();

            if(lights.Length > 0)
            {
                if(startLightColor == Color.white)
                {
                    startLightColor = lights[0].color;
                }

                foreach(Light light in lights)
                {
                    light.color = lightColor;
                }
            }
        }
    }

    private void ConnectTiles()
    {
        Transform connectFrom = GetRandomConnector(tileFrom);

        if(connectFrom == null) return;

        Transform connectTo = GetRandomConnector(tileTo);

        if(connectTo == null) return;

        connectTo.SetParent(connectFrom);

        tileTo.SetParent(connectTo);

        connectTo.localPosition = Vector3.zero;
        connectTo.localRotation = Quaternion.identity;
        connectTo.Rotate(0, 180f, 0);

        tileTo.SetParent(container);

        connectTo.SetParent(tileTo.Find("Connectors"));

        generatedTiles.Last().connector = connectFrom.GetComponent<Connector>();
    }

    private Transform GetRandomConnector(Transform tile)
    {
        if(tile == null) return null;

        List<Connector> connectorList = tile.GetComponentsInChildren<Connector>().ToList().FindAll(x => x.isConnected == false);

        if(connectorList.Count > 0)
        {
            int connectorIndex = Random.Range(0, connectorList.Count);

            connectorList[connectorIndex].isConnected = true;

            if(tile == tileFrom)
            {
                BoxCollider box = tile.GetComponent<BoxCollider>();

                if(box == null)
                {
                    box = tile.gameObject.AddComponent<BoxCollider>();
                    box.isTrigger = true;
                }
            }

            return connectorList[connectorIndex].transform;
        }

        return null;
    }

    private Transform CreateTile()
    {
        int index = Random.Range(0, tilePrefabs.Length);

        GameObject goTile = Instantiate(tilePrefabs[index], Vector3.zero, Quaternion.identity, container) as GameObject;

        goTile.name = tilePrefabs[index].name;

        Transform origin = generatedTiles[generatedTiles.FindIndex(x => x.tile == tileFrom)].tile;

        generatedTiles.Add(new Tile(goTile.transform, origin));

        return goTile.transform;
    }

    private Transform CreateExitTile()
    {
        int index = Random.Range(0, exitPrefabs.Length);

        GameObject goTile = Instantiate(exitPrefabs[index], Vector3.zero, Quaternion.identity, container) as GameObject;

        goTile.name = "Exit Room";

        Transform origin = generatedTiles[generatedTiles.FindIndex(x => x.tile == tileFrom)].tile;

        generatedTiles.Add(new Tile(goTile.transform, origin));

        return goTile.transform;
    }

    private Transform CreateStartTile()
    {
        int index = Random.Range(0, startPrefabs.Length);

        GameObject goTile = Instantiate(startPrefabs[index], Vector3.zero, Quaternion.identity, container) as GameObject;

        goTile.name = "Start Room";

        generatedTiles.Add(new Tile(goTile.transform, null));

        return goTile.transform;
    }
}
