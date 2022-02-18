using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EditorManager : MonoBehaviour
{
    public static EditorManager instance;

    public bool editing = false;

    public TileType selectedTileType;
    public Tile selectedTile;

    public GameObject tilePaletteLabel;
    public GameObject tilePalette;
    public GameObject clearButton;
    public GameObject loadMapButton;
    public GameObject saveMapButton;
    public GameObject designButton;
    public GameObject playtestButton;

    public GameObject tileLocLabel;

    public GameObject stageIOPanel;
    public GameObject loadButton;

    public TMPro.TMP_InputField stageData;

    public void HideEditUi()
    {
        tileLocLabel.SetActive(false);
        tilePaletteLabel.SetActive(false);
        tilePalette.SetActive(false);
        clearButton.SetActive(false);
        loadMapButton.SetActive(false);
        saveMapButton.SetActive(false);
    }

    public void ShowEditUI()
    {
        tileLocLabel.SetActive(true);
        tilePaletteLabel.SetActive(true);
        tilePalette.SetActive(true);
        clearButton.SetActive(true);
        loadMapButton.SetActive(true);
        saveMapButton.SetActive(true);
    }

    public void StartEditing()
    {
        if (!editing)
        {
            GameplayManager.instance.activeMap.ClearBlocks();
            GameplayManager.instance.state = GameState.LevelEditing;
            GameplayManager.instance.playTesting = false;
            designButton.SetActive(false);
            playtestButton.SetActive(true);
            ShowEditUI();
            editing = true;
        }
    }

    public void StopEditing()
    {
        if (editing)
        {
            GameplayManager.instance.activeMap.SetupBlocks();
            GameplayManager.instance.state = GameState.Playing;
            GameplayManager.instance.playTesting = true;
            designButton.SetActive(true);
            playtestButton.SetActive(false);
            HideEditUi();
            editing = false;
        }
    }

    public void ClearMap()
    {
        for(int x = 0; x < GameplayManager.instance.activeMap.sizeX; x++)
        {
            for(int y = 0; y < GameplayManager.instance.activeMap.sizeY; y++)
            {
                GameplayManager.instance.activeMap.mapTiles[x, y] = new GameTile(GameplayManager.instance.GameTileLookup[TileType.Dirt]);
                GameplayManager.instance.stageLayer.SetTile(new Vector3Int(x,y,0), GameplayManager.instance.dirtTile);
            }
        }
    }

    public void PlaceTile(Vector3Int position, TileType tileType)
    {

    }

    public void LoadMap()
    {
        stageIOPanel.SetActive(false);
        GameplayManager.instance.activeMap.ImportMapData(stageData.text);
    }

    public void ExportMap()
    {
        stageData.text = GameplayManager.instance.activeMap.ExportMapData();
        stageIOPanel.SetActive(true);
    }

    public void ShowIOPanel(bool showLoad = false)
    {
        stageIOPanel.SetActive(true);
        if (showLoad)
            loadButton.SetActive(true);
    }

    public void HideIOPanel()
    {
        stageIOPanel.SetActive(false);
        loadButton.SetActive(false);
        stageData.text = "";
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        } 
    }

}
