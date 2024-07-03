using DilmerGames.Core.Singletons;
using Unity.Netcode;

/// <summary>
/// ���� �� �÷��̾���� �����ϴ� �Ŵ��� Ŭ����
/// </summary>
public class PlayersManager : NetworkSingleton<PlayersManager>
{
    // ��Ʈ��ũ ������ ���� �� �÷��̾� �� ����
    NetworkVariable<int> playersInGame = new NetworkVariable<int>();

    // ���� �� �÷��̾� ��
    public int PlayersInGame
    {
        get
        {
            return playersInGame.Value;
        }
    }

    void Start()
    {
        // Ŭ���̾�Ʈ�� ����Ǿ��� �� ȣ��Ǵ� �ݹ� ���
        NetworkManager.Singleton.OnClientConnectedCallback += (id) => // �ݹ� �Լ��� id �ǳ��༭ � Ŭ���̾�Ʈ�� ��������� �ľ� ����
        {

            if(IsServer) // �������� ����Ǵ� ��� �÷��̾� �� ����, �ٸ� Ŭ���̾�Ʈ�� ���� �����ϸ� �÷��̾� ���� �ȸ����Ƿ� ������ �� �� �ְ� ��
                playersInGame.Value++; // �÷��̾� �� ����
        };

        // Ŭ���̾�Ʈ�� ���� �����Ǿ��� �� ȣ��Ǵ� �ݹ� ���
        NetworkManager.Singleton.OnClientDisconnectCallback += (id) =>
        {
            if(IsServer)
                playersInGame.Value--; // �÷��̾� �� ����
        };
    }
}
