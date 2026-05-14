using System.Collections.Generic;
using Assets._Scripts.Patterns;
using Assets._Scripts.Patterns.EventBus;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets._Scripts.Controllers
{
    public class PointerActionControl : Singleton<PointerActionControl>
    {
        // void Update()
        // {
        //     if (Input.GetMouseButtonDown(0)) // Dùng Input Manager
        //     {
        //         // 1. Tạo dữ liệu Pointer giả lập
        //         PointerEventData eventData = new(EventSystem.current)
        //         {
        //             position = Input.mousePosition
        //         };

        //         // 2. Danh sách chứa các kết quả va chạm (Raycast)
        //         var results = new List<RaycastResult>();

        //         // 3. Bắn Raycast từ EventSystem
        //         EventSystem.current.RaycastAll(eventData, results);
        //         EventBus<PlayerClickEvent>.Publish(new PlayerClickEvent{Data = eventData, Results = results});

        //         if (results.Count > 0)
        //         {
        //             Debug.Log("Bạn vừa click trúng UI: " + results[0].gameObject.name);
        //         }
        //     }
        // }
    }
}