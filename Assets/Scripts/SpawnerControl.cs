using DilmerGames.Core.Singletons;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// ���� ��Ʈ�ѷ� Ŭ����, ��Ʈ��ũ ��ü�� �����ϰ� ����, �����ùķ��̼� ��ư ������ �� ��ü ������ ��
/// </summary>
public class SpawnerControl : NetworkSingleton<SpawnerControl>
{
    [SerializeField]
    private GameObject objectPrefab; // ������ ��ü ������

    [SerializeField]
    private int maxObjectInstanceCount = 3; // �ִ� ��ü ��ȯ ����

    private void Awake()
    {
        // ������ ���۵Ǿ��� �� ��ü Ǯ �ʱ�ȭ
        NetworkManager.Singleton.OnServerStarted += () =>
        {
            NetworkObjectPool.Instance.InitializePool();
        };
    }

    public void SpawnObjects() // ��ü �����ϴ� �޼���, ���������� �����
    {
        if (!IsServer) return; // ���� �ƴ� ��� ����

        for (int i = 0; i < maxObjectInstanceCount; i++) // ������ �� ��ŭ ��ü ����
        {
            //GameObject go = Instantiate(objectPrefab, 
            //    new Vector3(Random.Range(-10, 10), 10.0f, Random.Range(-10, 10)), Quaternion.identity);
            GameObject go = NetworkObjectPool.Instance.GetNetworkObject(objectPrefab).gameObject; // NetworkObjectPool���� ��ü ������
            go.transform.position = new Vector3(Random.Range(-10, 10), 10.0f, Random.Range(-10, 10));   // ��ü ��ġ �����ϰ� ����
            go.GetComponent<NetworkObject>().Spawn();   // ��ü�� ��Ʈ��ũ�� ����
        }
    }
}

