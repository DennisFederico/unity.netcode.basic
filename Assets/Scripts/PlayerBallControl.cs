using UnityEngine;
using UnityEngine.Networking;
using Unity.Netcode;
using UnityEngine.InputSystem;
using Unity.Netcode.Components;
using Dennis.Unity.Utils.Loggers;
using Unity.Netcode.Samples;

[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(ClientNetworkTransform))]
public class PlayerBallControl : NetworkBehaviour
{
    [SerializeField]
    private float moveForce = 0.5f;

    [SerializeField]
    private float jumpForce = 1.5f;

    [SerializeField]
    private Vector2 startingPositionRange = new(-4, 4);

    private Rigidbody ballRigidBody;

    private void Awake()
    {
        ballRigidBody = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        if (IsClient && IsOwner)
        {
            transform.position = new Vector3(Random.Range(startingPositionRange.x, startingPositionRange.y),
                                             0,
                                             Random.Range(startingPositionRange.x, startingPositionRange.y));
        }

    }

    private void Update()
    {
        if (IsClient && IsOwner)
        {
            ClientInput();
        }
    }

    private void ClientInput()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        if (verticalInput != 0)
        {
            ballRigidBody.AddForce((verticalInput > 0 ? Vector3.forward : Vector3.back) * moveForce);
        }
        if (horizontalInput != 0)
        {
            ballRigidBody.AddForce((horizontalInput > 0 ? Vector3.right : Vector3.left) * moveForce);
        }
        if (Input.GetKey(KeyCode.Space))
        {
            ballRigidBody.AddForce(Vector3.up * jumpForce);
        }
    }
}