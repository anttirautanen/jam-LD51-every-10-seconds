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
            if (runway != null)
            {
                var plane = Instantiate(planePrefab, runway.Start - new Vector3Int(100, 0), Quaternion.identity, transform);
                plane.GetComponent<Plane>().Init(runway);
            }

            yield return new WaitForSeconds(5);
        }
    }
}