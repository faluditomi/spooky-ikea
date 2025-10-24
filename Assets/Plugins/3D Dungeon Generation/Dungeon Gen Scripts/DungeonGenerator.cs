using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Collections.Generic;
using System.Collections;

public enum DungeonState
{
    inactive, generatingMain, generatingBranches, cleanup, completed
}

public class DungeonGenerator : MonoBehaviour
{
    #region Attributes
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
    [Tooltip("Color codes room lights to differentiate pathways: start(blue), main path(yellow), branch path(green), exit(magenta)")]
    [SerializeField] bool useLightsForDebugging;
    [Tooltip("Restores the room lights back to their normal color after the dungeon is generated.")]
    [SerializeField] bool restoreLightsAfterDebugging;
    [Tooltip("Runs a final, intensive check to ensure no tiles overlap and logs a warning if they do.")]
    [SerializeField] bool runFinalCollisionCheck = true;


    [Header("Key Bindings")]
    [Tooltip("Reloads the scene and generates a new dungeon.")]
    [SerializeField] KeyCode reloadKey = KeyCode.Backspace; //temp
    [Tooltip("Toggles the view between player and dungeon overview camera.")]
    [SerializeField] KeyCode toggleMapKey = KeyCode.M; //temp

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

    private GameObject goCamera;
    private GameObject goPlayer;

    private Transform tileFrom, tileTo, tileRoot;
    private Transform containter;

    private int backtrackingAttempts = 0;
    private int maxBacktrackingAttempts = 10;

    private Color startLightColor = Color.white;
    #endregion

    #region MonoBehaviour Methods
    private void Start()
    {
        goCamera = GameObject.Find("OverheadCamera");
        goPlayer = GameObject.FindWithTag("Player");

        StartCoroutine(DungeonBuildCoroutine());
    }

    private void Update()
    {
        if(Input.GetKeyDown(reloadKey)) SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        if(Input.GetKeyDown(toggleMapKey))
        {
            if(goCamera != null) goCamera.SetActive(!goCamera.activeInHierarchy);
            if(goPlayer != null) goPlayer.SetActive(!goPlayer.activeInHierarchy);
        }
    }
    #endregion

    #region Tile Creation Methods
    private Transform CreateStartTile()
    {
        int index = Random.Range(0, startPrefabs.Length);

        GameObject goTile = Instantiate(startPrefabs[index], Vector3.zero, Quaternion.identity, containter);

        goTile.name = "Start Room";
        goTile.transform.Rotate(0, Random.Range(0, 4) * 90f, 0);

        if(goPlayer != null)
        {
            goPlayer.transform.position = goTile.transform.position;
            goPlayer.transform.LookAt(goTile.GetComponentInChildren<Connector>().transform);
        }

        generatedTiles.Add(new Tile(goTile.transform, null));

        return goTile.transform;
    }

    private Transform CreateTile(Transform origin)
    {
        int index = Random.Range(0, tilePrefabs.Length);

        GameObject goTile = Instantiate(tilePrefabs[index], Vector3.zero, Quaternion.identity, containter);

        goTile.name = tilePrefabs[index].name;

        generatedTiles.Add(new Tile(goTile.transform, origin));

        return goTile.transform;
    }

    private Transform CreateExitTile(Transform origin)
    {
        int index = Random.Range(0, exitPrefabs.Length);

        GameObject goTile = Instantiate(exitPrefabs[index], Vector3.zero, Quaternion.identity, containter);

        goTile.name = "Exit Room";

        generatedTiles.Add(new Tile(goTile.transform, origin));

        return goTile.transform;
    }
    #endregion

