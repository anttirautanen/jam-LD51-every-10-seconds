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

    public Runway ReserveRunwayForLanding()
    {
        var (runway, _) = _isRunwayFree.FirstOrDefault(kvp => kvp.Value);
        if (runway == null)
        {
            return null;
        }

        _isRunwayFree[runway] = false;
        return runway;
    }

    public Gate ReserveGate()
    {
        var (gate, _) = _isGateFree.FirstOrDefault(kvp => kvp.Value);
        if (gate == null)
        {
            return null;
        }

        _isGateFree[gate] = false;
        return gate;
    }

    public void FreeRunway(Runway runway)
    {
        _isRunwayFree[runway] = true;
    }

    public void FreeGate(Gate gate)
    {
        _isGateFree[gate] = true;
    }
}