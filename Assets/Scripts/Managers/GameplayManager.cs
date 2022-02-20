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

    #region UI Element Refs
    [Header("== UI Element Refs ==")]
    public GameObject pauseView;
    public GameObject undoButton;
    public GameObject gameOverText;
    public GameObject gameWonText;
    #endregion

    #region Tutorial Image Refs
    [Header("== Tutorial Refs ==")]
    public GameObject tutRef_1;
    public GameObject tutRef_3;
    public GameObject tutRef_4;
    public GameObject tutRef_5;
    public GameObject tutRef_10;
    public GameObject tutRef_14;
    #endregion

    #region Grid/Tile Refs
    [Header("== Grid/Tile Refs ==")]
    public Grid gameGrid;
    public Tilemap backgroundLayer;
    public Tilemap stageLayer;
    public Tilemap rootLayer;
    public Tilemap overlayLayer;
    public Tilemap debugLayer;
    public Tile debugTile;
    public Tile backgroundTile;
    public Tile[] alternateBGTiles;
    public Tile dirtTile;
    public Tile[] alternateDirtTiles;
    public TileBase poisonTile;
    public TileBase waterTile;
    private BoundsInt stageBounds;
    #endregion

    #region History Refs
    [Header("== History Refs ==")]
    public bool historyRecorded;
    public bool hasMoved;
    public bool usedUndo;
    public bool undoAvailable;
    public TileBase[] undoTiles;
    #endregion

    #region Root Refs/Data
    [Header("== Root Refs/Data ==")]
    public GameObject rootPrefab;
    public List<GameObject> activeRoots = new List<GameObject>();
    public Tile[] alternateHorRoots;
    public Tile[] alternateVertRoots;
    public bool rootsGrowing;
    public int totalRoots;
    public int rootsGrowingCounter;
    public int totalRootsAtDestination;
    public int prevTotalRootsStuck;
    public int totalRootsStuck;
    #endregion

    #region Game Tile Data
    [Header("== Game Tile Data ==")]
    public GameTileConfig[] TileList;
    public Dictionary<TileType, GameTileConfig> GameTileConfigLookup;
    public Dictionary<TileType, GameTile> GameTileLookup;
    public Dictionary<TileType, Tile> TileTypeLookup;
    public List<RootRedirectTile> rootRedirectTiles;
    public Dictionary<TileRedirectDirection, Tile> rootRedirectTileLookup;
    public List<RootDirRedir> rootDirRedirs;
    public Dictionary<GrowthDirection, TileRedirectDirection> rootReDirLookup;
    #endregion

    #region Map Data
    [Header("== Map Data ==")]
    public GameMap activeMap;
    public int MaxHistory = 1;
    public int MapMaxWidth = 32;
    public int MapMaxHeight = 18;
    #endregion

    #region Gameplay Data
    [Header("== Gameplay Data ==")]
    public GameState state;
    public int currentLevel = 1;
    public int maxLevel = 16;
    public int movesCounter;
    public bool playTesting = false;
    #endregion

    #region History Methods
    //////////////////////////////////////////////////////////
    ///
    ///         HISTORY METHODS
    ///
    //////////////////////////////////////////////////////////

    public void BackupLastRootHistory()
    {
        historyRecorded = true;
        activeMap.BackupTiles();
        prevTotalRootsStuck = totalRootsStuck;
        undoTiles = rootLayer.GetTilesBlock(stageBounds);
    }

    public void RestoreRootHistory()
    {
        totalRootsStuck = prevTotalRootsStuck;
        activeMap.RestoreTiles();
        rootLayer.SetTilesBlock(stageBounds, undoTiles);
    }

    public void PerformUndo()
    {
        usedUndo = true;
        undoAvailable = false;
        HideUndo();
        RestoreRootHistory();
        ResetLastRootPositions();
    }

    public void UndoPress()
    {
        if (undoAvailable && !rootsGrowing)
            PerformUndo();
    }
    #endregion

    #region Map Methods
    //////////////////////////////////////////////////////////
    ///
    ///         MAP METHODS
    ///
    //////////////////////////////////////////////////////////

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
    #endregion

    #region Root Methods
    //////////////////////////////////////////////////////////
    ///
    ///         ROOT METHODS
    ///
    //////////////////////////////////////////////////////////

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
                growDirection = GrowthDirection.South;
                break;
            case TileType.RootLeft:
                growDirection = GrowthDirection.West;
                break;
            case TileType.RootRight:
                growDirection = GrowthDirection.East;
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
        foreach (GameObject root in activeRoots)
        {
            if (root.GetComponent<Root>().Growing)
                return true;
        }
        return false;
    }

    public int TotalRootsGrowing()
    {
        int counter = 0;
        foreach (GameObject root in activeRoots)
        {
            if (root.GetComponent<Root>().Growing)
                counter++;
        }
        return counter;
    }

    public void ResetLastRootPositions()
    {
        if (activeRoots != null && activeRoots.Count > 0)
        {
            foreach (var root in activeRoots)
            {
                Debug.Log("current tip position: " + root.GetComponent<Root>().stageTipPosition.ToString());
                root.GetComponent<Root>().ResetPosition();
                Debug.Log("reset to old position: " + root.GetComponent<Root>().stageTipPosition.ToString());
            }
        }
    }

    public void MakeRootsGrow(GrowthDirection direction)
    {
        bool CanAnyRootsGrow = false;
        foreach (GameObject root in activeRoots)
        {
            if (root.GetComponent<Root>().CanRootGrow(direction))
            {
                CanAnyRootsGrow = true;
                break;
            }
        }
        if (CanAnyRootsGrow)
        {
            if (!usedUndo)
            {
                BackupLastRootHistory();
                if (!undoAvailable)
                {
                    undoAvailable = true;
                    ShowUndo();
                }
            }
            foreach (GameObject root in activeRoots)
            {
                root.GetComponent<Root>().TryGrow(direction);
            }
        }
    }
    #endregion

    #region Level Methods
    //////////////////////////////////////////////////////////
    ///
    ///         LEVEL METHODS
    ///
    //////////////////////////////////////////////////////////

    public void BeatGame()
    {

    }

    public void LoadLevel(int levelNum)
    {
        activeMap.LoadMapFile(levelNum.ToString());
    }

    public void AdvanceLevel()
    {
        currentLevel++;
        if (currentLevel > maxLevel)
        {
            GoToCredits();
        }
        else
        {
            LoadLevel(currentLevel);
            ShowTutorialImage(currentLevel);
            LevelDisplay.instance.Show(currentLevel);
        }
    }

    public void ResetMap()
    {
        if (undoAvailable)
            HideUndo();
        usedUndo = false;
        movesCounter = 0;
        totalRootsAtDestination = 0;
        totalRootsStuck = 0;
        prevTotalRootsStuck = 0;
        activeMap.ClearBlocks();
        activeMap.SetupBlocks();
        ClearRoots();
        PlaceRoots();
    }
    #endregion

    #region Navigation/Pause/UI Methods
    //////////////////////////////////////////////////////////
    ///
    ///         NAVIGATION/PAUSE/UI METHODS
    ///
    //////////////////////////////////////////////////////////

    public void ShowUndo()
    {
        undoAvailable = true;
        undoButton.GetComponent<Animator>().SetBool("ShowUndo", true);
    }

    public void HideUndo()
    {
        undoAvailable = false;
        undoButton.GetComponent<Animator>().SetBool("ShowUndo", false);
    }

    public void GoToMainMenu()
    {
        GameSceneManager.instance.LoadMainMenuScene();
    }

    public void GoToCredits()
    {
        GameSceneManager.instance.LoadCreditsScene();
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

    public void HideTutorialImages()
    {
        tutRef_1.SetActive(false);
        tutRef_3.SetActive(false);
        tutRef_4.SetActive(false);
        tutRef_5.SetActive(false);
        tutRef_10.SetActive(false);
        tutRef_14.SetActive(false);
    }

    public void ShowTutorialImage(int level)
    {
        HideTutorialImages();
        switch (level)
        {
            case 1:
                tutRef_1.SetActive(true);
                break;
            case 3:
                tutRef_3.SetActive(true);
                break;
            case 4:
                tutRef_4.SetActive(true);
                break;
            case 5:
                tutRef_5.SetActive(true);
                break;
            case 10:
                tutRef_10.SetActive(true);
                break;
            case 14:
                tutRef_14.SetActive(false);
                break;
        }
    }

    #endregion

    #region Gameplay Methods
    //////////////////////////////////////////////////////////
    ///
    ///         GAMEPLAY METHODS
    ///
    //////////////////////////////////////////////////////////

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

    public void SetLevelWon()
    {
        if (state == GameState.WonGame)
            return;

        AudioManager.instance.StopSound(SFXType.Grow);
        AudioManager.instance.PlaySound(SFXType.Win);
        state = GameState.WonGame;
        gameWonText.SetActive(true);
        if (playTesting)
            EditorManager.instance.designButton.SetActive(false);
        StartCoroutine(LevelWon());
    }

    IEnumerator LevelWon()
    {
        yield return new WaitForSeconds(5);
        gameWonText.SetActive(false);
        AdvanceLevel();
        ResetMap();
        state = GameState.Playing;
        if (playTesting)
            EditorManager.instance.designButton.SetActive(true);
    }

    public bool CheckIfWon()
    {
        return (totalRootsAtDestination > 0);
    }

    public bool CheckIfGameOver()
    {
        return (totalRootsStuck == totalRoots);
    }
    #endregion

    #region Unity Events
    //////////////////////////////////////////////////////////
    ///
    ///         UNITY EVENT METHODS
    ///
    //////////////////////////////////////////////////////////

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        AudioManager.instance.PlayMusic(MusicType.Gameplay);

        GameTileLookup = new Dictionary<TileType, GameTile>();
        TileTypeLookup = new Dictionary<TileType, Tile>();
        stageBounds = new BoundsInt(0, 0, 0, MapMaxWidth, MapMaxHeight, 1);
        undoTiles = new TileBase[MapMaxWidth * MapMaxHeight];

        foreach (GameTileConfig tilecfg in TileList)
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

        stageBounds = new BoundsInt(0, 0, 0, MapMaxWidth, MapMaxHeight, 1);
        InitializeMap();
        if (currentLevel == 0) currentLevel = 1;
        LoadLevel(currentLevel);
        PlaceRoots();
        ShowTutorialImage(currentLevel);
        LevelDisplay.instance.Show(currentLevel);
        state = GameState.Playing;
        usedUndo = false;
        //EditorManager.instance.StartEditing();
    }

    void Update()
    {
        rootsGrowing = RootsGrowing();
        if (state == GameState.Playing)
        {
            if (!rootsGrowing)
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                    ShowPause();
                else if (undoAvailable && (Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.Z)))
                {
                    UndoPress();
                }
                else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
                {
                    MakeRootsGrow(GrowthDirection.North);
                }
                else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
                {
                    MakeRootsGrow(GrowthDirection.East);
                }
                else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
                {
                    MakeRootsGrow(GrowthDirection.South);
                }
                else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
                {
                    MakeRootsGrow(GrowthDirection.West);
                }

                rootsGrowing = RootsGrowing();

                if (rootsGrowing)
                {
                    AudioManager.instance.PlaySound(SFXType.Grow);
                }
            }
            else
            {
                AudioManager.instance.StopSound(SFXType.Grow);
            }
        }
        else if (state == GameState.Paused && Input.GetKeyDown(KeyCode.Escape))
            HidePause();
    }
    #endregion
}
