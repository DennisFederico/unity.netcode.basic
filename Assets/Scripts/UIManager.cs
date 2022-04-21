using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using Dennis.Unity.Utils.Loggers;
using TMPro;

public class UIManager : MonoBehaviour
{

    [SerializeField]
    private Button StartHostBtn;

    [SerializeField]
    private Button StartServerBtn;
    [SerializeField]
    private Button StartClientBtn;

    [SerializeField]
    private Button TestPhysicsBtn;

    [SerializeField]
    private TMP_InputField JoinCodeInput;

    [SerializeField]
    private TMP_Text PlayerCounterText;

    [SerializeField]
    private TMP_InputField PlayerNameInput;

    private bool hasServerStarted = false;

    private void Awake()
    {
        Cursor.visible = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        StartHostBtn.onClick.AddListener(async () =>
        {            
            if (RelayManager.Instance.IsRelayEnabled) {            
                RelayHostData data;
                data = await RelayManager.Instance.SetupRelay();
                JoinCodeInput.text = data.JoinCode;
            }

            if (NetworkManager.Singleton.StartHost())
            {
                UILogger.Instance.LogInfo("Host Started!");
            }
            else
            {
                UILogger.Instance.LogError("Cannot Start Host!");
            }
        });

        StartServerBtn.onClick.AddListener(() =>
        {
            if (NetworkManager.Singleton.StartServer())
            {
                UILogger.Instance.LogInfo("Server Started!");
            }
            else
            {
                UILogger.Instance.LogError("Cannot Start Server!");
            }
        });

        StartClientBtn.onClick.AddListener(async () =>
        {

            if (RelayManager.Instance.IsRelayEnabled && !string.IsNullOrEmpty(JoinCodeInput.text))
                await RelayManager.Instance.JoinRelay(JoinCodeInput.text);

            if (NetworkManager.Singleton.StartClient())
            {
                UILogger.Instance.LogInfo("Client Started!");
            }
            else
            {
                UILogger.Instance.LogError("Cannot Start Client!");
            }
        });

        NetworkManager.Singleton.OnServerStarted += () => hasServerStarted = true;

        TestPhysicsBtn.onClick.AddListener(() =>
        {
            if (hasServerStarted) SpawnManager.Instance.SpawnBalls();
            else UILogger.Instance.LogWarning("The Server has not Started yet!!");
        });
    }

    // Update is called once per frame
    void Update()
    {
        PlayerCounterText.text = $"{PlayerManager.Instance.ConnectedPlayers}";
    }
}
