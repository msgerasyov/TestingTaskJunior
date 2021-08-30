using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    public float alpha;
    public float minScroll, maxScroll;

    [SerializeField]
    private RenderMap mapRenderer;

    [SerializeField]
    private MenuControls menuControls;

    void Update()
    {
        if (!menuControls.isActive)
        {
            var newSize = Camera.main.orthographicSize - alpha * Input.mouseScrollDelta.y * Time.deltaTime;
            newSize = Mathf.Clamp(newSize, minScroll, maxScroll);

            //Restrict camera position
            float height = 2f * newSize;
            float width = height * Camera.main.aspect;

            Camera.main.transform.position = mapRenderer.RestrictCamera(Camera.main.transform.position, width, height);
            Camera.main.orthographicSize = newSize;
        }
    }
}
