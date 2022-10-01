using System.Collections;
using UnityEngine;

public class PlaneController : MonoBehaviour
{
    public TrafficController trafficController;
    public Transform planePrefab;

    private void Start()
    {
        StartCoroutine(nameof(PlaneEnter));
    }

    private IEnumerator PlaneEnter()
    {
        for (;;)
        {
            var runway = trafficController.ReserveRunwayForLanding();
            var gate = trafficController.ReserveGate();
            if (runway != null && gate != null)
            {
                var plane = Instantiate(planePrefab, runway.Start - new Vector3Int(100, 0), Quaternion.identity, transform);
                plane.GetComponent<Plane>().Init(runway, gate);
            }
            else
            {
                Debug.Log($"Could not land, runway:{runway != null}, gate:{gate != null}");
                if (runway != null)
                {
                    trafficController.FreeRunway(runway);
                }
                if (gate != null)
                {
                    trafficController.FreeGate(gate);
                }
            }

            yield return new WaitForSeconds(5);
        }
    }
}