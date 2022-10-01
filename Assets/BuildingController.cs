using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BuildingController : MonoBehaviour
{
    public ToolController toolController;
    public Tilemap outsideTilemap;
    public Tilemap paintTilemap;

    private readonly List<Runway> _runways = new() { new Runway(new Vector3Int(-47, 3), new Vector3Int(46, 3)) };

    private void Start()
    {
        MouseController.OnBuild += OnBuild;
    }

    private void OnBuild(Tool tool, Vector3Int areaStart, Vector3Int areaEnd)
    {
        var allTilesInArea = Utils.GetAllTilesInArea(areaStart, areaEnd);
        if (tool == Tool.Runway)
        {
            foreach (var tileInArea in allTilesInArea)
            {
                paintTilemap.SetTile(tileInArea, toolController.TileTypes[tool]);
            }
        }
        else
        {
            foreach (var tileInArea in allTilesInArea)
            {
                outsideTilemap.SetTile(tileInArea, toolController.TileTypes[tool]);
            }
        }
    }

    public List<Runway> GetRunways()
    {
        return _runways;
    }
}