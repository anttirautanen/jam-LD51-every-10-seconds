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
    public BuildingController buildingController;
    public Tile errorTile;

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

        // On dragging start
        if (Input.GetMouseButtonDown(0))
        {
            _dragStartTilePosition = hoveredTilePosition;
            _isDragging = true;
        }

        var currentTool = toolController.currentTool;
        if (currentTool != Tool.None)
        {
            // Calculate dragged area
            var cornerA = _isDragging ? _dragStartTilePosition : hoveredTilePosition;
            var cornerB = hoveredTilePosition;

            // Can build runway only in horizontal line
            if (currentTool == Tool.Runway)
            {
                cornerB = new Vector3Int(cornerB.x, cornerA.y);
            }

            // Can build runway only in single tile
            if (currentTool == Tool.Gate)
            {
                cornerA = hoveredTilePosition;
                cornerB = hoveredTilePosition;
            }

            var canBuild = buildingController.CanBuild(currentTool, cornerA, cornerB);
            PreviewBuilding(currentTool, cornerA, cornerB, canBuild);

            if (canBuild && Input.GetMouseButtonUp(0))
            {
                OnBuild?.Invoke(currentTool, cornerA, cornerB);
            }
        }

        // On dragging end
        if (Input.GetMouseButtonUp(0))
        {
            _isDragging = false;
        }
    }

    private void PreviewBuilding(Tool tool, Vector3Int cornerA, Vector3Int cornerB, bool canBuild)
    {
        var allTilesInArea = Utils.GetAllTilesInArea(cornerA, cornerB);
        foreach (var tileInArea in allTilesInArea)
        {
            toolPreviewTilemap.SetTile(tileInArea, canBuild ? toolController.TileTypes[tool] : errorTile);
        }
    }
}