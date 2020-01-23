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

public class TwistChainConstraintEditorTests
{
    [Test]
    public void TwistChainConstraint_TransferMotionToSkeleton()
    {
        var data = TwistChainConstraintTests.SetupConstraintRig();
        var constraint = data.constraint;

        var rootGO = data.rigData.rootGO;
        var rigBuilder = rootGO.GetComponent<RigBuilder>();

        var tip = constraint.data.tip;
        var root = constraint.data.root;

        var tipTarget = constraint.data.tipTarget;
        var rootTarget = constraint.data.rootTarget;

        var clip = new AnimationClip();

        var tipTargetPath = AnimationUtility.CalculateTransformPath(tipTarget, rootGO.transform);
        var rootTargetPath = AnimationUtility.CalculateTransformPath(rootTarget, rootGO.transform);

        // Add keyframes for twist chain constraint.
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(rootTargetPath, typeof(Transform), "localEulerAnglesRaw.x"), AnimationCurve.Constant(0f, 1f, 0f));
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(rootTargetPath, typeof(Transform), "localEulerAnglesRaw.y"), AnimationCurve.Constant(0f, 1f, 0f));
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(rootTargetPath, typeof(Transform), "localEulerAnglesRaw.z"), AnimationCurve.Linear(0f, 0f, 1f, -50f));

        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(tipTargetPath, typeof(Transform), "localEulerAnglesRaw.x"), AnimationCurve.Constant(0f, 1f, 0f));
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(tipTargetPath, typeof(Transform), "localEulerAnglesRaw.y"), AnimationCurve.Constant(0f, 1f, 0f));
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(tipTargetPath, typeof(Transform), "localEulerAnglesRaw.z"), AnimationCurve.Linear(0f, 0f, 1f, 50f));

        RuntimeRiggingEditorTestFixture.TestTransferMotionToSkeleton(constraint, rigBuilder, clip, ConstraintsUtils.ExtractChain(root, tip), CompareFlags.TR);
    }

    [Test]
    public void TwistChainConstraint_TransferMotionToConstraint()
    {
        var data = TwistChainConstraintTests.SetupConstraintRig();
        var constraint = data.constraint;

        var rootGO = data.rigData.rootGO;
        var rigBuilder = rootGO.GetComponent<RigBuilder>();

        var tip = constraint.data.tip;
        var root = constraint.data.root;

        var clip = new AnimationClip();


        // Add keyframes for root and tips.
        Transform[] chain = ConstraintsUtils.ExtractChain(root,tip);

        var rootPath = AnimationUtility.CalculateTransformPath(root, rootGO.transform);
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(rootPath, typeof(Transform), "localEulerAnglesRaw.x"), AnimationCurve.Constant(0f, 1f, 0f));
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(rootPath, typeof(Transform), "localEulerAnglesRaw.y"), AnimationCurve.Constant(0f, 1f, 0f));
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(rootPath, typeof(Transform), "localEulerAnglesRaw.z"), AnimationCurve.Linear(0f, 0f, 1f, -50f));

        var tipPath = AnimationUtility.CalculateTransformPath(tip, rootGO.transform);
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(tipPath, typeof(Transform), "localEulerAnglesRaw.x"), AnimationCurve.Constant(0f, 1f, 0f));
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(tipPath, typeof(Transform), "localEulerAnglesRaw.y"), AnimationCurve.Constant(0f, 1f, 0f));
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(tipPath, typeof(Transform), "localEulerAnglesRaw.z"), AnimationCurve.Linear(0f, 0f, 1f, 50f));

        // Also keyframe inbetween keys
        for (int i = 1; i < chain.Length - 1; ++i)
        {
            var path = AnimationUtility.CalculateTransformPath(chain[i], rootGO.transform);
            AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(path, typeof(Transform), "localEulerAnglesRaw.x"), AnimationCurve.Constant(0f, 1f, 0f));
            AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(path, typeof(Transform), "localEulerAnglesRaw.y"), AnimationCurve.Constant(0f, 1f, 0f));
            AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(path, typeof(Transform), "localEulerAnglesRaw.z"), AnimationCurve.Constant(0f, 1f, 0f));
        }

        RuntimeRiggingEditorTestFixture.TestTransferMotionToConstraint(constraint, rigBuilder, clip, new Transform[] {root, tip}, CompareFlags.Rotation);
    }

}
