namespace UnityEngine.Animations.Rigging
{
    using System;

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class CustomOverlayAttribute : Attribute
    {
        private Type m_EffectorType;

        public CustomOverlayAttribute(Type effectorType)
        {
            if (effectorType == null || !effectorType.IsSubclassOf(typeof(IRigEffector)))
                Debug.LogError("Invalid effector for CustomOverlay attribute.");

            m_EffectorType = effectorType;
        }

        public Type effectorType { get => m_EffectorType; }
    }
}
