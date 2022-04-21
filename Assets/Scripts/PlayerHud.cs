using TMPro;
using Unity.Collections;
using Unity.Netcode;

public class PlayerHud : NetworkBehaviour
{
    private NetworkVariable<NetworkString> playerName = new();

    private bool overlaySet = false;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
        {
            playerName.Value = $"Player {OwnerClientId}";

        }
    }

    public void SetOverlay()
    {
        var localPlayerOverlay = gameObject.GetComponentInChildren<TMP_Text>();
        localPlayerOverlay.text = playerName.Value;
    }

    private void Update()
    {
        if (!overlaySet && !string.IsNullOrEmpty(playerName.Value))
        {
            SetOverlay();
            overlaySet = true;
        }
    }
}

