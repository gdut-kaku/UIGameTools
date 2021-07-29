using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimationLoopCount : MonoBehaviour
{
    [Header("Ñ­»·´ÎÊý")]
    public int LoopTime = -1;
    public Animator m_Animator;
    public AnimatorStateInfo m_AnimatorState;

    private void Start()
    {
        if (m_Animator == null)
        {
            m_Animator = this.GetComponent<Animator>();
        }
    }

    void Update()
    {
        if (m_Animator.enabled && LoopTime > 0)
        {
            m_AnimatorState = m_Animator.GetCurrentAnimatorStateInfo(0);
            if (m_AnimatorState.normalizedTime > LoopTime)
            {
                m_Animator.enabled = false;
            }
        }
    }
}
