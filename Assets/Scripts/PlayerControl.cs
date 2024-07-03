using Unity.Netcode;
using UnityEngine;

/// <summary>
/// �÷��̾� ���� Ŭ����
/// </summary>
[RequireComponent(typeof(NetworkObject))]
public class PlayerControl : NetworkBehaviour
{
    [SerializeField]
    private float walkSpeed = 3.5f; // �ȱ� �ӵ�

    [SerializeField]
    private float runSpeedOffset = 2.0f; // �ٱ� �ӵ� ������

    [SerializeField]
    private float rotationSpeed = 3.5f; // ȸ�� �ӵ�

    [SerializeField]
    private Vector2 defaultInitialPositionOnPlane = new Vector2(-4, 4); // �ʱ� ��ġ ��� ���� �⺻ ��ġ ����

    [SerializeField]
    private NetworkVariable<Vector3> networkPositionDirection = new NetworkVariable<Vector3>(); // ��Ʈ��ũ�� ���� ���۵Ǵ� ��ġ ����

    [SerializeField]
    private NetworkVariable<Vector3> networkRotationDirection = new NetworkVariable<Vector3>(); // ��Ʈ��ũ�� ���� ���۵Ǵ� ȸ�� ����

    [SerializeField]
    private NetworkVariable<PlayerState> networkPlayerState = new NetworkVariable<PlayerState>();   // ��Ʈ��ũ�� ���� ���۵Ǵ� �÷��̾� ����

    private CharacterController characterController;

    // Ŭ���̾�Ʈ���� ����� ��ġ�� ȸ�� ���� ĳ��
    private Vector3 oldInputPosition = Vector3.zero;
    private Vector3 oldInputRotation = Vector3.zero;
    private PlayerState oldPlayerState = PlayerState.Idle; // �ʱ� �÷��̾� ���´� Idle

    private Animator animator;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        if (IsClient && IsOwner) // Ŭ���̾�Ʈ�̰� ���� �÷��̾��
        {
            // �ʱ� ��ġ ���� ����
            transform.position = new Vector3(Random.Range(defaultInitialPositionOnPlane.x, defaultInitialPositionOnPlane.y), 0,
                   Random.Range(defaultInitialPositionOnPlane.x, defaultInitialPositionOnPlane.y));
        }
    }

    void Update()
    {
        if (IsClient && IsOwner) // Ŭ���̾�Ʈ�̰� ���� �÷��̾��
        {
            ClientInput();
        }

        /*
        ClientInput()�� ������� �Է¿� ���� ȣ��ȴ�. 
        ClientMoveAndRotate(), ClientVisuals()�� ��Ʈ��ũ ������ �� ��ȭ�� ���� �ڵ����� ȣ��Ǿ� Ŭ���̾�Ʈ ĳ������ ���¸� ������Ʈ�Ѵ�.
        ���� if�� �ȿ��� ó�� ���� ��
        */
        ClientMoveAndRotate();  // Ŭ���̾�Ʈ ��ġ �� ȸ�� ������Ʈ
        ClientVisuals();    // Ŭ���̾�Ʈ �ð��� ������Ʈ
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
        // �¿� ȸ��
        Vector3 inputRotation = new Vector3(0, Input.GetAxis("Horizontal"), 0);

        // ���� ���� ���
        Vector3 direction = transform.TransformDirection(Vector3.forward);
        float forwardInput = Input.GetAxis("Vertical");
        Vector3 inputPosition = direction * forwardInput;

        // �ִϸ��̼� ���� ����
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

        // ������ ��ġ �� ȸ�� ���� ���� ����
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
        // Ŭ���̾�Ʈ���� ���ο� ��ġ�� ȸ�� ����
        networkPositionDirection.Value = newPosition;
        networkRotationDirection.Value = newRotation;
    }

    [ServerRpc]
    public void UpdatePlayerStateServerRpc(PlayerState state)
    {
        // Ŭ���̾�Ʈ���� �÷��̾� ���� ������Ʈ ����
        networkPlayerState.Value = state;
    }
}