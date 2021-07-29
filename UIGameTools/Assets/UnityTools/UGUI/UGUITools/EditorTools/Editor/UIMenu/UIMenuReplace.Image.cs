using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.UI;

/*
 * 实现批量创建Image的工具。
 */

namespace KakuEditorTools
{
    public static partial class UIMenuReplace
    {
        [MenuItem("Assets/UITools/Image/Create Images From Sprites")]
        private static void CreateImages()
        {
            Texture2D texture2D = new Texture2D(200, 200);
            List<Sprite> list = new List<Sprite>();
            foreach (var item in Selection.objects)
            {
                if (item is Sprite)
                {
                    list.Add((Sprite)item);
                }
                else if (item is Texture2D)
                {
                    if (AssetDatabase.IsMainAsset(item))
                    {
                        Sprite sp = AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GetAssetPath(item.GetInstanceID()));
                        list.Add(sp);
                    }
                }
                if (list.Count > 0)
                {
                    SpritesToImagesWindow.OpenWindow(list);
                }
            }
        }

        [WindowBase("SpritesToImagesWindow", true)]
        public class SpritesToImagesWindow : WindowEditorBase<SpritesToImagesWindow>
        {
            private static List<Sprite> m_Spritelist;
            private RectTransform m_SpriteParent;
            private string m_Prefix = "";
            private bool m_Rename = false;

            public static void OpenWindow(List<Sprite> list)
            {
                m_Spritelist = list;
                Init();
            }

            protected override void OnDisable()
            {
                m_Spritelist = null;
                m_SpriteParent = null;
                base.OnDisable();
            }

            protected override void OnGUI()
            {
                base.OnGUI();
                var guiRect = EditorGUILayout.BeginVertical();
                m_SpriteParent = (RectTransform)EditorGUILayout.ObjectField("选择场景中的父物体", m_SpriteParent, typeof(RectTransform), true);
                m_Prefix = EditorGUILayout.TextField("(可选)设置前缀名", m_Prefix);
                if (GUILayout.Button("批量创建Image"))
                {
                    if (m_SpriteParent != null && m_SpriteParent.gameObject.scene.IsValid())
                    {
                        if (m_Spritelist != null)
                        {
                            m_Spritelist.Sort((x, y) => { return x.name.CompareTo(y.name); });
                            for (int i = 0; i < m_Spritelist.Count; i++)
                            {
                                Sprite sprite = m_Spritelist[i];
                                GameObject go = new GameObject(sprite.name, typeof(RectTransform), typeof(Image));
                                RectTransform rect = go.GetComponent<RectTransform>();
                                rect.SetParent(m_SpriteParent);
                                rect.anchoredPosition3D = Vector3.zero;
                                rect.localScale = Vector3.one;
                                rect.localRotation = Quaternion.identity;
                                Image image = go.GetComponent<Image>();
                                image.sprite = sprite;
                                image.raycastTarget = false;
                                image.SetNativeSize();
                                if (!string.IsNullOrEmpty(m_Prefix))
                                {
                                    go.name = m_Prefix + go.name;
                                }
                            }
                            this.Close();
                        }
                    }
                    else
                    {
                        m_SpriteParent = null;
                        Debug.LogError("请选择正确的父物体。");
                    }
                }



                EditorGUILayout.EndVertical();
                minSize = maxSize = new Vector2(400, 100);
            }
        }
    }
}