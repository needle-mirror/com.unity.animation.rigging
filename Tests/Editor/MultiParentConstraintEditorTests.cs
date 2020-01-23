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

public class MultiParentConstraintEditorTests
{
    [Test]
    public void MultiParentConstraint_TransferMotionToSkeleton()
    {
        var data = MultiParentConstraintTests.SetupConstraintRig();
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

        var weight0Attribute = ((IMultiParentConstraintData)constraint.data).sourceObjectsProperty + ".m_Item0.weight";
        var weight1Attribute = ((IMultiParentConstraintData)constraint.data).sourceObjectsProperty + ".m_Item1.weight";

        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(src0Path, typeof(Transform), "m_LocalPosition.x"), AnimationCurve.Constant(0f, 1f, src0.localPosition.x));
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(src0Path, typeof(Transform), "m_LocalPosition.y"), AnimationCurve.Linear(0f, src0.localPosition.y, 1f, src0.localPosition.y + 0.5f));
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(src0Path, typeof(Transform), "m_LocalPosition.z"), AnimationCurve.Constant(0f, 1f, src0.localPosition.z));

        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(src0Path, typeof(Transform), "localEulerAnglesRaw.x"), AnimationCurve.Constant(0f, 1f, 0f));
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(src0Path, typeof(Transform), "localEulerAnglesRaw.y"), AnimationCurve.Linear(0f, -50f, 1f, 50f));
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(src0Path, typeof(Transform), "localEulerAnglesRaw.z"), AnimationCurve.Constant(0f, 1f, 0f));

        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(constraintPath, typeof(MultiParentConstraint), weight0Attribute), AnimationCurve.Linear(0f, 0f, 1f, 1f));

        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(src1Path, typeof(Transform), "m_LocalPosition.x"), AnimationCurve.Linear(0f, src1.localPosition.x, 1f, src1.localPosition.x + 0.5f));
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(src1Path, typeof(Transform), "m_LocalPosition.y"), AnimationCurve.Constant(0f, 1f, src1.localPosition.y));
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(src1Path, typeof(Transform), "m_LocalPosition.z"), AnimationCurve.Constant(0f, 1f, src1.localPosition.z));

        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(src1Path, typeof(Transform), "localEulerAnglesRaw.x"), AnimationCurve.Linear(0f, -50f, 1f, 50f));
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(src1Path, typeof(Transform), "localEulerAnglesRaw.y"), AnimationCurve.Constant(0f, 1f, 0f));
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(src1Path, typeof(Transform), "localEulerAnglesRaw.z"), AnimationCurve.Constant(0f, 1f, 0f));

        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(constraintPath, typeof(MultiParentConstraint), weight1Attribute), AnimationCurve.Linear(0f, 1f, 0f, 0f));

        RuntimeRiggingEditorTestFixture.TestTransferMotionToSkeleton(constraint, rigBuilder, clip, new Transform[] {constrainedObject}, CompareFlags.TR);
    }

    [Test]
    public void MultiParentConstraint_TransferMotionToConstraint(
            [Values(false, true)] bool applySourcePositionOffsets,
            [Values(false, true)] bool applySourceRotationOffsets,
            [Values(AxesMask.X, AxesMask.XY, AxesMask.XYZ)] AxesMask constrainedPositionAxesMask,
            [Values(AxesMask.X, AxesMask.XY, AxesMask.XYZ)] AxesMask constrainedRotationAxesMask)
    {
        var data = MultiParentConstraintTests.SetupConstraintRig();
        var constraint = data.constraint;

        var rootGO = data.rigData.rootGO;
        var rigBuilder = rootGO.GetComponent<RigBuilder>();

        var constrainedObject = constraint.data.constrainedObject;
        var sources = constraint.data.sourceObjects;

        constraint.data.maintainPositionOffset = applySourcePositionOffsets;
        constraint.data.maintainRotationOffset = applySourceRotationOffsets;
        constraint.data.constrainedPositionXAxis = RuntimeRiggingEditorTestFixture.TestXAxis(constrainedPositionAxesMask);
        constraint.data.constrainedPositionYAxis = RuntimeRiggingEditorTestFixture.TestYAxis(constrainedPositionAxesMask);
        constraint.data.constrainedPositionZAxis = RuntimeRiggingEditorTestFixture.TestZAxis(constrainedPositionAxesMask);
        constraint.data.constrainedRotationXAxis = RuntimeRiggingEditorTestFixture.TestXAxis(constrainedRotationAxesMask);
        constraint.data.constrainedRotationYAxis = RuntimeRiggingEditorTestFixture.TestYAxis(constrainedRotationAxesMask);
        constraint.data.constrainedRotationZAxis = RuntimeRiggingEditorTestFixture.TestZAxis(constrainedRotationAxesMask);

        var src0 = sources[0].transform;
        var src1 = sources[1].transform;

        src0.transform.position += new Vector3(3f, 6f, 9f);
        src1.transform.position += new Vector3(9f, 6f, 3f);

        src0.transform.rotation *= Quaternion.Euler(90f, 60f, 30f);
        src1.transform.rotation *= Quaternion.Euler(30f, 60f, 90f);

        var clip = new AnimationClip();

        var constraintPath = AnimationUtility.CalculateTransformPath(constraint.transform, rootGO.transform);
        var constrainedObjectPath = AnimationUtility.CalculateTransformPath(constraint.data.constrainedObject, rootGO.transform);

        var weight0Attribute = ((IMultiParentConstraintData)constraint.data).sourceObjectsProperty + ".m_Item0.weight";
        var weight1Attribute = ((IMultiParentConstraintData)constraint.data).sourceObjectsProperty + ".m_Item1.weight";

        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(constraintPath, typeof(MultiParentConstraint), weight0Attribute), AnimationCurve.Constant(0f, 1f, 0.5f));
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(constraintPath, typeof(MultiParentConstraint), weight1Attribute), AnimationCurve.Constant(0f, 1f, 0.5f));

        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(constrainedObjectPath, typeof(Transform), "m_LocalPosition.x"), AnimationCurve.Linear(0f, -0.5f, 1f, 0.5f));
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(constrainedObjectPath, typeof(Transform), "m_LocalPosition.y"), AnimationCurve.Linear(0f, -1.5f, 1f, 1.5f));
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(constrainedObjectPath, typeof(Transform), "m_LocalPosition.z"), AnimationCurve.Linear(0f, -2.5f, 1f, 2.5f));

        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(constrainedObjectPath, typeof(Transform), "localEulerAnglesRaw.x"), AnimationCurve.Linear(0f, -20f, 1f, 20f));
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(constrainedObjectPath, typeof(Transform), "localEulerAnglesRaw.y"), AnimationCurve.Constant(0f, 1f, 0f));
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(constrainedObjectPath, typeof(Transform), "localEulerAnglesRaw.z"), AnimationCurve.Constant(0f, 1f, 0f));

        RuntimeRiggingEditorTestFixture.TestTransferMotionToConstraint(constraint, rigBuilder, clip, new Transform[] {constrainedObject}, CompareFlags.TR);
    }

}
