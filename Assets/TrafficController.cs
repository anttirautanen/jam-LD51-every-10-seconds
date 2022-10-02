using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TrafficController : MonoBehaviour
{
    public BuildingController buildingController;
    public PlaneController planeController;

    private List<Runway> _runways;
    private List<Gate> _gates;
    private readonly Dictionary<Runway, bool> _isRunwayFree = new();
    private readonly Dictionary<Gate, bool> _isGateFree = new();

    private void Start()
    {
        _runways = buildingController.GetRunways();
        _gates = buildingController.GetGates();

        foreach (var runway in _runways)
        {
            _isRunwayFree.Add(runway, true);
        }

        foreach (var gate in _gates)
        {
            _isGateFree.Add(gate, true);
        }
    }

    public (Runway, Gate) ReserveRunwayAndGateForLanding()
    {
        Debug.Log("ReserveRunwayAndGateForLanding");
        var (runway, _) = _isRunwayFree.FirstOrDefault(kvp => kvp.Value);
        if (runway == null)
        {
            Debug.Log("--no runway");
            return (null, null);
        }

        var (gate, _) = _isGateFree.FirstOrDefault(kvp => kvp.Value);
        if (gate == null)
        {
            Debug.Log("--no gate");
            return (null, null);
        }

        var pathExistsBetweenRunwayEndAndGate = buildingController.FindPath(runway.End, gate.Position) != null;
        if (!pathExistsBetweenRunwayEndAndGate)
        {
            Debug.Log("--no path");
            return (null, null);
        }

        _isGateFree[gate] = false;
        _isRunwayFree[runway] = false;

        return (runway, gate);
    }

    public Runway ReserveRunwayForTakeOff(Gate from)
    {
        var (runway, _) = _isRunwayFree.FirstOrDefault(kvp => kvp.Value);
        if (runway == null)
        {
            return null;
        }
        
        var pathExistsBetweenGateAndRunway = buildingController.FindPath(from.Position, runway.Start) != null;
        if (!pathExistsBetweenGateAndRunway)
        {
            return null;
        }

        _isRunwayFree[runway] = false;

        return runway;
    }

    public void UnAssignGate(Gate gate)
    {
        _isGateFree[gate] = true;
    }

    private void Update()
    {
        foreach (var runway in _runways)
        {
            if (runway.HasPlaneInLandingState())
            {
                continue;
            }
            
            if (runway.HasPlaneAboutToTakeOff())
            {
                continue;
            }

            var isAnyPlaneOnRunway = planeController.Planes().Any(plane => IsPlaneOnRunway(plane, runway));
            if (!isAnyPlaneOnRunway)
            {
                _isRunwayFree[runway] = true;
            }
        }
    }

    private static bool IsPlaneOnRunway(Plane plane, Runway runway)
    {
        var runwayTiles = Utils.GetAllTilesInArea(runway.Start, runway.End);
        return runwayTiles.Any(runwayTile => Vector3.Distance(runwayTile, plane.transform.localPosition) < 5f);
    }
}