using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.U2D;

[Serializable]
public enum RootState : int
{
    Idle,
    Moving,
    Dead,
    Stuck,
}

[Serializable]
public enum GrowthDirection : int
{
    None, North, East, South, West
}


public class Root : MonoBehaviour
{
    public GameObject root;

    public float growSpeed = 0.02f;
    public float redirectDelay = 0.05f;
    public float currentDelay = 0;

    public float startGrowTime;

    public Vector3Int stageStartPosition;
    public Vector3Int stageTipPosition;

    public GrowthDirection baseDirection;
    public GrowthDirection growthDirection;
    private GrowthDirection currentDirection;

    public TileType tileType;

    public bool Growing { get { return growing; } }

    private bool growing;
    private bool blocked;
    private bool reachedDestination;

    public Tuple<int, int> GetDestination(GrowthDirection direction)
    {
        int testX = stageTipPosition.x;
        int testY = stageTipPosition.y;
        switch (direction)
        {
            case GrowthDirection.North:
                testY += 1;
                break;
            case GrowthDirection.East:
                testX += 1;
                break;
            case GrowthDirection.West:
                testX -= 1;
                break;
            case GrowthDirection.South:
                testY -= 1;
                break;
        }
        return new Tuple<int, int>(testX, testY);
    }

    public bool DirectionBlocked(GrowthDirection direction)
    {
        Tuple<int, int> destinationPos = GetDestination(direction);
        return (direction == GetOppositeDirection(growthDirection)) || GameplayManager.instance.activeMap.mapTiles[destinationPos.Item1, destinationPos.Item2].IsBlockedTile;
    }

    public bool IsDestinationTile(Vector3Int position)
    {
        return GameplayManager.instance.activeMap.mapTiles[position.x, position.y].IsDestination;
    }

    public bool IsDeathTile(Vector3Int position)
    {
        return GameplayManager.instance.activeMap.mapTiles[position.x, position.y].IsDeathTile;
    }

    public bool IsRedirectTile(Vector3Int position)
    {
        return GameplayManager.instance.activeMap.mapTiles[position.x, position.y].IsRedirectTile;
    }

    public bool IsRedirectDirection(GrowthDirection direction)
    {
        Tuple<int, int> destinationPos = GetDestination(direction);
        return IsRedirectTile(new Vector3Int(destinationPos.Item1, destinationPos.Item2, 0));
    }

    public bool IsStuck()
    {
        bool blockedNorth, blockedSouth, blockedEast, blockedWest;
        blockedNorth = DirectionBlocked(GrowthDirection.North);
        blockedSouth = DirectionBlocked(GrowthDirection.South);
        blockedEast = DirectionBlocked(GrowthDirection.East);
        blockedWest = DirectionBlocked(GrowthDirection.West);

        switch (currentDirection)
        {
            case GrowthDirection.North:
                if (blockedNorth && blockedEast && blockedWest)
                    return true;
                break;
            case GrowthDirection.South:
                if (blockedSouth && blockedEast && blockedWest)
                    return true;
                break;
            case GrowthDirection.East:
                if (blockedEast && blockedNorth && blockedSouth)
                    return true;
                break;
            case GrowthDirection.West:
                if (blockedWest && blockedNorth && blockedSouth)
                    return true;
                break;
        }

        return false;
    }

    public GrowthDirection GetRedirectDirection(GrowthDirection enterDirection, TileType redirType)
    {
        switch (redirType)
        {
            case TileType.ReflectorBL:
                if (enterDirection == GrowthDirection.North)
                    return GrowthDirection.East;
                else if (enterDirection == GrowthDirection.East)
                    return GrowthDirection.North;
                break;
            case TileType.ReflectorBR:
                if (enterDirection == GrowthDirection.North)
                    return GrowthDirection.West;
                else if (enterDirection == GrowthDirection.West)
                    return GrowthDirection.North;
                break;
            case TileType.ReflectorTL:
                if (enterDirection == GrowthDirection.East)
                    return GrowthDirection.South;
                else if (enterDirection == GrowthDirection.South)
                    return GrowthDirection.East;
                break;
            case TileType.ReflectorTR:
                if (enterDirection == GrowthDirection.West)
                    return GrowthDirection.South;
                else if (enterDirection == GrowthDirection.South)
                    return GrowthDirection.West;
                break;
        }

        return GrowthDirection.None;
    }

