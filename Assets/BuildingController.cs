using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BuildingController : MonoBehaviour
{
    public ToolController toolController;
    public ScoreController scoreController;
    public Tilemap outsideTilemap;
    public Tilemap paintTilemap;
    public Tilemap debugTilemap;
    public Tile debugRouteTile;
    public Tile debugRouteDeadEndTile;

    private Dictionary<Vector3Int, List<Vector3Int>> _routeNetwork;
    private readonly List<Runway> _runways = new();
    private readonly List<Taxiway> _taxiways = new();
    private readonly List<Gate> _gates = new();
    private readonly List<(Vector3Int, Vector3Int)> _dirt = new();
    private readonly List<(Vector3Int, Vector3Int)> _tarmac = new();

    private void Start()
    {
        MouseController.OnBuild += OnBuild;
        OnBuild(Tool.Dirt, new Vector3Int(-10, 50), new Vector3Int(10, 40));
    }

    public bool CanBuild(Tool tool, Vector3Int areaStart, Vector3Int areaEnd)
    {
        var cost = GetCost(tool, areaStart, areaEnd);
        if (!scoreController.CanPay(cost))
        {
            return false;
        }
        
        switch (tool)
        {
            case Tool.Dirt:
                var isOnTopOfAnythingElse = IsAtLeastPartlyOnTopOfDirt(areaStart, areaEnd) || IsAtLeastPartlyOnTopOfTarmac(areaStart, areaEnd);
                return !isOnTopOfAnythingElse;

            case Tool.Tarmac:
                var isOnTopOfTarmac = IsAtLeastPartlyOnTopOfTarmac(areaStart, areaEnd);
                if (isOnTopOfTarmac)
                {
                    return false;
                }

                var paddedAreaStartX = areaStart.x < areaEnd.x ? areaStart.x : areaEnd.x;
                var paddedAreaStartY = areaStart.y < areaEnd.y ? areaStart.y : areaEnd.y;
                var paddedAreaEndX = areaStart.x < areaEnd.x ? areaEnd.x : areaStart.x;
                var paddedAreaEndY = areaStart.y < areaEnd.y ? areaEnd.y : areaStart.y;
                var paddedTarmacAreaStart = new Vector3Int(paddedAreaStartX - 2, paddedAreaStartY - 2);
                var paddedTarmacAreaEnd = new Vector3Int(paddedAreaEndX + 2, paddedAreaEndY + 2);
                return IsCompletelyOnTopOfDirt(paddedTarmacAreaStart, paddedTarmacAreaEnd);

            case Tool.Runway:
                var (paddedRunwayAreaStart, paddedRunwayAreaEnd) = GetPaddedRunwayArea(areaStart, areaEnd);
                return IsCompletelyOnTopOfTarmac(paddedRunwayAreaStart, paddedRunwayAreaEnd)
                       && !IsAtLeastPartlyOnTopOfGate(paddedRunwayAreaStart, paddedRunwayAreaEnd)
                       && paddedRunwayAreaEnd.x - paddedRunwayAreaStart.x > 60;

            case Tool.Taxiway:
                var (paddedTaxiwayAreaStart, paddedTaxiwayAreaEnd) = GetPaddedTaxiwayArea(areaStart, areaEnd);
                return IsCompletelyOnTopOfTarmac(paddedTaxiwayAreaStart, paddedTaxiwayAreaEnd)
                       && !IsAtLeastPartlyOnTopOfGate(areaStart, areaEnd);

            case Tool.Gate:
                var (gateAreaStart, gateAreaEnd) = GetPaddedGateArea(areaStart);
                return IsCompletelyOnTopOfTarmac(gateAreaStart, gateAreaEnd)
                       && !IsAtLeastPartlyOnTopOfTaxiway(gateAreaStart, gateAreaEnd)
                       && !IsAtLeastPartlyOnTopOfGate(gateAreaStart, gateAreaEnd)
                       && !IsAtLeastPartlyOnTopOfRunway(gateAreaStart, gateAreaEnd);

            default:
                return true;
        }
    }

    public decimal GetCost(Tool tool, Vector3Int areaStart, Vector3Int areaEnd)
    {
        var costsPerTile = new Dictionary<Tool, decimal>
        {
            { Tool.Dirt, 700 },
            { Tool.Tarmac, 50 },
            { Tool.Gate, 100000 }
        };
        var tileCount = Utils.GetAllTilesInArea(areaStart, areaEnd).Count;
        return costsPerTile.GetValueOrDefault(tool, 0) * tileCount;
    }

    private bool IsAtLeastPartlyOnTopOfDirt(Vector3Int areaStart, Vector3Int areaEnd)
    {
        var tilesInQuestion = Utils.GetAllTilesInArea(areaStart, areaEnd);
        foreach (var (dirtAreaStart, dirtAreaEnd) in _dirt)
        {
            var dirtTiles = Utils.GetAllTilesInArea(dirtAreaStart, dirtAreaEnd);
            foreach (var dirtTile in dirtTiles)
            {
                var index = tilesInQuestion.FindIndex(tileInQuestion => tileInQuestion == dirtTile);
                if (index >= 0)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool IsCompletelyOnTopOfDirt(Vector3Int areaStart, Vector3Int areaEnd)
    {
        var tilesInQuestion = Utils.GetAllTilesInArea(areaStart, areaEnd);
        foreach (var tileInQuestion in tilesInQuestion)
        {
            var isOnTopOfDirt = IsAtLeastPartlyOnTopOfDirt(tileInQuestion, tileInQuestion);
            if (!isOnTopOfDirt)
            {
                return false;
            }
        }

        return true;
    }

    private bool IsAtLeastPartlyOnTopOfTarmac(Vector3Int areaStart, Vector3Int areaEnd)
    {
        var tilesInQuestion = Utils.GetAllTilesInArea(areaStart, areaEnd);
        foreach (var (dirtAreaStart, dirtAreaEnd) in _tarmac)
        {
            var tarmacTiles = Utils.GetAllTilesInArea(dirtAreaStart, dirtAreaEnd);
            foreach (var tarmacTile in tarmacTiles)
            {
                var index = tilesInQuestion.FindIndex(tileInQuestion => tileInQuestion == tarmacTile);
                if (index >= 0)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool IsCompletelyOnTopOfTarmac(Vector3Int areaStart, Vector3Int areaEnd)
    {
        var tilesInQuestion = Utils.GetAllTilesInArea(areaStart, areaEnd);
        foreach (var tileInQuestion in tilesInQuestion)
        {
            var isOnTopOfTarmac = IsAtLeastPartlyOnTopOfTarmac(tileInQuestion, tileInQuestion);
            if (!isOnTopOfTarmac)
            {
                return false;
            }
        }

        return true;
    }

    private bool IsAtLeastPartlyOnTopOfTaxiway(Vector3Int areaStart, Vector3Int areaEnd)
    {
        var tilesInQuestion = Utils.GetAllTilesInArea(areaStart, areaEnd);
        foreach (var taxiway in _taxiways)
        {
            var taxiwayTiles = Utils.GetAllTilesInArea(taxiway.Start, taxiway.End);
            foreach (var taxiwayTile in taxiwayTiles)
            {
                var index = tilesInQuestion.FindIndex(tileInQuestion => tileInQuestion == taxiwayTile);
                if (index >= 0)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool IsAtLeastPartlyOnTopOfRunway(Vector3Int areaStart, Vector3Int areaEnd)
    {
        var tilesInQuestion = Utils.GetAllTilesInArea(areaStart, areaEnd);
        foreach (var runway in _runways)
        {
            var (paddedRunwayAreaStart, paddedRunwayAreaEnd) = GetPaddedRunwayArea(runway.Start, runway.End);
            var runwayAreaTiles = Utils.GetAllTilesInArea(paddedRunwayAreaStart, paddedRunwayAreaEnd);
            foreach (var runwayAreaTile in runwayAreaTiles)
            {
                var index = tilesInQuestion.FindIndex(tileInQuestion => tileInQuestion == runwayAreaTile);
                if (index >= 0)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool IsAtLeastPartlyOnTopOfGate(Vector3Int areaStart, Vector3Int areaEnd)
    {
        var tilesInQuestion = Utils.GetAllTilesInArea(areaStart, areaEnd);
        foreach (var gate in _gates)
        {
            var (paddedGateAreaStart, paddedGateAreaEnd) = GetPaddedGateArea(gate.Position);
            var gateAreaTiles = Utils.GetAllTilesInArea(paddedGateAreaStart, paddedGateAreaEnd);
            foreach (var gateAreaTile in gateAreaTiles)
            {
                var index = tilesInQuestion.FindIndex(tileInQuestion => tileInQuestion == gateAreaTile);
                if (index >= 0)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private void OnBuild(Tool tool, Vector3Int areaStart, Vector3Int areaEnd)
    {
        switch (tool)
        {
            case Tool.Dirt:
                _dirt.Add((areaStart, areaEnd));
                break;
            case Tool.Tarmac:
                _tarmac.Add((areaStart, areaEnd));
                break;
            case Tool.Runway:
                _runways.Add(new Runway(areaStart, areaEnd));
                break;
            case Tool.Taxiway:
                _taxiways.Add(new Taxiway(areaStart, areaEnd));
                break;
            case Tool.Gate:
                _gates.Add(new Gate(areaStart));
                var (gateTaxiwayStart, gateTaxiwayEnd) = GetGateTaxiway(areaStart);
                _taxiways.Add(new Taxiway(gateTaxiwayStart, gateTaxiwayEnd));
                break;
        }

        var allTilesInArea = Utils.GetAllTilesInArea(areaStart, areaEnd);
        foreach (var tileInArea in allTilesInArea)
        {
            (tool is Tool.Runway or Tool.Taxiway or Tool.Gate ? paintTilemap : outsideTilemap).SetTile(tileInArea, toolController.TileTypes[tool]);
        }

        if (tool is Tool.Gate)
        {
            var (gateTaxiwayStart, gateTaxiwayEnd) = GetGateTaxiway(areaStart);
            var allTilesInGateTaxiwayArea = Utils.GetAllTilesInArea(gateTaxiwayStart, gateTaxiwayEnd);
            foreach (var gateTaxiwayTile in allTilesInGateTaxiwayArea)
            {
                paintTilemap.SetTile(gateTaxiwayTile, toolController.TileTypes[Tool.Taxiway]);
            }
        }

        var cost = GetCost(tool, areaStart, areaEnd);
        scoreController.RemoveFromBalance(cost);

        _routeNetwork = DiscoverRouteNetwork();
        // DebugDrawRouteNetwork(_routeNetwork);
    }

    public List<Runway> GetRunways()
    {
        return _runways;
    }

    public List<Gate> GetGates()
    {
        return _gates;
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
                    tiles.TryAdd(tilePosition, emptyList);
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

    private (Vector3Int, Vector3Int) GetPaddedGateArea(Vector3Int gatePosition)
    {
        var gateAreaStart = new Vector3Int(gatePosition.x - 3, gatePosition.y - 3);
        var gateAreaEnd = new Vector3Int(gatePosition.x + 3, gatePosition.y + 3);
        return (gateAreaStart, gateAreaEnd);
    }

    private (Vector3Int, Vector3Int) GetPaddedRunwayArea(Vector3Int areaStart, Vector3Int areaEnd)
    {
        var (start, end) = Utils.SortCorners(areaStart, areaEnd);
        var runwayAreaStart = new Vector3Int(start.x, start.y - 3);
        var runwayAreaEnd = new Vector3Int(end.x, end.y + 3);
        return (runwayAreaStart, runwayAreaEnd);
    }

    private (Vector3Int, Vector3Int) GetPaddedTaxiwayArea(Vector3Int areaStart, Vector3Int areaEnd)
    {
        var (start, end) = Utils.SortCorners(areaStart, areaEnd);
        var runwayAreaStart = new Vector3Int(start.x - 2, start.y - 2);
        var runwayAreaEnd = new Vector3Int(end.x + 2, end.y + 2);
        return (runwayAreaStart, runwayAreaEnd);
    }

    private (Vector3Int, Vector3Int) GetGateTaxiway(Vector3Int gatePosition)
    {
        return (new Vector3Int(gatePosition.x, gatePosition.y - 1), new Vector3Int(gatePosition.x, gatePosition.y - 3));
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