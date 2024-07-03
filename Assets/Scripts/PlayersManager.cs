using DilmerGames.Core.Singletons;
using Unity.Netcode;

/// <summary>
/// 게임 내 플레이어들을 관리하는 매니저 클래스
/// </summary>
public class PlayersManager : NetworkSingleton<PlayersManager>
{
    // 네트워크 변수로 게임 내 플레이어 수 저장
    NetworkVariable<int> playersInGame = new NetworkVariable<int>();

    // 게임 내 플레이어 수
    public int PlayersInGame
    {
        get
        {
            return playersInGame.Value;
        }
    }

    void Start()
    {
        // 클라이언트가 연결되었을 때 호출되는 콜백 등록
        NetworkManager.Singleton.OnClientConnectedCallback += (id) => // 콜백 함수에 id 건네줘서 어떤 클라이언트가 연결었는지 파악 가능
        {

            if(IsServer) // 서버에서 실행되는 경우 플레이어 수 증가, 다른 클라이언트도 접근 가능하면 플레이어 수가 안맞으므로 서버만 할 수 있게 함
                playersInGame.Value++; // 플레이어 수 증가
        };

        // 클라이언트가 연결 해제되었을 때 호출되는 콜백 등록
        NetworkManager.Singleton.OnClientDisconnectCallback += (id) =>
        {
            if(IsServer)
                playersInGame.Value--; // 플레이어 수 감소
        };
    }
}
