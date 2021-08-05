using UnityEngine;
using UnityEngine.EventSystems;

public class DragedGraphicHelper : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{

    bool m_isDragging = false;
    RectTransform m_draggedItemRect;
    Camera m_camera;

    // Start is called before the first frame update
    void Start()
    {
        m_draggedItemRect = this.GetComponent<RectTransform>();
        Transform parent = m_draggedItemRect.parent;
        while (parent != null)
        {
            Canvas canvas = parent.GetComponent<Canvas>();
            if (canvas != null && canvas.worldCamera != null)
            {
                m_camera = canvas.worldCamera;
                break;
            }
            parent = parent.parent;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (m_isDragging)
        {
            Vector2 localPosition = Vector2.zero;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                m_draggedItemRect,
                Input.mousePosition,
                m_camera, //this is the thing for your camera
                out localPosition);
            m_draggedItemRect.position = m_draggedItemRect.TransformPoint(localPosition);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            m_isDragging = true;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            m_isDragging = false;
        }
    }
}
