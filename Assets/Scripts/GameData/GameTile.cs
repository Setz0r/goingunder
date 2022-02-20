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
    LeftDown,
    LeftRight,
    UpDown,
    Up,
    Down,
    Left,
    Right
}

[Serializable]
public class GameTile
{
    public GameTileConfig config;
    public TileType tileType;

    public GrowthDirection rootEnterDirection;
    public GrowthDirection rootExitDirection;

    public bool blocked;

    public GameTile(TileType _type)
    {
        tileType = _type;
    }

    public GameTile(GameTile source)
    {
        rootEnterDirection = source.rootEnterDirection;
        rootExitDirection = source.rootExitDirection;
        config = source.config;
        tileType = source.tileType;
        blocked = source.blocked;
    }

    public void Enter(GrowthDirection fromDirection)
    {
        blocked = true;
        rootEnterDirection = fromDirection;
    }

    public void Exit(GrowthDirection toDirection)
    {
        rootExitDirection = toDirection;
    }

    public bool IsDestination
    {
        get
        {
            return config.Type == TileType.Water;
        }
    }

    public bool IsDeathTile
    {
        get
        {
            return config.Type == TileType.Poison;
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

    public bool IsRedirectTile
    {
        get
        {
            return (config.Type == TileType.ReflectorTL ||
                    config.Type == TileType.ReflectorTR ||
                    config.Type == TileType.ReflectorBL ||
                    config.Type == TileType.ReflectorBR);
        }
    }

    public bool occupied;
}
