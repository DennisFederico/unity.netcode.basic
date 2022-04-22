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
        Punch,
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

    [SerializeField]
    private NetworkVariable<float> networkPlayerHealth = new(100);

    [SerializeField]
    private NetworkVariable<float> networkPlayerPunchBlend = new();

    [SerializeField]
    private GameObject lefHand;

    [SerializeField]
    private GameObject rightHand;

    [SerializeField]
    private float minPunchDistance = 0.25f;


    private CharacterController characterController;
    private Animator animator;

    private Vector3 inputPositionVector;
    private Vector3 inputRotationVector;

    private PlayerAnimationState oldPlayerAnimationState;

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

    private void FixedUpdate()
    {
        if (IsClient && IsOwner)
        {
            if (ActivePunchingKey() && networkPlayerAnimationState.Value == PlayerAnimationState.Punch)
            {
                CheckPunch(lefHand.transform, Vector3.up);
                CheckPunch(rightHand.transform, Vector3.down);
            }
        }
    }

    private void ClientInput()
    {
        // Handle Rotation (always)
        inputRotationVector = new Vector3(0, Input.GetAxis("Horizontal"), 0) * rotationSpeed;
        transform.Rotate(inputRotationVector, Space.World);


        // Handle Punching
        if (ActivePunchingKey())
        {
            UpdatePlayerAnimationStateServerRpc(PlayerAnimationState.Punch);
            return;
        }

        //Handle Position
        Vector3 direction = transform.TransformDirection(Vector3.forward);
        float forwardInput = Input.GetAxis("Vertical");
        inputPositionVector = direction * forwardInput * walkSpeed;

        if (ActiveRunningKey() && forwardInput > 0)
        {
            UpdatePlayerAnimationStateServerRpc(PlayerAnimationState.Run);
            inputPositionVector *= runSpeedOffset;
        }
        else if (forwardInput > 0)
        {
            UpdatePlayerAnimationStateServerRpc(PlayerAnimationState.Walk);
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

    }

    private void CheckPunch(Transform hand, Vector3 punchDirection)
    {
        RaycastHit hit;
        int layerMask = LayerMask.GetMask("Player");

        if (Physics.Raycast(hand.position, hand.transform.TransformDirection(punchDirection), out hit, minPunchDistance, layerMask))
        {
            Debug.DrawRay(hand.position, hand.transform.TransformDirection(punchDirection) * minPunchDistance, Color.yellow);
            var playerHit = hit.transform.GetComponent<NetworkObject>();
            if (playerHit != null)
            {
                UpdateHealthServerRpc(1, playerHit.OwnerClientId);
            }
        }
        else
        {
            Debug.DrawRay(hand.position, hand.transform.TransformDirection(punchDirection) * minPunchDistance, Color.red);
        }
    }

    private static bool ActiveRunningKey()
    {
        return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
    }

    private static bool ActivePunchingKey()
    {
        return Input.GetKey(KeyCode.Space);
    }

    private void ClientVisuals()
    {
        if (oldPlayerAnimationState != networkPlayerAnimationState.Value)
        {
            oldPlayerAnimationState = networkPlayerAnimationState.Value;
            animator.SetTrigger($"{networkPlayerAnimationState.Value}");
            if (networkPlayerAnimationState.Value == PlayerAnimationState.Punch)
            {
                animator.SetFloat($"{networkPlayerAnimationState.Value}Blend", networkPlayerPunchBlend.Value);
            }
        }
    }

    [ServerRpc]
    public void UpdateHealthServerRpc(int damage, ulong targetClientId)
    {
        var damagedPlayer = NetworkManager.Singleton.ConnectedClients[targetClientId]
                                .PlayerObject.GetComponent<PlayerControl>();

        if (damagedPlayer != null && damagedPlayer.networkPlayerHealth.Value > 0)
        {
            damagedPlayer.networkPlayerHealth.Value -= damage;
        }

        //Notify Client its getting punch
        NotifyHealthChangeClientRpc(damage, new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { targetClientId }
            }
        });
    }

    [ClientRpc]
    public void NotifyHealthChangeClientRpc(int damage, ClientRpcParams clientRpcParams = default)
    {
        // if (IsOwner) return;
        UILogger.Instance.LogInfo($"You got hit by {damage} points");
    }

    [ServerRpc]
    public void UpdatePlayerAnimationStateServerRpc(PlayerAnimationState newPlayerAnimationState)
    {
        networkPlayerAnimationState.Value = newPlayerAnimationState;
        if (newPlayerAnimationState == PlayerAnimationState.Punch)
        {
            networkPlayerPunchBlend.Value = Random.Range(0.0f, 1.0f);
        }
    }
}