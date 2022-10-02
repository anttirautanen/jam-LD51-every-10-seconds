using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TrafficController : MonoBehaviour
{
    public BuildingController buildingController;

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
        var (runway, _) = _isRunwayFree.FirstOrDefault(kvp => kvp.Value);
        if (runway == null)
        {
            return (null, null);
        }

        var (gate, _) = _isGateFree.FirstOrDefault(kvp => kvp.Value);
        if (gate == null)
        {
            return (null, null);
        }

        var pathExistsBetweenRunwayEndAndGate = buildingController.FindPath(runway.End, gate.Position) != null;
        if (!pathExistsBetweenRunwayEndAndGate)
        {
            return (null, null);
        }

        _isGateFree[gate] = false;
        _isRunwayFree[runway] = false;

        return (runway, gate);
    }
}