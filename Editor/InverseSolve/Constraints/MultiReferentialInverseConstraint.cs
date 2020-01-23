using UnityEngine.Animations.Rigging;

namespace UnityEditor.Animations.Rigging
{
    [InverseRigConstraint(typeof(MultiReferentialConstraint))]
    public class MultiReferentialInverseConstraint : OverrideRigConstraint<
        MultiReferentialConstraint,
        MultiReferentialInverseConstraintJob,
        MultiReferentialConstraintData,
        MultiReferentialInverseConstraintJobBinder<MultiReferentialConstraintData>
        >
    {
        public MultiReferentialInverseConstraint(MultiReferentialConstraint baseConstraint) : base(baseConstraint) {}
    }
}

