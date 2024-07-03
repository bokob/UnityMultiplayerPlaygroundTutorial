using Unity.Netcode;
using UnityEngine;

namespace DilmerGames.Core.Singletons
{
    /// <summary>
    /// 네트워크 상에서 관리될 오브젝트들을 관리하기 위한 매니저의 기본 클래스 
    /// 멀티 게임 환경에서 하나만 존재해야 되거나, 네트워크 동기화 필요한 경우 사용
    /// </summary>
    public class NetworkSingleton<T> : NetworkBehaviour
        where T : Component // NetworkBehavir를 상속받는 제네릭 클래스, T는 Component 클래스를 상속받아야 한다
    {
        private static T _instance;
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    var objs = FindObjectsOfType(typeof(T)) as T[]; // 씬에서 타입 T의 모든 객체를 찾는다
                    if (objs.Length > 0)    
                        _instance = objs[0];    // 맨앞(가장 먼저 찾은 요소)의 객체를 _instance에 할당
                    if (objs.Length > 1)
                    {
                        Debug.LogError("There is more than one " + typeof(T).Name + " in the scene."); // 동일한 타입의 객체가 여러 개 있음을 알림
                    }
                    if (_instance == null)
                    {
                        GameObject obj = new GameObject(); // 객체 생성
                        obj.name = string.Format("_{0}", typeof(T).Name); // 생성된 객체의 이름을 타입 T의 이름으로 설정
                        _instance = obj.AddComponent<T>(); // 새 객체에 타입 T의 컴포넌트 추가하고 _instance에 할당
                    }
                }
                return _instance;
            }
        }
    }
}