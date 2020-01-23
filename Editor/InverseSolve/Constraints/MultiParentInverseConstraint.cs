using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Animations.Rigging;

namespace UnityEditor.Animations.Rigging
{
    [InverseRigConstraint(typeof(MultiParentConstraint))]
    public class MultiParentInverseConstraint : OverrideRigConstraint<
        MultiParentConstraint,
        MultiParentInverseConstraintJob,
        MultiParentConstraintData,
        MultiParentInverseConstraintJobBinder<MultiParentConstraintData>
        >
    {
    #if UNITY_EDITOR
    #pragma warning disable 0414
        [NotKeyable, SerializeField, HideInInspector] bool m_SourceObjectsGUIToggle;
        [NotKeyable, SerializeField, HideInInspector] bool m_SettingsGUIToggle;
    #endif

        public MultiParentInverseConstraint(MultiParentConstraint baseConstraint) : base(baseConstraint) { }
    }
}
