using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Camera")]
        [SerializeField] private Camera mainCamera;
        [SerializeField] private GameObject playerCameraHelper;
        private static CinemachineFreeLook _playerCamera;
        private static Vector3 CameraTransformForward => ScaleCameraTransform(_playerCamera.transform.forward);
        private static Vector3 CameraTransformRight => ScaleCameraTransform(_playerCamera.transform.right);
        private static Vector3 ScaleCameraTransform(Vector3 cameraTransform) {
            cameraTransform.y = 0.0f;
            cameraTransform.Normalize();
            return cameraTransform;
        }
        private float xRot;
        private float yRot;
        
        // Input
        private PlayerInput _input;
        private struct IMovement {
            
            public Vector2 MovementInputVector;
            public Vector3 MovementOutputVector => ScaleCameraTransform(_playerCamera.transform.forward) * MovementInputVector.y + CameraTransformRight * MovementInputVector.x;
            public bool IsPressed => MovementInputVector != Vector2.zero;
        }
   
        private IMovement _iMovement;


        private Rigidbody rb;
        [SerializeField] private float rideHeight = 1.2f;
        [SerializeField] private float rideSpringStrength = 2000f;
        [SerializeField] private float rideSpringDamper = 100f;
        [SerializeField] private float maxSpeed = 8f;
        [SerializeField] private float rotationSpeed = 1f;
        [SerializeField] private float acceleration = 200f;
        [SerializeField] private AnimationCurve accelerationFactor;
        [SerializeField] private float maxAcceleration = 150;
        [SerializeField] private AnimationCurve maxAccelerationFactor;
        [SerializeField] private Vector3 forceScale;
        private Vector3 _unitGoal;
        private Vector3 _goalVel;
        [HideInInspector] public bool IsGrounded => Physics.CheckSphere(groundCheckOrigin.position, groundCheckRad, groundMask);
        private Vector3 _velocity;
        [SerializeField] private LayerMask groundMask;
        [SerializeField] private float groundCheckRad = 0.5f;
        [SerializeField] private Transform groundCheckOrigin;
        [SerializeField] private float playerJumpHeight = 1.0f;
        [SerializeField] private float gravityStrength = 9.81f;

        private Vector3 gravityVector;

        private void InitializeInput() {
            
            _input = new PlayerInput();

            _input.Player.Movement.started += OnMovementInput;
            _input.Player.Movement.canceled += OnMovementInput;
            _input.Player.Movement.performed += OnMovementInput;
            
        }
        private void OnMovementInput(InputAction.CallbackContext context) { _iMovement.MovementInputVector = context.ReadValue<Vector2>(); }

        private void InitializeCamera() {
            _playerCamera = playerCameraHelper.GetComponent<CinemachineFreeLook>();
        }
        
        
        #region Unity Event Methods
        
        private void Awake() {
            
            if(mainCamera == null) { mainCamera = Camera.main; }

            rb = GetComponent<Rigidbody>();

            gravityVector = Physics.gravity.normalized;
            Debug.Log(gravityVector);

            InitializeInput();

            InitializeCamera();

        }
        private void Start() {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        private void OnEnable() {
            _input.Player.Enable();
        }
        private void OnDisable() {
            _input.Player.Disable();
        }
        private void FixedUpdate()
        {
            ApplyGravity();
            
            RigidbodyRide();
            
            RotateToUpright();
            
            //RotateCamera();
            
            Movement();
        }
        

        private void Update() {
            
            //Jump();

            Exit();
        }
        
        #endregion
        
        #region Movement Methods
        
        private void ApplyGravity() {
            rb.AddForce(gravityVector * gravityStrength * rb.mass);
        }
        
        private void RigidbodyRide() {
            if (Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, rideHeight + 1)) {
                Vector3 velocity = rb.velocity;
                Vector3 rayDirection = -transform.up;

                Vector3 otherVelocity = Vector3.zero;
                Rigidbody hitRigidbody = hit.rigidbody;
                if (hitRigidbody != null) {
                    otherVelocity = hitRigidbody.velocity;
                }

                float rayDirectionVelocity = Vector3.Dot(rayDirection, velocity);
                float otherDirectionVelocity = Vector3.Dot(rayDirection, otherVelocity);

                float relativeVelocity = rayDirectionVelocity - otherDirectionVelocity;

                float x = hit.distance - rideHeight;

                float springForce = (x * rideSpringStrength) - (relativeVelocity * rideSpringDamper);

                Debug.DrawLine(transform.position, transform.position + (-transform.up * (rideHeight + 1)), Color.red);

                rb.AddForce(rayDirection * springForce);
            }
        }
        
        private void RotateToUpright() {
            Quaternion currentRotation = transform.rotation;
            Quaternion rotationTarget =
                ShortestRotation(Quaternion.LookRotation(transform.forward, -gravityVector), currentRotation);

            Vector3 rotationAxis;
            float rotationDegrees;
            
            rotationTarget.ToAngleAxis(out rotationDegrees, out rotationAxis);
            rotationAxis.Normalize();

            float rotationRadians = rotationDegrees * Mathf.Deg2Rad;
            
            rb.AddTorque((rotationAxis * (rotationRadians * 500)) - (rb.angularVelocity * 100));
        }

        // Applies movement to the player character based on the players input
        private void Movement() {
            // Get the desired movement direction from the input
            Vector3 moveDirection = Vector3.ClampMagnitude(_iMovement.MovementOutputVector, 1f);

            // Get the camera's transform   
            Transform camTransform = mainCamera.transform;   

            // Ignore the camera's pitch (up-down rotation) when calculating the moveDirection
            Vector3 cameraPlanarDirection = Vector3.Scale(camTransform.forward, new Vector3(1, 0, 1)).normalized;
            moveDirection = cameraPlanarDirection * moveDirection.z + camTransform.right * moveDirection.x;

            // Prevent the character from moving vertically
            moveDirection.y = 0;

            // Update the unit's goal to be the new moveDirection
            _unitGoal = moveDirection;
        
            // Normalized goal velocity
            Vector3 unitVel = _goalVel.normalized;
        
            // Dot product of unit goal and unit velocity (cosine of the angle between the two vectors)
            float velDot = Vector3.Dot(_unitGoal, unitVel);
        
            // Calculate acceleration based on the acceleration factor curve
            float accel = acceleration * accelerationFactor.Evaluate(velDot);
        
            // Calculate the goal velocity based on the unit goal and maximum speed
            Vector3 goalVel = _unitGoal * maxSpeed;
        
            // Gradually change the current goal velocity to the newly calculated goal velocity, at the rate of acceleration
            _goalVel = Vector3.MoveTowards(_goalVel, goalVel, accel * Time.fixedDeltaTime);
        
            // Calculate the acceleration required to reach the goal velocity from the current velocity
            Vector3 targetAccel = (_goalVel - rb.velocity) / Time.fixedDeltaTime;
        
            // Get the maximum acceleration based on the maxAcceleration factor curve
            float maxAccel = maxAcceleration * maxAccelerationFactor.Evaluate(velDot);
        
            // Ensure the target acceleration doesn't exceed the maximum acceleration
            targetAccel = Vector3.ClampMagnitude(targetAccel, maxAccel);
            
            // Calculate output force as before
            Vector3 outputForce = Vector3.Scale(targetAccel * rb.mass, forceScale);

            // Calculate the rotation from the world's up vector (Vector3.up) to the current gravity vector
            Quaternion toGravityRotation = Quaternion.FromToRotation(Vector3.up, -gravityVector);

            // Rotate outputForce
            Vector3 rotatedOutputForce = toGravityRotation * outputForce;

            // Apply the rotated force
            rb.AddForce(rotatedOutputForce);
            
            // Apply the force to the Rigidbody, scaled by its mass and the forceScale factor
            //Vector3 outputForce = Vector3.Scale(targetAccel * rb.mass, forceScale);
            //rb.AddForce(outputForce);
        
            // Rotate the character to face the direction of movement, only if the movement direction is not zero
            if (moveDirection != Vector3.zero) 
            {
                // Calculate the rotation that looks towards the move direction
                Quaternion toRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
        
                // Gradually rotate the character from its current rotation to the new rotation
                transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
                
                _playerCamera.m_RecenterToTargetHeading.m_enabled = true;
            }
            else
            {
                _playerCamera.m_RecenterToTargetHeading.m_enabled = false;
            }
            
            return;
            /*
            // Calculate final movement Vector
            Vector3 moveDirection = Vector3.ClampMagnitude(_iMovement.MovementOutputVector, 1f);

            _unitGoal = moveDirection;

            Vector3 unitVel = _goalVel.normalized;
            float velDot = Vector3.Dot(_unitGoal, unitVel);
            float accel = acceleration * accelerationFactor.Evaluate(velDot);
            Vector3 goalVel = _unitGoal * maxSpeed;
            _goalVel = Vector3.MoveTowards(_goalVel, goalVel, accel * Time.fixedDeltaTime);

            Vector3 targetAccel = (_goalVel - rb.velocity) / Time.fixedDeltaTime;
            float maxAccel = maxAcceleration * maxAccelerationFactor.Evaluate(velDot);
            targetAccel = Vector3.ClampMagnitude(targetAccel, maxAccel);
            rb.AddForce(Vector3.Scale(targetAccel * rb.mass, forceScale));
            */
        }

        private void Exit() {
            if (_input.Player.Exit.GetButtonDown()) {
                Application.Quit();
            }
        }
        
        private void Jump() {
            // Changes the height position of the player..
            if (_input.Player.Jump.GetButtonDown() && IsGrounded)
            {
                _velocity.y += Mathf.Sqrt(playerJumpHeight * -3.0f * gravityStrength);
            }
        }
        
        #endregion
        
        public void SetGravityVector(Vector3 gravityVector) {
            this.gravityVector = gravityVector;
        }
        
        private static Quaternion ShortestRotation(Quaternion to, Quaternion from)
        {
            if (Quaternion.Dot(to, from) < 0)
            {
                return to * Quaternion.Inverse(Multiply(from, -1));
            }

            else return to * Quaternion.Inverse(from);
        }
        
        private static Quaternion Multiply(Quaternion input, float scalar)
        {
            return new Quaternion(input.x * scalar, input.y * scalar, input.z * scalar, input.w * scalar);
        }
    }
}
