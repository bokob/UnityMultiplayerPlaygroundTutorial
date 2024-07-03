using DilmerGames.Core.Singletons;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI �����ϴ� �Ŵ��� Ŭ����
/// </summary>
public class UIManager : Singleton<UIManager>
{
    [SerializeField]
    private Button startServerButton;   // ���� ���� ��ư

    [SerializeField]
    private Button startHostButton;     // ȣ��Ʈ ���� ��ư

    [SerializeField]
    private Button startClientButton;   // Ŭ���̾�Ʈ ���� ��ư

    [SerializeField]
    private TextMeshProUGUI playersInGameText;  // ���� ���� �÷��̾� �� ǥ��

    [SerializeField]
    private TMP_InputField joinCodeInput;       // ���� �ڵ� �Է��ϴ� ��

    [SerializeField]
    private Button executePhysicsButton;        // ���� �ùķ��̼� ���� ��ư

    private bool hasServerStarted;              // ������ ���۵Ǿ����� ���θ� ��Ÿ���� �÷���

    private void Awake()
    {
        Cursor.visible = true; // ���콺 Ŀ�� ���̰� ����
    }

    void Update()
    {
        playersInGameText.text = $"Players in game: {PlayersManager.Instance.PlayersInGame}"; // ���ӿ� ���� ���� �÷��̾� �� ������Ʈ
    }

    void Start()
    {
        /*
        '?.' �����ڴ� Null �ʰǺ� ������, ��ü�� null�� �ƴ� ���� ����� �޼��忡 ������ �� �ְ� �Ѵ�. 
        ��ü�� null�� ���, NullReferenceException�� ������ �� �ִ�.
         */

        // START SERVER ��ư Ŭ�� �̺�Ʈ ó��
        startServerButton?.onClick.AddListener(() =>
        {
            // ���� ���� �õ��ϰ�, ���� �Ǵ� ���п� ���� ������ �α� �޽��� ���
             
            if (NetworkManager.Singleton.StartServer())
                Logger.Instance.LogInfo("Server started...");
            else
                Logger.Instance.LogInfo("Unable to start server...");
        });

        // START HOST ��ư Ŭ�� �̺�Ʈ ó��
        startHostButton?.onClick.AddListener(async () =>
        {
            // this allows the UnityMultiplayer and UnityMultiplayerRelay scene to work with and without
            // relay features - if the Unity transport is found and is relay protocol then we redirect all the 
            // traffic through the relay, else it just uses a LAN type (UNET) communication.

            // Relay ����� Ȱ��ȭ�� ���, Relay ���� ����
            if (RelayManager.Instance.IsRelayEnabled) 
                await RelayManager.Instance.SetupRelay();

            // ȣ��Ʈ ���� �õ��ϰ�, ���� �Ǵ� ���п� ���� ������ �α� �޽��� ���
            if (NetworkManager.Singleton.StartHost())
                Logger.Instance.LogInfo("Host started...");
            else
                Logger.Instance.LogInfo("Unable to start host...");
        });

        // START CLIENT ��ư Ŭ�� �̺�Ʈ ó��
        startClientButton?.onClick.AddListener(async () =>
        {
            // Relay ����� Ȱ��ȭ�Ǿ� �ְ�, ���� �ڵ尡 ��� ���� ���� ��� Relay ������ ����
            if (RelayManager.Instance.IsRelayEnabled && !string.IsNullOrEmpty(joinCodeInput.text))
                await RelayManager.Instance.JoinRelay(joinCodeInput.text);

            // Ŭ���̾�Ʈ ���� �õ��ϰ�, ���� �Ǵ� ���п� ���� ������ �α� �޽��� ���
            if(NetworkManager.Singleton.StartClient())
                Logger.Instance.LogInfo("Client started...");
            else
                Logger.Instance.LogInfo("Unable to start client...");
        });

        // Ŭ���̾�Ʈ�� ����Ǿ��� �� �α� ����ϴ� �ݹ� ���
        NetworkManager.Singleton.OnClientConnectedCallback += (id) =>
        {
            Logger.Instance.LogInfo($"{id} just connected...");
        };

        // ������ ���۵Ǿ��� �� ȣ��Ǵ� �ݹ��� ����Ͽ� ���� �밡 �÷��׸� true�� ����
        NetworkManager.Singleton.OnServerStarted += () =>
        {
            hasServerStarted = true;
        };

        // executePhysicsNutton Ŭ�� �̺�Ʈ ó��
        executePhysicsButton.onClick.AddListener(() => 
        {
            // ������ ���۵��� �ʾ����� ��� �޽��� ����ϰ� ����
            if (!hasServerStarted)
            {
                Logger.Instance.LogWarning("Server has not started...");
                return;
            }
            SpawnerControl.Instance.SpawnObjects();
        });
    }
}
