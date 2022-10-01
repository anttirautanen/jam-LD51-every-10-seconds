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

    public static readonly Vector3 IntToFloatPositionModifier = new(0.5f, -0.5f);

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

        switch (_state)
        {
            case PlaneState.Landing:
                MoveTowards(_runway.End);

                // When plane passes runway start, start breaking
                if (transform.localPosition.x >= _runway.Start.x)
                {
                    _state = PlaneState.Breaking;
                }

                break;

            case PlaneState.Breaking:
                _effectiveSpeed = Mathf.Clamp(_effectiveSpeed - 50f * Time.deltaTime, taxiSpeed, maxSpeed);
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
                    Debug.Log($"PATH FOUND, {_path.TilePositions.First()} -> {_path.TilePositions.Last()}");
                    _pathIndex = 0;
                    _state = PlaneState.TaxiToGate;
                }

                break;

            case PlaneState.TaxiToGate:
                var hasReachedTarget = FollowPath();
                if (hasReachedTarget)
                {
                    _state = PlaneState.StandBy;
                }

                break;

            case PlaneState.StandBy:
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
        var modifiedTargetPosition = targetPosition + IntToFloatPositionModifier;
        var localPosition = transform.localPosition;
        transform.localPosition = Vector3.MoveTowards(localPosition, modifiedTargetPosition, Time.deltaTime * _effectiveSpeed);
        return Vector3.Distance(localPosition, modifiedTargetPosition) < 0.1f;
    }
}