    public GrowthDirection GetOppositeDirection(GrowthDirection direction)
    {
        switch (direction)
        {
            case GrowthDirection.North:
                return GrowthDirection.South;
            case GrowthDirection.South:
                return GrowthDirection.North;
            case GrowthDirection.East:
                return GrowthDirection.West;
            case GrowthDirection.West:
                return GrowthDirection.East;
        }
        return GrowthDirection.None;
    }

    public Tile GetTileByDirection(GrowthDirection direction)
    {
        switch (direction)
        {
            case GrowthDirection.North:
                return GameplayManager.instance.TileTypeLookup[TileType.RootUp];
            case GrowthDirection.South:
                return GameplayManager.instance.TileTypeLookup[TileType.RootDown];
            case GrowthDirection.East:
                return GameplayManager.instance.TileTypeLookup[TileType.RootRight];
            case GrowthDirection.West:
                return GameplayManager.instance.TileTypeLookup[TileType.RootLeft];
        }
        return null;
    }

    public TileType GetRedirectionType(Vector3Int position)
    {
        return GameplayManager.instance.activeMap.mapTiles[position.x, position.y].tileType;
    }

    public IEnumerator Grow()
    {
        if (!DirectionBlocked(currentDirection))
        {
            blocked = true;
            yield return new WaitForSeconds(currentDelay);
            growing = true;
            growthDirection = currentDirection;
            Tuple<int, int> destination = GetDestination(currentDirection);
            int curX = stageTipPosition.x;
            int curY = stageTipPosition.y;
            int destX = destination.Item1;
            int destY = destination.Item2;
            GameplayManager.instance.activeMap.mapTiles[curX, curY].Exit(currentDirection);
            GameplayManager.instance.activeMap.mapTiles[destX, destY].Enter(GetOppositeDirection(currentDirection));
            GameTile prevTile = GameplayManager.instance.activeMap.mapTiles[curX, curY];
            GrowthDirection enterDir = prevTile.rootEnterDirection;
            GrowthDirection exitDir = prevTile.rootExitDirection;
            Tile setTile = null;

            if ((enterDir == GrowthDirection.West && exitDir == GrowthDirection.South) ||
                (enterDir == GrowthDirection.South && exitDir == GrowthDirection.West))
            {
                setTile = GameplayManager.instance.rootRedirectTileLookup[TileRedirectDirection.LeftDown];
            }
            else if ((enterDir == GrowthDirection.North && exitDir == GrowthDirection.South) ||
                (enterDir == GrowthDirection.South && exitDir == GrowthDirection.North))
            {
                int totalAlts = GameplayManager.instance.alternateVertRoots.Length;
                if (totalAlts > 0)
                {
                    int randTile = UnityEngine.Random.Range(0, totalAlts);
                    setTile = GameplayManager.instance.alternateVertRoots[randTile];
                }
                else
                {
                    setTile = GameplayManager.instance.rootRedirectTileLookup[TileRedirectDirection.UpDown];
                }
            }
            else if ((enterDir == GrowthDirection.West && exitDir == GrowthDirection.East) ||
                (enterDir == GrowthDirection.East && exitDir == GrowthDirection.West))
            {
                int totalAlts = GameplayManager.instance.alternateHorRoots.Length;
                if (totalAlts > 0)
                {
                    int randTile = UnityEngine.Random.Range(0, totalAlts);
                    setTile = GameplayManager.instance.alternateHorRoots[randTile];
                }
                else
                {
                    setTile = GameplayManager.instance.rootRedirectTileLookup[TileRedirectDirection.LeftRight];
                }
            }
            else if ((enterDir == GrowthDirection.West && exitDir == GrowthDirection.North) ||
                (enterDir == GrowthDirection.North && exitDir == GrowthDirection.West))
            {
                setTile = GameplayManager.instance.rootRedirectTileLookup[TileRedirectDirection.LeftUp];
            }
            else if ((enterDir == GrowthDirection.East && exitDir == GrowthDirection.South) ||
                (enterDir == GrowthDirection.South && exitDir == GrowthDirection.East))
            {
                setTile = GameplayManager.instance.rootRedirectTileLookup[TileRedirectDirection.RightDown];
            }
            else if ((enterDir == GrowthDirection.East && exitDir == GrowthDirection.North) ||
                (enterDir == GrowthDirection.North && exitDir == GrowthDirection.East))
            {
                setTile = GameplayManager.instance.rootRedirectTileLookup[TileRedirectDirection.RightUp];
            }
            GameplayManager.instance.rootLayer.SetTile(stageTipPosition, setTile);

            stageTipPosition = new Vector3Int(destX, destY, 0);

            GameplayManager.instance.rootLayer.SetTile(stageTipPosition, GetTileByDirection(currentDirection));

            if (IsDeathTile(stageTipPosition))
            {
                GameplayManager.instance.SetGameOver();
                
                if (IsDeathTile(stageTipPosition))
                    AudioManager.instance.PlaySound(SFXType.DeathSound);
                
                AudioManager.instance.PlaySound(SFXType.GameOver);

                growing = false;
            }
            else if (IsStuck() && !IsDestinationTile(stageTipPosition))
            {
                GameplayManager.instance.totalRootsStuck++;
                if (GameplayManager.instance.CheckIfGameOver())
                {
                    GameplayManager.instance.SetGameOver();

                    AudioManager.instance.PlaySound(SFXType.GameOver);

                    growing = false;
                }
            }    
            else if (IsDestinationTile(stageTipPosition) && DirectionBlocked(growthDirection))
            {
                GameplayManager.instance.totalRootsAtDestination++;
                AudioManager.instance.PlaySound(SFXType.Water);

                if (GameplayManager.instance.CheckIfWon())
                    GameplayManager.instance.SetGameWon();

                growing = false;
            }
            else
                blocked = false;
        }
        else
        {
            growing = false;
            AudioManager.instance.PlaySound(SFXType.Wall);
        }
    }

