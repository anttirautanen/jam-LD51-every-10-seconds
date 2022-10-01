using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MouseController : MonoBehaviour
{
    public static event Action<Tool, Vector3Int, Vector3Int> OnBuild;

    public ToolController toolController;
    public Tilemap toolPreviewTilemap;

    private bool _isDragging;
    private Vector3Int _dragStartTilePosition;

    private void Update()
    {
        var hoveredTilePosition = toolPreviewTilemap.WorldToCell(Camera.main!.ScreenToWorldPoint(Input.mousePosition));
        UpdateMouseDraggingState(hoveredTilePosition);

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
        var allTilesInArea = Utils.GetAllTilesInArea(cornerA, cornerB);
        foreach (var tileInArea in allTilesInArea)
        {
            toolPreviewTilemap.SetTile(tileInArea, toolController.TileTypes[tool]);            
        }
    }
}