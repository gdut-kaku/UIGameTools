using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
[DisallowMultipleComponent]
public class GridImageEffect : BaseMeshEffect
{
    public Color32 topColor = Color.white;
    public Color32 bottomColor = Color.black;

    public int rowCount = 5;
    public int columnCount = 5;
    public Vector2 SizeDelta = new Vector2(100, 100);
    public Vector2 Padding = new Vector2(5, 5);

    public int multipleSpriteId = 0;
    public Texture2D multipleSprite;
    public List<Sprite> m_SpriteList = new List<Sprite>();

    private static readonly Vector4 s_DefaultTangent = new Vector4(1f, 0f, 0f, -1f);
    private static readonly Vector3 s_DefaultNormal = Vector3.back;

    RectTransform rectTransform;
    Image image;
    readonly Vector2[] m_spUv = new Vector2[4];


#if UNITY_EDITOR

    protected override void OnValidate()
    {
        base.OnValidate();
        if (image == null)
        {
            image = this.GetComponent<Image>();
        }
        if (multipleSprite != null)
        {
            if (multipleSpriteId != multipleSprite.GetInstanceID())
            {
                multipleSpriteId = multipleSprite.GetInstanceID();
                if (AssetDatabase.IsSubAsset(multipleSpriteId))
                {
                    Object obj = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GetAssetPath(multipleSpriteId));
                    Debug.LogError(obj.GetType());
                    multipleSprite = (Texture2D)obj;
                    multipleSpriteId = multipleSprite.GetInstanceID();
                }

                Object[] sps = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(multipleSpriteId));
                m_SpriteList.Clear();
                foreach (var item in sps)
                {
                    if (item is Sprite sp)
                    {
                        if (sp != null)
                        {
                            m_SpriteList.Add((Sprite)item);
                        }
                    }
                }
            }
        }
        else
        {
            if (multipleSpriteId != 0)
            {
                multipleSpriteId = 0;
                m_SpriteList.Clear();
            }
        }

        if (image.sprite == null)
        {
            if (m_SpriteList.Count > 0)
            {
                image.sprite = m_SpriteList[0];
            }
            else
            {
                image.sprite = null;
            }
        }
    }

#endif


    public override void ModifyMesh(VertexHelper vh)
    {
        vh.Clear();
        if (!IsActive())
        {
            return;
        }
        if (image == null)
        {
            image = this.GetComponent<Image>();
        }
        if (rectTransform == null)
        {
            rectTransform = this.GetComponent<RectTransform>();
        }
        if (m_SpriteList.Count == 0)
        {
            return;
        }

        Vector2 beginPos = Vector2.zero;
        beginPos.x = -((SizeDelta.x * columnCount + Padding.x * (columnCount - 1)) / 2);
        beginPos.y = -((SizeDelta.y * columnCount + Padding.y * (rowCount - 1)) / 2);

        UIVertex[] vv = new UIVertex[4];
        for (int column = 0; column < columnCount; column++)
        {
            for (int row = 0; row < rowCount; row++)
            {
                int spIndex = (row * columnCount + column) % m_SpriteList.Count;
                GetUV(m_SpriteList[spIndex], m_spUv);

                Vector3 baseV = new Vector3(column * SizeDelta.x + column * Padding.x + beginPos.x,
                    row * SizeDelta.y + row * Padding.y + beginPos.y);
                var vertex1 = new UIVertex
                {
                    position = baseV + new Vector3(0, 0)
                };
                var vertex2 = new UIVertex
                {
                    position = baseV + new Vector3(0, SizeDelta.y)
                };
                var vertex3 = new UIVertex
                {
                    position = baseV + new Vector3(SizeDelta.x, SizeDelta.y)
                };
                var vertex4 = new UIVertex
                {
                    position = baseV + new Vector3(SizeDelta.x, 0)
                };
                vv[0] = vertex1;
                vv[1] = vertex2;
                vv[2] = vertex3;
                vv[3] = vertex4;
                for (int k = 0; k < vv.Length; k++)
                {
                    vv[k].uv0 = m_spUv[k];
                    vv[k].tangent = s_DefaultTangent;
                    vv[k].normal = s_DefaultNormal;
                    vv[k].color = Color.white;
                    vh.AddVert(vv[k]);
                }
                vh.AddUIVertexQuad(vv);
            }
        }

        /*

        var count = vh.currentVertCount;
        if (count == 0)
            return;

        var vertexs = new List<UIVertex>();
        for (var i = 0; i < count; i++)
        {
            var vertex = new UIVertex();
            vh.PopulateUIVertex(ref vertex, i);
            vertexs.Add(vertex);
        }

        var topY = vertexs[0].position.y;
        var bottomY = vertexs[0].position.y;

        for (var i = 1; i < count; i++)
        {
            var y = vertexs[i].position.y;
            if (y > topY)
            {
                topY = y;
            }
            else if (y < bottomY)
            {
                bottomY = y;
            }
        }

        var height = topY - bottomY;
        for (var i = 0; i < count; i++)
        {
            var vertex = vertexs[i];

            var color = Color32.Lerp(bottomColor, topColor, (vertex.position.y - bottomY) / height);

            vertex.color = color;

            vh.SetUIVertex(vertex, i);
        }

        */

    }


    private void GetUV(Sprite sprite, Vector2[] vectors)
    {
        Vector4 UVRect = UnityEngine.Sprites.DataUtility.GetOuterUV(sprite);
        vectors[0] = new Vector2(UVRect.x, UVRect.y);
        vectors[1] = new Vector2(UVRect.x, UVRect.w);
        vectors[2] = new Vector2(UVRect.z, UVRect.w);
        vectors[3] = new Vector2(UVRect.z, UVRect.y);
    }
}
