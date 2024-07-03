using Unity.Collections;
using Unity.Netcode;

// ����ü ����, INetworkSerializable �������̽� �����Ͽ� ��Ʈ��ũ���� ����ȭ�� �� �ֵ��� �ϰ� ��
public struct NetworkString : INetworkSerializable
{
    private FixedString32Bytes info; // �ִ� 32����Ʈ ���� ���� ���ڿ�

    // �������̽� �޼��带 ������ ���� 'override' Ű���� ��� �� ��, ��Ʈ��ũ ����ȭ �� ������ȭ ó��
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter // Net
    {
        serializer.SerializeValue(ref info);
    }

    // System.Object Ŭ�������� ��ӵ� �޼����̱� ������(��� Ŭ������ ������) �������ϰ� �Ǿ 'override' Ű���� ����� ��
    public override string ToString()
    {
        return info.ToString();
    }

    /*
    operator Ű����� ������ �����ε� �ϴµ� ���
     
    implicit Ű����� �Ͻ��� ��ȯ �����ڸ� ������ �� ����Ѵ�. 
    Ư�� Ÿ���� �ٸ� Ÿ������ ��ȯ�� �� ����� ĳ���� ���� �ڵ����� ��ȯ�ϰ� �� �� �ִ�.

    (����)
    ����� ��ȯ�� ���ؼ��� explicit Ű���� ���
     */

    // NetworkString���� 'string'���� ��ȯ�� �� ���
    public static implicit operator string(NetworkString s) => s.ToString();

    // 'string'���� 'NetworkString'���� ��ȯ�� �� ���
    public static implicit operator NetworkString(string s) => new NetworkString() { info = new FixedString32Bytes(s) };
}
