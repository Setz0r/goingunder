using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;

[Serializable]
public enum GameState : int
{
    MainMenu,
    LevelEditing,
    Playing,
    GameOver,
    WonGame
}

public class GameplayManager : MonoBehaviour
{
    public static GameplayManager instance;

    public Grid gameGrid;
    public Tilemap backgroundLayer;
    public Tilemap stageLayer;
    public Tilemap overlayLayer;
    public Tilemap debugLayer;
    
    public Tile debugTile;

    public GameObject gameOverText;
    public GameObject gameWonText;

    public Tile backgroundTile;
    public Tile dirtTile;

    public GameObject rootPrefab;
    public List<GameObject> activeRoots = new List<GameObject>();

    public GameTileConfig[] TileList;

    public Dictionary<TileType, GameTileConfig> GameTileConfigLookup;
    public Dictionary<TileType, GameTile> GameTileLookup;
    public Dictionary<TileType, Tile> TileTypeLookup; 

    public GameMap activeMap;

    public int MapMaxWidth = 32;
    public int MapMaxHeight = 18;

    public GameState state;
    public bool rootsGrowing;
    public int totalRoots;
    public int totalRootsAtDestination;

    public void InitializeMap()
    {
        activeMap = new GameMap(MapMaxWidth, MapMaxHeight);
        for (int x = 0; x < MapMaxWidth; x++)
        {
            for (int y = 0; y < MapMaxHeight; y++)
            {
                backgroundLayer.SetTile(new Vector3Int(x, y, 0), backgroundTile);
            }
        }
    }

    public void ClearRoots()
    {
        if (activeRoots != null && activeRoots.Count > 0)
        {
            foreach (var root in activeRoots)
            {
                stageLayer.SetTile(root.GetComponent<Root>().stageStartPosition, TileTypeLookup[root.GetComponent<Root>().tileType]);
                Destroy(root);
            }
            activeRoots.Clear();
        }
    }

    public void PlaceRoots()
    {
        if (activeRoots == null)
            activeRoots = new List<GameObject>();

        for (int x = 0; x < activeMap.sizeX; x++)
        {
            for (int y = 0; y < activeMap.sizeY; y++)
            {
                if (activeMap.mapTiles[x, y].IsRootTile)
                {
                    Vector3Int gridLocation = new Vector3Int(x, y, 0);
                    AddRoot(activeMap.mapTiles[x, y].tileType, gridLocation);
                    stageLayer.SetTile(gridLocation, null);
                }
            }
        }

        totalRoots = activeRoots.Count;
    }

    public void AddRoot(TileType type, Vector3Int position)
    {
        Vector3 destination = position;
        Quaternion rotation = Quaternion.identity;
        GrowthDirection growDirection = GrowthDirection.South;
        switch(type)
        {
            case TileType.RootLeft:
                {
                    destination.x += 0.5f;                    
                    destination.y += 0.4f;
                    rotation = Quaternion.Euler(0, 0, 90f);
                    growDirection = GrowthDirection.East;
                }
                break;
            case TileType.RootRight:
                {
                    destination.x += 0.5f;
                    destination.y += 0.6f;
                    rotation = Quaternion.Euler(0, 0, 270f);
                    growDirection = GrowthDirection.West;
                }
                break;
            case TileType.RootUp:
                {
                    destination.x += 0.4f;
                    destination.y += 0.5f;
                    
                    growDirection = GrowthDirection.South;
                }
                break;
            case TileType.RootDown:
                {
                    destination.x += 0.6f;
                    destination.y += 0.5f;
                    rotation = Quaternion.Euler(0, 0, 180f);
                    growDirection = GrowthDirection.North;
                }
                break;
        }
        GameObject newRoot = GameObject.Instantiate(rootPrefab, destination, Quaternion.identity);
        newRoot.GetComponent<Root>().root.transform.rotation = rotation;
        newRoot.GetComponent<Root>().lastStageTipPosition = new Vector2Int(position.x, position.y);
        newRoot.GetComponent<Root>().stageTipPosition = newRoot.GetComponent<Root>().lastStageTipPosition;
        newRoot.GetComponent<Root>().baseDirection = growDirection;
        newRoot.GetComponent<Root>().tileType = type;
        newRoot.GetComponent<Root>().stageStartPosition = position;
        newRoot.GetComponent<Root>().UpdateDirection();
        activeRoots.Add(newRoot);
    }

    public void LoadLevel(int levelNum)
    {
        activeMap.LoadMapFile(levelNum.ToString());
    }

    public void ResetMap()
    {
        ClearRoots();
        PlaceRoots();
        activeMap.ClearBlocks();
        activeMap.SetupBlocks();
        totalRootsAtDestination = 0;
    }

    public void SetGameOver()
    {
        if (state == GameState.GameOver)
            return;

        state = GameState.GameOver;
        gameOverText.SetActive(true);
        StartCoroutine(GameOver());
    }

    IEnumerator GameOver()
    {
        yield return new WaitForSeconds(5);
        gameOverText.SetActive(false);
        ResetMap();
        state = GameState.Playing;
    }

    public void SetGameWon()
    {
        if (state == GameState.WonGame)
            return;

        state = GameState.WonGame;
        gameWonText.SetActive(true);
        StartCoroutine(GameWon());
    }

    IEnumerator GameWon()
    {
        yield return new WaitForSeconds(5);
        gameWonText.SetActive(false);
        ResetMap();
        state = GameState.Playing;
    }

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        DontDestroyOnLoad(this);  

        GameTileLookup = new Dictionary<TileType, GameTile>();
        TileTypeLookup = new Dictionary<TileType, Tile>();
        foreach(GameTileConfig tilecfg in TileList)
        {
            TileTypeLookup[tilecfg.Type] = tilecfg.TileObject;
            GameTileLookup[tilecfg.Type] = new GameTile(tilecfg.Type) { config = tilecfg };
        }       

        InitializeMap();
        LoadLevel(1);
        
        EditorManager.instance.StartEditing();
    }

    public bool CheckIfWon()
    {
        return (totalRoots == totalRootsAtDestination);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
    }
}
