using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

/// <summary>
/// RayCast를 이용한 플레이어 조작
/// </summary>
[RequireComponent(typeof(NetworkTransform))]
[RequireComponent(typeof(NetworkObject))]
public class PlayerWithRaycastControl : NetworkBehaviour
{
    [SerializeField]
    private float walkSpeed = 3.5f; // 걷기 속도

    [SerializeField]
    private float runSpeedOffset = 2.0f;    // 달리기 속도 보정값

    [SerializeField]
    private float rotationSpeed = 3.5f;     // 회전 속도

    [SerializeField]
    private Vector2 defaultInitialPositionOnPlane = new Vector2(-4, 4); // 초기 위치

    [SerializeField]
    private NetworkVariable<Vector3> networkPositionDirection = new NetworkVariable<Vector3>(); // 네트워크 위치 방향 변수

    [SerializeField]
    private NetworkVariable<Vector3> networkRotationDirection = new NetworkVariable<Vector3>(); // 네트워크 회전 방향 변수

    [SerializeField]
    private NetworkVariable<PlayerState> networkPlayerState = new NetworkVariable<PlayerState>();   // 네트워크 플레이어 상태 변수


    [SerializeField]
    private NetworkVariable<float> networkPlayerHealth = new NetworkVariable<float>(1000);  // 네트워크 플레이어 체력 변수

    [SerializeField]
    private NetworkVariable<float> networkPlayerPunchBlend = new NetworkVariable<float>();  // 네트워크 플레이어 펀치 블렌드 변수

    [SerializeField]
    private GameObject leftHand;    // 왼손

    [SerializeField]
    private GameObject rightHand;   // 오른손

    [SerializeField]
    private float minPunchDistance = 1.0f;  // 최소 펀치 거리

    private CharacterController characterController;

    // 클라이언트 위치 캐시
    private Vector3 oldInputPosition = Vector3.zero;
    private Vector3 oldInputRotation = Vector3.zero;
    private PlayerState oldPlayerState = PlayerState.Idle;

