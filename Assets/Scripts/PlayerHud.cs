using TMPro;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// �÷��̾��� HUD (Heads-Up Display) (�Ӹ� ���� �ߴ� UI) ���� Ŭ����, ex) ����� �̸� ���� �͵�
/// </summary>
public class PlayerHud : NetworkBehaviour
{
    // ��Ʈ��ũ ������ �÷��̾� �̸� ����
    [SerializeField]
    private NetworkVariable<NetworkString> playerNetworkName = new NetworkVariable<NetworkString>();

    // �������̰� �����Ǿ����� ����
    private bool overlaySet = false;

    public override void OnNetworkSpawn() // ��Ʈ��ũ���� ������ �� ȣ��Ǵ� �޼���
    {
        if(IsServer)
        {
            playerNetworkName.Value = $"Player {OwnerClientId}";
        }
    }

    public void SetOverlay() // �÷��̾� �������� �����ϴ� �޼���
    {
        var localPlayerOverlay = gameObject.GetComponentInChildren<TextMeshProUGUI>();
        localPlayerOverlay.text = $"{playerNetworkName.Value}";
    }

    public void Update()
    {
        // �������̰� ���� �������� �ʰ� �÷��̾� �̸��� ������� �ʴٸ�
        if(!overlaySet && !string.IsNullOrEmpty(playerNetworkName.Value))
        {
            SetOverlay();       // �������� ����
            overlaySet = true;  // �������� ���� �Ǿ��� ǥ��
        }
    }
}
