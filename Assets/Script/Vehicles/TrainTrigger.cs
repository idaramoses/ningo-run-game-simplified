using UnityEngine;

namespace Vehicles
{
    public class TrainTrigger : MonoBehaviour
    {
        [Tooltip("Reference to the VehicleMover of the train to start moving.")]
        public VehicleMover trainMover;

        [Tooltip("Reference to the TrainMover of the train to start moving.")]
        public TrainMover trainMoverNew;

        private bool triggered = false;

        private void OnTriggerEnter(Collider other)
        {
            if (triggered) return;

            // Detect Player tag
            if (other.CompareTag("Player"))
            {
                triggered = true;
                
                if (trainMover != null)
                {
                    trainMover.StartMoving();
                    Debug.Log($"[TrainTrigger] Triggered blue train movement via explicit reference!");
                }
                else if (trainMoverNew != null)
                {
                    trainMoverNew.StartMoving();
                    Debug.Log($"[TrainTrigger] Triggered blue train movement via explicit TrainMover reference!");
                }
                else
                {
                    // Fallback: search on parent RoadSegment's children
                    RoadSegment roadSeg = GetComponentInParent<RoadSegment>();
                    if (roadSeg != null)
                    {
                        VehicleMover foundMover = roadSeg.GetComponentInChildren<VehicleMover>();
                        if (foundMover != null)
                        {
                            trainMover = foundMover;
                            trainMover.StartMoving();
                            Debug.Log($"[TrainTrigger] Found and triggered blue train '{trainMover.gameObject.name}' dynamically in parent segment!");
                        }
                        else
                        {
                            TrainMover foundTrainMover = roadSeg.GetComponentInChildren<TrainMover>();
                            if (foundTrainMover != null)
                            {
                                trainMoverNew = foundTrainMover;
                                trainMoverNew.StartMoving();
                                Debug.Log($"[TrainTrigger] Found and triggered blue train '{trainMoverNew.gameObject.name}' via TrainMover dynamically!");
                            }
                            else
                            {
                                Debug.LogWarning("[TrainTrigger] No VehicleMover or TrainMover found in parent RoadSegment!");
                            }
                        }
                    }
                    else
                    {
                        Debug.LogWarning("[TrainTrigger] Could not find parent RoadSegment to trigger train!");
                    }
                }
            }
        }

        private void OnDisable()
        {
            // Reset trigger state when disabled (e.g. when road segment is recycled by pool)
            triggered = false;
        }
    }
}
