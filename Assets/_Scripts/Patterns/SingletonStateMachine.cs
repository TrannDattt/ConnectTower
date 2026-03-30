// using System;
// using UnityEngine;

// namespace Assets._Scripts.Patterns
// {
//     public abstract class SingletonStateMachine<T, E> : StateMachine<E> 
//         where T : SingletonStateMachine<T, E>
//         where E : Enum
//     {
//         private static T _instance;

//         public static T Instance
//         {
//             get
//             {
//                 if (_instance == null)
//                 {
//                     _instance = FindFirstObjectByType<T>();
//                     if (_instance == null)
//                     {
//                         GameObject singletonObject = new GameObject(typeof(T).Name);
//                         _instance = singletonObject.AddComponent<T>();
//                     }
//                 }
//                 return _instance;
//             }
//         }

//         protected virtual void Awake()
//         {
//             if (_instance == null)
//             {
//                 _instance = this as T;
//                 // DontDestroyOnLoad(gameObject);
//             }
//             else if (_instance != this)
//             {
//                 Destroy(gameObject);
//             }
//         }
//     }
// }
