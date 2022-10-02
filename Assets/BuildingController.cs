using System;
using System.Collections.Generic;
using System.Linq;
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

    private Dictionary<Vector3Int, List<Vector3Int>> _routeNetwork;

    private readonly List<Runway> _runways = new()
    {
        // new Runway(new Vector3Int(-47, 3), new Vector3Int(46, 3))
    };

    private readonly List<Taxiway> _taxiways = new()
    {
        // new Taxiway(new Vector3Int(-37, 4), new Vector3Int(-37, 21)),
        // new Taxiway(new Vector3Int(-36, 21), new Vector3Int(-12, 21)),
        // new Taxiway(new Vector3Int(-26, 23), new Vector3Int(-26, 22)),
        // new Taxiway(new Vector3Int(-12, 23), new Vector3Int(-12, 22))
    };

    private readonly List<Gate> _gates = new()
    {
        // new Gate(new Vector3Int(-12, 24)),
        // new Gate(new Vector3Int(-26, 24))
    };

    private void Start()
    {
        MouseController.OnBuild += OnBuild;
    }

    private void OnBuild(Tool tool, Vector3Int areaStart, Vector3Int areaEnd)
    {
        var allTilesInArea = Utils.GetAllTilesInArea(areaStart, areaEnd);
        if (tool is Tool.Runway or Tool.Taxiway or Tool.Gate)
        {
            if (tool == Tool.Runway)
            {
                _runways.Add(new Runway(areaStart, areaEnd));
            }

            if (tool == Tool.Taxiway)
            {
                _taxiways.Add(new Taxiway(areaStart, areaEnd));
            }

            if (tool == Tool.Gate)
            {
                _gates.Add(new Gate(areaStart));
            }

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

        _routeNetwork = DiscoverRouteNetwork();
        DrawRouteNetwork();
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
        DebugDrawRouteNetwork(_routeNetwork);
    }

    private Dictionary<Vector3Int, List<Vector3Int>> DiscoverRouteNetwork()
    {
        var emptyList = new List<Vector3Int>();
        var tiles = new Dictionary<Vector3Int, List<Vector3Int>>();
        foreach (var runway in _runways)
        {
            var startX = runway.Start.x < runway.End.x ? runway.Start.x : runway.End.x;
            var endX = runway.Start.x < runway.End.x ? runway.End.x : runway.Start.x;
            for (var x = startX; x <= endX; ++x)
            {
                var tilePosition = new Vector3Int(x, runway.Start.y);
                tiles.Add(tilePosition, emptyList);
            }
        }

        foreach (var taxiway in _taxiways)
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

        foreach (var gate in _gates)
        {
            tiles.Add(gate.Position, emptyList);
        }

        foreach (var (tilePosition, _) in new Dictionary<Vector3Int, List<Vector3Int>>(tiles))
        {
            var neighbours = new List<Vector3Int>();

            var left = new Vector3Int(tilePosition.x - 1, tilePosition.y);
            if (tiles.GetValueOrDefault(left) != null)
            {
                neighbours.Add(left);
            }

            var right = new Vector3Int(tilePosition.x + 1, tilePosition.y);
            if (tiles.GetValueOrDefault(right) != null)
            {
                neighbours.Add(right);
            }

            var up = new Vector3Int(tilePosition.x, tilePosition.y + 1);
            if (tiles.GetValueOrDefault(up) != null)
            {
                neighbours.Add(up);
            }

            var down = new Vector3Int(tilePosition.x, tilePosition.y - 1);
            if (tiles.GetValueOrDefault(down) != null)
            {
                neighbours.Add(down);
            }

            tiles[tilePosition] = neighbours;
        }

        return tiles;
    }

    public Path FindPath(Vector3Int from, Vector3Int to)
    {
        var open = new List<Vector3Int> { from };
        var gScore = new Dictionary<Vector3Int, float> { { from, 0 } };
        var fScore = new Dictionary<Vector3Int, float> { { from, DistanceToTarget(from, to) } };
        var cameFrom = new Dictionary<Vector3Int, Vector3Int>();

        while (open.Count > 0)
        {
            open.Sort(SortByFScoreFunction(fScore));

            var current = open.First();
            if (current == to)
            {
                return ReconstructPath(cameFrom, current);
            }

            open.Remove(current);

            var neighbours = _routeNetwork[current];
            foreach (var neighbour in neighbours)
            {
                var tentativeGScore = gScore[current] + 1;
                if (tentativeGScore < gScore.GetValueOrDefault(neighbour, float.PositiveInfinity))
                {
                    cameFrom[neighbour] = current;
                    gScore[neighbour] = tentativeGScore;
                    fScore[neighbour] = tentativeGScore + DistanceToTarget(neighbour, to);
                    open.Add(neighbour);
                }
            }
        }

        return null;
    }

    private Path ReconstructPath(Dictionary<Vector3Int, Vector3Int> cameFrom, Vector3Int current)
    {
        var pathPart = current;
        var tilePositions = new List<Vector3Int> { pathPart };
        while (cameFrom.ContainsKey(pathPart))
        {
            pathPart = cameFrom[pathPart];
            tilePositions.Insert(0, pathPart);
        }

        return new Path(tilePositions);
    }

    private float DistanceToTarget(Vector3Int from, Vector3Int target)
    {
        return Vector3Int.Distance(from, target);
    }

    private Comparison<Vector3Int> SortByFScoreFunction(Dictionary<Vector3Int, float> fScore)
    {
        return (a, b) =>
        {
            var distanceA = fScore[a];
            var distanceB = fScore[b];
            if (distanceA < distanceB)
            {
                return -1;
            }

            if (distanceA > distanceB)
            {
                return 1;
            }

            return 0;
        };
    }

    private void DebugDrawRouteNetwork(Dictionary<Vector3Int, List<Vector3Int>> routeNetwork)
    {
        debugTilemap.ClearAllTiles();
        foreach (var (tilePosition, neighbours) in routeNetwork)
        {
            var tile = neighbours.Count < 2 ? debugRouteDeadEndTile : debugRouteTile;
            debugTilemap.SetTile(tilePosition, tile);
        }
    }
}