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

public class ChainIKConstraintEditorTests
{
    [Test]
    public void ChainIKConstraint_TransferMotionToSkeleton()
    {
        var data = ChainIKConstraintTests.SetupConstraintRig();
        var constraint = data.constraint;

        var rootGO = data.rigData.rootGO;
        var rigBuilder = rootGO.GetComponent<RigBuilder>();

        var tip = constraint.data.tip;
        var root = constraint.data.root;

        var target = constraint.data.target;

        var clip = new AnimationClip();

        var targetPath = AnimationUtility.CalculateTransformPath(target, rootGO.transform);

        // Add keyframes for twist chain constraint.
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(targetPath, typeof(Transform), "m_LocalPosition.x"), AnimationCurve.Constant(0f, 1f, target.localPosition.x));
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(targetPath, typeof(Transform), "m_LocalPosition.y"), AnimationCurve.Linear(0f, target.localPosition.y, 1f, target.localPosition.y + 0.5f));
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(targetPath, typeof(Transform), "m_LocalPosition.z"), AnimationCurve.Constant(0f, 1f, target.localPosition.z));

        RuntimeRiggingEditorTestFixture.TestTransferMotionToSkeleton(constraint, rigBuilder, clip, ConstraintsUtils.ExtractChain(root, tip), CompareFlags.TR);
    }
}
