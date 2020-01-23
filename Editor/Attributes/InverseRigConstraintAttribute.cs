using System;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace UnityEditor.Animations.Rigging
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class InverseRigConstraintAttribute : Attribute
    {
        public InverseRigConstraintAttribute(Type targetBinderType)
        {
            if (targetBinderType == null || !typeof(IRigConstraint).IsAssignableFrom(targetBinderType))
                Debug.LogError("Invalid constraint for InverseRigConstraint attribute.");

            this.baseConstraint = targetBinderType;
        }

        public Type baseConstraint { get; }
    }
}
