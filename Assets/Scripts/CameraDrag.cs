using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class CameraDrag : MonoBehaviour
{
    [SerializeField]
    private RenderMap mapRenderer;

    [SerializeField]
    private MenuControls menuControls;

    private Vector3 origin;
    private Vector3 difference;
    private bool drag = false;

    //Returns the coordinates of the upper left corner of the visible area
    public Vector3 GetUpperLeftCorner()
    {
        float height = 2f * Camera.main.orthographicSize;
        float width = height * Camera.main.aspect;
        return new Vector3(Camera.main.transform.position.x - width / 2,
                           Camera.main.transform.position.y + height / 2,
                           0);
    }

    void Update()
    {
        if (!menuControls.isActive)
        {

            if (Input.GetMouseButton(0))
            {
                if (drag == false)
                {
                    drag = true;
                    origin = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                }
                difference = origin - Camera.main.ScreenToWorldPoint(Input.mousePosition);
            }
            else
            {
                drag = false;

            }
            if (drag)
            {
                var newPosition = Camera.main.transform.position + difference;

                //Restrict camera position
                float height = 2f * Camera.main.orthographicSize;
                float width = height * Camera.main.aspect;

                Camera.main.transform.position = mapRenderer.RestrictCamera(newPosition, width, height);

            }
        }
    }
}
