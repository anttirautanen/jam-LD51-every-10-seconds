using UnityEngine;

public class Runway
{
    public readonly Vector3Int Start;
    public readonly Vector3Int End;
    public Plane AssignedPlane;

    public Runway(Vector3Int start, Vector3Int end)
    {
        Start = start;
        End = end;
    }

    public void AssignPlane(Plane plane)
    {
        AssignedPlane = plane;
    }

    public void UnAssignPlane(Plane plane)
    {
        if (AssignedPlane == plane)
        {
            AssignedPlane = null;
        }
    }

    public bool HasPlaneInLandingState()
    {
        if (AssignedPlane != null)
        {
            return AssignedPlane.state == PlaneState.Landing;
        }

        return false;
    }

    public bool HasPlaneAboutToTakeOff()
    {
        if (AssignedPlane != null)
        {
            return AssignedPlane.state is PlaneState.TakeOff or PlaneState.TaxiToRunway or PlaneState.StandBy;
        }

        return false;
    }
}