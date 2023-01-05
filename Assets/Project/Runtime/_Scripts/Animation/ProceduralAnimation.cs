using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralAnimation : MonoBehaviour
{
    
    [SerializeField] private float stepDistance = 0.1f;
    [SerializeField] private float stepHeight = 0.1f;
    [SerializeField] private AnimationCurve stepCurve;
    [SerializeField] private float stepDuration = 0.5f;

    [Serializable] 
    private class Foot
    {
        public Transform footTransform;
        public Transform targetTransform;
        internal Vector3 Position => footTransform.position;
        internal Vector3 PreviousPosition;
        internal Vector3 TargetPosition => targetTransform.position;
        internal float stepProgress = 0f;
        internal bool isStepping = false;
        
        internal void UpdatePreviousPosition()
        {
            PreviousPosition = Position;
        }
    }

    [SerializeField] private Transform rootTransform;
    private Vector3 worldUp;
    
    // NonReorderable is a custom attribute to prevent reordering in the inspector
    // It's used in this case to prevent a nested-class array, overlapping-inspector bug
    [SerializeField] [NonReorderable] private Foot[] feet;

    
    

    private float raycastRange = 1f;
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
        }
    }

    private void FixedUpdate()
    {
        DriveLegs();
    }
    
    private void DriveLegs()
    {
        foreach (var foot in feet)
        {
            if (foot.isStepping) continue;
            
            // Introduced Variables to reduce the amount of calls to the transform
            Vector3 targetPosition = foot.TargetPosition;
            Vector3 footPosition = foot.Position;
            
            // Calculate the distance between foot and target
            float distance = Vector3.Distance(footPosition, targetPosition);

            if (distance > stepDistance)
            {
                // foot.PreviousPosition = targetPosition;
                // foot.footTransform.position = Vector3.Lerp(footPosition, targetPosition, stepCurve.Evaluate(foot.stepProgress));
                // foot.stepProgress += Time.deltaTime / stepDuration;

                StartCoroutine(PerformStep(foot, targetPosition));
            } else
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
            float height = stepCurve.Evaluate(foot.stepProgress) * stepHeight;
            Vector3 newPos = Vector3.Lerp(startPos, targetPosition, foot.stepProgress);
            foot.footTransform.position = new Vector3(newPos.x, height, newPos.z);
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
        CheckTransformArray();

        foreach (var foot in feet)
        {
            // Introduced Variables to reduce the amount of calls to the transform
            Vector3 targetPosition = foot.TargetPosition;
            Vector3 footPosition = foot.Position;
            
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