    private Animator animator;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        if (IsClient && IsOwner)
        {
            transform.position = new Vector3(Random.Range(defaultInitialPositionOnPlane.x, defaultInitialPositionOnPlane.y), 0,
                   Random.Range(defaultInitialPositionOnPlane.x, defaultInitialPositionOnPlane.y));

            PlayerCameraFollow.Instance.FollowPlayer(transform.Find("PlayerCameraRoot"));
        }
    }

    void Update()
    {
        if (IsClient && IsOwner)
        {
            ClientInput();
        }

        // 클라이언트 위치 및 회전 업데이트
        ClientMoveAndRotate();

        // 클라이언트 시각적 상태 업데이트
        ClientVisuals();
    }

    private void FixedUpdate()
    {
        if (IsClient && IsOwner)
        {
            // 플레이어 상태가 펀치이고 펀치 액션 키가 활성화된 경우
            if (networkPlayerState.Value == PlayerState.Punch && ActivePunchActionKey())
            {
                // 왼손, 오른손 펀치 검사
                CheckPunch(leftHand.transform, Vector3.up);
                CheckPunch(rightHand.transform, Vector3.down);
            }
        }
    }

    // 특정 방향으로 펀치 검사
    private void CheckPunch(Transform hand, Vector3 aimDirection)
    {
        RaycastHit hit;

        int layerMask = LayerMask.GetMask("Player");

        // 레이캐스트를 통해 펀치 가능 여부를 검사
        if (Physics.Raycast(hand.position, hand.transform.TransformDirection(aimDirection), out hit, minPunchDistance, layerMask))
        {
            Debug.DrawRay(hand.position, hand.transform.TransformDirection(aimDirection) * minPunchDistance, Color.yellow);

            // 충돌한 객체가 네트워크 오브젝트인 경우
            var playerHit = hit.transform.GetComponent<NetworkObject>();
            if (playerHit != null)
            {
                // 상대 플레이어의 체력을 감소시키는 서버 RPC 호출
                UpdateHealthServerRpc(1, playerHit.OwnerClientId);
            }
        }
        else
        {
            Debug.DrawRay(hand.position, hand.transform.TransformDirection(aimDirection) * minPunchDistance, Color.red);
        }
    }

    // 클라이언트 위치 및 회전 업데이트
    private void ClientMoveAndRotate()
    {
        if (networkPositionDirection.Value != Vector3.zero)
        {
            characterController.SimpleMove(networkPositionDirection.Value);
        }
        if (networkRotationDirection.Value != Vector3.zero)
        {
            transform.Rotate(networkRotationDirection.Value, Space.World);
        }
    }

    // 클라이언트 시각적 상태 업데이트
    private void ClientVisuals()
    {
        if (oldPlayerState != networkPlayerState.Value) // 이전 플레이어 상태와 네트워크 플레이어 상태가 다른 경우
        {
            oldPlayerState = networkPlayerState.Value;
            animator.SetTrigger($"{networkPlayerState.Value}");

            // 플레이어 상태가 펀치인 경우 펀치 블렌드 값 설정
            if (networkPlayerState.Value == PlayerState.Punch)
            {
                animator.SetFloat($"{networkPlayerState.Value}Blend", networkPlayerPunchBlend.Value);
            }
        }
    }

    // 클라이언트의 입력을 처리
    private void ClientInput()
    {
        // left & right rotation
        Vector3 inputRotation = new Vector3(0, Input.GetAxis("Horizontal"), 0);

        // forward & backward direction
        Vector3 direction = transform.TransformDirection(Vector3.forward);
        float forwardInput = Input.GetAxis("Vertical");
        Vector3 inputPosition = direction * forwardInput;

        // change fighting states
        if (ActivePunchActionKey() && forwardInput == 0)
        {
            UpdatePlayerStateServerRpc(PlayerState.Punch);
            return;
        }

        // change motion states
        if (forwardInput == 0)
            UpdatePlayerStateServerRpc(PlayerState.Idle);
        else if (!ActiveRunningActionKey() && forwardInput > 0 && forwardInput <= 1)
            UpdatePlayerStateServerRpc(PlayerState.Walk);
        else if (ActiveRunningActionKey() && forwardInput > 0 && forwardInput <= 1)
        {
            inputPosition = direction * runSpeedOffset;
            UpdatePlayerStateServerRpc(PlayerState.Run);
        }
        else if (forwardInput < 0)
            UpdatePlayerStateServerRpc(PlayerState.ReverseWalk);

        // 클라이언트의 위치 및 회전 변경 사항 서버에 알림
        if (oldInputPosition != inputPosition ||
            oldInputRotation != inputRotation)
        {
            oldInputPosition = inputPosition;
            oldInputRotation = inputRotation;
            UpdateClientPositionAndRotationServerRpc(inputPosition * walkSpeed, inputRotation * rotationSpeed);
        }
    }

    private static bool ActiveRunningActionKey()
    {
        return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
    }

    private static bool ActivePunchActionKey()
    {
        return Input.GetKey(KeyCode.Space);
    }

    // 서버에서 클라이언트의 위치와 회전을 업데이트하는 ServerRpc 메서드
    [ServerRpc]
    public void UpdateClientPositionAndRotationServerRpc(Vector3 newPosition, Vector3 newRotation)
    {
        // 네트워크 변수에 새로운 위치와 회전을 할당
        networkPositionDirection.Value = newPosition;
        networkRotationDirection.Value = newRotation;
    }

    // 서버에서 플레이어의 체력을 감소시키는 ServerRoc 메서드
    [ServerRpc]
    public void UpdateHealthServerRpc(int takeAwayPoint, ulong clientId)
    {
        // 클라이언트의 NetworkObject를 찾아서 해당 플레이어의 체력을 감소시킴
        var clientWithDamaged = NetworkManager.Singleton.ConnectedClients[clientId]
            .PlayerObject.GetComponent<PlayerWithRaycastControl>();

        if (clientWithDamaged != null && clientWithDamaged.networkPlayerHealth.Value > 0)
        {
            clientWithDamaged.networkPlayerHealth.Value -= takeAwayPoint;
        }

        // 클라이언트에게 피격 효과를 알리기 위해 ClientRpc 호출
        NotifyHealthChangedClientRpc(takeAwayPoint, new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                // 해당 클라이언트에게만 피격 알림을 보냄
                TargetClientIds = new ulong[] { clientId }
            }
        });
    }

    // 클라이언트에서 서버로부터 받은 피격 알림을 처리하는 ClientRpc 메서드
    [ClientRpc]
    public void NotifyHealthChangedClientRpc(int takeAwayPoint, ClientRpcParams clientRpcParams = default)
    {
        // 자신이 피격을 당한 경우, 로그를 출력하지 않음
        if (IsOwner) return;

        // 로그를 출력하여 피격을 받은 사실을 기록함
        Logger.Instance.LogInfo($"Client got punch {takeAwayPoint}");
    }

    // 서버에서 플레이어의 상태를 업데이트하는 ServerRpc 메서드
    [ServerRpc]
    public void UpdatePlayerStateServerRpc(PlayerState state)
    {
        // 네트워크 플레이어 상태를 주어진 상태로 변경
        networkPlayerState.Value = state;

        // 플레이어 상태가 펀치인 경우, 펀치 블렌드 값을 랜덤하게 설정
        if (state == PlayerState.Punch)
        {
            networkPlayerPunchBlend.Value = Random.Range(0.0f, 1.0f);
        }
    }
}
