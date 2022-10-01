using System.Collections.Generic;
using UnityEngine;

public class TrafficController : MonoBehaviour
{
    public BuildingController buildingController;

    private List<Runway> _runways;

    private void Start()
    {
        _runways = buildingController.GetRunways();
    }

    public Runway ReserveRunwayForLanding()
    {
        return _runways[0];
    }
}
