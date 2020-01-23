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

public class MultiReferentialConstraintEditorTests
{
    [Test]
    public void MultiReferentialConstraint_TransferMotionToSkeleton()
    {
        var data = MultiReferentialConstraintTests.SetupConstraintRig();
        var constraint = data.constraint;

        var rootGO = data.rigData.rootGO;
        var rigBuilder = rootGO.GetComponent<RigBuilder>();

        var sources = constraint.data.sourceObjects;

        var clip = new AnimationClip();

        var src0 = sources[0];
        var src1 = sources[1];
        var src2 = sources[2];

        var constraintPath = AnimationUtility.CalculateTransformPath(constraint.transform, rootGO.transform);
        var src0Path = AnimationUtility.CalculateTransformPath(src0, rootGO.transform);
        var src1Path = AnimationUtility.CalculateTransformPath(src1, rootGO.transform);
        var src2Path = AnimationUtility.CalculateTransformPath(src2, rootGO.transform);

        var driverAttribute = ((IMultiReferentialConstraintData)constraint.data).driverIntProperty;

        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(src0Path, typeof(Transform), "m_LocalPosition.x"), AnimationCurve.Linear(0f, src0.localPosition.x, 1f, src0.localPosition.x + 0.5f));
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(src0Path, typeof(Transform), "m_LocalPosition.y"), AnimationCurve.Constant(0f, 1f, src0.localPosition.y));
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(src0Path, typeof(Transform), "m_LocalPosition.z"), AnimationCurve.Constant(0f, 1f, src0.localPosition.z));

        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(src0Path, typeof(Transform), "localEulerAnglesRaw.x"), AnimationCurve.Linear(0f, -50f, 1f, 50f));
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(src0Path, typeof(Transform), "localEulerAnglesRaw.y"), AnimationCurve.Constant(0f, 1f, 0f));
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(src0Path, typeof(Transform), "localEulerAnglesRaw.z"), AnimationCurve.Constant(0f, 1f, 0f));

        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(src1Path, typeof(Transform), "m_LocalPosition.x"), AnimationCurve.Constant(0f, 1f, src1.localPosition.x));
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(src1Path, typeof(Transform), "m_LocalPosition.y"), AnimationCurve.Linear(0f, src1.localPosition.y, 1f, src1.localPosition.y + 1.5f));
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(src1Path, typeof(Transform), "m_LocalPosition.z"), AnimationCurve.Constant(0f, 1f, src1.localPosition.z));

        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(src1Path, typeof(Transform), "localEulerAnglesRaw.x"), AnimationCurve.Constant(0f, 1f, 0f));
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(src1Path, typeof(Transform), "localEulerAnglesRaw.y"), AnimationCurve.Linear(0f, -40f, 1f, 40f));
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(src1Path, typeof(Transform), "localEulerAnglesRaw.z"), AnimationCurve.Constant(0f, 1f, 0f));

        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(src2Path, typeof(Transform), "m_LocalPosition.x"), AnimationCurve.Constant(0f, 1f, src2.localPosition.x));
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(src2Path, typeof(Transform), "m_LocalPosition.y"), AnimationCurve.Constant(0f, 1f, src2.localPosition.y));
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(src2Path, typeof(Transform), "m_LocalPosition.z"), AnimationCurve.Linear(0f, src2.localPosition.z, 1f, src2.localPosition.z + 2.5f));

        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(src2Path, typeof(Transform), "localEulerAnglesRaw.x"), AnimationCurve.Constant(0f, 1f, 0f));
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(src2Path, typeof(Transform), "localEulerAnglesRaw.y"), AnimationCurve.Constant(0f, 1f, 0f));
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(src2Path, typeof(Transform), "localEulerAnglesRaw.z"), AnimationCurve.Linear(0f, -30f, 1f, 30f));

        var driverCurve = new AnimationCurve(new Keyframe[] {new Keyframe(0f, 0f), new Keyframe(0.33f, 1f, Mathf.Infinity, Mathf.Infinity), new Keyframe(0.66f, 2f, Mathf.Infinity, Mathf.Infinity), new Keyframe(1f, 2f)});
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(constraintPath, typeof(MultiReferentialConstraint), driverAttribute), driverCurve);

        RuntimeRiggingEditorTestFixture.TestTransferMotionToSkeleton(constraint, rigBuilder, clip, new Transform[] {src0}, CompareFlags.TR);
    }

    [Test]
    public void MultiReferentialConstraint_TransferMotionToConstraint()
    {
        var data = MultiReferentialConstraintTests.SetupConstraintRig();
        var constraint = data.constraint;

        var rootGO = data.rigData.rootGO;
        var rigBuilder = rootGO.GetComponent<RigBuilder>();

        var sources = constraint.data.sourceObjects;

        var clip = new AnimationClip();

        var src0 = sources[0];

        var constraintPath = AnimationUtility.CalculateTransformPath(constraint.transform, rootGO.transform);
        var src0Path = AnimationUtility.CalculateTransformPath(src0, rootGO.transform);

        var driverAttribute = ((IMultiReferentialConstraintData)constraint.data).driverIntProperty;

        var driverCurve = new AnimationCurve(new Keyframe[] {new Keyframe(0f, 0f), new Keyframe(0.33f, 1f, Mathf.Infinity, Mathf.Infinity), new Keyframe(0.66f, 2f, Mathf.Infinity, Mathf.Infinity), new Keyframe(1f, 2f)});
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(constraintPath, typeof(MultiReferentialConstraint), driverAttribute), driverCurve);

        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(src0Path, typeof(Transform), "m_LocalPosition.x"), AnimationCurve.Linear(0f, -0.5f, 1f, 0.5f));
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(src0Path, typeof(Transform), "m_LocalPosition.y"), AnimationCurve.Linear(0f, -1.5f, 1f, 1.5f));
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(src0Path, typeof(Transform), "m_LocalPosition.z"), AnimationCurve.Linear(0f, -2.5f, 1f, 2.5f));

        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(src0Path, typeof(Transform), "localEulerAnglesRaw.x"), AnimationCurve.Linear(0f, -20f, 1f, 20f));
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(src0Path, typeof(Transform), "localEulerAnglesRaw.y"), AnimationCurve.Constant(0f, 1f, 0f));
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(src0Path, typeof(Transform), "localEulerAnglesRaw.z"), AnimationCurve.Constant(0f, 1f, 0f));

        RuntimeRiggingEditorTestFixture.TestTransferMotionToConstraint(constraint, rigBuilder, clip, new Transform[] {src0}, CompareFlags.TR);
    }

}
