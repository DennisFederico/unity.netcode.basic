using System.Collections;
using System.Collections.Generic;
using Dennis.Unity.Utils.Loggers;
using Dennis.Unity.Utils.Singletons;
using Unity.Netcode;
using UnityEngine;

public class PlayerManager : NetworkSingleton<PlayerManager>
{
    [SerializeField]
    private NetworkVariable<int> connectedPlayers = new();

    public int ConnectedPlayers { get => connectedPlayers.Value; set => connectedPlayers.Value = value; }

    // Start is called before the first frame update
    void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += (id) =>
        {
            UILogger.Instance.LogInfo($"Player {id} connected");
            if (NetworkManager.IsServer)
            {
                connectedPlayers.Value++;
            }
        };

        NetworkManager.Singleton.OnClientDisconnectCallback += (id) =>
        {
            UILogger.Instance.LogInfo($"Player {id} disconnected");
            if (NetworkManager.IsServer) {
                connectedPlayers.Value--;
            }
        };
    }
}