    #region Core Generation Logic
    private Transform AttemptToPlaceTile(Transform fromTile, bool isExitTile)
    {
        List<Connector> fromConnectors = fromTile.GetComponentsInChildren<Connector>().ToList().FindAll(c => !c.isConnected);

        fromConnectors = fromConnectors.OrderBy(c => Random.value).ToList();

        foreach(Connector fromConnector in fromConnectors)
        {
            Transform toTile = isExitTile ? CreateExitTile(fromTile) : CreateTile(fromTile);

            List<Connector> toConnectors = toTile.GetComponentsInChildren<Connector>().ToList().FindAll(c => !c.isConnected);

            if(toConnectors.Count == 0)
            {
                generatedTiles.RemoveAt(generatedTiles.Count - 1);

                Destroy(toTile.gameObject);

                continue;
            }

            Connector toConnector = toConnectors[Random.Range(0, toConnectors.Count)];

            Transform connectToTransform = toConnector.transform;

            connectToTransform.SetParent(fromConnector.transform);
            toTile.SetParent(connectToTransform);
            connectToTransform.localPosition = Vector3.zero;
            connectToTransform.localRotation = Quaternion.identity;
            connectToTransform.Rotate(0, 180f, 0);
            toTile.SetParent(containter);

            Physics.SyncTransforms();

            if(CheckForOverlap(toTile, fromTile))
            {
                Destroy(toTile.gameObject);

                generatedTiles.RemoveAt(generatedTiles.Count - 1);

                continue;
            }

            fromConnector.isConnected = true;
            toConnector.isConnected = true;

            generatedTiles.Last().connector = fromConnector;

            if(fromTile.GetComponent<BoxCollider>() == null)
            {
                BoxCollider box = fromTile.gameObject.AddComponent<BoxCollider>();

                box.isTrigger = true;
            }

            return toTile;
        }

        return null;
    }

    private bool CheckForOverlap(Transform tileTo, Transform tileFrom)
    {
        BoxCollider box = tileTo.GetComponent<BoxCollider>();

        if(box == null)
        {
            box = tileTo.gameObject.AddComponent<BoxCollider>();
            box.isTrigger = true;
        }

        Vector3 offset = (tileTo.right * box.center.x) + (tileTo.up * box.center.y) + (tileTo.forward * box.center.z);
        Vector3 halfExtents = box.size / 2;

        List<Collider> hits = Physics.OverlapBox(tileTo.position + offset, halfExtents, tileTo.rotation, LayerMask.GetMask("Tile")).ToList();

        if(hits.Exists(c => c.transform != tileFrom && c.transform != tileTo)) return true;

        return false;
    }
    #endregion

    #region Cleanup Methods
    private void CleanupAndFinalize()
    {
        dungeonState = DungeonState.cleanup;

        if(runFinalCollisionCheck) FinalCollisionCheck();

        LightRestoration();
        CleanupCollisionBoxes();
        BlockPassages();
        SpawnDoorways();

        dungeonState = DungeonState.completed;
    }

    private void FinalCollisionCheck()
    {
        for(int i = 0; i < generatedTiles.Count; i++)
        {
            Tile tileA = generatedTiles[i];

            if(tileA.tile == null) continue;

            BoxCollider boxA = tileA.tile.GetComponent<BoxCollider>();

            if(boxA == null) continue;

            Vector3 centerA = tileA.tile.position + (tileA.tile.right * boxA.center.x) + (tileA.tile.up * boxA.center.y) + (tileA.tile.forward * boxA.center.z);
            Vector3 halfExtentsA = boxA.size / 2;

            Collider[] hits = Physics.OverlapBox(centerA, halfExtentsA, tileA.tile.rotation, LayerMask.GetMask("Tile"));

            foreach(Collider hitCollider in hits)
            {
                if(hitCollider == boxA) continue;

                Tile tileB = generatedTiles.Find(t => t.tile == hitCollider.transform);

                if(tileB != null)
                {
                    if(tileA.origin == tileB.tile || tileB.origin == tileA.tile) continue;

                    int indexB = generatedTiles.IndexOf(tileB);

                    if(i < indexB)
                    {
                        Debug.LogWarning($"FINAL CHECK: Overlap detected between '{tileA.tile.name}' at {tileA.tile.position} and '{tileB.tile.name}' at {tileB.tile.position}.", tileA.tile.gameObject);
                    }
                }
            }
        }
    }


    private void CleanupCollisionBoxes()
    {
        foreach(Tile myTile in generatedTiles)
        {
            if(myTile.tile == null) continue;

            BoxCollider box = myTile.tile.GetComponent<BoxCollider>();

            if(box != null) Destroy(box);
        }
    }

    private void BlockPassages()
    {
        foreach(Connector connector in transform.GetComponentsInChildren<Connector>())
        {
            if(connector != null && !connector.isConnected)
            {
                Vector3 pos = connector.transform.position;

                int wallIndex = Random.Range(0, blockedPrefabs.Length);

                GameObject goWall = Instantiate(blockedPrefabs[wallIndex], pos, connector.transform.rotation, connector.transform);

                goWall.name = blockedPrefabs[wallIndex].name;
            }
        }
    }

