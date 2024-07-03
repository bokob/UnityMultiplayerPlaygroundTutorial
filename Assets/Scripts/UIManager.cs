using DilmerGames.Core.Singletons;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI 관리하는 매니저 클래스
/// </summary>
public class UIManager : Singleton<UIManager>
{
    [SerializeField]
    private Button startServerButton;   // 서버 시작 버튼

    [SerializeField]
    private Button startHostButton;     // 호스트 시작 버튼

    [SerializeField]
    private Button startClientButton;   // 클라이언트 시작 버튼

    [SerializeField]
    private TextMeshProUGUI playersInGameText;  // 게임 참여 플레이어 수 표시

    [SerializeField]
    private TMP_InputField joinCodeInput;       // 참여 코드 입력하는 곳

    [SerializeField]
    private Button executePhysicsButton;        // 물리 시뮬레이션 실행 버튼

    private bool hasServerStarted;              // 서버가 시작되었는지 여부를 나타내는 플래그

    private void Awake()
    {
        Cursor.visible = true; // 마우스 커서 보이게 설정
    }

    void Update()
    {
        playersInGameText.text = $"Players in game: {PlayersManager.Instance.PlayersInGame}"; // 게임에 참여 중인 플레이어 수 업데이트
    }

    void Start()
    {
        /*
        '?.' 연산자는 Null 초건부 연산자, 객체가 null이 아닐 때만 멤버나 메서드에 접근할 수 있게 한다. 
        객체가 null인 경우, NullReferenceException을 방지할 수 있다.
         */

        // START SERVER 버튼 클릭 이벤트 처리
        startServerButton?.onClick.AddListener(() =>
        {
            // 서버 시작 시도하고, 성공 또는 실패에 따라 적절한 로그 메시지 출력
             
            if (NetworkManager.Singleton.StartServer())
                Logger.Instance.LogInfo("Server started...");
            else
                Logger.Instance.LogInfo("Unable to start server...");
        });

        // START HOST 버튼 클릭 이벤트 처리
        startHostButton?.onClick.AddListener(async () =>
        {
            // this allows the UnityMultiplayer and UnityMultiplayerRelay scene to work with and without
            // relay features - if the Unity transport is found and is relay protocol then we redirect all the 
            // traffic through the relay, else it just uses a LAN type (UNET) communication.

            // Relay 기능이 활성화된 경우, Relay 설정 수행
            if (RelayManager.Instance.IsRelayEnabled) 
                await RelayManager.Instance.SetupRelay();

            // 호스트 시작 시도하고, 성공 또는 실패에 따라 적절한 로그 메시지 출력
            if (NetworkManager.Singleton.StartHost())
                Logger.Instance.LogInfo("Host started...");
            else
                Logger.Instance.LogInfo("Unable to start host...");
        });

        // START CLIENT 버튼 클릭 이벤트 처리
        startClientButton?.onClick.AddListener(async () =>
        {
            // Relay 기능이 활성화되어 있고, 참여 코드가 비어 있지 않은 경우 Relay 서버에 참여
            if (RelayManager.Instance.IsRelayEnabled && !string.IsNullOrEmpty(joinCodeInput.text))
                await RelayManager.Instance.JoinRelay(joinCodeInput.text);

            // 클라이언트 시작 시도하곡, 성공 또는 실패에 따라 적절한 로그 메시지 출력
            if(NetworkManager.Singleton.StartClient())
                Logger.Instance.LogInfo("Client started...");
            else
                Logger.Instance.LogInfo("Unable to start client...");
        });

        // 클라이언트가 연결되었을 때 로그 출력하는 콜백 등록
        NetworkManager.Singleton.OnClientConnectedCallback += (id) =>
        {
            Logger.Instance.LogInfo($"{id} just connected...");
        };

        // 서버가 시작되었을 때 호출되는 콜백을 등록하여 서버 싲가 플래그를 true로 설정
        NetworkManager.Singleton.OnServerStarted += () =>
        {
            hasServerStarted = true;
        };

        // executePhysicsNutton 클릭 이벤트 처리
        executePhysicsButton.onClick.AddListener(() => 
        {
            // 서버가 시작되지 않았으면 경고 메시지 출력하고 리턴
            if (!hasServerStarted)
            {
                Logger.Instance.LogWarning("Server has not started...");
                return;
            }
            SpawnerControl.Instance.SpawnObjects();
        });
    }
}
