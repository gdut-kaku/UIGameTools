using UnityEngine;
using UnityEditor;
using System.IO;
using System.Xml;
using System;

public class BitmapFontExporter : ScriptableWizard
{
    [MenuItem("Tools/Lzh/BMFont��������2��(2021.05.26)")]
    private static void CreateFont()
    {
        ScriptableWizard.DisplayWizard<BitmapFontExporter>("Create Font");
    }


    public TextAsset fontFile;
    public Texture2D textureFile;

    private void OnWizardCreate()
    {
        if (fontFile == null || textureFile == null)
        {
            return;
        }

        string path = EditorUtility.SaveFilePanelInProject("Save Font", fontFile.name, "", "");

        if (!string.IsNullOrEmpty(path))
        {
            ResolveFont(path);
        }
    }


    private void ResolveFont(string exportPath)
    {
        if (!fontFile) throw new UnityException(fontFile.name + "is not a valid font-xml file");

        Font font = new Font();

        XmlDocument xml = new XmlDocument();
        xml.LoadXml(fontFile.text);

        XmlNode info = xml.GetElementsByTagName("info")[0];
        XmlNodeList chars = xml.GetElementsByTagName("chars")[0].ChildNodes;

        CharacterInfo[] charInfos = new CharacterInfo[chars.Count];

        for (int cnt = 0; cnt < chars.Count; cnt++)
        {
            XmlNode node = chars[cnt];
            CharacterInfo charInfo = new CharacterInfo();

            charInfo.index = ToInt(node, "id");
            charInfo.advance = (int)ToFloat(node, "xadvance");
            Rect r = GetUV(node);
            charInfo.uvBottomLeft = new Vector2(r.xMin, r.yMin);
            charInfo.uvBottomRight = new Vector2(r.xMax, r.yMin);
            charInfo.uvTopLeft = new Vector2(r.xMin, r.yMax);
            charInfo.uvTopRight = new Vector2(r.xMax, r.yMax);

            r = GetVert(node);
            charInfo.minX = (int)r.xMin;
            charInfo.maxX = (int)r.xMax;

            //���治����
            //charInfo.minY = (int)r.yMax; 
            //charInfo.maxY = (int)r.yMin;
            //������ʾ
            charInfo.minY = (int)((r.yMax - r.yMin) / 2);
            charInfo.maxY = -(int)((r.yMax - r.yMin) / 2);

            //Debug.Log("charInfo.advance = " + charInfo.advance);
            //Debug.Log("charInfo.glyphHeight = " + charInfo.glyphHeight);
            //Debug.Log("charInfo.glyphWidth = " + charInfo.glyphWidth);
            //Debug.Log("charInfo.minY = " + charInfo.minY);
            //Debug.Log("charInfo.maxY = " + charInfo.maxY);
            //Debug.Log("charInfo.minX = " + charInfo.minX);
            //Debug.Log("charInfo.maxX = " + charInfo.maxX);
            //Debug.Log("------------------------");
            charInfos[cnt] = charInfo;
        }


        Shader shader = Shader.Find("Unlit/Transparent");
        Material material = new Material(shader);
        material.mainTexture = textureFile;
        AssetDatabase.CreateAsset(material, exportPath + ".mat");


        font.material = material;
        font.name = info.Attributes.GetNamedItem("face").InnerText;
        font.characterInfo = charInfos;
        AssetDatabase.CreateAsset(font, exportPath + ".fontsettings");
    }


    private Rect GetUV(XmlNode node)
    {
        Rect uv = new Rect();

        uv.x = ToFloat(node, "x") / textureFile.width;
        uv.y = ToFloat(node, "y") / textureFile.height;
        uv.width = ToFloat(node, "width") / textureFile.width;
        uv.height = ToFloat(node, "height") / textureFile.height;
        uv.y = 1f - uv.y - uv.height;

        return uv;
    }


    private Rect GetVert(XmlNode node)
    {
        Rect uv = new Rect();

        uv.x = ToFloat(node, "xoffset");
        uv.y = ToFloat(node, "yoffset");
        uv.width = ToFloat(node, "width");
        uv.height = ToFloat(node, "height");
        uv.y = -uv.y;
        uv.height = -uv.height;

        return uv;
    }


    private int ToInt(XmlNode node, string name)
    {
        return Convert.ToInt32(node.Attributes.GetNamedItem(name).InnerText);
    }


    private float ToFloat(XmlNode node, string name)
    {
        return (float)ToInt(node, name);
    }
}