using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class SecondOrderDynamicsDemo : MonoBehaviour
{
    public Transform target;
    public float frequency = 1;
    public float dampingCoefficient;
    public float initialResponse;
    
    private Vector3 xp; // previous input
    private Vector3 y, yd; // state variables
    private float k1, k2, k3; // dynamics constants
    private float T_crit; // critical stable time step

    public void InitSecondOrderDynamics(float f, float z, float r, Vector3 x0)
    {
        // compute constants
        k1 = z / (Mathf.PI * f);
        k2 = 1 / ((2 * Mathf.PI * f) * (2 * Mathf.PI * f));
        k3 = r * z / (2 * Mathf.PI * f);
        T_crit = 0.8f * (Mathf.Sqrt(4 * k2 + k1 * k1) - k1); // multiply by 0.8 to be safe
        // initialize variables
        xp = x0;
        y = x0;
        yd = Vector3.zero;
    }

    private void Awake() {
        InitSecondOrderDynamics(frequency, dampingCoefficient, initialResponse, transform.position);
    }

    private void FixedUpdate() {
        target.position = UpdatePosition(Time.fixedDeltaTime, transform.position, Vector3.zero);
    }

    public Vector3 UpdatePosition(float deltaTime, Vector3 x, Vector3 xd)
    {
        if (xd == Vector3.zero) { // estimate velocity
            xd = (x - xp) / deltaTime;
            xp = x;
        }
        int iterations = (int)Mathf.Ceil(deltaTime / T_crit); // take extra iterations if T > T_crit
        deltaTime /= iterations; // each iteration now has a smaller time step
        for (int i = 0; i < iterations; i++) {
            y += deltaTime * yd; // integrate position by velocity
            yd += deltaTime * (x + k3*xd - y - k1 * yd) / k2; // integrate velocity by acceleration
        }
        return y;
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, target.position);
    }
}
