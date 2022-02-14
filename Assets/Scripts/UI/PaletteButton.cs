using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PaletteButton : MonoBehaviour
{
    public Tile tileDisplay;
    public TileType tileType;

    public void SetActiveTile()
    {
        EditorManager.instance.selectedTileType = tileType;
        EditorManager.instance.selectedTile = tileDisplay;
    }
}
