using System;
using UnityEngine;

public class SecondOrderDynamics
{
    // Previous input
    private Vector3 _previousPosition;

    // State variables
    private Vector3 _position; 
    private Vector3 _velocity;
    
    // dynamics constants
    private readonly float _w;
    private readonly float _z;
    private readonly float _d;
    private readonly float _k1;
    private readonly float _k2;
    private readonly float _k3;

    public SecondOrderDynamics(float frequency, float dampingCoefficient, float initialResponse, Vector3 initialPosition)
    {
        // compute constants
        _w = 2 * Mathf.PI * frequency;
        _z = dampingCoefficient;
        _d = _w * Mathf.Sqrt(Mathf.Abs(dampingCoefficient * dampingCoefficient - 1)); 
        _k1 =  dampingCoefficient / (Mathf.PI * frequency);
        _k2 = 1 / (_w * _w);
        _k3 = initialResponse * dampingCoefficient / _w;
        // initialize-variables
        _previousPosition = initialPosition;
        _position  =  initialPosition; 
        _velocity = Vector3.zero;
    }

    public Vector3 UpdatePosition(float deltaTime, Vector3 targetPosition, Vector3 velocity)
    {
        if (velocity == Vector3.zero){// estimate velocity
            velocity = (targetPosition - _previousPosition) / deltaTime;
            _previousPosition = targetPosition;
        }
        float k1Stable;
        float k2Stable;
        if (_w * deltaTime < _z){// clamp k2 to guarantee stability without jitter
            k1Stable = _k1;
            k2Stable = Mathf.Max(_k2, deltaTime * deltaTime / 2 + deltaTime * _k1 / 2, deltaTime * _k1);
        }else {// use pole matching when the system is very fast
            float t1 = Mathf.Exp(-_z * _w * deltaTime);
            float alpha = 2 * t1 * (_z <= 1 ? Mathf.Cos(deltaTime * _d) : (float)Math.Cosh(deltaTime * _d));
            float beta = t1 * t1;
            float t2 = deltaTime / (1 + beta - alpha);
            k1Stable = (1 - beta) * t2;
            k2Stable = deltaTime * t2;
        }
        _position += deltaTime * _velocity; // integrate position by velocity
        _velocity += deltaTime * (targetPosition + _k3 * velocity - _position - k1Stable * _velocity) / k2Stable; // integrate velocity by acceleration
        return _position;
    }
}
