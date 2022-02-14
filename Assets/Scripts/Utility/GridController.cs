using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class GridController : MonoBehaviour
{
    private Grid grid;
    [SerializeField] private Tilemap overlayLayer = null;
    [SerializeField] private Tilemap gameLayer = null;

    [SerializeField] private Tile hoverTile = null;
    
    private Vector3Int previousMousePos = new Vector3Int();

    void Start()
    {
        grid = gameObject.GetComponent<Grid>();
    }

    public void ResetPreviousHighlight(Vector3Int mousePos)
    {
        if (!mousePos.Equals(previousMousePos))
        {
            overlayLayer.SetTile(previousMousePos, null);
        }
    }

    public void UpdateHighlight(Vector3Int mousePos)
    {
        if (!mousePos.Equals(previousMousePos))
        {
            EditorManager.instance.tileLocLabel.GetComponent<TMPro.TMP_Text>().text = string.Format("{0}, {1}", mousePos.x.ToString(), mousePos.y.ToString());
            overlayLayer.SetTile(mousePos, hoverTile);
            previousMousePos = mousePos;
        }
    }

    public void CheckForPlacement(Vector3Int mousePos)
    {

    }

    void Update()
    {
        Vector3Int mousePos = GetMousePosition();

        ResetPreviousHighlight(mousePos);

        if (EventSystem.current.IsPointerOverGameObject() || GameplayManager.instance.state != GameState.LevelEditing)
            return;

        UpdateHighlight(mousePos);

        if (Input.GetMouseButton(0))
        {
            gameLayer.SetTile(mousePos, EditorManager.instance.selectedTile);
            GameplayManager.instance.activeMap.UpdateMapTile(mousePos.x, mousePos.y, EditorManager.instance.selectedTileType);
        }
    }

    Vector3Int GetMousePosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = -Camera.main.transform.localPosition.z;
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mousePos);
        return grid.WorldToCell(mouseWorldPos);
    }
}