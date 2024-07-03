using TMPro;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 플레이어의 HUD (Heads-Up Display) (머리 위에 뜨는 UI) 관리 클래스, ex) 사용자 이름 같은 것들
/// </summary>
public class PlayerHud : NetworkBehaviour
{
    // 네트워크 변수로 플레이어 이름 저장
    [SerializeField]
    private NetworkVariable<NetworkString> playerNetworkName = new NetworkVariable<NetworkString>();

    // 오버레이가 설정되었는지 여부
    private bool overlaySet = false;

    public override void OnNetworkSpawn() // 네트워크에서 스폰될 때 호출되는 메서드
    {
        if(IsServer)
        {
            playerNetworkName.Value = $"Player {OwnerClientId}";
        }
    }

    public void SetOverlay() // 플레이어 오버레이 설정하는 메서드
    {
        var localPlayerOverlay = gameObject.GetComponentInChildren<TextMeshProUGUI>();
        localPlayerOverlay.text = $"{playerNetworkName.Value}";
    }

    public void Update()
    {
        // 오버레이가 아직 설정되이 않고 플레이어 이름이 비어있지 않다면
        if(!overlaySet && !string.IsNullOrEmpty(playerNetworkName.Value))
        {
            SetOverlay();       // 오버레이 설정
            overlaySet = true;  // 오버레이 설정 되었음 표시
        }
    }
}
