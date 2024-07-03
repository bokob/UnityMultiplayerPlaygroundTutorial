using Unity.Collections;
using Unity.Netcode;

// 구조체 정의, INetworkSerializable 인터페이스 구현하여 네트워크에서 직렬화할 수 있도록 하게 함
public struct NetworkString : INetworkSerializable
{
    private FixedString32Bytes info; // 최대 32바이트 고정 길이 문자열

    // 인터페이스 메서드를 구현할 때는 'override' 키워드 사용 안 함, 네트워크 직렬화 및 역직렬화 처리
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter // Net
    {
        serializer.SerializeValue(ref info);
    }

    // System.Object 클래스에서 상속된 메서드이기 때문에(모든 클래스의 조상임) 재정의하게 되어서 'override' 키워드 사용한 것
    public override string ToString()
    {
        return info.ToString();
    }

    /*
    operator 키워드는 연산자 오버로딩 하는데 사용
     
    implicit 키워드는 암시적 변환 연산자를 정의할 때 사용한다. 
    특정 타입을 다른 타입으로 변환할 때 명시적 캐스팅 없이 자동으로 변환하게 할 수 있다.

    (참고)
    명시적 변환을 위해서는 explicit 키워드 사용
     */

    // NetworkString에서 'string'으로 변환할 때 사용
    public static implicit operator string(NetworkString s) => s.ToString();

    // 'string'에서 'NetworkString'으로 변환할 때 사용
    public static implicit operator NetworkString(string s) => new NetworkString() { info = new FixedString32Bytes(s) };
}
