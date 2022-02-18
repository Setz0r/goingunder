using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;

[Serializable]
public enum GameState : int
{
    MainMenu,
    LevelEditing,
    Playing,
    GameOver,
    WonGame
}

[Serializable]
public class RootRedirectTile
{
    public TileRedirectDirection redirectDirection;
    public Tile rootPieceTile;
}

[Serializable]
public class RootDirRedir
{
    public GrowthDirection direction;
    public TileRedirectDirection redirectDirection;
}

public class GameplayManager : MonoBehaviour
{
    public static GameplayManager instance;

    public Grid gameGrid;
    public Tilemap backgroundLayer;
    public Tilemap stageLayer;
    public Tilemap rootLayer;
    public Tilemap overlayLayer;
    public Tilemap debugLayer;
    
    public Tile debugTile;

    public GameObject gameOverText;
    public GameObject gameWonText;

    public Tile backgroundTile;
    public Tile[] alternateBGTiles;
    public Tile dirtTile;
    public Tile[] alternateDirtTiles;
    public TileBase poisonTile;
    public TileBase waterTile;    

    public GameObject rootPrefab;
    public List<GameObject> activeRoots = new List<GameObject>();
    public Tile[] alternateHorRoots;
    public Tile[] alternateVertRoots;
    
    public GameTileConfig[] TileList;

    public Dictionary<TileType, GameTileConfig> GameTileConfigLookup;
    public Dictionary<TileType, GameTile> GameTileLookup;
    public Dictionary<TileType, Tile> TileTypeLookup; 

    public List<RootRedirectTile> rootRedirectTiles;
    public Dictionary<TileRedirectDirection, Tile> rootRedirectTileLookup;
    
    public List<RootDirRedir> rootDirRedirs;
    public Dictionary<GrowthDirection, TileRedirectDirection> rootReDirLookup;

    public GameMap activeMap;

    public int MaxHistory = 5;

    public int MapMaxWidth = 32;
    public int MapMaxHeight = 18;

    public GameState state;
    public bool rootsGrowing;
    public int totalRoots;
    public int totalRootsAtDestination;

    public void PushHistory()
    {

        activeMap.previousMapTiles.Push(activeMap.mapTiles.Clone() as GameTile[,]);
    }

    public void PopHistory()
    {
        if (activeMap.previousMapTiles.Items.Count > 0)
            activeMap.mapTiles = activeMap.previousMapTiles.Pop().Clone() as GameTile[,];
    }

    public void InitializeMap()
    {
        activeMap = new GameMap(MapMaxWidth, MapMaxHeight);
        for (int x = 0; x < MapMaxWidth; x++)
        {
            for (int y = 0; y < MapMaxHeight; y++)
            {
                int totalBGAlts = alternateBGTiles.Length;
                int randomTile = UnityEngine.Random.Range(0, totalBGAlts);
                backgroundLayer.SetTile(new Vector3Int(x, y, 0), alternateBGTiles[randomTile]);
            }
        }
    }

    public void ClearRoots()
    {
        for (int x = 0; x < activeMap.sizeX; x++)
        {
            for (int y = 0; y < activeMap.sizeY; y++)
            {
                rootLayer.SetTile(new Vector3Int(x, y, 0), null);
                if (activeMap.mapTiles[x, y].config.Animated)
                {
                    Vector3Int gridLocation = new Vector3Int(x, y, 0);
                    stageLayer.SetTile(gridLocation, activeMap.mapTiles[x, y].config.TileObject);
                }
            }
        }
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
                Vector3Int gridLocation = new Vector3Int(x, y, 0);
                if (activeMap.mapTiles[x, y].IsRootTile)
                {
                    AddRoot(activeMap.mapTiles[x, y].tileType, gridLocation);
                    stageLayer.SetTile(gridLocation, null);
                }
                if (activeMap.mapTiles[x, y].config.Animated)
                {
                    stageLayer.SetTile(gridLocation, activeMap.mapTiles[x, y].config.AnimatedTileObject);
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
        switch (type)
        {
            case TileType.RootUp:
                growDirection = GrowthDirection.North;
                break;
            case TileType.RootDown:
                growDirection= GrowthDirection.South;
                break;
            case TileType.RootLeft:
                growDirection = GrowthDirection.West;
                break;
            case TileType.RootRight:
                growDirection= GrowthDirection.East;
                break;
        }

        rootPrefab.SetActive(false);
        GameObject newRoot = GameObject.Instantiate(rootPrefab, destination, Quaternion.identity);
        newRoot.GetComponent<Root>().root.transform.rotation = rotation;
        newRoot.GetComponent<Root>().baseDirection = growDirection;
        newRoot.GetComponent<Root>().tileType = type;
        newRoot.GetComponent<Root>().stageStartPosition = position;
        activeRoots.Add(newRoot);
        newRoot.SetActive(true);
    }

    public bool RootsGrowing()
    {
        foreach(GameObject root in activeRoots)
        {
            if (root.GetComponent<Root>().Growing)
                return true;
        }
        return false;
    }


    public void LoadLevel(int levelNum)
    {
        activeMap.LoadMapFile(levelNum.ToString());
    }

    public void ResetMap()
    {
        totalRootsAtDestination = 0;
        activeMap.ClearBlocks();
        activeMap.SetupBlocks();
        ClearRoots();
        PlaceRoots();
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
        AudioManager.instance.PlaySound(SFXType.Win);
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

        rootRedirectTileLookup = new Dictionary<TileRedirectDirection, Tile>();
        foreach (RootRedirectTile tile in rootRedirectTiles)
        {
            rootRedirectTileLookup[tile.redirectDirection] = tile.rootPieceTile;
        }

        rootReDirLookup = new Dictionary<GrowthDirection, TileRedirectDirection>();
        foreach (RootDirRedir dr in rootDirRedirs)
        {
            rootReDirLookup[dr.direction] = dr.redirectDirection;
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
        if (Input.GetKeyDown(KeyCode.F4))
            AudioManager.instance.PlayMusic(MusicType.Gameplay);
        if (Input.GetKeyDown(KeyCode.F5))
            AudioManager.instance.StopMusic();

    }
}
