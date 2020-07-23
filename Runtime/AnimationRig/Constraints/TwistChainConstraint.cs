namespace UnityEngine.Animations.Rigging
{
    [System.Serializable]
    public struct TwistChainConstraintData : IAnimationJobData, ITwistChainConstraintData
    {
        [SerializeField] private Transform m_Root;
        [SerializeField] private Transform m_Tip;

        [SyncSceneToStream, SerializeField] private Transform m_RootTarget;
        [SyncSceneToStream, SerializeField] private Transform m_TipTarget;

        [SerializeField] private AnimationCurve m_Curve;

        public Transform root { get => m_Root; set => m_Root = value; }
        public Transform tip { get => m_Tip; set => m_Tip = value; }

        public Transform rootTarget { get => m_RootTarget; set => m_RootTarget = value; }
        public Transform tipTarget { get => m_TipTarget; set => m_TipTarget = value; }

        public AnimationCurve curve { get => m_Curve; set => m_Curve = value; }

        bool IAnimationJobData.IsValid() => !(root == null || tip == null || !tip.IsChildOf(root) || rootTarget == null || tipTarget == null || curve == null);

        void IAnimationJobData.SetDefaultValues()
        {
            root = tip = rootTarget = tipTarget = null;
            curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        }
    }

    [DisallowMultipleComponent, AddComponentMenu("Animation Rigging/Twist Chain Constraint")]
    [HelpURL("https://docs.unity3d.com/Packages/com.unity.animation.rigging@0.3?preview=1&subfolder=/manual/constraints/TwistChainConstraint.html")]
    public class TwistChainConstraint : RigConstraint<
        TwistChainConstraintJob,
        TwistChainConstraintData,
        TwistChainConstraintJobBinder<TwistChainConstraintData>
        >
    {
    #if UNITY_EDITOR
    #pragma warning disable 0414
            [NotKeyable, SerializeField, HideInInspector] bool m_SourceObjectsGUIToggle;
            [NotKeyable, SerializeField, HideInInspector] bool m_SettingsGUIToggle;
    #endif
    }
}
