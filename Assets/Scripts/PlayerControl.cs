using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 플레이어 조작 클래스
/// </summary>
[RequireComponent(typeof(NetworkObject))]
public class PlayerControl : NetworkBehaviour
{
    [SerializeField]
    private float walkSpeed = 3.5f; // 걷기 속도

    [SerializeField]
    private float runSpeedOffset = 2.0f; // 뛰기 속도 보정값

    [SerializeField]
    private float rotationSpeed = 3.5f; // 회전 속도

    [SerializeField]
    private Vector2 defaultInitialPositionOnPlane = new Vector2(-4, 4); // 초기 위치 평면 상의 기본 위치 범위

    [SerializeField]
    private NetworkVariable<Vector3> networkPositionDirection = new NetworkVariable<Vector3>(); // 네트워크를 통해 전송되는 위치 방향

    [SerializeField]
    private NetworkVariable<Vector3> networkRotationDirection = new NetworkVariable<Vector3>(); // 네트워크를 통해 전송되는 회전 방향

    [SerializeField]
    private NetworkVariable<PlayerState> networkPlayerState = new NetworkVariable<PlayerState>();   // 네트워크를 통해 전송되는 플레이어 상태

    private CharacterController characterController;

    // 클라이언트에서 사용할 위치와 회전 방향 캐시
    private Vector3 oldInputPosition = Vector3.zero;
    private Vector3 oldInputRotation = Vector3.zero;
    private PlayerState oldPlayerState = PlayerState.Idle; // 초기 플레이어 상태는 Idle

    private Animator animator;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        if (IsClient && IsOwner) // 클라이언트이고 로컬 플레이어면
        {
            // 초기 위치 랜덤 설정
            transform.position = new Vector3(Random.Range(defaultInitialPositionOnPlane.x, defaultInitialPositionOnPlane.y), 0,
                   Random.Range(defaultInitialPositionOnPlane.x, defaultInitialPositionOnPlane.y));
        }
    }

    void Update()
    {
        if (IsClient && IsOwner) // 클라이언트이고 로컬 플레이어면
        {
            ClientInput();
        }

        /*
        ClientInput()은 사용자의 입력에 의해 호출된다. 
        ClientMoveAndRotate(), ClientVisuals()는 네트워크 변수의 값 변화에 의해 자동으로 호출되어 클라이언트 캐릭터의 상태를 업데이트한다.
        따라서 if문 안에서 처리 안한 것
        */
        ClientMoveAndRotate();  // 클라이언트 위치 및 회전 업데이트
        ClientVisuals();    // 클라이언트 시각적 업데이트
    }

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

    private void ClientVisuals()
    {
        if (oldPlayerState != networkPlayerState.Value)
        {
            oldPlayerState = networkPlayerState.Value;
            animator.SetTrigger($"{networkPlayerState.Value}");
        }
    }

    private void ClientInput()
    {
        // 좌우 회전
        Vector3 inputRotation = new Vector3(0, Input.GetAxis("Horizontal"), 0);

        // 전후 방향 계산
        Vector3 direction = transform.TransformDirection(Vector3.forward);
        float forwardInput = Input.GetAxis("Vertical");
        Vector3 inputPosition = direction * forwardInput;

        // 애니메이션 상태 변경
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

        // 서버에 위치 및 회전 변경 사항 전달
        if (oldInputPosition != inputPosition ||
            oldInputRotation != inputRotation)
        {
            oldInputPosition = inputPosition;
            UpdateClientPositionAndRotationServerRpc(inputPosition * walkSpeed, inputRotation * rotationSpeed);
        }
    }

    private static bool ActiveRunningActionKey()
    {
        return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
    }

    [ServerRpc]
    public void UpdateClientPositionAndRotationServerRpc(Vector3 newPosition, Vector3 newRotation)
    {
        // 클라이언트에게 새로운 위치와 회전 전달
        networkPositionDirection.Value = newPosition;
        networkRotationDirection.Value = newRotation;
    }

    [ServerRpc]
    public void UpdatePlayerStateServerRpc(PlayerState state)
    {
        // 클라이언트에게 플레이어 상태 업데이트 전달
        networkPlayerState.Value = state;
    }
}