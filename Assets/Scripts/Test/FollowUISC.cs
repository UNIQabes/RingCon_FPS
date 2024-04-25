using UnityEngine;
using UnityEngine.UI;


public class FollowUISC : MonoBehaviour
{
    [SerializeField] private RectTransform m_uiTarget;
    [SerializeField] private Canvas m_canvas;
    private RectTransform canvasRect;

    private void Start()
    {
        canvasRect = m_canvas.GetComponent<RectTransform>();
    }

    void Update()
    {
        /*
        var position = m_canvas.worldCamera.WorldToScreenPoint(m_uiTarget.position);
        position = Camera.main.ScreenToWorldPoint(position);
        position.z = 0;
        transform.position = position;
        */
        //Debug.Log("うごいてます");

        Vector2 screenPoint= RectTransformUtility.WorldToScreenPoint(Camera.main, m_uiTarget.transform.position);
        //Vector3 screenPoint =Input.mousePosition;
        //this.transform.position= Camera.main.ScreenToWorldPoint(RTransform_MyUtility.OverLayRectToScreen(m_uiTarget.transform.position, canvasRect));
        this.transform.position = RTransform_MyUtil.OverLayRectToWorld(m_uiTarget.transform.position, canvasRect, Camera.main, 20);
    }

    /*
    //RenderMode
    Vector3 RectToScreen(Vector3 WorldPoint,RectTransform canvasRect)
    {
        Vector3[] corners = new Vector3[4];
        canvasRect.GetWorldCorners(corners);
        float minx = corners[0].x;
        float maxx = corners[2].x;
        float miny = corners[0].y;
        float maxy = corners[2].y;
        
        float retx = (WorldPoint.x - minx)/(maxx-minx)* Screen.width;
        float rety = (WorldPoint.y - miny) / (maxy - miny) * Screen.height;
        return new Vector3(retx,rety,10);
    }
    */
}