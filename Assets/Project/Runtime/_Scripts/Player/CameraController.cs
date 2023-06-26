/*
using Player;
using UnityEngine;
using UnityEngine.Serialization;

public class CameraController : MonoBehaviour
{
    [SerializeField] float smoothTime = 0.5f;
    [SerializeField] private GameObject targetPlayer;
    private Vector3 _lerpVelocity = Vector3.zero;
    private float _currentOffset = 0.0f;
    private Camera _camera;
    private Rigidbody _targetRigidbody;
    private PlayerController _targetPlayerController;

// Start is called before the first frame update
    void Awake()
    {
        InitializeProperties();
        
        transform.SetPositionAndRotation(new Vector3(targetPlayer.transform.position.x,
            targetPlayer.transform.position.y + 2, targetPlayer.transform.position.z), transform.rotation);
    }
    
    private void InitializeProperties()
    {
        _camera = Camera.main;
        _targetRigidbody = targetPlayer.GetComponent<Rigidbody>();
        _targetPlayerController = targetPlayer.GetComponent<PlayerController>();
        
    }

// Update is called once per frame
    void Update()
    {
        Vector3 cameraRelativeVelocity = _camera.transform.InverseTransformVector(_targetRigidbody.velocity);
        
        Debug.Log(cameraRelativeVelocity.normalized.x);
        _currentOffset += cameraRelativeVelocity.normalized.x;
        
        Vector3 cameraRelativePosition = _camera.transform.InverseTransformVector(targetPlayer.transform.position);
        Vector3 expectedPositionRelativeToCamera = new Vector3(cameraRelativePosition.x + cameraRelativeVelocity.x, cameraRelativePosition.y, cameraRelativePosition.z);
        Vector3 expectedPositionRelativeToWorld = _camera.transform.TransformVector(expectedPositionRelativeToCamera);
        Vector3 expectedPositionWithYOffset = new Vector3(expectedPositionRelativeToWorld.x, expectedPositionRelativeToWorld.y + 2, expectedPositionRelativeToWorld.z);
        
        if (_targetPlayerController.IsGrounded && cameraRelativeVelocity.magnitude != 0)
        {
            transform.SetPositionAndRotation(Vector3.SmoothDamp(transform.position, expectedPositionWithYOffset,  ref _lerpVelocity, smoothTime), _camera.transform.rotation);
        }
    }
}
*/
using Player;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private GameObject targetPlayer;
    [SerializeField, Range(0, 1)] float smoothTime = 0.5f;
    [SerializeField, Range(0, 1)] private float anticipationFactor = 1.0f; // how far ahead to look, based on the player's velocity
    [SerializeField, Range(0, 2)] private float targetHeight = 1.0f; // height above the player to look at

    private Vector3 _lerpVelocity = Vector3.zero;
    private Camera _camera;
    private Rigidbody _targetRigidbody;
    private PlayerController _targetPlayerController;

    void Awake()
    {
        InitializeProperties();

        transform.SetPositionAndRotation(new Vector3(targetPlayer.transform.position.x,
            targetPlayer.transform.position.y + targetHeight, targetPlayer.transform.position.z), transform.rotation);
    }
    
    private void InitializeProperties()
    {
        _camera = Camera.main;
        _targetRigidbody = targetPlayer.GetComponent<Rigidbody>();
        _targetPlayerController = targetPlayer.GetComponent<PlayerController>();
    }

    void FixedUpdate()
    {
        Vector3 anticipatedPosition = targetPlayer.transform.position + (_targetRigidbody.velocity * anticipationFactor);
        Vector3 cameraRelativeAnticipatedPosition = _camera.transform.InverseTransformPoint(anticipatedPosition);
        
        Vector3 expectedPositionRelativeToCamera = new Vector3(cameraRelativeAnticipatedPosition.x, cameraRelativeAnticipatedPosition.y, cameraRelativeAnticipatedPosition.z);
        Vector3 expectedPositionRelativeToWorld = _camera.transform.TransformPoint(expectedPositionRelativeToCamera);
        Vector3 expectedPositionWithYOffset = new Vector3(expectedPositionRelativeToWorld.x, expectedPositionRelativeToWorld.y + targetHeight, expectedPositionRelativeToWorld.z);

        if (_targetPlayerController.IsGrounded && _targetRigidbody.velocity.magnitude != 0)
        {
            float t = Time.deltaTime / smoothTime;
            t = t * t * (3f - 2f * t);
            
            Vector3 newPosition = Vector3.Lerp(transform.position, expectedPositionWithYOffset, t);
            transform.SetPositionAndRotation(newPosition, _camera.transform.rotation);
        }
    }
}