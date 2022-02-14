using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "NewTile", menuName = "ScriptableObjects/Game Tile", order = 1)]
public class GameTileConfig : ScriptableObject
{
    [SerializeField] public string TileName;
    [SerializeField] public Tile TileObject;
    [SerializeField] public TileType Type;

    [SerializeField] public bool GameOverTile;
    [SerializeField] public bool DestinationTile;

    [SerializeField] public TileRedirectDirection RedirectDirection;
}
