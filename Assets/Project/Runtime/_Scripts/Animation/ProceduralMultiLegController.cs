using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralMultiLegController : MonoBehaviour
{
    [SerializeField] private bool GizmosEnabled = true;
    [SerializeField] private float stepDistance = 0.1f;
    [SerializeField] private float stepHeight = 0.1f;
    [SerializeField] private AnimationCurve stepCurveVertical;
    [SerializeField] private AnimationCurve stepCurveHorizontal;
    [SerializeField] private float stepDuration = 0.5f;
    [SerializeField] private float minLegStretchDistance = 0.1f;
    [SerializeField] private float maxLegStretchDistance = 0.5f;

    [Serializable] 
    private class Foot
    {
        public Transform rootLegTransform;
        public Transform footTransform;
        public Transform targetTransform;
        internal Vector3 Position => footTransform.position;
        internal Vector3 LegPosition => rootLegTransform.position;
        internal Vector3 PreviousPosition;
        internal Vector3 TargetPosition => targetTransform.position;
        internal float stepProgress = 0f;
        internal bool isStepping = false;
        
        internal SecondOrderDynamics dynamics;
        
        internal void UpdatePreviousPosition()
        {
            PreviousPosition = Position;
        }
    }
    
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float raycastRange = 1f;
    
    // Introduced a variable to control the stepping overlap
    [SerializeField] private float stepOverlap = 0.2f;

    // Body control variables
    private Vector3 bodyPosition;
    private Vector3 bodyRotation;

    // NonReorderable is a custom attribute to prevent reordering in the inspector
    // It's used in this case to prevent a nested-class array, overlapping-inspector bug
    [SerializeField] [NonReorderable] private Foot[] feet;

    [Range(0, 15)] public float frequency = 1f;
    [Range(0, 5)] public float dampingCoefficient = 0.5f;
    [Range(-5, 5)] public float initialResponse = 2f;
    
    private Vector3 lastBodyUp;
    private bool[] legMoving;
    private int nbLegs;
    
    private Vector3 velocity;
    private Vector3 lastVelocity;
    private Vector3 lastBodyPos;

    private float velocityMultiplier = 15f;

    private void Awake()
    {
        CheckTransformArray();
        
        foreach (var foot in feet)
        {
            foot.PreviousPosition = foot.Position;
            foot.dynamics = new SecondOrderDynamics(frequency, dampingCoefficient, initialResponse, foot.Position);
        }
    }

    private void FixedUpdate()
    {
        AdjustTargetPositions();
        DriveLegs();
        PositionBody();
    }
    
    private void AdjustTargetPositions()
    {
        foreach (var foot in feet)
        {
            RaycastHit hit;
            if (Physics.Raycast(foot.targetTransform.position, Vector3.down, out hit, raycastRange, groundLayer))
            {
                foot.targetTransform.position = hit.point;
            }
        }
    }

    
    private void DriveLegs()
    {
        // Introduce a variable to count how many feet are on the ground
        int feetOnGroundCount = 0;
        foreach (var foot in feet)
        {
            if (!foot.isStepping) feetOnGroundCount++;
        }

        foreach (var foot in feet)
        {
            // Only allow a foot to lift if there will still be at least 2 feet on the ground
            if (foot.isStepping || feetOnGroundCount <= 2) continue;
            
            Vector3 targetPosition = foot.TargetPosition;
            Vector3 footPosition = foot.Position;
            Vector3 legPosition = foot.LegPosition;

            float footToTargetDistance = Vector3.Distance(footPosition, targetPosition);
            float legStretchDistance = Vector3.Distance(footPosition, legPosition);

            if (footToTargetDistance > stepDistance || legStretchDistance > maxLegStretchDistance || legStretchDistance < minLegStretchDistance)
            {
                StartCoroutine(PerformStep(foot, targetPosition));
                feetOnGroundCount--;
            }
            else
            {
                foot.footTransform.position = foot.PreviousPosition;
                foot.stepProgress = 0f;
            }
        }
    }
    
    private void PositionBody()
    {
        // Compute average position and rotation
        Vector3 avgPos = Vector3.zero;
        foreach (var foot in feet)
        {
            avgPos += foot.Position;
        }
        avgPos /= feet.Length;

        // Apply to body
        transform.position = avgPos;
    }
    
    private IEnumerator PerformStep(Foot foot, Vector3 targetPosition)
    {
        foot.isStepping = true;
        foot.stepProgress = 0f;
        
        Vector3 startPos = foot.Position; // Set the starting postition to the legs current postition

        while (foot.stepProgress < 1f)
        {
            foot.stepProgress += Time.deltaTime / stepDuration;
            float height = stepCurveVertical.Evaluate(foot.stepProgress) * stepHeight;
            float horizontal = stepCurveHorizontal.Evaluate(foot.stepProgress);
            Vector3 newPos = Vector3.Lerp(startPos, targetPosition, horizontal);
            foot.footTransform.position = foot.dynamics.UpdatePosition(Time.fixedDeltaTime, new Vector3(newPos.x, height, newPos.z), Vector3.zero);
            yield return new WaitForFixedUpdate(); // Waits until next physics step to continue
        }
        
        foot.footTransform.position = targetPosition;
        foot.PreviousPosition = targetPosition;
        foot.stepProgress = 0f;
        foot.isStepping = false;
    }
    
    
    
    // Fix Leg to the ground
    
    
    // Raycast from target point to adjust it to the ground
    
    
    // Check Distance from the target point
    
    
    // Move leg to the target when distance is greater than stepDistance
    
    
    // Only move leg when opposite leg is on the ground
    
    
    // Position the body based of the foot positions + offset rotated to the average angle of the feet
    
    private void OnDrawGizmos()
    {
        if (!GizmosEnabled) return;
        
        CheckTransformArray();

        foreach (var foot in feet)
        {
            // Introduced Variables to reduce the amount of calls to the transform
            Vector3 targetPosition = foot.TargetPosition;
            Vector3 footPosition = foot.Position;
            Vector3 legPosition = foot.LegPosition;
            
            // Draw the target position and it's size
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(targetPosition, stepDistance);

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(footPosition, footPosition + Vector3.up * stepHeight);
            
            #region LineBetweenFootAndTarget

            // Calculate the distance between foot and target
            float distance = Vector3.Distance(footPosition, targetPosition);

            // Colour the distance between foot and target green if it's less than stepDistance
            if (distance > stepDistance)
            {
                Gizmos.color = Color.red;
            }
            else
            {
                Gizmos.color = Color.green;
            }
            
            // Draw the distance between foot and target
            Gizmos.DrawLine(footPosition, targetPosition);
            
            #endregion
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(legPosition, (footPosition - legPosition).normalized * maxLegStretchDistance);
            
            #region LineBetweenFootAndLeg
            
            float legStretchDistance = Vector3.Distance(footPosition, legPosition);
            
            if (legStretchDistance > maxLegStretchDistance || legStretchDistance < minLegStretchDistance)
            {
                Gizmos.color = Color.red;
            } else
            {
                Gizmos.color = Color.green;
            }

            Gizmos.DrawLine(footPosition, legPosition);
            
            #endregion
            
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(legPosition, (footPosition - legPosition).normalized * minLegStretchDistance);
        }
    }
    
    /// <summary>
    /// If the footTransforms and footTargets arrays are not the same length, this will throw an error.
    /// </summary>
    private void CheckTransformArray()
    {
        foreach (var foot in feet)
        {
            if (foot.footTransform == null || foot.targetTransform == null)
            {
                throw new Exception("Foot Transform or Target Transform for element: " + foot +" is null");
            }
        }
    }
}
