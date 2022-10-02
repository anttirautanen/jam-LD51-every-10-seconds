using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneController : MonoBehaviour
{
    public TrafficController trafficController;
    public BuildingController buildingController;
    public ScoreController scoreController;
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
            var (runway, gate, reason) = trafficController.ReserveRunwayAndGateForLanding();
            if (runway != null && gate != null)
            {
                var planeTransform = Instantiate(
                    planePrefab,
                    runway.Start - new Vector3(100, 0.5f),
                    Quaternion.identity,
                    transform
                );
                var plane = planeTransform.GetComponent<Plane>();
                plane.Init(runway, gate, buildingController, trafficController, this);
                runway.AssignPlane(plane);
                _planes.Add(plane);
            }
            else
            {
                scoreController.RecordMissedPlane(reason);
            }

            yield return new WaitForSeconds(10);
        }
    }

    public List<Plane> Planes()
    {
        return _planes;
    }

    public void ExitPlane(Plane plane)
    {
        _planes.Remove(plane);
        Destroy(plane.gameObject);
    }
}