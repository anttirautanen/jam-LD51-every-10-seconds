using System;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class MouseController : MonoBehaviour
{
    public static event Action<Tool, Vector3Int, Vector3Int> OnBuild;

    public ToolController toolController;
    public Tilemap toolPreviewTilemap;
    public UIDocument uiDocument;

    private bool _isDragging;
    private Vector3Int _dragStartTilePosition;
    private VisualElement root;

    private void Start()
    {
        root = uiDocument.rootVisualElement;
    }

    private void Update()
    {
        toolPreviewTilemap.ClearAllTiles();
        
        // Is hovering HUD? Then do not do anything here.
        var mousePosition = Input.mousePosition;
        var hoveredElement = root.panel.Pick(mousePosition);
        if (hoveredElement != null)
        {
            return;
        }
     
        var hoveredTilePosition = toolPreviewTilemap.WorldToCell(Camera.main!.ScreenToWorldPoint(mousePosition));
        UpdateMouseDraggingState(hoveredTilePosition);

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
                if (toolController.currentTool != Tool.None)
                {
                    OnBuild?.Invoke(toolController.currentTool, _dragStartTilePosition, hoveredTilePosition);
                }

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