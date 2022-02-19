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
    Paused,
    GameOver,
    WonGame,
    Leaving
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

    public GameObject pauseView;
    public GameObject undoButton;
    
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
    public int prevTotalRootsAtDestination;
    public int totalRootsAtDestination;
    public int prevTotalRootsStuck;
    public int totalRootsStuck;

    public bool undoAvailable;

    public bool playTesting = false;

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
        undoAvailable = true;
        totalRootsAtDestination = 0;
        totalRootsStuck = 0;
        activeMap.ClearBlocks();
        activeMap.SetupBlocks();
        ClearRoots();
        PlaceRoots();
        ShowUndo();
    }

    public void SetGameOver()
    {
        if (state == GameState.GameOver)
            return;

        AudioManager.instance.StopSound(SFXType.Grow);
        state = GameState.GameOver;
        gameOverText.SetActive(true);
        if (playTesting)
            EditorManager.instance.designButton.SetActive(false);
        StartCoroutine(GameOver());
    }

    IEnumerator GameOver()
    {
        yield return new WaitForSeconds(5);
        gameOverText.SetActive(false);
        ResetMap();
        state = GameState.Playing;
        if (playTesting)
            EditorManager.instance.designButton.SetActive(true);
    }

    public void SetGameWon()
    {
        if (state == GameState.WonGame)
            return;

        AudioManager.instance.StopSound(SFXType.Grow);

        AudioManager.instance.PlaySound(SFXType.Win);
        state = GameState.WonGame;
        gameWonText.SetActive(true);
        if (playTesting)
            EditorManager.instance.designButton.SetActive(false);
        StartCoroutine(GameWon());
    }

    IEnumerator GameWon()
    {
        yield return new WaitForSeconds(5);
        gameWonText.SetActive(false);
        ResetMap();
        state = GameState.Playing;
        if (playTesting)
            EditorManager.instance.designButton.SetActive(true);
    }

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
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
        LoadLevel(16);
        PlaceRoots();
        state = GameState.Playing;
        undoAvailable = true;
        ShowUndo();
        //EditorManager.instance.StartEditing();
    }

    public bool CheckIfWon()
    {
        return (totalRootsAtDestination > 0);
    }

    public bool CheckIfGameOver()
    {
        return (totalRootsStuck == totalRoots);
    }

    public void GoToMainMenu()
    {
        GameSceneManager.instance.LoadMainMenuScene();
    }

    public void ShowPause()
    {
        pauseView.SetActive(true);
        state = GameState.Paused;
    }

    public void HidePause()
    {
        pauseView.SetActive(false);
        state = GameState.Playing;
    }

    public void ResumeButtonPress()
    {
        HidePause();
    }

    public void RetryButtonPress()
    {
        ResetMap();
        state = GameState.Playing;
        HidePause();
    }

    public void QuitToMenuPress()
    {
        state = GameState.Leaving;
        Fader.instance.FadeOut();
        HidePause();
    }

    public void PerformUndo()
    {

    }

    public void ShowUndo()
    {
        undoButton.GetComponent<Animator>().SetBool("ShowUndo", true);
    }

    public void HideUndo()
    {
        undoButton.GetComponent<Animator>().SetBool("ShowUndo", false);
    }

    public void UndoPress()
    {
        if (undoAvailable)
        {
            undoAvailable = false;
            HideUndo();
        }
    }

    public void QuitToDesktopPress()
    {
        HidePause();
        state = GameState.Leaving;
#if (UNITY_EDITOR || DEVELOPMENT_BUILD)
        Debug.Log(this.name + " : " + this.GetType() + " : " + System.Reflection.MethodBase.GetCurrentMethod().Name);
#endif
#if (UNITY_EDITOR)
        UnityEditor.EditorApplication.isPlaying = false;
#elif (UNITY_STANDALONE)
        Application.Quit();
#elif (UNITY_WEBGL)
        Application.OpenURL("about:blank");
#endif
    }

    void Update()
    {
        if (state == GameState.Playing && Input.GetKeyDown(KeyCode.Escape))
            ShowPause();
        else if (state == GameState.Paused && Input.GetKeyDown(KeyCode.Escape))
            HidePause();
        else if (Input.GetKeyDown(KeyCode.F4))
            AudioManager.instance.PlayMusic(MusicType.Gameplay);
        else if (Input.GetKeyDown(KeyCode.F5))
            AudioManager.instance.StopMusic();

        if (state == GameState.Playing && RootsGrowing())
            AudioManager.instance.PlaySound(SFXType.Grow);
        else
            AudioManager.instance.StopSound(SFXType.Grow);

        if (state == GameState.Playing && undoAvailable && (Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.Z)))
        {
            UndoPress();
        }
    }
}
