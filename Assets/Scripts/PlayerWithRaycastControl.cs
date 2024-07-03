using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

/// <summary>
/// RayCast�� �̿��� �÷��̾� ����
/// </summary>
[RequireComponent(typeof(NetworkTransform))]
[RequireComponent(typeof(NetworkObject))]
public class PlayerWithRaycastControl : NetworkBehaviour
{
    [SerializeField]
    private float walkSpeed = 3.5f; // �ȱ� �ӵ�

    [SerializeField]
    private float runSpeedOffset = 2.0f;    // �޸��� �ӵ� ������

    [SerializeField]
    private float rotationSpeed = 3.5f;     // ȸ�� �ӵ�

    [SerializeField]
    private Vector2 defaultInitialPositionOnPlane = new Vector2(-4, 4); // �ʱ� ��ġ

    [SerializeField]
    private NetworkVariable<Vector3> networkPositionDirection = new NetworkVariable<Vector3>(); // ��Ʈ��ũ ��ġ ���� ����

    [SerializeField]
    private NetworkVariable<Vector3> networkRotationDirection = new NetworkVariable<Vector3>(); // ��Ʈ��ũ ȸ�� ���� ����

    [SerializeField]
    private NetworkVariable<PlayerState> networkPlayerState = new NetworkVariable<PlayerState>();   // ��Ʈ��ũ �÷��̾� ���� ����


    [SerializeField]
    private NetworkVariable<float> networkPlayerHealth = new NetworkVariable<float>(1000);  // ��Ʈ��ũ �÷��̾� ü�� ����

    [SerializeField]
    private NetworkVariable<float> networkPlayerPunchBlend = new NetworkVariable<float>();  // ��Ʈ��ũ �÷��̾� ��ġ ���� ����

    [SerializeField]
    private GameObject leftHand;    // �޼�

    [SerializeField]
    private GameObject rightHand;   // ������

    [SerializeField]
    private float minPunchDistance = 1.0f;  // �ּ� ��ġ �Ÿ�

    private CharacterController characterController;

    // Ŭ���̾�Ʈ ��ġ ĳ��
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

        // Ŭ���̾�Ʈ ��ġ �� ȸ�� ������Ʈ
        ClientMoveAndRotate();

