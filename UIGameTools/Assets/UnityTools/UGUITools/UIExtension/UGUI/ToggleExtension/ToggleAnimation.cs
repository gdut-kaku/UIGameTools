using UnityEngine;
using UnityEngine.UI;

public class ToggleAnimation : MonoBehaviour
{
    bool m_IsOn = true;
    bool m_IsSwitching = false;
    float m_SwitchTime = 0;
    [SerializeField][Header("Toggle本体,仅有通知作用")]
    Toggle m_TargetToggle;
    [SerializeField]
    [Header("点击触发按钮")]
    Button m_TargetButton;
    [Range(5, 50)]
    [SerializeField]
    [Header("动画移动的速度")]
    int m_Speed = 5;
    [SerializeField]
    [Header("Toggle切换的Handle")]
    RectTransform m_Handle;
    [SerializeField]
    [Header("On状态时Handle位置")]
    Vector2 m_HandleX_On = new Vector2(50, 0);
    [SerializeField]
    [Header("Off状态时Handle位置")]
    Vector2 m_HandleX_Off = new Vector2(-50, 0);
    [SerializeField]
    [Header("背景图片")]
    Image m_BgImage;
    [SerializeField]
    [Header("On状态背景图片颜色")]
    Color m_OnBgColor = Color.green;
    [SerializeField]
    [Header("Off状态背景图片颜色")]
    Color m_OffBgColor = Color.red;
    [SerializeField]
    [Header("On状态的显示物件组")]
    CanvasGroup m_OnIconGroup;
    [SerializeField]
    [Header("Off状态的显示物件组")]
    CanvasGroup m_OffIconGroup;

    private void Awake()
    {
        m_IsOn = m_TargetToggle.isOn;
        m_TargetToggle.interactable = false;
        m_TargetToggle.toggleTransition = Toggle.ToggleTransition.None;
        m_TargetToggle.transition = Selectable.Transition.None;
        m_TargetButton.onClick.RemoveAllListeners();
        m_TargetButton.onClick.AddListener(Switch);
        if (!m_OnIconGroup.gameObject.activeSelf || !m_OffIconGroup.gameObject.activeSelf)
        {
            m_OnIconGroup.gameObject.SetActive(true);
            m_OffIconGroup.gameObject.SetActive(true);
        }
        ResetStateImmediately(m_IsOn);
    }

    private void Update()
    {
        if (m_IsSwitching)
        {
            Move();
        }
    }

    private void ResetStateImmediately(bool toggleState)
    {
        if (toggleState)
        {
            m_Handle.anchoredPosition = m_HandleX_On;
            m_BgImage.color = m_OnBgColor;
            m_OnIconGroup.alpha = 1;
            m_OffIconGroup.alpha = 0;
        }
        else
        {
            m_Handle.anchoredPosition = m_HandleX_Off;
            m_BgImage.color = m_OffBgColor;
            m_OnIconGroup.alpha = 0;
            m_OffIconGroup.alpha = 1;
        }
    }

    private void Move()
    {
        float deltaTime = Time.deltaTime;
        m_SwitchTime += m_Speed * deltaTime;
        if (m_IsOn)
        {
            SmoothMove(m_HandleX_On, m_HandleX_Off, m_SwitchTime);
            SmoothColor(m_OnBgColor, m_OffBgColor, m_SwitchTime);
            Transparency(m_OnIconGroup, 1, 0, m_SwitchTime);
            Transparency(m_OffIconGroup, 0, 1, m_SwitchTime);
        }
        else
        {
            SmoothMove(m_HandleX_Off, m_HandleX_On, m_SwitchTime);
            SmoothColor(m_OffBgColor, m_OnBgColor, m_SwitchTime);
            Transparency(m_OnIconGroup, 0, 1, m_SwitchTime);
            Transparency(m_OffIconGroup, 1, 0, m_SwitchTime);
        }
        StopSwitching();
    }

    private void SmoothMove(Vector2 begin, Vector2 end, float lerpTime)
    {
        m_Handle.anchoredPosition = Vector2.Lerp(begin, end, lerpTime);
    }

    private void SmoothColor(Color begin, Color end, float lerpTime)
    {
        m_BgImage.color = Color.Lerp(begin, end, lerpTime);
    }

    private void Transparency(CanvasGroup canvasGroup, float begin, float end, float lerpTime)
    {
        canvasGroup.alpha = Mathf.Lerp(begin, end, lerpTime);
    }



    public void Switch()
    {
        m_IsSwitching = true;
    }

    public void StopSwitching()
    {
        if (m_SwitchTime >= 1.0)
        {
            m_IsSwitching = false;
            m_SwitchTime = 0;
            m_IsOn = !m_IsOn;
            ResetStateImmediately(m_IsOn);
            OnToggleChange(m_IsOn);
        }
    }

    private void OnToggleChange(bool toggleState)
    {
        m_TargetToggle.isOn = toggleState;
    }
}
