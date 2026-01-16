using UnityEngine;

namespace UnityEditor.Animations.Rigging
{
    [InitializeOnLoad]
    static class AnimationWindowUtils
    {
        static UnityEditor.AnimationWindow m_AnimationWindow = null;

        public static UnityEditor.AnimationWindow animationWindow
        {
            get
            {
                if (m_AnimationWindow == null)
                    m_AnimationWindow = FindWindowOpen();

                return m_AnimationWindow;
            }
        }

        public static AnimationClip activeAnimationClip
        {
            get
            {
                if (animationWindow != null)
                    return animationWindow.animationClip;

                return null;
            }
            set
            {
                if (animationWindow != null)
                    animationWindow.animationClip = value;
            }
        }

        public static void StartPreview()
        {
            if (animationWindow != null)
                animationWindow.previewing = true;
        }

        public static void StopPreview()
        {
            if (animationWindow != null)
                animationWindow.previewing = false;
        }

        public static bool isPreviewing
        {
            get
            {
                if (animationWindow != null)
                    return animationWindow.previewing;

                return false;
            }
        }

        // This does not check if there is an AnimationClip to play
        public static bool canPreview
        {
            get
            {
                if (animationWindow != null)
                    return animationWindow.canPreview;

                return false;
            }
        }

        static UnityEditor.AnimationWindow FindWindowOpen()
        {
            UnityEngine.Object[] objs = Resources.FindObjectsOfTypeAll(typeof(UnityEditor.AnimationWindow));

            foreach (UnityEngine.Object o in objs)
            {
                if (o.GetType() == typeof(UnityEditor.AnimationWindow))
                    return (UnityEditor.AnimationWindow)o;
            }

            return null;
        }
    }
}
