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
    private string environment = "production";  // ���� ������ ������ ��, ����ϴ� �� ȯ�� �̸��� ���ϴ� �� �ϴ�.

    [SerializeField]
    private int maxNumberOfConnections = 10;    // �ִ� ���� ���� ��

    /*
    '=>' �����ڴ� ���� �� �Ǵ� ����(Single Expression Body) ����� ������ �� ����ϴ� ����
     */
    public bool IsRelayEnabled => Transport != null && Transport.Protocol == UnityTransport.ProtocolType.RelayUnityTransport;   // Relay ����� Ȱ��ȭ�Ǿ� �ִ��� ����

    public UnityTransport Transport => NetworkManager.Singleton.gameObject.GetComponent<UnityTransport>();  // ��Ʈ��ũ ����� ó���ϴ� �� ���, Unity ��Ʈ��ũ ���� ���� ��Ÿ��

    /* 
    Task�� �񵿱������� ����� �� �ִ� �۾��� ��Ÿ���� Ŭ����
    �񵿱� �۾��� ���α׷��� �۾��� ������ �� �ٸ� �۾��� �����ϰų� ����� �� �ֵ��� ���ش�.
     */
    public async Task<RelayHostData> SetupRelay() // Relay ���� �����ϰ� �ʱ�ȭ�ϴ� �޼���
    {
        // Logger Ŭ�������� ����� UI �޽��� ���
        Logger.Instance.LogInfo($"Relay Server Starting With Max Connections: {maxNumberOfConnections}");

        // �ʱ�ȭ �ɼ����� ȯ�� �̸� ����
        InitializationOptions options = new InitializationOptions()
            .SetEnvironmentName(environment);

        // Unity ���񽺸� �񵿱������� �ʱ�ȭ
        await UnityServices.InitializeAsync(options);

        // ����ó�� �κ�
        if (!AuthenticationService.Instance.IsSignedIn) // �α��� ���� Ȯ��
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync(); // �͸����� �α���
        }

        // Relay ���� �Ҵ� ����
        Allocation allocation = await Relay.Instance.CreateAllocationAsync(maxNumberOfConnections);

        // �Ҵ� (allocation)���κ��� Relay ���� ���� ������ ������ RelayHostData ����ü ��ü ���� �� �ʱ�ȭ
        RelayHostData relayHostData = new RelayHostData
        {
            Key = allocation.Key,
            Port = (ushort) allocation.RelayServer.Port,
            AllocationID = allocation.AllocationId,
            AllocationIDBytes = allocation.AllocationIdBytes,
            IPv4Address = allocation.RelayServer.IpV4,
            ConnectionData = allocation.ConnectionData
        };

        // Relay ������ ������ �� �ִ� �ڵ带 �񵿱������� ��´�
        relayHostData.JoinCode = await Relay.Instance.GetJoinCodeAsync(relayHostData.AllocationID);

        // Relay ������ ���õ� ������ ����
        Transport.SetRelayServerData(relayHostData.IPv4Address, relayHostData.Port, relayHostData.AllocationIDBytes,
                relayHostData.Key, relayHostData.ConnectionData);

        // Relay ���� Join Code ����� UI �޽��� ���
        Logger.Instance.LogInfo($"Relay Server Generated Join Code: {relayHostData.JoinCode}");

        return relayHostData;
    }

    public async Task<RelayJoinData> JoinRelay(string joinCode) // Ư�� Join Code�� �̿��� Relay ������ �����ϴ� �޼���
    {
        // Logger Ŭ�������� ����� UI �޽��� ���
        Logger.Instance.LogInfo($"Client Joining Game With Join Code: {joinCode}");

        // �ʱ�ȭ �ɼ����� ȯ�� �̸� ����
        InitializationOptions options = new InitializationOptions()
            .SetEnvironmentName(environment);

        // Unity ���񽺸� �񵿱������� �ʱ�ȭ
        await UnityServices.InitializeAsync(options);
        
        // ����ó�� �κ�
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync(); // �͸����� �α���
        }

        // Relay ���� �Ҵ� ����
        JoinAllocation allocation = await Relay.Instance.JoinAllocationAsync(joinCode);

        // �Ҵ� (allocation)���κ��� Relay ���� ���� ������ ������ RelayJoinData ����ü ��ü ���� �� �ʱ�ȭ
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

        // Relay ������ ���õ� ������ ����
        Transport.SetRelayServerData(relayJoinData.IPv4Address, relayJoinData.Port, relayJoinData.AllocationIDBytes,
            relayJoinData.Key, relayJoinData.ConnectionData, relayJoinData.HostConnectionData);

        // Relay ���� Join Code ����� UI �޽��� ���
        Logger.Instance.LogInfo($"Client Joined Game With Join Code: {joinCode}");

        return relayJoinData;
    }
}
