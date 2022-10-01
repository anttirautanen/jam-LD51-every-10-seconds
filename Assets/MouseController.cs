using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MouseController : MonoBehaviour
{
    public event Action<Tool, Vector3Int, Vector3Int> OnBuild;

    public ToolController toolController;
    public Tilemap toolPreviewTilemap;

    private bool _isDragging;
    private Vector3Int _dragStartTilePosition;

    private void Update()
    {
        var hoveredTilePosition = toolPreviewTilemap.WorldToCell(Camera.main!.ScreenToWorldPoint(Input.mousePosition));
        UpdateMouseDraggingState(hoveredTilePosition);

        Debug.Log($"isDragging:{_isDragging}, dragStart:{_dragStartTilePosition}, dragEnd:{hoveredTilePosition}");

        toolPreviewTilemap.ClearAllTiles();

        if (toolController.currentTool != Tool.None)
        {
            PreviewBuilding(toolController.currentTool, _isDragging ? _dragStartTilePosition : hoveredTilePosition, hoveredTilePosition);
        }
    }

    private void UpdateMouseDraggingState(Vector3Int hoveredTilePosition)
    {
        var wasDraggingPreviously = _isDragging;
        var isDraggingNow = Input.GetMouseButton(0);
        switch (isDraggingNow)
        {
            case true when !wasDraggingPreviously:
                _dragStartTilePosition = hoveredTilePosition;
                _isDragging = true;
                break;
            case false when wasDraggingPreviously:
                _isDragging = false;
                OnBuild?.Invoke(toolController.currentTool, _dragStartTilePosition, hoveredTilePosition);
                break;
        }
    }

    private void PreviewBuilding(Tool tool, Vector3Int cornerA, Vector3Int cornerB)
    {
        var areaStartX = cornerA.x < cornerB.x ? cornerA.x : cornerB.x;
        var areaEndX = cornerA.x < cornerB.x ? cornerB.x : cornerA.x;
        var areaStartY = cornerA.y < cornerB.y ? cornerA.y : cornerB.y;
        var areaEndY = cornerA.y < cornerB.y ? cornerB.y : cornerA.y;

        var allTilesInArea = new List<Vector3Int>();
        for (var x = areaStartX; x <= areaEndX; ++x)
        {
            for (var y = areaStartY; y <= areaEndY; ++y)
            {
                allTilesInArea.Add(new Vector3Int(x, y));
            }
        }

        foreach (var tileInArea in allTilesInArea)
        {
            toolPreviewTilemap.SetTile(tileInArea, toolController.TileTypes[tool]);            
        }
    }
}