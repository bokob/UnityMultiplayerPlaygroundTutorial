using Unity.Netcode;
using Unity.Netcode.Samples;
using UnityEngine;

/// <summary>
/// 네트워크 상에서 플레이어가 공을 컨트롤할 수 있게 하는 클래스
/// </summary>

// [RequireComponent]는 Unity가 자동으로 지정된 컴포넌트 추가
[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(ClientNetworkTransform))]
public class PlayerBallControl : NetworkBehaviour
{
    [SerializeField]
    private float speed = 3.5f; // 공 이동 속도

    [SerializeField]
    private float flySpeed = 3.5f; // 공이 날아오르는 속도

    [SerializeField]
    private Vector2 defaultInitialPositionOnPlane = new Vector2(-4, 4); // 초기 위치 범위

    private Rigidbody ballRigidBody;

    void Awake()
    {
        ballRigidBody = GetComponent<Rigidbody>();
    }
   
    void Start()
    {
        if (IsClient && IsOwner) // 클라이언트이며 로컬 플레이어인 경우
        {
            // 초기 위치 랜덤하게 설정
            transform.position = new Vector3(Random.Range(defaultInitialPositionOnPlane.x, defaultInitialPositionOnPlane.y), 0,
                   Random.Range(defaultInitialPositionOnPlane.x, defaultInitialPositionOnPlane.y));
        }
    }

    void Update()
    {
        if (IsClient && IsOwner) // 클라이언트이며 로컬 플레이어인 경우
        {
            ClientInput();
        }
    }

    private void ClientInput() // 클라이언트 입력 처리하는 메서드
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // 수직 입력 값에 따라 앞뒤로 힘 가함
        if (vertical > 0 || vertical < 0)
            ballRigidBody.AddForce(vertical > 0 ? Vector3.forward * speed : Vector3.back * speed);

        // 수평 입력 값에 따라 좌우로 힘 가함
        if (horizontal > 0 || horizontal < 0)
            ballRigidBody.AddForce(horizontal > 0 ? Vector3.right * speed : Vector3.left * speed);
        
        // 'Space' 키 눌리면 위로 힘 가함
        if (Input.GetKey(KeyCode.Space))
            ballRigidBody.AddForce(Vector3.up * flySpeed);
    }
}
