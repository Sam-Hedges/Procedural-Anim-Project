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
    }

    [SerializeField] private Foot[] feet;


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
        foreach (var foot in feet)
        {
            // Introduced Variables to reduce the amount of calls to the transform
            Vector3 targetPosition = foot.TargetPosition;
            Vector3 footPosition = foot.Position;
            
            // Calculate the distance between foot and target
            float distance = Vector3.Distance(footPosition, targetPosition);

            if (distance > stepDistance)
            {
                foot.PreviousPosition = targetPosition;
                foot.footTransform.position = Vector3.Lerp(footPosition, targetPosition, stepCurve.Evaluate(foot.stepProgress));
                foot.stepProgress += Time.deltaTime / stepDuration;
            } else
            {
                foot.footTransform.position = foot.PreviousPosition;
                foot.stepProgress = 0f;
            }
        }
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
