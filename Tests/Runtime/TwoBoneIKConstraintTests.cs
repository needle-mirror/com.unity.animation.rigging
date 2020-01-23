using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.Animations.Rigging;
using NUnit.Framework;
using System.Collections;
using System;

using RigTestData = RuntimeRiggingTestFixture.RigTestData;

public class TwoBoneIKConstraintTests {

    const float k_Epsilon = 0.05f;

    public struct ConstraintTestData
    {
        public RigTestData rigData;
        public TwoBoneIKConstraint constraint;
    }

    public static ConstraintTestData SetupConstraintRig()
    {
        var data = new ConstraintTestData();

        data.rigData = RuntimeRiggingTestFixture.SetupRigHierarchy();

        var twoBoneIKGO = new GameObject("twoBoneIK");
        var twoBoneIK = twoBoneIKGO.AddComponent<TwoBoneIKConstraint>();
        twoBoneIK.Reset();

        twoBoneIKGO.transform.parent = data.rigData.rigGO.transform;

        twoBoneIK.data.root = data.rigData.hipsGO.transform.Find("Chest/LeftArm");
        Assert.IsNotNull(twoBoneIK.data.root, "Could not find root transform");

        twoBoneIK.data.mid = twoBoneIK.data.root.transform.Find("LeftForeArm");
        Assert.IsNotNull(twoBoneIK.data.mid, "Could not find mid transform");

        twoBoneIK.data.tip = twoBoneIK.data.mid.transform.Find("LeftHand");
        Assert.IsNotNull(twoBoneIK.data.tip, "Could not find tip transform");

        var targetGO = new GameObject ("target");
        targetGO.transform.parent = twoBoneIKGO.transform;

        var hintGO = new GameObject ("hint");
        hintGO.transform.parent = twoBoneIKGO.transform;

        twoBoneIK.data.target = targetGO.transform;
        twoBoneIK.data.hint = hintGO.transform;

        data.rigData.rootGO.GetComponent<RigBuilder>().Build();

        targetGO.transform.position = twoBoneIK.data.tip.position;

        data.constraint = twoBoneIK;

        return data;
    }


    [UnityTest]
    public IEnumerator TwoBoneIKConstraint_FollowsTarget()
    {
        var data = SetupConstraintRig();
        var constraint = data.constraint;

        var target = constraint.data.target;
        var tip = constraint.data.tip;
        var root = constraint.data.root;

        var positionComparer = new RuntimeRiggingTestFixture.Vector3EqualityComparer(k_Epsilon);

        for (int i = 0; i < 5; ++i)
        {
            target.position += new Vector3(0f, 0.1f, 0f);
            yield return RuntimeRiggingTestFixture.YieldTwoFrames();

            Vector3 rootToTip = (tip.position - root.position).normalized;
            Vector3 rootToTarget = (target.position - root.position).normalized;

            Assert.That(rootToTip, Is.EqualTo(rootToTarget).Using(positionComparer), String.Format("Expected rootToTip to be {0}, but was {1}", rootToTip, rootToTarget));
        }
    }

    [UnityTest]
    public IEnumerator TwoBoneIKConstraint_UsesHint()
    {
        var data = SetupConstraintRig();
        var constraint = data.constraint;

        var target = constraint.data.target;
        var hint = constraint.data.hint;
        var mid = constraint.data.mid;

        var floatComparer = new RuntimeRiggingTestFixture.FloatEqualityComparer(k_Epsilon);

        Vector3 midPos1 = mid.position;

        // Make left arm flex.
        target.position += new Vector3(0.2f, 0.0f, 0f);

        hint.position = mid.position + new Vector3(0f, 1f, 0f);
        yield return RuntimeRiggingTestFixture.YieldTwoFrames();

        Vector3 midPos2 = mid.position;
        Assert.That(midPos2.y, Is.GreaterThan(midPos1.y).Using(floatComparer), String.Format("Expected mid2.y to be greater than mid1.y"));
        Assert.That(midPos1.z, Is.EqualTo(midPos2.z).Using(floatComparer), String.Format("Expected mid2.z to be {0}, but was {1}", midPos1.z, midPos2.z));

        hint.position = mid.position + new Vector3(0f, -1f, 0f);
        yield return RuntimeRiggingTestFixture.YieldTwoFrames();

        midPos2 = mid.position;
        Assert.That(midPos2.y, Is.LessThan(midPos1.y).Using(floatComparer), String.Format("Expected mid2.y to be lower than mid1.y"));
        Assert.That(midPos1.z, Is.EqualTo(midPos2.z).Using(floatComparer), String.Format("Expected mid2.z to be {0}, but was {1}", midPos1.z, midPos2.z));

        hint.position = mid.position + new Vector3(0f, 0f, 1f);
        yield return RuntimeRiggingTestFixture.YieldTwoFrames();

        midPos2 = mid.position;
        Assert.That(midPos1.y, Is.EqualTo(midPos2.y).Using(floatComparer), String.Format("Expected mid2.y to be {0}, but was {1}", midPos1.y, midPos2.y));
        Assert.That(midPos2.z, Is.GreaterThan(midPos1.z).Using(floatComparer), String.Format("Expected mid2.y to be greater than mid1.y"));

        hint.position = mid.position + new Vector3(0f, 0f, -1f);
        yield return RuntimeRiggingTestFixture.YieldTwoFrames();

        midPos2 = mid.position;
        Assert.That(midPos1.y, Is.EqualTo(midPos2.y).Using(floatComparer), String.Format("Expected mid2.y to be {0}, but was {1}", midPos1.y, midPos2.y));
        Assert.That(midPos2.z, Is.LessThan(midPos1.z).Using(floatComparer), String.Format("Expected mid2.y to be greater than mid1.y"));
    }

    [UnityTest]
    public IEnumerator TwoBoneIKConstraint_ApplyWeight()
    {
        var data = SetupConstraintRig();
        var constraint = data.constraint;

        var tip = constraint.data.tip;
        var target = constraint.data.target;

        Vector3 tipPos1 = tip.position;

        target.position += new Vector3(0f, 0.5f, 0f);
        yield return RuntimeRiggingTestFixture.YieldTwoFrames();

        Vector3 tipPos2 = tip.position;

        var positionComparer = new RuntimeRiggingTestFixture.Vector3EqualityComparer(k_Epsilon);

        for (int i = 0; i <= 5; ++i)
        {
            float w = i / 5.0f;

            constraint.weight = w;
            yield return null;

            Vector3 weightedTipPos = Vector3.Lerp(tipPos1, tipPos2, w);
            Vector3 tipPos = tip.position;

            Assert.That(tipPos, Is.EqualTo(weightedTipPos).Using(positionComparer), String.Format("Expected tip to be {0} for a weight of {1}, but was {2}", weightedTipPos, w, tipPos));
        }
    }

}
