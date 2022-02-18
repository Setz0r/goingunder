using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameMap
{
    public int sizeX;
    public int sizeY;
    public bool DebugMode;

    public HistoryStack<GameTile[,]> previousMapTiles;
    public GameTile[,] mapTiles;

    public bool IsDirty;

    public Dictionary<char, TileType> TileTypes = new Dictionary<char, TileType>()
    {
        {'0', TileType.Dirt },
        {'1', TileType.Blank },
        {'2', TileType.Water },
        {'3', TileType.Poison },
        {'4', TileType.ReflectorTL },
        {'5', TileType.ReflectorTR },
        {'6', TileType.ReflectorBL },
        {'7', TileType.ReflectorBR },
        {'8', TileType.RootUp },
        {'9', TileType.RootDown },
        {'B', TileType.RootRight },
        {'A', TileType.RootLeft },
    };

    public Dictionary<TileType, char> ReverseTileTypes = new Dictionary<TileType, char>()
    {
        { TileType.Dirt, '0' },
        { TileType.Blank, '1' },
        { TileType.Water, '2' },
        { TileType.Poison, '3' },
        { TileType.ReflectorTL, '4' },
        { TileType.ReflectorTR, '5' },
        { TileType.ReflectorBL, '6' },
        { TileType.ReflectorBR, '7' },
        { TileType.RootUp, '8' },
        { TileType.RootDown, '9' },
        { TileType.RootRight, 'B' },
        { TileType.RootLeft, 'A' }
    };

    public GameMap(int _sizeX, int _sizeY)
    {
        sizeX = _sizeX;
        sizeY = _sizeY;
        mapTiles = new GameTile[sizeX, sizeY];
    }

    public void UpdateMapTile(int x, int y, TileType type)
    {           
        mapTiles[x, y] = new GameTile(GameplayManager.instance.GameTileLookup[type]);
    }

    public void SetBlocked(int x, int y)
    {
        mapTiles[x, y].blocked = true;
        if (DebugMode) GameplayManager.instance.debugLayer.SetTile(new Vector3Int(x,y,0), GameplayManager.instance.debugTile);
    }

    public void SetUnblocked(int x, int y)
    {
        mapTiles[x, y].blocked = false;
        if (DebugMode) GameplayManager.instance.debugLayer.SetTile(new Vector3Int(x, y, 0), null);
    }

    public string ExportMapData()
    {
        StringBuilder sb = new StringBuilder();
        for(int y = sizeY-1;y >= 0; y--)
        {
            StringBuilder outString = new StringBuilder();
            for (int x = 0; x < sizeX; x++)
            {
                outString.Append(ReverseTileTypes[mapTiles[x,y].tileType]);
            }            
            sb.AppendLine(outString.ToString());
        }
        return sb.ToString();
    }

    public void ImportMapData(string mapData)
    {
        try
        {
            using (StringReader reader = new StringReader(mapData))
            {
                string line = string.Empty;
                int row = sizeY - 1;
                int col = 0;
                do
                {
                    line = reader.ReadLine();
                    if (line != null)
                    {
                        foreach (char c in line)
                        {
                            if (col >= sizeX)
                                break;
                            mapTiles[col, row] = new GameTile(GameplayManager.instance.GameTileLookup[TileTypes[c]]);
                            col++;
                        }
                    }
                    col = 0;
                    row--;
                    if (row < 0)
                        break;
                } while (line != null);
            }

            GameplayManager.instance.ClearRoots();

            SetupBlocks();
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            throw ex;
        }
    }

    public void ClearBlocks()
    {
        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                SetUnblocked(x, y);
            }
        }
    }

    public void SetupBlocks()
    {
        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                TileType type = mapTiles[x, y].tileType;
                Tile tile;
                if (type == TileType.Dirt)
                {
                    int totalAlts = GameplayManager.instance.alternateDirtTiles.Length;
                    int dirtTileIndex = UnityEngine.Random.Range(0, totalAlts);
                    tile = GameplayManager.instance.alternateDirtTiles[dirtTileIndex];
                }
                else
                {
                    tile = GameplayManager.instance.TileTypeLookup[type];
                }

                GameplayManager.instance.stageLayer.SetTile(new Vector3Int(x, y, 0), tile);

                if (mapTiles[x, y].IsBlockedTile)
                {
                    SetBlocked(x, y);
                } else
                {
                    SetUnblocked(x, y);
                }
            }
        }
    }


    public void LoadMapFile(string fileName)
    {
        var textFile = Resources.Load<TextAsset>("Levels/"+fileName);
        ImportMapData(textFile.text);
    }

}
