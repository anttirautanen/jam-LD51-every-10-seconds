using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BuildingController : MonoBehaviour
{
    public ToolController toolController;
    public Tilemap outsideTilemap;
    public Tilemap paintTilemap;
    public Tilemap debugTilemap;
    public Tile debugRouteTile;
    public Tile debugRouteDeadEndTile;

    private readonly List<Runway> _runways = new()
    {
        new Runway(new Vector3Int(-47, 3), new Vector3Int(46, 3))
    };

    private readonly List<Taxiway> _taxiways = new()
    {
        new Taxiway(new Vector3Int(-37, 4), new Vector3Int(-37, 21)),
        new Taxiway(new Vector3Int(-36, 21), new Vector3Int(-12, 21)),
        new Taxiway(new Vector3Int(-26, 23), new Vector3Int(-26, 22)),
        new Taxiway(new Vector3Int(-12, 23), new Vector3Int(-12, 22))
    };

    private readonly List<Gate> _gates = new()
    {
        new Gate(new Vector3Int(-12, 24)),
        new Gate(new Vector3Int(-26, 24))
    };

    private void Start()
    {
        MouseController.OnBuild += OnBuild;

        DrawRouteNetwork();
    }

    private void OnBuild(Tool tool, Vector3Int areaStart, Vector3Int areaEnd)
    {
        Debug.Log($"{areaStart}, {areaEnd}");
        var allTilesInArea = Utils.GetAllTilesInArea(areaStart, areaEnd);
        if (tool is Tool.Runway or Tool.Taxiway)
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

    public List<Gate> GetGates()
    {
        return _gates;
    }

    private void DrawRouteNetwork()
    {
        // listaa kaikki tilet dictionaryyn niin, että on nopea queryttää naapuri-tilejä
        var allTilePositions = DiscoverTiles(_runways, _taxiways);

        // listaa jokaiselle tilelle, mihin toisiin tileihin siitä tilestä pääsee
        foreach (var (tilePosition, _) in new Dictionary<Vector3Int, List<Vector3Int>>(allTilePositions))
        {
            var neighbours = new List<Vector3Int>();
            
            var left = new Vector3Int(tilePosition.x - 1, tilePosition.y);
            if (allTilePositions.GetValueOrDefault(left) != null)
            {
                neighbours.Add(left);
            }

            var right = new Vector3Int(tilePosition.x + 1, tilePosition.y);
            if (allTilePositions.GetValueOrDefault(right) != null)
            {
                neighbours.Add(right);
            }

            var up = new Vector3Int(tilePosition.x, tilePosition.y + 1);
            if (allTilePositions.GetValueOrDefault(up) != null)
            {
                neighbours.Add(up);
            }

            var down = new Vector3Int(tilePosition.x, tilePosition.y - 1);
            if (allTilePositions.GetValueOrDefault(down) != null)
            {
                neighbours.Add(down);
            }

            allTilePositions[tilePosition] = neighbours;
        }

        // piirrä kaikki tilet valkoisena omalle tilemapille
        debugTilemap.ClearAllTiles();
        foreach (var (tilePosition, neighbours) in allTilePositions)
        {
            // piirrä ne tilet, joista pääsee vain yhteen toiseen tileen eri tavalla
            var tile = neighbours.Count < 2 ? debugRouteDeadEndTile : debugRouteTile;
            debugTilemap.SetTile(tilePosition, tile);
        }
    }

    private Dictionary<Vector3Int, List<Vector3Int>> DiscoverTiles(List<Runway> runways, List<Taxiway> taxiways)
    {
        var emptyList = new List<Vector3Int>();
        var tiles = new Dictionary<Vector3Int, List<Vector3Int>>();
        foreach (var runway in runways)
        {
            var startX = runway.Start.x < runway.End.x ? runway.Start.x : runway.End.x;
            var endX = runway.Start.x < runway.End.x ? runway.End.x : runway.Start.x;
            for (var x = startX; x <= endX; ++x)
            {
                var tilePosition = new Vector3Int(x, runway.Start.y);
                tiles.Add(tilePosition, emptyList);
            }
        }

        foreach (var taxiway in taxiways)
        {
            var startX = taxiway.Start.x < taxiway.End.x ? taxiway.Start.x : taxiway.End.x;
            var endX = taxiway.Start.x < taxiway.End.x ? taxiway.End.x : taxiway.Start.x;
            var startY = taxiway.Start.y < taxiway.End.y ? taxiway.Start.y : taxiway.End.y;
            var endY = taxiway.Start.y < taxiway.End.y ? taxiway.End.y : taxiway.Start.y;
            for (var x = startX; x <= endX; ++x)
            {
                for (var y = startY; y <= endY; ++y)
                {
                    var tilePosition = new Vector3Int(x, y);
                    tiles.Add(tilePosition, emptyList);
                }
            }
        }

        return tiles;
    }
}