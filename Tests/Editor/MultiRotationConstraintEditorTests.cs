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

public class MultiRotationConstraintEditorTests
{
    [Test]
    public void MultiRotationConstraint_TransferMotionToSkeleton()
    {
        var data = MultiRotationConstraintTests.SetupConstraintRig();
        var constraint = data.constraint;

        var rootGO = data.rigData.rootGO;
        var rigBuilder = rootGO.GetComponent<RigBuilder>();

        var constrainedObject = constraint.data.constrainedObject;
        var sources = constraint.data.sourceObjects;

        var clip = new AnimationClip();

        var src0 = sources[0].transform;
        var src1 = sources[1].transform;

        var constraintPath = AnimationUtility.CalculateTransformPath(constraint.transform, rootGO.transform);
        var src0Path = AnimationUtility.CalculateTransformPath(src0, rootGO.transform);
        var src1Path = AnimationUtility.CalculateTransformPath(src1, rootGO.transform);

        var weight0Attribute = ((IMultiRotationConstraintData)constraint.data).sourceObjectsProperty + ".m_Item0.weight";
        var weight1Attribute = ((IMultiRotationConstraintData)constraint.data).sourceObjectsProperty + ".m_Item1.weight";

        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(src0Path, typeof(Transform), "localEulerAnglesRaw.x"), AnimationCurve.Constant(0f, 1f, 0f));
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(src0Path, typeof(Transform), "localEulerAnglesRaw.y"), AnimationCurve.Linear(0f, -50f, 1f, 50f));
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(src0Path, typeof(Transform), "localEulerAnglesRaw.z"), AnimationCurve.Constant(0f, 1f, 0f));

        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(constraintPath, typeof(MultiRotationConstraint), weight0Attribute), AnimationCurve.Linear(0f, 0f, 1f, 1f));

        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(src1Path, typeof(Transform), "localEulerAnglesRaw.x"), AnimationCurve.Linear(0f, -50f, 1f, 50f));
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(src1Path, typeof(Transform), "localEulerAnglesRaw.y"), AnimationCurve.Constant(0f, 1f, 0f));
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(src1Path, typeof(Transform), "localEulerAnglesRaw.z"), AnimationCurve.Constant(0f, 1f, 0f));

        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(constraintPath, typeof(MultiRotationConstraint), weight1Attribute), AnimationCurve.Linear(0f, 1f, 0f, 0f));

        RuntimeRiggingEditorTestFixture.TestTransferMotionToSkeleton(constraint, rigBuilder, clip, new Transform[] {constrainedObject}, CompareFlags.TR);
    }

    [Test]
    public void MultiRotationConstraint_TransferMotionToConstraint(
            [Values(false, true)] bool applySourceOffsets,
            [Values(false, true)] bool applyDrivenOffset)
    {
        var data = MultiRotationConstraintTests.SetupConstraintRig();
        var constraint = data.constraint;

        var rootGO = data.rigData.rootGO;
        var rigBuilder = rootGO.GetComponent<RigBuilder>();

        var constrainedObject = constraint.data.constrainedObject;
        var sources = constraint.data.sourceObjects;

        constraint.data.maintainOffset = applySourceOffsets;
        constraint.data.offset = applyDrivenOffset ? new Vector3(10f, 20f, 30f) : Vector3.zero;
        constraint.data.constrainedXAxis = true;
        constraint.data.constrainedYAxis = true;
        constraint.data.constrainedZAxis = true;

        var src0 = sources[0].transform;
        var src1 = sources[1].transform;

        src0.transform.localRotation *= Quaternion.Euler(new Vector3(45f, 0f, 0f));
        src1.transform.localRotation *= Quaternion.Euler(new Vector3(0f, 0f, 45f));

        var clip = new AnimationClip();

        var constraintPath = AnimationUtility.CalculateTransformPath(constraint.transform, rootGO.transform);
        var constrainedObjectPath = AnimationUtility.CalculateTransformPath(constraint.data.constrainedObject, rootGO.transform);

        var weight0Attribute = ((IMultiRotationConstraintData)constraint.data).sourceObjectsProperty + ".m_Item0.weight";
        var weight1Attribute = ((IMultiRotationConstraintData)constraint.data).sourceObjectsProperty + ".m_Item1.weight";

        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(constrainedObjectPath, typeof(Transform), "localEulerAnglesRaw.x"), AnimationCurve.Linear(0f, -20f, 1f, 20f));
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(constrainedObjectPath, typeof(Transform), "localEulerAnglesRaw.y"), AnimationCurve.Constant(0f, 1f, 0f));
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(constrainedObjectPath, typeof(Transform), "localEulerAnglesRaw.z"), AnimationCurve.Constant(0f, 1f, 0f));

        RuntimeRiggingEditorTestFixture.TestTransferMotionToConstraint(constraint, rigBuilder, clip, new Transform[] {constrainedObject}, CompareFlags.TR);
    }

}
