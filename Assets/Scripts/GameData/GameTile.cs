using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public enum TileType : int
{
    Blank,
    Dirt,
    Water,
    Poison,
    ReflectorTL,
    ReflectorTR,
    ReflectorBL,
    ReflectorBR,
    RootDown,
    RootUp,
    RootLeft,
    RootRight
}

[Serializable]
public enum TileRedirectDirection : int
{
    None,
    RightUp,
    LeftUp,
    RightDown,
    LeftDown
}

[Serializable]
public class GameTile
{
    public GameTileConfig config;
    public TileType tileType;
    public bool blocked;

    public GameTile(TileType _type)
    {
        tileType = _type;
    }

    public GameTile(GameTile source)
    {
        config = source.config;
        tileType = source.tileType;
        blocked = source.blocked;
    }

    public bool IsDestination
    {
        get
        {
            return config.Type == TileType.Water;
        }
    }

    public bool IsBlockedTile
    {
        get
        {
            return (blocked || config.Type == TileType.Dirt || IsRootTile);
        }
    }

    public bool IsRootTile
    {
        get
        {
            return (config.Type == TileType.RootDown ||
                    config.Type == TileType.RootUp ||
                    config.Type == TileType.RootLeft ||
                    config.Type == TileType.RootRight);
        }
    }

    public bool occupied;
}
