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
using AxesMask = RuntimeRiggingEditorTestFixture.AxesMask;

public class MultiPositionConstraintEditorTests
{
    [Test]
    public void MultiPositionConstraint_TransferMotionToSkeleton()
    {
        var data = MultiPositionConstraintTests.SetupConstraintRig();
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

        var weight0Attribute = ((IMultiPositionConstraintData)constraint.data).sourceObjectsProperty + ".m_Item0.weight";
        var weight1Attribute = ((IMultiPositionConstraintData)constraint.data).sourceObjectsProperty + ".m_Item1.weight";

        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(src0Path, typeof(Transform), "m_LocalPosition.x"), AnimationCurve.Constant(0f, 1f, src0.localPosition.x));
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(src0Path, typeof(Transform), "m_LocalPosition.y"), AnimationCurve.Linear(0f, src0.localPosition.y, 1f, src0.localPosition.y + 0.5f));
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(src0Path, typeof(Transform), "m_LocalPosition.z"), AnimationCurve.Constant(0f, 1f, src0.localPosition.z));

        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(constraintPath, typeof(MultiPositionConstraint), weight0Attribute), AnimationCurve.Linear(0f, 0f, 1f, 1f));

        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(src1Path, typeof(Transform), "m_LocalPosition.x"), AnimationCurve.Linear(0f, src1.localPosition.x, 1f, src1.localPosition.x + 0.5f));
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(src1Path, typeof(Transform), "m_LocalPosition.y"), AnimationCurve.Constant(0f, 1f, src1.localPosition.y));
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(src1Path, typeof(Transform), "m_LocalPosition.z"), AnimationCurve.Constant(0f, 1f, src1.localPosition.z));

        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(constraintPath, typeof(MultiPositionConstraint), weight1Attribute), AnimationCurve.Linear(0f, 1f, 0f, 0f));

        RuntimeRiggingEditorTestFixture.TestTransferMotionToSkeleton(constraint, rigBuilder, clip, new Transform[] {constrainedObject}, CompareFlags.TR);
    }

    [Test]
    public void MultiPositionConstraint_TransferMotionToConstraint(
            [Values(false, true)] bool applySourceOffsets,
            [Values(false, true)] bool applyDrivenOffset,
            [Values(AxesMask.X, AxesMask.XY, AxesMask.XYZ)] AxesMask axesMask)
    {
        var data = MultiPositionConstraintTests.SetupConstraintRig();
        var constraint = data.constraint;

        var rootGO = data.rigData.rootGO;
        var rigBuilder = rootGO.GetComponent<RigBuilder>();

        var constrainedObject = constraint.data.constrainedObject;
        var sources = constraint.data.sourceObjects;

        var vectorMask = new Vector3(
                System.Convert.ToSingle(RuntimeRiggingEditorTestFixture.TestXAxis(axesMask)),
                System.Convert.ToSingle(RuntimeRiggingEditorTestFixture.TestYAxis(axesMask)),
                System.Convert.ToSingle(RuntimeRiggingEditorTestFixture.TestZAxis(axesMask))
                );

        constraint.data.maintainOffset = applySourceOffsets;
        constraint.data.offset = applyDrivenOffset ? AnimationRuntimeUtils.Select(Vector3.zero, new Vector3(1f, 2f, 3f), vectorMask) : Vector3.zero;
        constraint.data.constrainedXAxis = RuntimeRiggingEditorTestFixture.TestXAxis(axesMask);
        constraint.data.constrainedYAxis = RuntimeRiggingEditorTestFixture.TestYAxis(axesMask);
        constraint.data.constrainedZAxis = RuntimeRiggingEditorTestFixture.TestZAxis(axesMask);

        var src0 = sources[0].transform;
        var src1 = sources[1].transform;

        src0.transform.position += Vector3.right;
        src1.transform.position += Vector3.up;

        var clip = new AnimationClip();

        var constraintPath = AnimationUtility.CalculateTransformPath(constraint.transform, rootGO.transform);
        var constrainedObjectPath = AnimationUtility.CalculateTransformPath(constraint.data.constrainedObject, rootGO.transform);

        var weight0Attribute = ((IMultiPositionConstraintData)constraint.data).sourceObjectsProperty + ".m_Item0.weight";
        var weight1Attribute = ((IMultiPositionConstraintData)constraint.data).sourceObjectsProperty + ".m_Item1.weight";

        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(constraintPath, typeof(MultiPositionConstraint), weight0Attribute), AnimationCurve.Constant(0f, 1f, 0.5f));
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(constraintPath, typeof(MultiPositionConstraint), weight1Attribute), AnimationCurve.Constant(0f, 1f, 0.5f));

        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(constrainedObjectPath, typeof(Transform), "m_LocalPosition.x"), AnimationCurve.Linear(0f, -0.5f, 1f, 0.5f));
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(constrainedObjectPath, typeof(Transform), "m_LocalPosition.y"), AnimationCurve.Linear(0f, -1.5f, 1f, 1.5f));
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(constrainedObjectPath, typeof(Transform), "m_LocalPosition.z"), AnimationCurve.Linear(0f, -2.5f, 1f, 2.5f));

        RuntimeRiggingEditorTestFixture.TestTransferMotionToConstraint(constraint, rigBuilder, clip, new Transform[] {constrainedObject}, CompareFlags.TR);
    }

}
