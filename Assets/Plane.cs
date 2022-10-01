using UnityEngine;

public enum PlaneState
{
    Landing,
    Breaking,
    TaxiToTerminal,
}

public class Plane : MonoBehaviour
{
    public float maxSpeed = 75f;
    public float taxiSpeed = 15f;

    private Runway _runway;
    private bool _slowDownLanding;
    private PlaneState _state = PlaneState.Landing;
    private float _effectiveSpeed = 0;

    public void Init(Runway runway)
    {
        _runway = runway;
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
            _state = PlaneState.TaxiToTerminal;
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
            
            case PlaneState.TaxiToTerminal:
                break;
        }
    }

    private void MoveTowardsRunwayEnd()
    {
        transform.localPosition = Vector3.MoveTowards(transform.localPosition, _runway.End, Time.deltaTime * _effectiveSpeed);
    }
}