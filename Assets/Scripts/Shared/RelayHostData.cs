using System;

public struct RelayHostData
{
    public string JoinCode;             // Relay 서버에 참가할 때 사용하는 코드
    public string IPv4Address;          // Relay 서버 IP 주소
    public ushort Port;                 // Relay 서버 포트 번호
    public Guid AllocationID;           // Relay 서버 할당의 고유 ID
    public byte[] AllocationIDBytes;    // Relay 서버 할당 고유 ID 바이트 배열
    public byte[] ConnectionData;       // 클라이언트와 Relay 서버 간의 연결 설정에 사용하는 추가 데이터
    public byte[] Key;                  // Relay 서버와 클라이언트 간의 암호화된 통신을 위한 키
}