    private void SpawnDoorways()
    {
        if(doorwayPercent <= 0) return;

        foreach(var tile in generatedTiles)
        {
            if(tile?.connector == null) continue;

            if(tile.connector.isConnected)
            {
                int roll = Random.Range(1, 101);

                if(roll <= doorwayPercent)
                {
                    Vector3 pos = tile.connector.transform.position;

                    if(Physics.CheckBox(pos, Vector3.one * 0.5f, Quaternion.identity, LayerMask.GetMask("Door"))) continue;

                    int doorIndex = Random.Range(0, doorwayPrefabs.Length);

                    Instantiate(doorwayPrefabs[doorIndex], pos, tile.connector.transform.rotation, tile.connector.transform);
                }
            }
        }
    }
    #endregion

    #region Debugging Methods
    private void DebugRoomLighting(Transform tile, Color lightColor)
    {
        if(useLightsForDebugging && Application.isEditor && tile != null)
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

    private void LightRestoration()
    {
        if(useLightsForDebugging && restoreLightsAfterDebugging && Application.isEditor)
        {
            Light[] lights = transform.GetComponentsInChildren<Light>();

            foreach(Light light in lights)
            {
                if(light != null) light.color = startLightColor;
            }
        }
    }
    #endregion

    #region Coroutine
    IEnumerator DungeonBuildCoroutine()
    {
        if(goCamera != null) goCamera.SetActive(true);
        if(goPlayer != null) goPlayer.SetActive(false);

        dungeonState = DungeonState.generatingMain;

        GameObject goContainer = new GameObject("Main Path");

        containter = goContainer.transform;
        containter.SetParent(transform);

        tileRoot = CreateStartTile();

        DebugRoomLighting(tileRoot, Color.cyan);

        tileTo = tileRoot;

        int mainPathCount = 1;

        while(mainPathCount < mainLength)
        {
            if(constructionDelay > 0) yield return new WaitForSeconds(constructionDelay);

            tileFrom = tileTo;

            bool isExit = mainPathCount == mainLength - 1;

            Transform newTile = AttemptToPlaceTile(tileFrom, isExit);

            if(newTile != null)
            {
                tileTo = newTile;

                mainPathCount++;

                backtrackingAttempts = 0;

                DebugRoomLighting(tileTo, isExit ? Color.magenta : Color.yellow);
            }
            else
            {
                backtrackingAttempts++;

                if(backtrackingAttempts >= maxBacktrackingAttempts)
                {
                    Debug.LogWarning("Max backtracking attempts reached. Halting generation.");

                    break;
                }

                Tile lastGoodTile = generatedTiles.Find(t => t.tile == tileFrom);

                if(lastGoodTile != null && lastGoodTile.origin != null)
                {
                    tileTo = lastGoodTile.origin;
                }
                else
                {
                    Debug.LogWarning("Could not backtrack further. Halting generation.");

                    break;
                }
            }
        }

        foreach(Tile tile in generatedTiles)
        {
            if(tile.tile == null) continue;

            foreach(Connector connector in tile.tile.GetComponentsInChildren<Connector>())
            {
                if(!connector.isConnected) availableConnectors.Add(connector);
            }
        }

        dungeonState = DungeonState.generatingBranches;

        for(int j = 0; j < numBranches; j++)
        {
            if(availableConnectors.Count == 0) break;

            goContainer = new GameObject("Branch " + (j + 1));

            containter = goContainer.transform;
            containter.SetParent(transform);

            int availIndex = Random.Range(0, availableConnectors.Count);

            Connector branchStartConnector = availableConnectors[availIndex];

            availableConnectors.RemoveAt(availIndex);

            if(branchStartConnector == null) continue;

            tileRoot = branchStartConnector.transform.parent.parent;
            tileTo = tileRoot;

            for(int i = 0; i < branchLength; i++)
            {
                if(constructionDelay > 0) yield return new WaitForSeconds(constructionDelay);

                tileFrom = tileTo;

                Transform newBranchTile = AttemptToPlaceTile(tileFrom, false);

                if(newBranchTile != null)
                {
                    tileTo = newBranchTile;

                    DebugRoomLighting(tileTo, Color.green);

                    foreach(Connector c in newBranchTile.GetComponentsInChildren<Connector>())
                    {
                        if(!c.isConnected) availableConnectors.Add(c);
                    }
                }
                else
                {
                    break;
                }
            }
        }

        CleanupAndFinalize();

        yield return new WaitForSeconds(0.5f);

        if(goCamera != null) goCamera.SetActive(false);
        if(goPlayer != null) goPlayer.SetActive(true);
    }
    #endregion
}