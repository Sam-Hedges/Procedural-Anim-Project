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
    [SerializeField] private LayerMask groundLayer;

    [Serializable] 
    private class Foot
    {
        public Transform rootLegTransform;
        public Transform footTransform;
        public Transform targetTransform;
        
        public float stepWindow = 1f; // New field
        public float stepDelayOffset = 0f; // New field
        internal float lastStepTime = 0f; // New field
        
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
    
    private Vector3 worldUp;
    
    // NonReorderable is a custom attribute to prevent reordering in the inspector
    // It's used in this case to prevent a nested-class array, overlapping-inspector bug
    [SerializeField] [NonReorderable] private Foot[] feet;

    [Range(0, 15)] public float frequency = 1f;
    [Range(0, 5)] public float dampingCoefficient = 0.5f;
    [Range(-5, 5)] public float initialResponse = 2f;

    private float raycastRange = 15f;
    private Vector3 lastBodyUp;
    private bool[] legMoving;
    private int nbLegs;
    
    private Rigidbody parentRigidbody;
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
            
        parentRigidbody = GetComponentInParent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        RaycastIKTargets();
        DriveLegs();
    }

    private void RaycastIKTargets()
    {
        foreach (var foot in feet)
        {
            // Check for ground and adjust foot target if needed
            Ray groundRay = new Ray(foot.TargetPosition + Vector3.up * 7f, Vector3.down);
            if (Physics.Raycast(groundRay, out RaycastHit hit, raycastRange, groundLayer))
            {
                foot.footTransform.position = hit.point;
                foot.footTransform.rotation = Quaternion.LookRotation(foot.footTransform.forward, hit.normal);
            }
        }
    }
    
    private void DriveLegs()
    {
        foreach (var foot in feet)
        {
            // Introduced Variables to reduce the amount of calls to the transform
            Vector3 targetPosition = foot.TargetPosition;
            Vector3 footPosition = foot.Position;
            Vector3 legPosition = foot.LegPosition;

            // Calculate the distance between foot and target
            float footToTargetDistance = Vector3.Distance(footPosition, targetPosition);
            float legStretchDistance = Vector3.Distance(footPosition, legPosition);

            if (foot.isStepping) continue;

            float currentStepWindow = foot.stepWindow * (1f / (1f + parentRigidbody.velocity.magnitude)); // Adjust step window based on velocity

            if (footToTargetDistance > stepDistance || legStretchDistance > maxLegStretchDistance || legStretchDistance < minLegStretchDistance)
            {
                // Only move if the step window has passed since the last step for this foot
                if (Time.time - foot.lastStepTime > currentStepWindow + foot.stepDelayOffset)
                {
                    foot.lastStepTime = Time.time;
                    StartCoroutine(PerformStep(foot, targetPosition));
                }
            } 
            else 
            {
                foot.footTransform.position = foot.PreviousPosition;
                foot.stepProgress = 0f;
            }
        }
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
            DrawGizmoRing(targetPosition, stepDistance, foot.footTransform.up);

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
    
    private void DrawGizmoRing(Vector3 center, float radius, Vector3 normal, int resolution = 30)
    {
        Vector3 lastPoint = center + radius * CalculateTangent(normal);
        Vector3 nextPoint = Vector3.zero;

        for (var i = 1; i <= resolution; i++)
        {
            float angle = 2.0f * Mathf.PI * i / resolution;
            Vector3 direction = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
            Vector3 tangent = CalculateTangent(normal);
            Quaternion rotation = Quaternion.AngleAxis(angle * Mathf.Rad2Deg, normal);
            nextPoint = center + radius * (rotation * tangent);

            Gizmos.DrawLine(lastPoint, nextPoint);
            lastPoint = nextPoint;
        }
    }
    
    public static Vector3 CalculateTangent(Vector3 normal)
    {
        Vector3 tangent;
        // Check if your normal is not Vector3.up or its opposite.
        if (Mathf.Abs(Vector3.Dot(normal, Vector3.up)) < 0.99f)
        {
            tangent = Vector3.Cross(normal, Vector3.up).normalized;
        }
        else
        {
            tangent = Vector3.Cross(normal, Vector3.right).normalized;
        }
        return tangent;
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
