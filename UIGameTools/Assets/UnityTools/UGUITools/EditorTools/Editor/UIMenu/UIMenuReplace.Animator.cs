using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

/*
 * 实现创建AnimationClip并将其设置为动画器的SubAsset的工具。
 */

namespace KakuEditorTools
{
    public static partial class UIMenuReplace
    {
        static EditorApplication.CallbackFunction s_UpdateCallbackFunction_CreateSubAnimatorControll;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="menuCommand"></param>
        [MenuItem("Assets/UITools/Animator/Create Sub Animation Clip", false, 0)]
        public static void CreateSubAnimatorControll(MenuCommand menuCommand)
        {
            //由于中断式弹出窗口会中断Unity的所有行为，此时会导致GUIStyle的设置没有恢复，所以延迟2帧执行。
            s_UpdateCallbackFunction_CreateSubAnimatorControll = InnerCreateSubAnimatorControll;
            EditorApplication.update += s_UpdateCallbackFunction_CreateSubAnimatorControll;
            delay_CreateSubAnimatorControll = 1;
            InnerCreateSubAnimatorControll();
        }

        static int delay_CreateSubAnimatorControll = -1;
        static void InnerCreateSubAnimatorControll()
        {
            delay_CreateSubAnimatorControll--;
            if (delay_CreateSubAnimatorControll >= 0) return;
            EditorApplication.update -= s_UpdateCallbackFunction_CreateSubAnimatorControll;
            s_UpdateCallbackFunction_CreateSubAnimatorControll = null;

            string name = "";
            name = EditorInputDialog.Show("输入内容弹窗", "请输入动画片段名字");
            var controller = Selection.activeObject as AnimatorController;
            if (string.IsNullOrEmpty(name))
            {
                Debug.Log("请输入名字。");
                return;
            }
            // Create the clip
            var clip = AnimatorController.AllocateAnimatorClip(name);
            AssetDatabase.AddObjectToAsset(clip, controller);
            var state = controller.AddMotion(clip);
            //var stateMachine = controller.layers[0].stateMachine;
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            /* From UnityEngine.UI.SelectableEditor.cs
            // Create the clip
            var clip = Animations.AnimatorController.AllocateAnimatorClip(name);
            AssetDatabase.AddObjectToAsset(clip, controller);

            // Create a state in the animatior controller for this clip
            var state = controller.AddMotion(clip);

            // Add a transition property
            controller.AddParameter(name, AnimatorControllerParameterType.Trigger);

            // Add an any state transition
            var stateMachine = controller.layers[0].stateMachine;
            var transition = stateMachine.AddAnyStateTransition(state);
            transition.AddCondition(Animations.AnimatorConditionMode.If, 0, name);
            */
        }

        [MenuItem("Assets/UITools/Animator/Delete Sub Animation Clip", false, 0)]
        public static void DeleteSubAnimatorControll(MenuCommand menuCommand)
        {
            var clip = Selection.activeObject as AnimationClip;
            string mainAssetPath = AssetDatabase.GetAssetPath(clip);
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(mainAssetPath);
            var stateMachine = controller.layers[0].stateMachine;
            ChildAnimatorState[] states = controller.layers[0].stateMachine.states;
            foreach (var item in states)
            {
                if (item.state.motion == clip)
                {
                    stateMachine.RemoveState(item.state);
                    break;
                }
            }
            AssetDatabase.RemoveObjectFromAsset(clip);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        //MenuItem 菜单中，第二个参数，意思是为允许点击检测函数。设置为True，那么就可以控制按钮是否能够被点击。
        [MenuItem("Assets/UITools/Animator/Create Sub Animation Clip", true, 0)]
        static bool CreateSubAnimatorControll()
        {
            var controller = Selection.activeObject as AnimatorController;
            return delay_CreateSubAnimatorControll < 0 && controller != null;
        }

        [MenuItem("Assets/UITools/Animator/Delete Sub Animation Clip", true, 0)]
        static bool DeleteSubAnimatorControll()
        {
            var clip = Selection.activeObject as AnimationClip;
            if (clip != null && AssetDatabase.IsSubAsset(clip.GetInstanceID()))
            {
                return true;
            }
            return false;
        }
    }
}