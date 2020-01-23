using UnityEngine.Animations.Rigging;

namespace UnityEditor.Animations.Rigging
{
    [InverseRigConstraint(typeof(TwoBoneIKConstraint))]
    public class TwoBoneIKInverseConstraint : OverrideRigConstraint<
        TwoBoneIKConstraint,
        TwoBoneIKInverseConstraintJob,
        TwoBoneIKConstraintData,
        TwoBoneIKInverseConstraintJobBinder<TwoBoneIKConstraintData>
        >
    {
        public TwoBoneIKInverseConstraint(TwoBoneIKConstraint baseConstraint) : base(baseConstraint) {}
    }
}

