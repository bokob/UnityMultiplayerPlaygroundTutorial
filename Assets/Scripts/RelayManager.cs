using DilmerGames.Core.Singletons;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using Unity.Netcode.Transports.UTP;

public class RelayManager : Singleton<RelayManager>
{
    [SerializeField]
    private string environment = "production";  // 실제 게임을 서비스할 때, 사용하는 그 환경 이름을 말하는 듯 하다.

    [SerializeField]
    private int maxNumberOfConnections = 10;    // 최대 연결 가능 수

    /*
    '=>' 연산자는 람다 식 또는 본문(Single Expression Body) 멤버를 정의할 때 사용하는 구문
     */
    public bool IsRelayEnabled => Transport != null && Transport.Protocol == UnityTransport.ProtocolType.RelayUnityTransport;   // Relay 기능이 활성화되어 있는지 여부

    public UnityTransport Transport => NetworkManager.Singleton.gameObject.GetComponent<UnityTransport>();  // 네트워크 통신을 처리하는 데 사용, Unity 네트워크 전송 계층 나타냄

    /* 
    Task는 비동기적으로 실행될 수 있는 작업을 나타내는 클래스
    비동기 작업은 프로그램이 작업을 시작한 후 다른 작업을 수행하거나 대기할 수 있도록 해준다.
     */
    public async Task<RelayHostData> SetupRelay() // Relay 서버 설정하고 초기화하는 메서드
    {
        // Logger 클래스에서 디버깅 UI 메시지 기록
        Logger.Instance.LogInfo($"Relay Server Starting With Max Connections: {maxNumberOfConnections}");

        // 초기화 옵션으로 환경 이름 설정
        InitializationOptions options = new InitializationOptions()
            .SetEnvironmentName(environment);

        // Unity 서비스를 비동기적으로 초기화
        await UnityServices.InitializeAsync(options);

        // 인증처리 부분
        if (!AuthenticationService.Instance.IsSignedIn) // 로그인 상태 확인
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync(); // 익명으로 로그인
        }

        // Relay 서버 할당 생성
        Allocation allocation = await Relay.Instance.CreateAllocationAsync(maxNumberOfConnections);

        // 할당 (allocation)으로부터 Relay 서버 관련 정보를 가져와 RelayHostData 구조체 객체 생성 및 초기화
        RelayHostData relayHostData = new RelayHostData
        {
            Key = allocation.Key,
            Port = (ushort) allocation.RelayServer.Port,
            AllocationID = allocation.AllocationId,
            AllocationIDBytes = allocation.AllocationIdBytes,
            IPv4Address = allocation.RelayServer.IpV4,
            ConnectionData = allocation.ConnectionData
        };

        // Relay 서버에 참가할 수 있는 코드를 비동기적으로 얻는다
        relayHostData.JoinCode = await Relay.Instance.GetJoinCodeAsync(relayHostData.AllocationID);

        // Relay 서버와 관련된 데이터 설정
        Transport.SetRelayServerData(relayHostData.IPv4Address, relayHostData.Port, relayHostData.AllocationIDBytes,
                relayHostData.Key, relayHostData.ConnectionData);

        // Relay 서버 Join Code 디버깅 UI 메시지 기록
        Logger.Instance.LogInfo($"Relay Server Generated Join Code: {relayHostData.JoinCode}");

        return relayHostData;
    }

    public async Task<RelayJoinData> JoinRelay(string joinCode) // 특정 Join Code를 이용해 Relay 서버에 참여하는 메서드
    {
        // Logger 클래스에서 디버깅 UI 메시지 기록
        Logger.Instance.LogInfo($"Client Joining Game With Join Code: {joinCode}");

        // 초기화 옵션으로 환경 이름 설정
        InitializationOptions options = new InitializationOptions()
            .SetEnvironmentName(environment);

        // Unity 서비스를 비동기적으로 초기화
        await UnityServices.InitializeAsync(options);
        
        // 인증처리 부분
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync(); // 익명으로 로그인
        }

        // Relay 서버 할당 생성
        JoinAllocation allocation = await Relay.Instance.JoinAllocationAsync(joinCode);

        // 할당 (allocation)으로부터 Relay 서버 관련 정보를 가져와 RelayJoinData 구조체 객체 생성 및 초기화
        RelayJoinData relayJoinData = new RelayJoinData
        {
            Key = allocation.Key,
            Port = (ushort)allocation.RelayServer.Port,
            AllocationID = allocation.AllocationId,
            AllocationIDBytes = allocation.AllocationIdBytes,
            ConnectionData = allocation.ConnectionData,
            HostConnectionData = allocation.HostConnectionData,
            IPv4Address = allocation.RelayServer.IpV4,
            JoinCode = joinCode
        };

        // Relay 서버와 관련된 데이터 설정
        Transport.SetRelayServerData(relayJoinData.IPv4Address, relayJoinData.Port, relayJoinData.AllocationIDBytes,
            relayJoinData.Key, relayJoinData.ConnectionData, relayJoinData.HostConnectionData);

        // Relay 서버 Join Code 디버깅 UI 메시지 기록
        Logger.Instance.LogInfo($"Client Joined Game With Join Code: {joinCode}");

        return relayJoinData;
    }
}
