using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public class CreateImageTools
{
    static bool s_IsOpen = false;
    static bool isTrigger = true;
    [MenuItem("UGUITools/ImageTools/打开拖拽创建Image功能", priority = 10)]
    public static void OpenImageTools()
    {
        s_IsOpen = true;
    }

    [MenuItem("UGUITools/ImageTools/关闭拖拽创建Image功能", priority = 10)]
    public static void CloseImageTools()
    {
        s_IsOpen = false;
    }

    [InitializeOnLoadMethod]
    private static void Init()
    {
        EditorApplication.projectWindowItemOnGUI += ProjectWindowItemOnGui;
        EditorApplication.hierarchyChanged += HierarchyWindowChanged;
    }

    private static void ProjectWindowItemOnGui(string guid, Rect selectionRect)
    {
        if (!s_IsOpen)
        {
            return;
        }
        // 拖动图片出Project窗口时
        if (Event.current.type == EventType.DragPerform || Event.current.type == EventType.DragUpdated || Event.current.type == EventType.DragExited)
        {
            isTrigger = true;
        }
    }

    private static void HierarchyWindowChanged()
    {
        if (!s_IsOpen)
        {
            return;
        }
        if (!isTrigger)
        {
            return;
        }
        // 此时Unity会默认创建Sprite并定位到该GameObject上
        GameObject go = Selection.activeGameObject;
        if (go == null)
        {
            return;
        }

        SpriteRenderer spriteRenderer = go.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            return;
        }
        go.name = "Image";
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.localPosition = Vector3.zero;
        rt.localScale = Vector3.one;
        Image image = go.AddComponent<Image>();
        image.raycastTarget = false;
        image.sprite = spriteRenderer.sprite;
        Object.DestroyImmediate(spriteRenderer);
        image.SetNativeSize();

        isTrigger = false;
    }

}
