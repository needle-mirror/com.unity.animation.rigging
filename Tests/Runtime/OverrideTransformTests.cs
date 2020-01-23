using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.Animations.Rigging;
using NUnit.Framework;
using System.Collections;
using System;

using RigTestData = RuntimeRiggingTestFixture.RigTestData;

public class OverrideTransformTests
{
    const float k_Epsilon = 1e-5f;

    public struct ConstraintTestData
    {
        public RigTestData rigData;
        public OverrideTransform constraint;
    }

    public static ConstraintTestData SetupConstraintRig()
    {
        var data = new ConstraintTestData();

        data.rigData = RuntimeRiggingTestFixture.SetupRigHierarchy();

        var overrideTransformGO = new GameObject("overrideTransform");
        var overrideTransform = overrideTransformGO.AddComponent<OverrideTransform>();
        overrideTransform.Reset();

        overrideTransformGO.transform.parent = data.rigData.rigGO.transform;
        overrideTransform.data.constrainedObject = data.rigData.hipsGO.transform.Find("Chest");

        var overrideSourceGO = new GameObject ("source");
        overrideSourceGO.transform.parent = overrideTransformGO.transform;

        overrideTransform.data.sourceObject = overrideSourceGO.transform;

        data.rigData.rootGO.GetComponent<RigBuilder>().Build();

        data.constraint = overrideTransform;

        return data;
    }

    [UnityTest]
    public IEnumerator OverrideTransform_FollowsSource_WorldSpace()
    {
        var data = SetupConstraintRig();
        var constraint = data.constraint;

        constraint.data.space = OverrideTransformData.Space.World;
        yield return null;

        var constrainedTransform = constraint.data.constrainedObject;
        var sourceTransform = constraint.data.sourceObject;

        var positionComparer = new RuntimeRiggingTestFixture.Vector3EqualityComparer(k_Epsilon);

        for (int i = 0; i < 5; ++i)
        {
            sourceTransform.position += new Vector3(0f, 0.1f, 0.0f);
            yield return null;

            Vector3 sourcePosition = sourceTransform.position;
            Vector3 constrainedPosition = constrainedTransform.position;

            Assert.That(sourcePosition, Is.EqualTo(constrainedPosition).Using(positionComparer), String.Format("Expected constrainedPosition.x to be {0}, but was {1}", sourcePosition, constrainedPosition));
        }
    }

    [UnityTest]
    public IEnumerator OverrideTransform_FollowsSource_PivotSpace()
    {
        var data = SetupConstraintRig();
        var constraint = data.constraint;

        var constrainedTransform = constraint.data.constrainedObject;
        var sourceTransform = constraint.data.sourceObject;

        var positionComparer = new RuntimeRiggingTestFixture.Vector3EqualityComparer(k_Epsilon);

        Vector3 originalPosition = constrainedTransform.position;

        constraint.data.space = OverrideTransformData.Space.Pivot;
        yield return null;

        for (int i = 0; i < 5; ++i)
        {
            sourceTransform.position += new Vector3(0f, 0.1f, 0.0f);
            yield return null;

            Vector3 sourcePosition = sourceTransform.position;
            Vector3 constrainedPosition = constrainedTransform.position;
            Vector3 expectedPosition = sourcePosition + originalPosition;

            Assert.That(expectedPosition, Is.EqualTo(constrainedPosition).Using(positionComparer), String.Format("Expected constrainedPosition.x to be {0}, but was {1}", expectedPosition, constrainedPosition));
        }
    }

    [UnityTest]
    public IEnumerator OverrideTransform_FollowsSource_LocalSpace()
    {
        var data = SetupConstraintRig();
        var constraint = data.constraint;

        var constrainedTransform = constraint.data.constrainedObject;
        var sourceTransform = constraint.data.sourceObject;

        var positionComparer = new RuntimeRiggingTestFixture.Vector3EqualityComparer(k_Epsilon);

        Vector3 parentPosition = constrainedTransform.parent.position;

        constraint.data.space = OverrideTransformData.Space.Local;
        yield return null;

        for (int i = 0; i < 5; ++i)
        {
            sourceTransform.position += new Vector3(0f, 0.1f, 0.0f);
            yield return null;

            Vector3 sourcePosition = sourceTransform.position;
            Vector3 constrainedPosition = constrainedTransform.position;
            Vector3 expectedPosition = sourcePosition + parentPosition;

            Assert.That(expectedPosition, Is.EqualTo(constrainedPosition).Using(positionComparer), String.Format("Expected constrainedPosition.x to be {0}, but was {1}", expectedPosition, constrainedPosition));
        }
    }

    [UnityTest]
    public IEnumerator OverrideTransform_ApplyWeight()
    {
        var data = SetupConstraintRig();
        var constraint = data.constraint;

        var constrainedTransform = constraint.data.constrainedObject;
        var sourceTransform = constraint.data.sourceObject;

        var positionComparer = new RuntimeRiggingTestFixture.Vector3EqualityComparer(k_Epsilon);

        Vector3 constrainedPos1 = constrainedTransform.position;

        constraint.data.space = OverrideTransformData.Space.World;
        yield return null;

        sourceTransform.position = new Vector3(0f, 0.5f, 0f);
        yield return null;

        Vector3 constrainedPos2 = constrainedTransform.position;

        for (int i = 0; i <= 5; ++i)
        {
            float w = i / 5.0f;

            data.constraint.weight = w;

            yield return RuntimeRiggingTestFixture.YieldTwoFrames();

            Vector3 weightedConstrainedPos = Vector3.Lerp(constrainedPos1, constrainedPos2, w);
            Vector3 constrainedPos = constrainedTransform.position;

            Assert.That(weightedConstrainedPos, Is.EqualTo(constrainedPos).Using(positionComparer), String.Format("Expected constrainedPos.x to be {0} for a weight of {1}, but was {2}", weightedConstrainedPos, w, constrainedPos));
        }
    }

}
