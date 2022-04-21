using UnityEngine;
using UnityEngine.Networking;
using Unity.Netcode;
using UnityEngine.InputSystem;

public class PlayerControl : NetworkBehaviour
{

    public enum PlayerAnimationState
    {
        Idle,
        Walk,
        ReverseWalk
    }

    [SerializeField]
    private float walkSpeed = 0.5f;

    [SerializeField]
    private float rotationSpeed = 3.5f;

    [SerializeField]
    private Vector2 startingPositionRange = new(-4, 4);

    [SerializeField]
    private NetworkVariable<Vector3> networkPosition = new();

    [SerializeField]
    private NetworkVariable<Vector3> networkRotation = new();

    [SerializeField]
    private NetworkVariable<PlayerAnimationState> networkPlayerAnimationState = new();

    private CharacterController characterController;
    private Animator animator;

    // client caching
    private Vector3 oldPositionVector;
    private Vector3 oldRotationVector;
    private Vector2 movementInput;

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
        }

    }

    private void OnMove(InputValue moveValue)
    {
        if (IsClient && IsOwner)
        {
            movementInput = moveValue.Get<Vector2>();
        }
    }

    private void Update()
    {
        
        if (IsClient && IsOwner)
        {
            ClientInput();
        }
        ClientMoveAndRotate();
        ClientVisuals();
    }

    private void ClientInput()
    {
        Vector3 rotationVector = new(0, Input.GetAxis("Horizontal"), 0);
        // Where is the player looking at?
        Vector3 direction = transform.TransformDirection(Vector3.forward);
        float forwardValue = Input.GetAxis("Vertical");
        Vector3 positionVector = direction * forwardValue;

        //Only send changes
        if (positionVector != oldPositionVector || rotationVector != oldRotationVector) {
            oldPositionVector = positionVector;
            oldRotationVector = rotationVector;
            UpdateClientPositionServerRpc(positionVector * walkSpeed, rotationVector * rotationSpeed);
        }

        if (forwardValue > 0) {
            UpdatePlayerAnimationStateServerRpc(PlayerAnimationState.Walk);
        }
        else if (forwardValue == 0) {
            UpdatePlayerAnimationStateServerRpc(PlayerAnimationState.Idle);
        }
        else {
            UpdatePlayerAnimationStateServerRpc(PlayerAnimationState.ReverseWalk);
        }        
    }

    private void ClientMoveAndRotate()
    {
        if (networkPosition.Value != Vector3.zero) {
            characterController.SimpleMove(networkPosition.Value);
        }
        if (networkRotation.Value != Vector3.zero) {
            transform.Rotate(networkRotation.Value);
        }
    }

    private void ClientVisuals()
    {
        switch(networkPlayerAnimationState.Value) {
            case PlayerAnimationState.Idle:
                animator.SetFloat("Walk", 0f);
            break;
            case PlayerAnimationState.Walk:
                animator.SetFloat("Walk", 1f);
            break;
            case PlayerAnimationState.ReverseWalk:
                animator.SetFloat("Walk", -1f);
            break;
        }
    }

    [ServerRpc]
    public void UpdateClientPositionServerRpc(Vector3 newPosition, Vector3 newRotation)
    {
        networkPosition.Value = newPosition;
        networkRotation.Value = newRotation;
    }

    [ServerRpc]
    public void UpdatePlayerAnimationStateServerRpc(PlayerAnimationState newPlayerAnimationState) {
        networkPlayerAnimationState.Value = newPlayerAnimationState;
    }
}