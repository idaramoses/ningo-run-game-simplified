using UnityEngine;

[DisallowMultipleComponent]
public class JunctionTrafficController : MonoBehaviour
{
    [Header("Junction Taxis")]
    [SerializeField] private JunctionTaxiController[] _taxis;
    [Header("Nearby Activation")]
    [Tooltip("Seconds before the runner reaches each side-road entrance.")]
    [SerializeField, Min(1f)] private float _noticeSeconds = 3.5f;
    [SerializeField, Min(1f)] private float _minimumActivationDistance = 20f;
    [SerializeField, Min(1f)] private float _maximumActivationDistance = 65f;
    [SerializeField, Min(0.1f)] private float _minimumDepartureGap = 1.25f;
    [Tooltip("Combined Z span of both maximum-radius turns including the taxi bodies.")]
    [SerializeField, Min(1f)] private float _entranceEnvelopeClearance = 13.1f;
    [SerializeField, Min(0.1f)] private float _retryInterval = 0.35f;

    private SimplePlayerController _runner;
    private GameStateController _game;
    private float _nextDepartureTime;
    private float _nextCheckTime;
    private bool _resetPending;

    private void OnEnable()
    {
        _resetPending = true;
        _nextDepartureTime = 0f;
        _nextCheckTime = 0f;
    }

    private void Update()
    {
        if (_taxis == null || _taxis.Length == 0) return;
        if (_runner == null) _runner = SimplePlayerController.Instance;
        if (_game == null) _game = GameStateController.Instance;
        if (_resetPending)
        {
            for (int i = 0; i < _taxis.Length; i++)
            {
                if (_taxis[i] == null) continue;
                _taxis[i].gameObject.SetActive(true);
                _taxis[i].ResetEncounter();
            }
            _resetPending = false;
        }
        if (_runner == null || !_runner.isActiveAndEnabled || _game == null
            || !_game.IsPlaying() || !_runner.IsMoving || Time.deltaTime <= 0f) return;

        for (int i = 0; i < _taxis.Length; i++)
            if (_taxis[i] != null && _taxis[i].gameObject.activeInHierarchy)
                _taxis[i].Advance(Time.deltaTime, _runner.transform);

        if (Time.time < _nextCheckTime || Time.time < _nextDepartureTime) return;
        _nextCheckTime = Time.time + _retryInterval;
        // Entrances have different Z positions: process the nearest waiting one first.
        JunctionTaxiController nearest = null;
        float nearestDistance = float.PositiveInfinity;
        for (int i = 0; i < _taxis.Length; i++)
        {
            JunctionTaxiController taxi = _taxis[i];
            if (taxi == null || !taxi.gameObject.activeInHierarchy || taxi.State != JunctionTaxiController.TaxiState.Waiting) continue;
            float distance = taxi.EntryWorldZ - _runner.transform.position.z;
            if (distance <= 0f) { taxi.SkipEncounter(); continue; }
            if (distance < nearestDistance) { nearest = taxi; nearestDistance = distance; }
        }
        if (nearest == null) return;
        float runnerSpeed = Mathf.Max(0f, _runner.forwardSpeed);
        if (runnerSpeed <= 0f) return;
        float activationDistance = Mathf.Clamp(runnerSpeed * _noticeSeconds,
            _minimumActivationDistance, _maximumActivationDistance);
        if (nearestDistance > activationDistance) return;

        // These authored entrances are 13.5m apart. Their largest turn/body
        // envelopes occupy 13.1m together, so staggered turns can safely overlap
        // in time. Only serialize departures for overlapping entrance envelopes.
        for (int i = 0; i < _taxis.Length; i++)
            if (_taxis[i] != null && (_taxis[i].State == JunctionTaxiController.TaxiState.Approaching
                || _taxis[i].State == JunctionTaxiController.TaxiState.Turning)
                && Mathf.Abs(_taxis[i].EntryWorldZ - nearest.EntryWorldZ) < _entranceEnvelopeClearance) return;

        int first = Random.Range(0, 6);
        for (int candidate = 0; candidate < 6; candidate++)
        {
            int route = (first + candidate) % 6;
            int lane = route / 2;
            int direction = route % 2 == 0 ? -1 : 1;
            if (!nearest.AllowsOutgoingDirection(direction)) continue;
            bool reserved = false;
            for (int i = 0; i < _taxis.Length; i++)
                if (_taxis[i] != null && _taxis[i].IsActive && _taxis[i].SelectedLane == lane)
                    reserved = true;
            if (reserved) continue;
            float laneX = (lane - 1) * _runner.laneDistance;
            float entryTime = nearest.EstimateEntryTime(laneX);
            // At the default speed the turn should finish while the junction is
            // still ahead, not after the runner has already passed the encounter.
            if (entryTime + 0.4f > nearestDistance / runnerSpeed) continue;
            if (!nearest.IsRouteClear(laneX, direction, _runner.transform)) continue;
            if (nearest.BeginRoute(lane, laneX, direction))
            {
                _nextDepartureTime = Time.time + _minimumDepartureGap;
                return;
            }
        }
        // A blocked or too-late entrance stays parked, instead of spawning a
        // last-second unavoidable hazard. Subsequent checks may find clearance.
    }
}
