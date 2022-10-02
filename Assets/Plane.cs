using UnityEngine;

public enum PlaneState
{
    Landing,
    Breaking,
    TaxiToGate,
    StandBy,
    TaxiToRunway,
    TakeOff
}

public class Plane : MonoBehaviour
{
    public float maxSpeed = 75f;
    public float taxiSpeed = 15f;
    public float breakingSpeed = 45f;
    public PlaneState state = PlaneState.Landing;

    private Runway _runway;
    private Gate _gate;
    private BuildingController _buildingController;
    private TrafficController _trafficController;
    private PlaneController _planeController;
    private bool _slowDownLanding;
    private float _effectiveSpeed;
    private Path _path;
    private int _pathIndex;
    private float _standByStartedAt;

    public void Init(
        Runway runway,
        Gate gate,
        BuildingController buildingController,
        TrafficController trafficController,
        PlaneController planeController
    )
    {
        _runway = runway;
        _gate = gate;
        _buildingController = buildingController;
        _trafficController = trafficController;
        _planeController = planeController;
        _effectiveSpeed = maxSpeed;
    }

    private void Update()
    {
        // This plane has not yet been initialized
        if (state == PlaneState.Landing && _runway == null)
        {
            return;
        }

        switch (state)
        {
            case PlaneState.Landing:
                MoveTowards(_runway.End);

                // When plane passes runway start, start breaking
                if (transform.localPosition.x >= _runway.Start.x)
                {
                    state = PlaneState.Breaking;
                }

                break;

            case PlaneState.Breaking:
                _effectiveSpeed = Mathf.Clamp(_effectiveSpeed - breakingSpeed * Time.deltaTime, taxiSpeed, maxSpeed);
                MoveTowards(_runway.End);

                // When breaking reaches taxi speed
                if (_effectiveSpeed - taxiSpeed < 2f)
                {
                    _effectiveSpeed = taxiSpeed;
                    var localPosition = transform.localPosition;
                    _path = _buildingController.FindPath(
                        new Vector3Int(
                            (int)Mathf.Floor(localPosition.x),
                            (int)Mathf.Ceil(localPosition.y)),
                        _gate.Position);
                    if (_path != null)
                    {
                        _pathIndex = 0;
                        state = PlaneState.TaxiToGate;
                    }
                }

                break;

            case PlaneState.TaxiToGate:
                var hasReachedGate = FollowPath();
                if (hasReachedGate)
                {
                    _runway.UnAssignPlane(this);
                    _runway = null;
                    _path = null;
                    state = PlaneState.StandBy;
                    _standByStartedAt = Time.time;
                }

                break;

            case PlaneState.StandBy:
                var hasCompletedStandBy = Time.time - _standByStartedAt > 20;
                var hasReservedRunway = _runway != null;
                if (hasCompletedStandBy && !hasReservedRunway)
                {
                    var runway = _trafficController.ReserveRunwayForTakeOff(_gate);
                    if (runway != null)
                    {
                        _gate.UnAssignPlane(this);
                        runway.AssignPlane(this);
                        _runway = runway;
                    }
                }

                var hasPathToRunway = _path != null;
                if (hasReservedRunway && !hasPathToRunway)
                {
                    _path = _buildingController.FindPath(_gate.Position, _runway.Start);
                    if (_path != null)
                    {
                        _trafficController.UnAssignGate(_gate);
                        _pathIndex = 0;
                        state = PlaneState.TaxiToRunway;
                    }
                }

                break;
            case PlaneState.TaxiToRunway:
                var hasReachedRunway = FollowPath();
                if (hasReachedRunway)
                {
                    _path = null;
                    state = PlaneState.TakeOff;
                }

                break;
            case PlaneState.TakeOff:
                _effectiveSpeed = Mathf.Clamp(_effectiveSpeed + 35f * Time.deltaTime, taxiSpeed, maxSpeed);
                var hasReachedExitPoint = MoveTowards(_runway.End + new Vector3Int(100, 0));
                if (hasReachedExitPoint)
                {
                    _runway.UnAssignPlane(this);
                    _planeController.ExitPlane(this);
                }

                break;
        }
    }

    private bool FollowPath()
    {
        var targetPosition = _path.TilePositions[_pathIndex];
        var hasReachedTarget = MoveTowards(targetPosition);
        if (hasReachedTarget)
        {
            var isOnFinalLegOfPath = _path.TilePositions.Count - 1 == _pathIndex;
            if (isOnFinalLegOfPath)
            {
                return true;
            }

            _pathIndex++;
        }

        return false;
    }

    private bool MoveTowards(Vector3Int targetPosition)
    {
        var localPosition = transform.localPosition;
        transform.localPosition = Vector3.MoveTowards(localPosition, targetPosition, Time.deltaTime * _effectiveSpeed);
        return Vector3.Distance(localPosition, targetPosition) < 0.1f;
    }
}