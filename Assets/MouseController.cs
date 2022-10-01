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

        // On dragging start
        if (Input.GetMouseButtonDown(0))
        {
            _dragStartTilePosition = hoveredTilePosition;
            _isDragging = true;
        }

        if (toolController.currentTool != Tool.None)
        {
            // Calculate dragged area
            var cornerA = _isDragging ? _dragStartTilePosition : hoveredTilePosition;
            var cornerB = hoveredTilePosition;

            // Can build runway only in horizontal line
            if (toolController.currentTool == Tool.Runway)
            {
                cornerB = new Vector3Int(cornerB.x, cornerA.y);
            }

            PreviewBuilding(toolController.currentTool, cornerA, cornerB);

            if (Input.GetMouseButtonUp(0))
            {
                OnBuild?.Invoke(toolController.currentTool, cornerA, cornerB);
            }
        }

        // On dragging end
        if (Input.GetMouseButtonUp(0))
        {
            _isDragging = false;
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