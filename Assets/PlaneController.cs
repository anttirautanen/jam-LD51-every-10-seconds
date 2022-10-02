using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneController : MonoBehaviour
{
    public TrafficController trafficController;
    public BuildingController buildingController;
    public Transform planePrefab;

    private readonly List<Plane> _planes = new();

    private void Start()
    {
        StartCoroutine(nameof(PlaneEnter));
    }

    private IEnumerator PlaneEnter()
    {
        for (;;)
        {
            var (runway, gate) = trafficController.ReserveRunwayAndGateForLanding();
            if (runway != null && gate != null)
            {
                var planeTransform = Instantiate(
                    planePrefab,
                    runway.Start - new Vector3(100, 0.5f),
                    Quaternion.identity,
                    transform
                );
                var plane = planeTransform.GetComponent<Plane>();
                plane.Init(runway, gate, buildingController);
                runway.AssignPlane(plane);
                _planes.Add(plane);
            }

            yield return new WaitForSeconds(5);
        }
    }

    public List<Plane> Planes()
    {
        return _planes;
    }
}