    private void Start()
    {
        currentDirection = baseDirection;
        growthDirection = baseDirection;
        stageTipPosition = stageStartPosition;
        int curX = stageTipPosition.x;
        int curY = stageTipPosition.y;
        GameplayManager.instance.activeMap.mapTiles[curX, curY].Enter(GetOppositeDirection(currentDirection));
        GameplayManager.instance.activeMap.SetBlocked(stageTipPosition.x, stageTipPosition.y);
        GameplayManager.instance.rootLayer.SetTile(stageStartPosition, GameplayManager.instance.rootRedirectTileLookup[GameplayManager.instance.rootReDirLookup[currentDirection]]);
    }

    private void OnDestroy()
    {

    }

    void Update()
    {
        if (GameplayManager.instance.state != GameState.Playing || reachedDestination)
            return;

        currentDelay = growSpeed;

        if (!blocked)
        {
            if (!growing)
            {
                if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
                {
                    currentDirection = GrowthDirection.North;
                    StartCoroutine(Grow());
                }
                else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
                {
                    currentDirection = GrowthDirection.East;
                    StartCoroutine(Grow());
                }
                else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
                {
                    currentDirection = GrowthDirection.South;
                    StartCoroutine(Grow());
                }
                else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
                {
                    currentDirection = GrowthDirection.West;
                    StartCoroutine(Grow());
                }
            }

            if (growing)
            {
                if (IsRedirectTile(stageTipPosition))
                {
                    currentDelay = redirectDelay;
                    AudioManager.instance.PlaySound(SFXType.Turn);
                    currentDirection = GetRedirectDirection(GetOppositeDirection(currentDirection), GetRedirectionType(stageTipPosition));
                }
                StartCoroutine(Grow());
            }
        }
    }
}
