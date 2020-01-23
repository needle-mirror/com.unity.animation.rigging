using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Animations.Rigging;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Utils;
using UnityEngine.Playables;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using System.IO;
using System;

public class RuntimeRiggingTestFixture
{
    public struct RigTestData
    {
        public GameObject rootGO;
        public GameObject hipsGO;
        public GameObject rigGO;

        public Animator animator;
    }

    static public RigTestData SetupRigHierarchy()
    {
        var data = new RigTestData();

        data.rootGO = UnityEngine.Object.Instantiate(Resources.Load("Dude_low")) as GameObject;
        Assert.IsNotNull(data.rootGO, "Could not load rig hierarchy.");

        data.hipsGO = data.rootGO.transform.Find("Reference/Hips").gameObject;
        Assert.IsNotNull(data.hipsGO, "Could not find hips game object in hierarchy.");

        data.rigGO = new GameObject("Rig");
        data.rigGO.transform.parent = data.rootGO.transform;
        var rig = data.rigGO.AddComponent<Rig>();

        data.animator = data.rootGO.GetComponent<Animator>();
        if (data.animator == null)
            data.animator = data.rootGO.AddComponent<Animator>();

        data.animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        data.animator.avatar = null;

        var rigBuilder = data.rootGO.AddComponent<RigBuilder>();
        rigBuilder.layers.Add(new RigLayer(rig));

        return data;
    }

    public static IEnumerator YieldTwoFrames()
    {
        // this is necessary when we changed the constraint weight in a test,
        // because test are executed like coroutine so they are called right after all MonoBehaviour.Update thus missing the RigBuilder.Update
        yield return null;
        yield return null;
    }

    public class Vector3EqualityComparer : IEqualityComparer<Vector3>
    {
        readonly float AllowedError;

        public Vector3EqualityComparer(float allowedError)
        {
            this.AllowedError = allowedError;
        }

        public bool Equals(Vector3 expected, Vector3 actual)
        {
            return Utils.AreFloatsEqualAbsoluteError(expected.x, actual.x, AllowedError) &&
                Utils.AreFloatsEqualAbsoluteError(expected.y, actual.y, AllowedError) &&
                Utils.AreFloatsEqualAbsoluteError(expected.z, actual.z, AllowedError);
        }

        public int GetHashCode(Vector3 vec3)
        {
            return 0;
        }
    }

    public class QuaternionEqualityComparer : IEqualityComparer<Quaternion>
    {
        readonly float AllowedError;

        public QuaternionEqualityComparer(float allowedError)
        {
            this.AllowedError = allowedError;
        }

        public bool Equals(Quaternion expected, Quaternion actual)
        {
            return Mathf.Abs(Quaternion.Angle(expected, actual)) <= AllowedError;
        }

        public int GetHashCode(Quaternion quaternion)
        {
            return 0;
        }
    }

    public class FloatEqualityComparer : IComparer<float>
    {
        readonly float AllowedError;

        public FloatEqualityComparer(float allowedError)
        {
            this.AllowedError = allowedError;
        }

        public int Compare(float x, float y)
        {
            if (Utils.AreFloatsEqualAbsoluteError(x, y, AllowedError))
                return 0;
            if (x < y)
                return -1;

            return 1;
        }

        public int GetHashCode(float value)
        {
            return 0;
        }
    }
}

