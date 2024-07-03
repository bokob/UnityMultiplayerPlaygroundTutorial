using DilmerGames.Core.Singletons;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 스폰 컨트롤러 클래스, 네트워크 객체를 스폰하고 관리, 물리시뮬레이션 버튼 눌렀을 때 구체 나오는 것
/// </summary>
public class SpawnerControl : NetworkSingleton<SpawnerControl>
{
    [SerializeField]
    private GameObject objectPrefab; // 스폰할 객체 프리팹

    [SerializeField]
    private int maxObjectInstanceCount = 3; // 최대 객체 소환 개수

    private void Awake()
    {
        // 서버가 시작되었을 때 객체 풀 초기화
        NetworkManager.Singleton.OnServerStarted += () =>
        {
            NetworkObjectPool.Instance.InitializePool();
        };
    }

    public void SpawnObjects() // 객체 스폰하는 메서드, 서버에서만 실행됨
    {
        if (!IsServer) return; // 서버 아닌 경우 종료

        for (int i = 0; i < maxObjectInstanceCount; i++) // 지정된 수 만큼 객체 스폰
        {
            //GameObject go = Instantiate(objectPrefab, 
            //    new Vector3(Random.Range(-10, 10), 10.0f, Random.Range(-10, 10)), Quaternion.identity);
            GameObject go = NetworkObjectPool.Instance.GetNetworkObject(objectPrefab).gameObject; // NetworkObjectPool에서 객체 가져옴
            go.transform.position = new Vector3(Random.Range(-10, 10), 10.0f, Random.Range(-10, 10));   // 객체 위치 랜덤하게 설정
            go.GetComponent<NetworkObject>().Spawn();   // 객체를 네트워크에 스폰
        }
    }
}

