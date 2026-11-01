using UnityEngine;

[DisallowMultipleComponent]
public class JunctionTaxiController : MonoBehaviour
{
    public enum TaxiState { Waiting, Approaching, Turning, Cruising, Finished }
    public enum TurnDirection { Random, Left, Right }

    [Header("Turn Choice")]
    [Tooltip("Left and Right are relative to the taxi entering from its side street. Applied when the next encounter starts; blocked turns wait rather than switching direction.")]
    [SerializeField] private TurnDirection _turnDirection = TurnDirection.Random;

    public bool AllowsOutgoingDirection(int outgoingSign)
    {
        if (_turnDirection == TurnDirection.Random) return true;
        int leftOutgoingSign = _incomingSign < 0 ? -1 : 1;
        int selectedSign = _turnDirection == TurnDirection.Left ? leftOutgoingSign : -leftOutgoingSign;
        return (outgoingSign < 0 ? -1 : 1) == selectedSign;
    }

    [Header("Authored Route")]
    [SerializeField] private Transform _tileRoot;
    [SerializeField] private Transform _entryAnchor;
    [Tooltip("+1 enters from the left street; -1 enters from the right street.")]
    [SerializeField] private int _incomingSign = -1;
    [SerializeField, Min(0.1f)] private float _speed = 5f;
    [SerializeField, Min(0.1f)] private float _turnRadius = 2f;
    [Tooltip("A wider turn is needed when crossing to the opposite outer lane.")]
    [SerializeField, Min(0.1f)] private float _farLaneTurnRadius = 6f;
    private float _activeRadius;
    private float RadiusForLane(float laneX) => laneX * _incomingSign > 0.1f ? _farLaneTurnRadius : _turnRadius;
    [SerializeField, Min(1f)] private float _cleanupBehindDistance = 35f;
    [Tooltip("Limit the encounter lifetime so a fast taxi never outlives its owning tile.")]
    [SerializeField, Min(1f)] private float _maximumTravelDistance = 65f;

    private Vector3 _initialLocalPosition;
    private Quaternion _initialLocalRotation;
    private Quaternion _modelCorrection;
    private Vector3 _start;
    private Vector3 _turnStart;
    private Vector3 _turnEnd;
    private Vector3 _incoming;
    private Vector3 _outgoing;
    private float _approachLength;
    private float _arcLength;
    private float _distance;
    private bool _initialized;

    public TaxiState State { get; private set; }
    public int SelectedLane { get; private set; } = -1;
    public int OutgoingSign { get; private set; }
    public float Speed => _speed;
    public float EntryWorldZ => _entryAnchor == null ? transform.position.z : _entryAnchor.position.z;
    public float TimeUntilCruising => Mathf.Max(0f, _approachLength + _arcLength - _distance) / _speed;
    public bool IsActive => State == TaxiState.Approaching || State == TaxiState.Turning || State == TaxiState.Cruising;

    private void Awake()
    {
        Initialize();
    }

    private void OnEnable()
    {
        Initialize();
        ResetEncounter();
    }

    private void Initialize()
    {
        if (_initialized || _tileRoot == null || _entryAnchor == null) return;
        _initialLocalPosition = transform.localPosition;
        _initialLocalRotation = transform.localRotation;
        _incoming = _tileRoot.right * (_incomingSign < 0 ? -1f : 1f);
        _modelCorrection = Quaternion.Inverse(Quaternion.LookRotation(_incoming, _tileRoot.up)) * transform.rotation;
        _initialized = true;
    }

    public void ResetEncounter()
    {
        if (!_initialized) return;
        transform.localPosition = _initialLocalPosition;
        transform.localRotation = _initialLocalRotation;
        _distance = 0f;
        SelectedLane = -1;
        OutgoingSign = 0;
        State = TaxiState.Waiting;
    }

    public float EstimateEntryTime(float laneX)
    {
        if (_tileRoot == null || _entryAnchor == null) return 0f;
        Vector3 turnStart = GetTurnStart(laneX);
        return (Vector3.Distance(transform.position, turnStart) + RadiusForLane(laneX) * Mathf.PI * 0.5f) / _speed;
    }

    private Vector3 GetTurnStart(float laneX)
    {
        Vector3 local = _tileRoot.InverseTransformPoint(_entryAnchor.position);
        local.x = laneX;
        Vector3 lanePoint = _tileRoot.TransformPoint(local);
        // Radius and speed are world metres, not scaled ancestor-local units.
        return lanePoint - _tileRoot.right * _incomingSign * RadiusForLane(laneX);
    }

