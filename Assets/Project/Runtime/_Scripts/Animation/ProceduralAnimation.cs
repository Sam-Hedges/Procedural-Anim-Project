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
    
    [SerializeField] private Transform[] footTransforms;
    [SerializeField] private Transform[] footTargets;


    private void Awake()
    {
        CheckTransformArray();
    }
    
    
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    private void OnDrawGizmos()
    {
        CheckTransformArray();

        foreach (var transform in footTargets)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(transform.position, stepDistance);
        }
        
        for (int i = 0; i < footTransforms.Length; i++)
        {
            //calculate the distance between two vectors
            float distance = Vector3.Distance(footTransforms[i].position, footTargets[i].position);
            
            if (distance > stepDistance)
            {
                Gizmos.color = Color.red;
            }
            else
            {
                Gizmos.color = Color.green;
            }

            Gizmos.DrawLine(footTransforms[i].position, footTargets[i].position);
        }
    }
    
    /// <summary>
    /// If the footTransforms and footTargets arrays are not the same length, this will throw an error.
    /// </summary>
    private void CheckTransformArray()
    {
        if (footTransforms.Length != footTargets.Length)
        {
            throw new Exception("Foot transforms and targets must be the same length");
        }
    }
    
}
