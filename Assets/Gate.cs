using UnityEngine;

public class Gate
{
    public Vector3Int Position;

    private Plane _plane;

    public Gate(Vector3Int position)
    {
        Position = position;
    }

    public void AssignPlane(Plane plane)
    {
        _plane = plane;
    }

    public void UnAssignPlane(Plane plane)
    {
        if (_plane == plane)
        {
            _plane = null;
        }
    }

    public bool HasPlaneAboutToEnterOrInStandBy()
    {
        if (_plane == null)
        {
            return false;
        }

        return _plane.state is PlaneState.Landing or PlaneState.Breaking or PlaneState.TaxiToGate or PlaneState.StandBy;
    }
}