using System;
using System.Reflection;
using UnityEngine;

namespace UnityEditor.Animations.Rigging
{
    [InitializeOnLoad]
    internal static class AnimationWindowReflection
    {
        static Type m_AnimationWindowType = typeof(EditorWindow).Assembly.GetType("UnityEditor.AnimationWindow");
        static Type m_AnimationWindowStateType = typeof(EditorWindow).Assembly.GetType("UnityEditorInternal.AnimationWindowState");
        static Type m_AnimEditorType = typeof(EditorWindow).Assembly.GetType("UnityEditor.AnimEditor");
        static Type m_ControlType = typeof(EditorWindow).Assembly.GetType("UnityEditorInternal.AnimationWindowControl");

        static FieldInfo m_AnimEditorField = null;
        static FieldInfo m_StateField = null;
        static FieldInfo m_AnimationWindowControlField = null;

        static MethodInfo m_StartPreview = null;
        static MethodInfo m_StopPreview = null;

        static PropertyInfo m_ActiveAnimationClipProperty = null;
        static PropertyInfo m_PreviewingProperty = null;
        static PropertyInfo m_CanPreviewProperty = null;

        static EditorWindow m_AnimationWindow = null;

        public static EditorWindow animationWindow
        {
            get
            {
                if (m_AnimationWindow == null)
                    m_AnimationWindow = FindWindowOpen(m_AnimationWindowType);

                return m_AnimationWindow;
            }
        }

        static AnimationWindowReflection()
        {
            m_AnimEditorField = m_AnimationWindowType.GetField("m_AnimEditor", BindingFlags.Instance | BindingFlags.NonPublic);
            m_StateField = m_AnimEditorType.GetField("m_State", BindingFlags.Instance | BindingFlags.NonPublic);
            m_AnimationWindowControlField = m_AnimationWindowStateType.GetField("m_ControlInterface", BindingFlags.Instance | BindingFlags.NonPublic);

            m_StartPreview = m_ControlType.GetMethod("StartPreview", BindingFlags.Public | BindingFlags.Instance);
            m_StopPreview = m_ControlType.GetMethod("StopPreview", BindingFlags.Public | BindingFlags.Instance);

            m_ActiveAnimationClipProperty = m_AnimationWindowStateType.GetProperty("activeAnimationClip", BindingFlags.Instance | BindingFlags.Public);
            m_PreviewingProperty = m_ControlType.GetProperty("previewing", BindingFlags.Instance | BindingFlags.Public);
            m_CanPreviewProperty = m_ControlType.GetProperty("canPreview", BindingFlags.Instance | BindingFlags.Public);
        }

        static ScriptableObject animEditor
        {
            get
            {
                if (animationWindow != null && m_AnimEditorField != null)
                    return (ScriptableObject)m_AnimEditorField.GetValue(animationWindow);

                return null;
            }
        }

        static object state
        {
            get
            {
                if (animEditor && m_StateField != null)
                    return m_StateField.GetValue(animEditor);

                return null;
            }
        }

        static object control
        {
            get
            {
                if (state != null && m_AnimationWindowControlField != null)
                    return m_AnimationWindowControlField.GetValue(state);

                return null;
            }
        }

        public static AnimationClip activeAnimationClip
        {
            get
            {
                if (state != null && m_ActiveAnimationClipProperty != null)
                    return (AnimationClip)m_ActiveAnimationClipProperty.GetValue(state);

                return null;
            }
            set
            {
                if (state != null && m_ActiveAnimationClipProperty != null)
                    m_ActiveAnimationClipProperty.SetValue(state, value);
            }
        }

        public static void StartPreview()
        {
            if (control != null && m_StartPreview != null)
                m_StartPreview.Invoke(control, null);
        }

        public static void StopPreview()
        {
            if (control != null && m_StopPreview != null)
                m_StopPreview.Invoke(control, null);
        }

        public static bool isPreviewing
        {
            get
            {
                if (control != null && m_PreviewingProperty != null)
                    return (bool)m_PreviewingProperty.GetValue(control);

                return false;
            }
        }

        // This does not check if there is an AnimationClip to play
        public static bool canPreview
        {
            get
            {
                if (control != null && m_CanPreviewProperty != null)
                    return (bool)m_CanPreviewProperty.GetValue(control);

                return false;
            }
        }

        static EditorWindow FindWindowOpen(Type windowType)
        {
            UnityEngine.Object[] objs = Resources.FindObjectsOfTypeAll(windowType);

            foreach (UnityEngine.Object o in objs)
            {
                if (o.GetType() == windowType)
                    return (EditorWindow)o;
            }

            return null;
        }
    }
}
