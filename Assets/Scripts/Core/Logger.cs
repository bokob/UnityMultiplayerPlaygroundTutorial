using System.Linq;
using DilmerGames.Core.Singletons;
using TMPro;
using UnityEngine;
using System;

/// <summary>
/// 화면 UI애 멀티 플레이 관련 문구 UI에 표시하는 클래스
/// </summary>
public class Logger : Singleton<Logger>
{
    [SerializeField]
    private TextMeshProUGUI debugAreaText = null;

    [SerializeField]
    private bool enableDebug = false;

    [SerializeField]
    private int maxLines = 15;

    void Awake()
    {
        if (debugAreaText == null)
        {
            debugAreaText = GetComponent<TextMeshProUGUI>();
        }
        debugAreaText.text = string.Empty;
    }

    void OnEnable() // 활성화 되었을 때 문구
    {
        debugAreaText.enabled = enableDebug;
        enabled = enableDebug;

        if (enabled)
        {
            debugAreaText.text += $"<color=\"white\">{DateTime.Now.ToString("HH:mm:ss.fff")} {this.GetType().Name} enabled</color>\n";
        }
    }

    public void LogInfo(string message) // 호스트, 서버, 클라이언트 연결되었을 때 나오는 초록 문구
    {
        ClearLines();

        debugAreaText.text += $"<color=\"green\">{DateTime.Now.ToString("HH:mm:ss.fff")} {message}</color>\n";
    }

    public void LogError(string message) // 에러 문구
    {
        ClearLines();
        debugAreaText.text += $"<color=\"red\">{DateTime.Now.ToString("HH:mm:ss.fff")} {message}</color>\n";
    }

    public void LogWarning(string message) // 경고 문구
    {
        ClearLines();
        debugAreaText.text += $"<color=\"yellow\">{DateTime.Now.ToString("HH:mm:ss.fff")} {message}</color>\n";
    }

    private void ClearLines()   // 디버깅 문구들 클리어
    {
        if (debugAreaText.text.Split('\n').Count() >= maxLines)
        {
            debugAreaText.text = string.Empty;
        }
    }
}