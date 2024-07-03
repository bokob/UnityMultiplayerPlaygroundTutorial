using Unity.Netcode;
using Unity.Netcode.Samples;
using UnityEngine;

/// <summary>
/// ��Ʈ��ũ �󿡼� �÷��̾ ���� ��Ʈ���� �� �ְ� �ϴ� Ŭ����
/// </summary>

// [RequireComponent]�� Unity�� �ڵ����� ������ ������Ʈ �߰�
[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(ClientNetworkTransform))]
public class PlayerBallControl : NetworkBehaviour
{
    [SerializeField]
    private float speed = 3.5f; // �� �̵� �ӵ�

    [SerializeField]
    private float flySpeed = 3.5f; // ���� ���ƿ����� �ӵ�

    [SerializeField]
    private Vector2 defaultInitialPositionOnPlane = new Vector2(-4, 4); // �ʱ� ��ġ ����

    private Rigidbody ballRigidBody;

    void Awake()
    {
        ballRigidBody = GetComponent<Rigidbody>();
    }
   
    void Start()
    {
        if (IsClient && IsOwner) // Ŭ���̾�Ʈ�̸� ���� �÷��̾��� ���
        {
            // �ʱ� ��ġ �����ϰ� ����
            transform.position = new Vector3(Random.Range(defaultInitialPositionOnPlane.x, defaultInitialPositionOnPlane.y), 0,
                   Random.Range(defaultInitialPositionOnPlane.x, defaultInitialPositionOnPlane.y));
        }
    }

    void Update()
    {
        if (IsClient && IsOwner) // Ŭ���̾�Ʈ�̸� ���� �÷��̾��� ���
        {
            ClientInput();
        }
    }

    private void ClientInput() // Ŭ���̾�Ʈ �Է� ó���ϴ� �޼���
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // ���� �Է� ���� ���� �յڷ� �� ����
        if (vertical > 0 || vertical < 0)
            ballRigidBody.AddForce(vertical > 0 ? Vector3.forward * speed : Vector3.back * speed);

        // ���� �Է� ���� ���� �¿�� �� ����
        if (horizontal > 0 || horizontal < 0)
            ballRigidBody.AddForce(horizontal > 0 ? Vector3.right * speed : Vector3.left * speed);
        
        // 'Space' Ű ������ ���� �� ����
        if (Input.GetKey(KeyCode.Space))
            ballRigidBody.AddForce(Vector3.up * flySpeed);
    }
}
