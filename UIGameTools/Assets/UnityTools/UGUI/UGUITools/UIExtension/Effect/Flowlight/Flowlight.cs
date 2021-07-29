using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;//com.unity.uiextensions

[RequireComponent(typeof(Image))]
public class Flowlight : MonoBehaviour
{
    private float widthRate = 1;
    private float heightRate = 1;
    private float xOffsetRate = 0;
    private float yOffsetRate = 0;

    [Header("延迟时间(秒)")]
    [Range(0, 10)]
    public int delayTime = 0;
    [Header("流光颜色")]
    public Color color = Color.white;
    [Header("流光颜色强度")]
    public float power = 1f;
    [Range(0.1f, 5)]
    [Header("移动速度")]
    public float speed = 1;
    [Header("泛光宽度")]
    [Range(0.001f, 0.05f)]
    public float largeWidth = 0.003f;
    [Header("高光宽度")]
    [Range(0.0001f, 0.005f)]
    public float littleWidth = 0.0003f;
    [Header("倾斜角")]
    [Range(-1f, 1f)]
    public float skewRadio = 0f;//倾斜
    [Header("扫光次数")]
    [Range(1, 5)]
    public int MoveRound = 1;
    [Header("流光条数")]
    [Range(0.0f, 1.0f)]
    private float length = 0;

    private float moveTime = 0;
    private float endMoveTime = 0;
    private float delayTimeCounter = 0;
    private Image image;
    private Material imageMat = null;
    void Awake()
    {
        image = GetComponent<Image>();
        imageMat = new Material(ShaderLibrary.GetShaderInstance("Custom/UI/Flowlight"));
        widthRate = image.sprite.textureRect.width * 1.0f / image.sprite.texture.width;
        heightRate = image.sprite.textureRect.height * 1.0f / image.sprite.texture.height;
        xOffsetRate = (image.sprite.textureRect.xMin) * 1.0f / image.sprite.texture.width;
        yOffsetRate = (image.sprite.textureRect.yMin) * 1.0f / image.sprite.texture.height;
        // Debug.Log(string.Format(" widthRate{0}, heightRate{1}， xOffsetRate{2}， yOffsetRate{3}", widthRate, heightRate, xOffsetRate, yOffsetRate));
        image.material = null;
    }

    public void ShowFlowLight(int count = 1)
    {
        float endTime = (1 / speed) * MoveRound;
        OnWaitAnim(endTime);
    }

    private void OnWaitAnim(float time)
    {
        if (image == null)
        {
            return;
        }
        StopCoroutine("SlowLight");
        endMoveTime = time;
        StartCoroutine("SlowLight");
    }

    IEnumerator SlowLight()
    {
        if (image)
        {
            image.material = imageMat;
        }
        delayTimeCounter = delayTime;
        moveTime = 0;
        while (delayTimeCounter > 0)
        {
            delayTimeCounter -= Time.deltaTime;
            yield return null;
        }
        while (moveTime < endMoveTime)
        {
            moveTime += Time.deltaTime;
            SetShader();
            yield return null;
        }
        if (image)
        {
            image.material = null;
            this.enabled = false;
        }
    }

    private void OnEnable()
    {
        float endTime = (1 / speed) * MoveRound;
        OnWaitAnim(endTime);
    }

    void OnDisable()
    {
        if (image)
        {
            image.material = null;
        }
        StopCoroutine("SlowLight");
    }

    void Start()
    {
        SetShader();
    }

    public void SetShader()
    {
        skewRadio = Mathf.Clamp(skewRadio, -1, 1);
        length = Mathf.Clamp(length, 0, 0.5f);
        imageMat.SetColor("_FlowlightColor", color);
        imageMat.SetFloat("_Power", power);
        imageMat.SetFloat("_MoveSpeed", speed);
        imageMat.SetFloat("_LargeWidth", largeWidth);
        imageMat.SetFloat("_LittleWidth", littleWidth);
        imageMat.SetFloat("_SkewRadio", skewRadio);
        imageMat.SetFloat("_Lengthlitandlar", length);
        imageMat.SetFloat("_MoveTime", moveTime);

        imageMat.SetFloat("_WidthRate", widthRate);
        imageMat.SetFloat("_HeightRate", heightRate);
        imageMat.SetFloat("_XOffset", xOffsetRate);
        imageMat.SetFloat("_YOffset", yOffsetRate);
    }
}
