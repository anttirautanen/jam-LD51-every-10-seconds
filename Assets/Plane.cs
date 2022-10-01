using System.Linq;
using UnityEngine;

public enum PlaneState
{
    Landing,
    Breaking,
    TaxiToGate,
    StandBy
}

public class Plane : MonoBehaviour
{
    public float maxSpeed = 75f;
    public float taxiSpeed = 15f;

    private Runway _runway;
    private Gate _gate;
    private BuildingController _buildingController;
    private bool _slowDownLanding;
    private PlaneState _state = PlaneState.Landing;
    private float _effectiveSpeed;
    private Path _path;
    private int _pathIndex;

    public void Init(Runway runway, Gate gate, BuildingController buildingController)
    {
        _runway = runway;
        _gate = gate;
        _buildingController = buildingController;
        _effectiveSpeed = maxSpeed;
    }

    private void Update()
    {
        // This plane has not yet been initialized
        if (_state == PlaneState.Landing && _runway == null)
        {
            return;
        }

        // Start breaking after passing runway start
        if (_state == PlaneState.Landing && transform.localPosition.x >= _runway.Start.x)
        {
            _state = PlaneState.Breaking;
        }

        // When breaking reaches taxi speed
        if (_state == PlaneState.Breaking && _effectiveSpeed - taxiSpeed < 2f)
        {
            _effectiveSpeed = taxiSpeed;
            _state = PlaneState.TaxiToGate;
            var localPosition = transform.localPosition;
            _path = _buildingController.FindPath(
                new Vector3Int(
                    (int)Mathf.Floor(localPosition.x),
                    (int)Mathf.Floor(localPosition.y)),
                _gate.Position);
            _pathIndex = 0;
        }

        if (_state == PlaneState.TaxiToGate && Vector3.Distance(transform.localPosition, _path.TilePositions.Last()) < 0.1f)
        {
            _state = PlaneState.StandBy;
        }

        switch (_state)
        {
            case PlaneState.Landing:
                MoveTowardsRunwayEnd();
                break;

            case PlaneState.Breaking:
                _effectiveSpeed = Mathf.Clamp(_effectiveSpeed - 50f * Time.deltaTime, taxiSpeed, maxSpeed);
                MoveTowardsRunwayEnd();
                break;

            case PlaneState.TaxiToGate:
                FollowPath();
                break;

            case PlaneState.StandBy:
                break;
        }
    }

    private void MoveTowardsRunwayEnd()
    {
        transform.localPosition = Vector3.MoveTowards(transform.localPosition, _runway.End, Time.deltaTime * _effectiveSpeed);
    }

    private void FollowPath()
    {
        var targetPosition = _path.TilePositions[_pathIndex];
        if (Vector3.Distance(transform.localPosition, targetPosition) < 0.1f)
        {
            _pathIndex++;
            targetPosition = _path.TilePositions[_pathIndex];
        }

        transform.localPosition = Vector3.MoveTowards(transform.localPosition, targetPosition, Time.deltaTime * _effectiveSpeed);
    }
}