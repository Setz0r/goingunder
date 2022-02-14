using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

[Serializable]
public enum RootState: int
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

public class RootTipData
{
    public Vector3 position;
    public Vector3 leftTangent;
    public Vector3 rightTangent;
}

public class Root : MonoBehaviour
{
    public GameObject root;
    public SpriteShapeController shapeController;

    public float growSpeed = 0.1f;
    public float growDuration = 1f;

    public float startGrowTime;

    public GrowthDirection baseDirection;
    public GrowthDirection growthDirection;
    private GrowthDirection lastDirection;
    private GrowthDirection currentDirection;

    public TileType tileType;

    public bool Growing { get { return growing; } }

    private bool growing;
    private bool newGrowth;
    private bool reachedDestination;

    private RootTipData lastTipPosition;
    private RootTipData newTipPosition;

    public Vector2Int lastStageTipPosition;
    public Vector2Int stageTipPosition;
    public Vector3Int stageStartPosition;

    private int PointCount { get { return shapeController.spline.GetPointCount(); } }
    private Vector3 CurrentTipPosition { get { return shapeController.spline.GetPosition(PointCount - 1); } }
    private Vector3 CurrentLeftTangent { get { return shapeController.spline.GetLeftTangent(PointCount - 1); } }
    private Vector3 CurrentRightTangent { get { return shapeController.spline.GetRightTangent(PointCount - 1); } }

    private Vector3 GetLastPosition()
    {
        return shapeController.spline.GetPosition(PointCount - 1);
    }

    private void AddNewPreviousPosition()
    {
        shapeController.spline.InsertPointAt(PointCount - 1, lastTipPosition.position);
        shapeController.spline.SetTangentMode(PointCount - 2, ShapeTangentMode.Continuous);
        shapeController.spline.SetLeftTangent(PointCount - 2, lastTipPosition.leftTangent);
        shapeController.spline.SetRightTangent(PointCount - 2, lastTipPosition.rightTangent);
    }

    private void UpdatePreviousTangents(Vector3 leftTangent, Vector3 rightTangent)
    {
        shapeController.spline.SetLeftTangent(PointCount - 2, leftTangent);
        shapeController.spline.SetRightTangent(PointCount - 2, rightTangent);
    }

    private void MoveTipToPosition(RootTipData newPosition)
    {
        shapeController.spline.SetPosition(PointCount - 1, newPosition.position);
        shapeController.spline.SetLeftTangent(PointCount - 1, newPosition.leftTangent);
        shapeController.spline.SetRightTangent(PointCount - 1, newPosition.rightTangent);
    }

    RootTipData GetDataFromDirection(Vector3 oldPosition, GrowthDirection direction)
    {
        Vector3 newPosition = oldPosition;
        Vector3 lt = Vector3.zero;
        Vector3 rt = Vector3.zero;

        switch (direction)
        {
            case GrowthDirection.North:
                {
                    newPosition.y += 2f;
                    lt = new Vector3(0, -0.5f, 0);
                    rt = new Vector3(0, 0.5f, 0);
                }
                break;
            case GrowthDirection.South:
                {
                    newPosition.y -= 2f;
                    lt = new Vector3(0, 0.5f, 0);
                    rt = new Vector3(0, -0.5f, 0);
                }
                break;
            case GrowthDirection.West:
                {
                    newPosition.x -= 2f;
                    lt = new Vector3(0.5f, 0, 0);
                    rt = new Vector3(-0.5f, 0, 0);
                }
                break;
            case GrowthDirection.East:
                {
                    newPosition.x += 2f;
                    lt = new Vector3(-0.5f, 0, 0);
                    rt = new Vector3(0.5f, 0, 0);
                }
                break;
        }
        return new RootTipData()
        {
            position = newPosition,
            leftTangent = lt,
            rightTangent = rt
        };
    }

    public Vector2Int GetDirectionPosition(GrowthDirection direction)
    {
        Vector2Int testPosition = new Vector2Int(stageTipPosition.x, stageTipPosition.y);
        switch (direction)
        {
            case GrowthDirection.North:
                testPosition.y += 1;
                break;
            case GrowthDirection.South:
                testPosition.y -= 1;
                break;
            case GrowthDirection.West:
                testPosition.x -= 1;
                break;
            case GrowthDirection.East:
                testPosition.x += 1;
                break;
        }

        return testPosition;
    }

