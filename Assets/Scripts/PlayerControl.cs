using UnityEngine;
using UnityEngine.Networking;
using Unity.Netcode;
using UnityEngine.InputSystem;
using Unity.Netcode.Components;
using Dennis.Unity.Utils.Loggers;

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

    private NetworkTransform networkTrasform;
    private NetworkObject networkObject;
    private CharacterController characterController;
    private Animator animator;

    //Lambda to check the Authority (usually only the server can commit to the transform / has authority)
    private bool HasAuthority => networkTrasform.CanCommitToTransform;
    private bool hasAuthority;
    private bool IsTransformOwner => networkTrasform.IsOwner;
    private bool isTransformOwner;

    // client caching
    private Vector3 oldPositionVector;
    private Vector3 oldRotationVector;
    private Vector2 movementInput;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        networkTrasform = GetComponent<NetworkTransform>();
        networkObject = GetComponent<NetworkObject>();

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

    public override void OnNetworkSpawn() {
        isTransformOwner = IsTransformOwner;
        hasAuthority = HasAuthority;        
    }

    // private void OnMove(InputValue moveValue)
    // {
    //     if (IsClient && IsOwner)
    //     {
    //         movementInput = moveValue.Get<Vector2>();
    //     }
    // }

    private void Update()
    {
        // if (Input.GetKey("j")) {
        //     // These are the real ones that evaluate to true for the player/owner LocalTransform:{isTransformLocal}, OwnerOfTransform:{isTransformOwner}, OwnerOfObject:{networkObject.IsOwner}
        //     UILogger.Instance.LogWarning($"ID:{networkObject.NetworkObjectId}, ObjClientId:{networkObject.OwnerClientId}, ClientId:{NetworkManager.LocalClientId},Authority:{hasAuthority}, LocalTransform:{isTransformLocal}, OwnerOfTransform:{isTransformOwner}, OwnerOfObject:{networkObject.IsOwner}, ObjectOfPlayer:{networkObject.IsPlayerObject}");
        // }
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