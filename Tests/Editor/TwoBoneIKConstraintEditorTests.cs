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

public class TwoBoneIKConstraintEditorTests
{
    [Test]
    public void TwoBoneIKConstraint_TransferMotionToSkeleton()
    {
        var data = TwoBoneIKConstraintTests.SetupConstraintRig();
        var constraint = data.constraint;

        var rootGO = data.rigData.rootGO;
        var rigBuilder = rootGO.GetComponent<RigBuilder>();

        var tip = constraint.data.tip;
        var mid = constraint.data.mid;
        var root = constraint.data.root;

        var target = constraint.data.target;

        var clip = new AnimationClip();

        var targetPath = AnimationUtility.CalculateTransformPath(target, rootGO.transform);

        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(targetPath, typeof(Transform), "m_LocalPosition.x"), AnimationCurve.Constant(0f, 1f, target.localPosition.x));
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(targetPath, typeof(Transform), "m_LocalPosition.y"), AnimationCurve.Linear(0f, target.localPosition.y, 1f, target.localPosition.y + 0.5f));
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(targetPath, typeof(Transform), "m_LocalPosition.z"), AnimationCurve.Constant(0f, 1f, target.localPosition.z));

        RuntimeRiggingEditorTestFixture.TestTransferMotionToSkeleton(constraint, rigBuilder, clip, new Transform[] { tip, mid, root }, CompareFlags.TR);
    }

    [Test]
    public void TwoBoneIKConstraint_TransferMotionToConstraint(
            [Values(false, true)] bool applyTargetPositionOffset,
            [Values(false, true)] bool applyTargetRotationOffset,
            [Values(1f)] float targetPositionWeight,
            [Values(1f)] float targetRotationWeight)
    {
        var data = TwoBoneIKConstraintTests.SetupConstraintRig();
        var constraint = data.constraint;

        var rootGO = data.rigData.rootGO;
        var rigBuilder = rootGO.GetComponent<RigBuilder>();

        var tip = constraint.data.tip;
        var mid = constraint.data.mid;
        var root = constraint.data.root;

        var target = constraint.data.target;

        constraint.data.maintainTargetPositionOffset = applyTargetPositionOffset;
        constraint.data.maintainTargetRotationOffset = applyTargetRotationOffset;

        constraint.data.targetPositionWeight = targetPositionWeight;
        constraint.data.targetRotationWeight = targetRotationWeight;

        target.position += new Vector3(9f, 6f, 3f);
        target.rotation *= Quaternion.Euler(90f, 60f, 30f);

        var clip = new AnimationClip();

        var rootPath = AnimationUtility.CalculateTransformPath(root, rootGO.transform);
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(rootPath, typeof(Transform), "localEulerAnglesRaw.x"), AnimationCurve.Constant(0f, 1f, 0f));
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(rootPath, typeof(Transform), "localEulerAnglesRaw.y"), AnimationCurve.Constant(0f, 1f, 0f));
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(rootPath, typeof(Transform), "localEulerAnglesRaw.z"), AnimationCurve.Linear(0f, 0f, 1f, 20f));

        var midPath = AnimationUtility.CalculateTransformPath(mid, rootGO.transform);
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(midPath, typeof(Transform), "localEulerAnglesRaw.x"), AnimationCurve.Constant(0f, 1f, 0f));
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(midPath, typeof(Transform), "localEulerAnglesRaw.y"), AnimationCurve.Constant(0f, 1f, 0f));
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(midPath, typeof(Transform), "localEulerAnglesRaw.z"), AnimationCurve.Linear(0f, 0f, 1f, 20f));

        var tipPath = AnimationUtility.CalculateTransformPath(tip, rootGO.transform);
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(tipPath, typeof(Transform), "localEulerAnglesRaw.x"), AnimationCurve.Constant(0f, 1f, 0f));
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(tipPath, typeof(Transform), "localEulerAnglesRaw.y"), AnimationCurve.Constant(0f, 1f, 0f));
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(tipPath, typeof(Transform), "localEulerAnglesRaw.z"), AnimationCurve.Linear(0f, 0f, 1f, 20f));

        RuntimeRiggingEditorTestFixture.TestTransferMotionToConstraint(constraint, rigBuilder, clip, new Transform[] {tip, mid, root}, CompareFlags.TR);
    }

    [Test]
    public void TwoBoneIKConstraint_TransferMotionToConstraint_TargetWeights(
        [Values(0f, .25f, .5f, .75f)] float targetPositionWeight,
        [Values(0f, .25f, .5f, .75f)] float targetRotationWeight)
    {
        TwoBoneIKConstraint_TransferMotionToConstraint(true, true, targetPositionWeight, targetRotationWeight);
    }
}
