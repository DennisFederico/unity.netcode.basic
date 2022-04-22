using UnityEngine;
using UnityEngine.Networking;
using Unity.Netcode;
using UnityEngine.InputSystem;
using Unity.Netcode.Components;
using Dennis.Unity.Utils.Loggers;
using Unity.Netcode.Samples;

[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(ClientNetworkTransform))]
public class PlayerControl : NetworkBehaviour
{

    public enum PlayerAnimationState
    {
        Idle,
        Walk,
        Run,
        ReverseWalk
    }

    [SerializeField]
    private float walkSpeed = 0.5f;

    [SerializeField]
    private float rotationSpeed = 3.5f;

    [SerializeField]
    private float runSpeedOffset = 2.0f;


    [SerializeField]
    private Vector2 startingPositionRange = new(-4, 4);

    [SerializeField]
    private NetworkVariable<PlayerAnimationState> networkPlayerAnimationState = new();

    private CharacterController characterController;
    private Animator animator;

    Vector3 inputPositionVector;
    Vector3 inputRotationVector;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        if (IsClient && IsOwner)
        {
            transform.position = new Vector3(Random.Range(startingPositionRange.x, startingPositionRange.y),
                                             0,
                                             Random.Range(startingPositionRange.x, startingPositionRange.y));
            PlayerCameraFollow.Instance.AttachTo(transform.Find("PlayerCameraRoot"));
        }

    }

    private void Update()
    {
        if (IsClient && IsOwner)
        {
            ClientInput();
        }
        ClientVisuals();
    }

    private void ClientInput()
    {
        // y axis client rotation
        inputRotationVector = new Vector3(0, Input.GetAxis("Horizontal"), 0) * rotationSpeed;

        // forward & backward direction
        Vector3 direction = transform.TransformDirection(Vector3.forward);
        float forwardInput = Input.GetAxis("Vertical");
        forwardInput = (Input.GetKey(KeyCode.LeftShift) && forwardInput > 0) ? 2f : forwardInput;

        inputPositionVector = direction * forwardInput * walkSpeed;

        // Client is responsible for moving itself
        if (forwardInput > 0 && forwardInput <= 1)
        {
            UpdatePlayerAnimationStateServerRpc(PlayerAnimationState.Walk);
        }
        else if (forwardInput > 1)
        {
            UpdatePlayerAnimationStateServerRpc(PlayerAnimationState.Run);
            inputPositionVector *= runSpeedOffset;
        }
        else if (forwardInput < 0)
        {
            UpdatePlayerAnimationStateServerRpc(PlayerAnimationState.ReverseWalk);
        }
        else
        {
            UpdatePlayerAnimationStateServerRpc(PlayerAnimationState.Idle);
        }

        characterController.SimpleMove(inputPositionVector);
        transform.Rotate(inputRotationVector, Space.World);
    }

    private void ClientVisuals()
    {
        switch (networkPlayerAnimationState.Value)
        {
            case PlayerAnimationState.Idle:
                animator.SetFloat("Walk", 0f);
                break;
            case PlayerAnimationState.Walk:
                animator.SetFloat("Walk", 1f);
                break;
            case PlayerAnimationState.Run:
                animator.SetFloat("Walk", 2f);
                break;
            case PlayerAnimationState.ReverseWalk:
                animator.SetFloat("Walk", -1f);
                break;
        }
    }

    [ServerRpc]
    public void UpdatePlayerAnimationStateServerRpc(PlayerAnimationState newPlayerAnimationState)
    {
        networkPlayerAnimationState.Value = newPlayerAnimationState;
    }
}