    public bool CheckIfBlocked(GrowthDirection direction)
    {
        Vector2Int testPosition = GetDirectionPosition(direction);
        Debug.Log("Current Position: " + stageTipPosition.ToString());
        Debug.Log("Test Position: " + testPosition.ToString());
        if (testPosition.x < 0 || testPosition.x >= GameplayManager.instance.MapMaxWidth ||
            testPosition.y < 0 || testPosition.y >= GameplayManager.instance.MapMaxHeight)
            return true;
        if (GameplayManager.instance.activeMap.mapTiles[testPosition.x, testPosition.y].IsBlockedTile)
        {
            Debug.Log("Block check returned true : " + testPosition.ToString());            
        }
        return GameplayManager.instance.activeMap.mapTiles[testPosition.x, testPosition.y].IsBlockedTile;
    }

    public TileRedirectDirection CheckIfRedirected(Vector2Int position)
    {
        TileRedirectDirection redir = GameplayManager.instance.activeMap.mapTiles[position.x, position.y].config.RedirectDirection;
        return redir;
    }

    public GrowthDirection GetRedirectDirection(Vector2Int position, GrowthDirection direction)
    {
        TileRedirectDirection redir = CheckIfRedirected(position);
        if (redir != TileRedirectDirection.None)
        {
            switch (redir)
            {
                case TileRedirectDirection.RightUp:
                    {
                        if (direction == GrowthDirection.West)
                            return GrowthDirection.North;
                        if (direction == GrowthDirection.South)
                            return GrowthDirection.East;
                    }
                    break;
                case TileRedirectDirection.LeftUp:
                    {
                        if (direction == GrowthDirection.East)
                            return GrowthDirection.North;
                        if (direction == GrowthDirection.South)
                            return GrowthDirection.West;
                    }
                    break;
                case TileRedirectDirection.RightDown:
                    {
                        if (direction == GrowthDirection.West)
                            return GrowthDirection.South;
                        if (direction == GrowthDirection.North)
                            return GrowthDirection.East;
                    }
                    break;
                case TileRedirectDirection.LeftDown:
                    {
                        if (direction == GrowthDirection.East)
                            return GrowthDirection.South;
                        if (direction == GrowthDirection.North)
                            return GrowthDirection.West;
                    }
                    break;
            }
        }
        return direction;
    }

