using UnityEngine;

public class SecondOrderDynamics
{
    private Vector3 xp; // previous input
    private Vector3 y, yd; // state variables
    private float k1, k2, k3; // dynamics constants
    private float T_crit; // critical stable time step

    public SecondOrderDynamics(float frequency, float dampingCoefficient, float initialResponse, Vector3 initialPosition)
    {
        // compute constants
        k1 = dampingCoefficient / (Mathf.PI * frequency);
        k2 = 1 / ((2 * Mathf.PI * frequency) * (2 * Mathf.PI * frequency));
        k3 = initialResponse * dampingCoefficient / (2 * Mathf.PI * frequency);
        T_crit = 0.8f * (Mathf.Sqrt(4 * k2 + k1 * k1) - k1); // multiply by 0.8 to be safe
        // initialize variables
        xp = initialPosition;
        y = initialPosition;
        yd = Vector3.zero;
    }

    public Vector3 UpdatePosition(float deltaTime, Vector3 targetPosition, Vector3 velocity)
    {
        if (velocity == Vector3.zero) { // estimate velocity
            velocity = (targetPosition - xp) / deltaTime;
            xp = targetPosition;
        }
        int iterations = (int)Mathf.Ceil(deltaTime / T_crit); // take extra iterations if T > T_crit
        deltaTime /= iterations; // each iteration now has a smaller time step
        for (int i = 0; i < iterations; i++) {
            y += deltaTime * yd; // integrate position by velocity
            yd += deltaTime * (targetPosition + k3*velocity - y - k1 * yd) / k2; // integrate velocity by acceleration
        }
        return y;
    }
}
