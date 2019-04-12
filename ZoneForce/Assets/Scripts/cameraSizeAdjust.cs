using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraSizeAdjust : MonoBehaviour
{
    Camera mainCamera;
    void Start()
    {
        float res = 441.0f / Screen.dpi;
        float camXRatio = Screen.width / 1920.0f * res;
        float camYRatio = Screen.height / 1080.0f * res;
        mainCamera = Camera.main;
        if (Application.platform == RuntimePlatform.IPhonePlayer)
            mainCamera.rect = new Rect(0.5f - camXRatio / 2.0f,
            0.5f - camYRatio / 2.0f, camXRatio, camYRatio);

    }
}