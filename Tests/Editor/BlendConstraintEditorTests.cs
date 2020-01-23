using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.Animations.Rigging;
using UnityEditor;
using UnityEditor.Animations.Rigging;
using UnityEditor.Animations;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System;

using RigTestData = RuntimeRiggingTestFixture.RigTestData;
using CompareFlags = RuntimeRiggingEditorTestFixture.CompareFlags;

public class BlendConstraintEditorTests
{
    [Test]
    public void BlendConstraint_TransferMotionToSkeleton()
    {
        var data = BlendConstraintTests.SetupConstraintRig();
        var constraint = data.constraint;

        var rootGO = data.rigData.rootGO;
        var rigBuilder = rootGO.GetComponent<RigBuilder>();

        var constrainedObject = constraint.data.constrainedObject;
        var src0 = constraint.data.sourceObjectA;
        var src1 = constraint.data.sourceObjectB;

        var clip = new AnimationClip();

        var constraintPath = AnimationUtility.CalculateTransformPath(constraint.transform, rootGO.transform);
        var src0Path = AnimationUtility.CalculateTransformPath(src0, rootGO.transform);
        var src1Path = AnimationUtility.CalculateTransformPath(src1, rootGO.transform);

        var positionWeightAttribute = ((IBlendConstraintData)constraint.data).positionWeightFloatProperty;
        var rotationWeightAttribute = ((IBlendConstraintData)constraint.data).rotationWeightFloatProperty;

        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(src0Path, typeof(Transform), "m_LocalPosition.x"), AnimationCurve.Constant(0f, 1f, src0.localPosition.x));
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(src0Path, typeof(Transform), "m_LocalPosition.y"), AnimationCurve.Constant(0f, 1f, src0.localPosition.y));
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(src0Path, typeof(Transform), "m_LocalPosition.z"), AnimationCurve.Constant(0f, 1f, src0.localPosition.z));

        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(src1Path, typeof(Transform), "m_LocalPosition.x"), AnimationCurve.Constant(0f, 1f, src1.localPosition.x));
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(src1Path, typeof(Transform), "m_LocalPosition.y"), AnimationCurve.Constant(0f, 1f, src1.localPosition.y));
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(src1Path, typeof(Transform), "m_LocalPosition.z"), AnimationCurve.Constant(0f, 1f, src1.localPosition.z));

        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(constraintPath, typeof(BlendConstraint), positionWeightAttribute), AnimationCurve.Linear(0f, 0f, 1f, 1f));
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(constraintPath, typeof(BlendConstraint), rotationWeightAttribute), AnimationCurve.Linear(0f, 0f, 1f, 1f));

        RuntimeRiggingEditorTestFixture.TestTransferMotionToSkeleton(constraint, rigBuilder, clip, new Transform[] {constrainedObject}, CompareFlags.TR);
    }
}
