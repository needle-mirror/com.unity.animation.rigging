namespace UnityEngine.Animations.Rigging
{
    /// <summary>
    /// The OverrideTransform constraint data.
    /// </summary>
    [System.Serializable]
    public struct OverrideTransformData : IAnimationJobData, IOverrideTransformData
    {
        /// <summary>
        /// The override space controls how the override source Transform
        /// is copied unto constrained Transform.
        /// </summary>
        [System.Serializable]
        public enum Space
        {
            /// <summary>Copy override world TR components into world TR components of the constrained Transform.</summary>
            World = OverrideTransformJob.Space.World,
            /// <summary>Copy override local TR components into local TR components of the constrained Transform.</summary>
            Local = OverrideTransformJob.Space.Local,
            /// <summary>Add override local TR components to local TR components of the constrained Transform. </summary>
            Pivot = OverrideTransformJob.Space.Pivot
        }

        [SerializeField] Transform m_ConstrainedObject;

        [SyncSceneToStream, SerializeField] Transform m_OverrideSource;
        [SyncSceneToStream, SerializeField] Vector3 m_OverridePosition;
        [SyncSceneToStream, SerializeField] Vector3 m_OverrideRotation;
        [SyncSceneToStream, SerializeField, Range(0f, 1f)] float m_PositionWeight;
        [SyncSceneToStream, SerializeField, Range(0f, 1f)] float m_RotationWeight;

        [NotKeyable, SerializeField] Space m_Space;

        /// <inheritdoc />
        public Transform constrainedObject { get => m_ConstrainedObject; set => m_ConstrainedObject = value; }
        /// <inheritdoc />
        public Transform sourceObject { get => m_OverrideSource; set => m_OverrideSource = value; }
        /// <summary>The override space.</summary>
        public Space space { get => m_Space; set => m_Space = value; }
        /// <summary>The override position. This is taken into account only if sourceObject is null.</summary>
        public Vector3 position { get => m_OverridePosition; set => m_OverridePosition = value; }
        /// <summary>The override rotation. This is taken into account only if sourceObject is null.</summary>
        public Vector3 rotation { get => m_OverrideRotation; set => m_OverrideRotation = value; }
        /// <summary>The weight for which override position has an effect on constrained Transform. This is a value in between 0 and 1.</summary>
        public float positionWeight { get => m_PositionWeight; set => m_PositionWeight = Mathf.Clamp01(value); }
        /// <summary>The weight for which override rotation has an effect on constrained Transform. This is a value in between 0 and 1.</summary>
        public float rotationWeight { get => m_RotationWeight; set => m_RotationWeight = Mathf.Clamp01(value); }

        /// <inheritdoc />
        int IOverrideTransformData.space => (int)m_Space;
        /// <inheritdoc />
        string IOverrideTransformData.positionWeightFloatProperty => ConstraintsUtils.ConstructConstraintDataPropertyName(nameof(m_PositionWeight));
        /// <inheritdoc />
        string IOverrideTransformData.rotationWeightFloatProperty => ConstraintsUtils.ConstructConstraintDataPropertyName(nameof(m_RotationWeight));
        /// <inheritdoc />
        string IOverrideTransformData.positionVector3Property => ConstraintsUtils.ConstructConstraintDataPropertyName(nameof(m_OverridePosition));
        /// <inheritdoc />
        string IOverrideTransformData.rotationVector3Property => ConstraintsUtils.ConstructConstraintDataPropertyName(nameof(m_OverrideRotation));

        /// <inheritdoc />
        bool IAnimationJobData.IsValid() => m_ConstrainedObject != null;

        /// <inheritdoc />
        void IAnimationJobData.SetDefaultValues()
        {
            m_ConstrainedObject = null;
            m_OverrideSource = null;
            m_OverridePosition = Vector3.zero;
            m_OverrideRotation = Vector3.zero;
            m_Space = Space.Pivot;
            m_PositionWeight = 1f;
            m_RotationWeight = 1f;
        }
    }

    /// <summary>
    /// OverrideTransform constraint.
    /// </summary>
    [DisallowMultipleComponent, AddComponentMenu("Animation Rigging/Override Transform")]
    [HelpURL("https://docs.unity3d.com/Packages/com.unity.animation.rigging@1.1/manual/constraints/OverrideTransform.html")]
    public class OverrideTransform : RigConstraint<
        OverrideTransformJob,
        OverrideTransformData,
        OverrideTransformJobBinder<OverrideTransformData>
        >
    {
        /// <inheritdoc />
        protected override void OnValidate()
        {
            base.OnValidate();
            m_Data.positionWeight = Mathf.Clamp01(m_Data.positionWeight);
            m_Data.rotationWeight = Mathf.Clamp01(m_Data.rotationWeight);
        }
    }
}
