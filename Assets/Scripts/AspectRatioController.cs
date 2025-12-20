using UnityEngine;

public class AspectRatioController : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private float targetAspect = 9f / 16f;

    private int lastW, lastH;

    void Awake()
    {
        if (!targetCamera) targetCamera = Camera.main;
        if (targetCamera) targetCamera.backgroundColor = Color.black;
        Apply();
    }

    void Update()
    {
        if (Screen.width != lastW || Screen.height != lastH)
            Apply();
    }

    private void Apply()
    {
        lastW = Screen.width;
        lastH = Screen.height;

        if (!targetCamera) return;

        float windowAspect = (float)Screen.width / Screen.height;
        float scaleHeight = windowAspect / targetAspect;

        Rect rect = targetCamera.rect;

        if (scaleHeight < 1.0f)
        {
            // 위/아래 레터박스
            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) * 0.5f;
        }
        else
        {
            // 좌/우 필러박스
            float scaleWidth = 1.0f / scaleHeight;
            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) * 0.5f;
            rect.y = 0;
        }

        targetCamera.rect = rect;
    }
}
