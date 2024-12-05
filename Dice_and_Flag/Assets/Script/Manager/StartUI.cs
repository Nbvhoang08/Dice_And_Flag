using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartUI : MonoBehaviour
{
    // Start is called before the first frame update
    public Canvas canvas;

    void Start()
    {
        UIManager.Instance.OpenUI<HomeCanvas>();

        canvas = GetComponent<Canvas>();
    }

    void Update()
    {

        // Đảm bảo chế độ render của Canvas luôn là ScreenSpace
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        if (canvas.worldCamera == null)
        {
            // Tìm Main Camera trong scene nếu renderCamera bị null
            canvas.worldCamera = Camera.main;
        }
    }

}