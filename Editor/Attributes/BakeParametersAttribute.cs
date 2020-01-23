using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace UnityEditor.Animations.Rigging
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class BakeParametersAttribute : Attribute
    {
        public BakeParametersAttribute(Type constraintType)
        {
            if (constraintType == null || !typeof(IRigConstraint).IsAssignableFrom(constraintType))
                Debug.LogError("Invalid constraint for InverseRigConstraint attribute.");

            this.constraintType = constraintType;
        }

        public Type constraintType { get; }
    }

    public abstract class BakeParameters<T> : IBakeParameters
        where T : IRigConstraint
    {
        public abstract bool canBakeToSkeleton { get; }
        public abstract bool canBakeToConstraint { get; }

        public abstract IEnumerable<EditorCurveBinding> GetSourceCurveBindings(RigBuilder rigBuilder, T constraint);
        public abstract IEnumerable<EditorCurveBinding> GetConstrainedCurveBindings(RigBuilder rigBuilder, T constraint);

        bool IBakeParameters.canBakeToSkeleton => canBakeToSkeleton;
        bool IBakeParameters.canBakeToConstraint => canBakeToConstraint;

        IEnumerable<EditorCurveBinding> IBakeParameters.GetSourceCurveBindings(RigBuilder rigBuilder, IRigConstraint constraint)
        {
            Debug.Assert(constraint is T);
            T tConstraint = (T)constraint;
            return GetSourceCurveBindings(rigBuilder, tConstraint);
        }

        IEnumerable<EditorCurveBinding> IBakeParameters.GetConstrainedCurveBindings(RigBuilder rigBuilder, IRigConstraint constraint)
        {
            Debug.Assert(constraint is T);
            T tConstraint = (T)constraint;
            return GetConstrainedCurveBindings(rigBuilder, tConstraint);
        }
    }

    public interface IBakeParameters
    {
        bool canBakeToSkeleton { get; }
        bool canBakeToConstraint { get; }

        IEnumerable<EditorCurveBinding> GetSourceCurveBindings(RigBuilder rigBuilder, IRigConstraint constraint);
        IEnumerable<EditorCurveBinding> GetConstrainedCurveBindings(RigBuilder rigBuilder, IRigConstraint constraint);
    }

}
