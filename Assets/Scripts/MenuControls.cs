using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuControls : MonoBehaviour
{
    public bool isActive = false;
    public GameObject menuPanel;
    public GameObject spriteName;

    [SerializeField]
    private RenderMap mapRenderer;

    [SerializeField]
    private CameraDrag cameraDrag;


    public void OpenMenu()
    {
        menuPanel.SetActive(true);
        isActive = true;
        var coordinates = cameraDrag.GetUpperLeftCorner();
        var nearestNode = mapRenderer.tree.NearestNeighbor(mapRenderer.tree.root, coordinates.x, coordinates.y, 0);
        var text = spriteName.GetComponent<Text>();
        text.text = nearestNode.tile.Id;
    }

    public void CloseMenu()
    {
        menuPanel.SetActive(false);
        isActive = false;
    }

}