        // Ŭ���̾�Ʈ �ð��� ���� ������Ʈ
        ClientVisuals();
    }

    private void FixedUpdate()
    {
        if (IsClient && IsOwner)
        {
            // �÷��̾� ���°� ��ġ�̰� ��ġ �׼� Ű�� Ȱ��ȭ�� ���
            if (networkPlayerState.Value == PlayerState.Punch && ActivePunchActionKey())
            {
                // �޼�, ������ ��ġ �˻�
                CheckPunch(leftHand.transform, Vector3.up);
                CheckPunch(rightHand.transform, Vector3.down);
            }
        }
    }

    // Ư�� �������� ��ġ �˻�
    private void CheckPunch(Transform hand, Vector3 aimDirection)
    {
        RaycastHit hit;

        int layerMask = LayerMask.GetMask("Player");

        // ����ĳ��Ʈ�� ���� ��ġ ���� ���θ� �˻�
        if (Physics.Raycast(hand.position, hand.transform.TransformDirection(aimDirection), out hit, minPunchDistance, layerMask))
        {
            Debug.DrawRay(hand.position, hand.transform.TransformDirection(aimDirection) * minPunchDistance, Color.yellow);

            // �浹�� ��ü�� ��Ʈ��ũ ������Ʈ�� ���
            var playerHit = hit.transform.GetComponent<NetworkObject>();
            if (playerHit != null)
            {
                // ��� �÷��̾��� ü���� ���ҽ�Ű�� ���� RPC ȣ��
                UpdateHealthServerRpc(1, playerHit.OwnerClientId);
            }
        }
        else
        {
            Debug.DrawRay(hand.position, hand.transform.TransformDirection(aimDirection) * minPunchDistance, Color.red);
        }
    }

    // Ŭ���̾�Ʈ ��ġ �� ȸ�� ������Ʈ
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

    // Ŭ���̾�Ʈ �ð��� ���� ������Ʈ
    private void ClientVisuals()
    {
        if (oldPlayerState != networkPlayerState.Value) // ���� �÷��̾� ���¿� ��Ʈ��ũ �÷��̾� ���°� �ٸ� ���
        {
            oldPlayerState = networkPlayerState.Value;
            animator.SetTrigger($"{networkPlayerState.Value}");

            // �÷��̾� ���°� ��ġ�� ��� ��ġ ���� �� ����
            if (networkPlayerState.Value == PlayerState.Punch)
            {
                animator.SetFloat($"{networkPlayerState.Value}Blend", networkPlayerPunchBlend.Value);
            }
        }
    }

    // Ŭ���̾�Ʈ�� �Է��� ó��
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

        // Ŭ���̾�Ʈ�� ��ġ �� ȸ�� ���� ���� ������ �˸�
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

    // �������� Ŭ���̾�Ʈ�� ��ġ�� ȸ���� ������Ʈ�ϴ� ServerRpc �޼���
    [ServerRpc]
    public void UpdateClientPositionAndRotationServerRpc(Vector3 newPosition, Vector3 newRotation)
    {
        // ��Ʈ��ũ ������ ���ο� ��ġ�� ȸ���� �Ҵ�
        networkPositionDirection.Value = newPosition;
        networkRotationDirection.Value = newRotation;
    }

    // �������� �÷��̾��� ü���� ���ҽ�Ű�� ServerRoc �޼���
    [ServerRpc]
    public void UpdateHealthServerRpc(int takeAwayPoint, ulong clientId)
    {
        // Ŭ���̾�Ʈ�� NetworkObject�� ã�Ƽ� �ش� �÷��̾��� ü���� ���ҽ�Ŵ
        var clientWithDamaged = NetworkManager.Singleton.ConnectedClients[clientId]
            .PlayerObject.GetComponent<PlayerWithRaycastControl>();

        if (clientWithDamaged != null && clientWithDamaged.networkPlayerHealth.Value > 0)
        {
            clientWithDamaged.networkPlayerHealth.Value -= takeAwayPoint;
        }

        // Ŭ���̾�Ʈ���� �ǰ� ȿ���� �˸��� ���� ClientRpc ȣ��
        NotifyHealthChangedClientRpc(takeAwayPoint, new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                // �ش� Ŭ���̾�Ʈ���Ը� �ǰ� �˸��� ����
                TargetClientIds = new ulong[] { clientId }
            }
        });
    }

    // Ŭ���̾�Ʈ���� �����κ��� ���� �ǰ� �˸��� ó���ϴ� ClientRpc �޼���
    [ClientRpc]
    public void NotifyHealthChangedClientRpc(int takeAwayPoint, ClientRpcParams clientRpcParams = default)
    {
        // �ڽ��� �ǰ��� ���� ���, �α׸� ������� ����
        if (IsOwner) return;

        // �α׸� ����Ͽ� �ǰ��� ���� ����� �����
        Logger.Instance.LogInfo($"Client got punch {takeAwayPoint}");
    }

    // �������� �÷��̾��� ���¸� ������Ʈ�ϴ� ServerRpc �޼���
    [ServerRpc]
    public void UpdatePlayerStateServerRpc(PlayerState state)
    {
        // ��Ʈ��ũ �÷��̾� ���¸� �־��� ���·� ����
        networkPlayerState.Value = state;

        // �÷��̾� ���°� ��ġ�� ���, ��ġ ���� ���� �����ϰ� ����
        if (state == PlayerState.Punch)
        {
            networkPlayerPunchBlend.Value = Random.Range(0.0f, 1.0f);
        }
    }
}