    private void Grow(GrowthDirection direction)
    {
        if (CheckIfBlocked(direction))
        {
            growing = false;
            return;
        }

        lastStageTipPosition = new Vector2Int(stageTipPosition.x, stageTipPosition.y);

        stageTipPosition = GetDirectionPosition(direction);

        Vector3 lastPosition = GetLastPosition();

        lastTipPosition.position = CurrentTipPosition;
        lastTipPosition.leftTangent = CurrentLeftTangent;
        lastTipPosition.rightTangent = CurrentRightTangent;

        growing = true;

        startGrowTime = Time.time;

        if (direction != lastDirection)
        {
            lastDirection = direction;
            newGrowth = true;
        }

        RootTipData newGrowthData = new RootTipData();
        switch (direction)
        {
            case GrowthDirection.North:
                {
                    switch (baseDirection)
                    {
                        case GrowthDirection.East:
                            newGrowthData = GetDataFromDirection(lastPosition, GrowthDirection.East);
                            break;
                        case GrowthDirection.West:
                            newGrowthData = GetDataFromDirection(lastPosition, GrowthDirection.West);
                            break;
                        case GrowthDirection.North:
                            newGrowthData = GetDataFromDirection(lastPosition, GrowthDirection.South);
                            break;
                        default:
                            newGrowthData = GetDataFromDirection(lastPosition, direction);
                            break;
                    }
                }
                break;
            case GrowthDirection.East:
                {
                    switch (baseDirection)
                    {
                        case GrowthDirection.East:
                            newGrowthData = GetDataFromDirection(lastPosition, GrowthDirection.South);
                            break;
                        case GrowthDirection.West:
                            newGrowthData = GetDataFromDirection(lastPosition, GrowthDirection.North);
                            break;
                        case GrowthDirection.North:
                            newGrowthData = GetDataFromDirection(lastPosition, GrowthDirection.West);
                            break;
                        default:
                            newGrowthData = GetDataFromDirection(lastPosition, direction);
                            break;
                    }
                }
                break;
            case GrowthDirection.South:
                {
                    switch (baseDirection)
                    {
                        case GrowthDirection.East:
                            newGrowthData = GetDataFromDirection(lastPosition, GrowthDirection.West);
                            break;
                        case GrowthDirection.West:
                            newGrowthData = GetDataFromDirection(lastPosition, GrowthDirection.East);
                            break;
                        case GrowthDirection.North:
                            newGrowthData = GetDataFromDirection(lastPosition, GrowthDirection.North);
                            break;
                        default:
                            newGrowthData = GetDataFromDirection(lastPosition, direction);
                            break;
                    }
                }
                break;
            case GrowthDirection.West:
                {
                    switch (baseDirection)
                    {
                        case GrowthDirection.East:
                            newGrowthData = GetDataFromDirection(lastPosition, GrowthDirection.North);
                            break;
                        case GrowthDirection.West:
                            newGrowthData = GetDataFromDirection(lastPosition, GrowthDirection.South);
                            break;
                        case GrowthDirection.North:
                            newGrowthData = GetDataFromDirection(lastPosition, GrowthDirection.East);
                            break;
                        default:
                            newGrowthData = GetDataFromDirection(lastPosition, direction);
                            break;
                    }
                }
                break;
        }

        if (newGrowth)
        {
            UpdatePreviousTangents(newGrowthData.leftTangent, newGrowthData.rightTangent);
        }

        currentDirection = direction;

        newTipPosition.position = newGrowthData.position;
        newTipPosition.leftTangent = newGrowthData.leftTangent;
        newTipPosition.rightTangent = newGrowthData.rightTangent;
    }

    public void UpdateDirection()
    {
        growthDirection = baseDirection;
        currentDirection = growthDirection;
        lastDirection = currentDirection;
    }

    void Start()
    {
        lastTipPosition = new RootTipData();
        newTipPosition = new RootTipData();
        UpdateDirection();
    }

    public bool CheckForDeath()
    {
        return (GameplayManager.instance.activeMap.mapTiles[stageTipPosition.x, stageTipPosition.y].config.GameOverTile);
    }

    void Update()
    {
        if (GameplayManager.instance.state != GameState.Playing || reachedDestination)
            return;

        if (!growing)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            {
                Grow(GrowthDirection.North);
            }
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            {
                Grow(GrowthDirection.East);
            }
            if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            {
                Grow(GrowthDirection.South);
            }
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            {
                Grow(GrowthDirection.West);
            }
        }

        if (growing)
        {
            float t = (Time.time - startGrowTime) / growDuration;

            RootTipData data = new RootTipData()
            {
                position = Vector3.Lerp(lastTipPosition.position, newTipPosition.position, t),
                leftTangent = Vector3.Lerp(lastTipPosition.leftTangent, newTipPosition.leftTangent, t),
                rightTangent = Vector3.Lerp(lastTipPosition.rightTangent, newTipPosition.rightTangent, t)
            };
            MoveTipToPosition(data);

            if (t > growDuration / 2)
            {
                if (newGrowth) { 
                    AddNewPreviousPosition();
                    newGrowth = false;
                }
                if (t >= growDuration)
                {
                    if (GameplayManager.instance.activeMap.mapTiles[stageTipPosition.x, stageTipPosition.y].IsDestination)
                    {
                        reachedDestination = true;
                        GameplayManager.instance.totalRootsAtDestination++;
                        if (GameplayManager.instance.CheckIfWon())
                            GameplayManager.instance.SetGameWon();
                        return;
                    }

                    GameplayManager.instance.activeMap.SetBlocked(stageTipPosition.x, stageTipPosition.y);

                    growing = false;

                    t = growDuration;                    

                    if (CheckForDeath()) { 
                        GameplayManager.instance.SetGameOver();
                        return;
                    }
                    
                    currentDirection = GetRedirectDirection(stageTipPosition, currentDirection);
                    Grow(currentDirection);

                }
            }

        }
    }
}
