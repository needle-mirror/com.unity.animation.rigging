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

public class OverrideTransformEditorTests
{
    [TestCase(OverrideTransformData.Space.World)]
    [TestCase(OverrideTransformData.Space.Local)]
    [TestCase(OverrideTransformData.Space.Pivot)]
    public void OverrideTransform_TransferMotionToSkeleton(OverrideTransformData.Space space)
    {
        var data = OverrideTransformTests.SetupConstraintRig();
        var constraint = data.constraint;

        constraint.data.space = space;

        var rootGO = data.rigData.rootGO;
        var rigBuilder = rootGO.GetComponent<RigBuilder>();

        var constrainedObject = constraint.data.constrainedObject;
        var sourceObject = constraint.data.sourceObject;

        var clip = new AnimationClip();

        var constrainedObjectPath = AnimationUtility.CalculateTransformPath(constrainedObject, rootGO.transform);
        var sourceObjectPath = AnimationUtility.CalculateTransformPath(sourceObject, rootGO.transform);

        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(sourceObjectPath, typeof(Transform), "m_LocalPosition.x"), AnimationCurve.Linear(0f, sourceObject.localPosition.x, 1f, sourceObject.localPosition.y + 0.5f));
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(sourceObjectPath, typeof(Transform), "m_LocalPosition.y"), AnimationCurve.Linear(0f, sourceObject.localPosition.y, 1f, sourceObject.localPosition.y + 2.5f));
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(sourceObjectPath, typeof(Transform), "m_LocalPosition.z"), AnimationCurve.Linear(0f, sourceObject.localPosition.z, 1f, sourceObject.localPosition.y + 4.5f));

        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(sourceObjectPath, typeof(Transform), "localEulerAnglesRaw.x"), AnimationCurve.Linear(0f, -50f, 1f, 50f));
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(sourceObjectPath, typeof(Transform), "localEulerAnglesRaw.y"), AnimationCurve.Constant(0f, 1f, 0f));
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(sourceObjectPath, typeof(Transform), "localEulerAnglesRaw.z"), AnimationCurve.Constant(0f, 1f, 0f));

        RuntimeRiggingEditorTestFixture.TestTransferMotionToSkeleton(constraint, rigBuilder, clip, new Transform[] {constrainedObject}, CompareFlags.TR);
    }
}
