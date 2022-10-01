using UnityEngine;
using UnityEngine.Tilemaps;

public class BuildingController : MonoBehaviour
{
    public ToolController toolController;
    public Tilemap outsideTilemap;

    private void Start()
    {
        MouseController.OnBuild += OnBuild;
    }

    private void OnBuild(Tool tool, Vector3Int areaStart, Vector3Int areaEnd)
    {
        var allTilesInArea = Utils.GetAllTilesInArea(areaStart, areaEnd);
        foreach (var tileInArea in allTilesInArea)
        {
            outsideTilemap.SetTile(tileInArea, toolController.TileTypes[tool]);
        }
    }
}