    public bool BeginRoute(int lane, float laneX, int outgoingSign)
    {
        Initialize();
        if (!_initialized || State != TaxiState.Waiting || lane < 0 || lane > 2) return false;
        _incoming = _tileRoot.right * (_incomingSign < 0 ? -1f : 1f);
        _outgoing = _tileRoot.forward * (outgoingSign < 0 ? -1f : 1f);
        _start = transform.position;
        _activeRadius = RadiusForLane(laneX);
        _turnStart = GetTurnStart(laneX);
        _turnStart.y = _start.y;
        if (Vector3.Dot(_turnStart - _start, _incoming) <= 0f) return false;
        _turnEnd = _turnStart + (_incoming + _outgoing) * _activeRadius;
        _approachLength = Vector3.Distance(_start, _turnStart);
        _arcLength = _activeRadius * Mathf.PI * 0.5f;
        _distance = 0f;
        SelectedLane = lane;
        OutgoingSign = outgoingSign < 0 ? -1 : 1;
        State = TaxiState.Approaching;
        return true;
    }

    // Called once by the coordinator, which owns the gameplay/pause gate.
    public void Advance(float deltaTime, Transform runner)
    {
        if (!IsActive || deltaTime <= 0f) return;
        _distance += _speed * deltaTime;
        Vector3 position;
        Vector3 tangent;
        if (_distance < _approachLength)
        {
            State = TaxiState.Approaching;
            position = _start + _incoming * _distance;
            tangent = _incoming;
        }
        else if (_distance < _approachLength + _arcLength)
        {
            State = TaxiState.Turning;
            float angle = (_distance - _approachLength) / _activeRadius;
            position = _turnStart + _incoming * (_activeRadius * Mathf.Sin(angle))
                + _outgoing * (_activeRadius * (1f - Mathf.Cos(angle)));
            tangent = _incoming * Mathf.Cos(angle) + _outgoing * Mathf.Sin(angle);
        }
        else
        {
            State = TaxiState.Cruising;
            position = _turnEnd + _outgoing * (_distance - _approachLength - _arcLength);
            tangent = _outgoing;
        }
        transform.SetPositionAndRotation(position, Quaternion.LookRotation(tangent, _tileRoot.up) * _modelCorrection);
        if ((runner != null && runner.position.z - position.z > _cleanupBehindDistance)
            || _distance > _maximumTravelDistance)
        {
            State = TaxiState.Finished;
            gameObject.SetActive(false);
        }
    }

    public void SkipEncounter()
    {
        if (State == TaxiState.Waiting) State = TaxiState.Finished;
    }

    private readonly Collider[] _routeHits = new Collider[32];

    public bool IsRouteClear(float laneX, int outgoingSign, Transform runner)
    {
        if (_tileRoot == null || _entryAnchor == null) return false;
        Vector3 incoming = _tileRoot.right * _incomingSign;
        Vector3 outgoing = _tileRoot.forward * (outgoingSign < 0 ? -1f : 1f);
        Vector3 turnStart = GetTurnStart(laneX);
        turnStart.y = transform.position.y;
        if (Vector3.Dot(turnStart - transform.position, incoming) <= 0f) return false;
        // Test the body, not just the pivot, with a small clearance margin.
        for (int sample = 0; sample <= 12; sample++)
        {
            Vector3 p;
            Vector3 tangent;
            if (sample < 4)
            {
                p = Vector3.Lerp(transform.position, turnStart, sample / 3f);
                tangent = incoming;
            }
            else
            {
                float angle = (sample - 4) / 8f * Mathf.PI * 0.5f;
                float radius = RadiusForLane(laneX);
                p = turnStart + incoming * (radius * Mathf.Sin(angle))
                    + outgoing * (radius * (1f - Mathf.Cos(angle)));
                tangent = incoming * Mathf.Cos(angle) + outgoing * Mathf.Sin(angle);
            }
            int count = Physics.OverlapBoxNonAlloc(p + _tileRoot.up * 0.79f,
                new Vector3(0.95f, 0.75f, 2.05f), _routeHits,
                Quaternion.LookRotation(tangent, _tileRoot.up), ~0, QueryTriggerInteraction.Collide);
            if (count == _routeHits.Length) return false;
            for (int i = 0; i < count; i++)
            {
                Collider hit = _routeHits[i];
                if (hit == null || hit.transform == transform || hit.transform.IsChildOf(transform)) continue;
                if (runner != null && (hit.transform == runner || hit.transform.IsChildOf(runner))) continue;
                if (hit.CompareTag("Obstacle") || hit.GetComponentInParent<VehicleMover>() != null
                    || hit.GetComponentInParent<TrainMover>() != null) return false;
            }
        }
        return true;
    }

    private void OnDrawGizmosSelected()
    {
        if (_entryAnchor == null || _tileRoot == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, _entryAnchor.position);
        Gizmos.DrawWireSphere(_entryAnchor.position, _turnRadius);
    }
}
