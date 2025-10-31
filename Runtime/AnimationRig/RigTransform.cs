namespace UnityEngine.Animations.Rigging
{
    /// <summary>
    /// RigTransform is a marker component used to identify Transforms that are part of a rig.
    /// This is sometimes required when a Transform is not referenced by any constraint directly
    /// but still affect constraints evaluation.
    /// </summary>
    [DisallowMultipleComponent, AddComponentMenu("Animation Rigging/Setup/Rig Transform")]
    [HelpURL("https://docs.unity3d.com/Packages/com.unity.animation.rigging@1.4/manual/RiggingWorkflow.html#rig-transform")]
    public class RigTransform : MonoBehaviour
    {
    }
}
