using Cinemachine;
using DilmerGames.Core.Singletons;
using UnityEngine;

/// <summary>
/// 플레이어를 따라다니느 카메라 관리 클래스
/// </summary>
public class PlayerCameraFollow : Singleton<PlayerCameraFollow>
{
    [SerializeField]
    private float amplitudeGain = 0.5f; // 진폭 게인 설정

    [SerializeField]
    private float frequencyGain = 0.5f; // 주파수 게인 설정

    private CinemachineVirtualCamera cinemachineVirtualCamera;  // 시네마신 가상 카메라 변수

    private void Awake()
    {
        cinemachineVirtualCamera = GetComponent<CinemachineVirtualCamera>();
    }

    public void FollowPlayer(Transform transform) // 카메라가 플레이어 따라다니도록 설정하는 메서드
    {
        // not all scenes have a cinemachine virtual camera so return in that's the case
        if (cinemachineVirtualCamera == null) return;

        cinemachineVirtualCamera.Follow = transform; // 카메라가 따라다닐 대상 설정

        // 카메라 흔들림 효과를 위한 Perlin 노이즈 설정
        var perlin = cinemachineVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        perlin.m_AmplitudeGain = amplitudeGain;
        perlin.m_FrequencyGain = frequencyGain;
    }
}
