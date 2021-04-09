using System.Collections.Generic;

namespace UnityEngine.Animations.Rigging
{
    /// <summary>
    /// The BoneRenderer component is responsible for displaying pickable bones in the Scene View.
    /// This component does nothing during runtime.
    /// </summary>
    [ExecuteInEditMode]
    [AddComponentMenu("Animation Rigging/Setup/Bone Renderer")]
    [HelpURL("https://docs.unity3d.com/Packages/com.unity.animation.rigging@1.1/manual/RiggingWorkflow.html#bone-renderer-component")]
    public class BoneRenderer : MonoBehaviour
    {
        /// <summary>
        /// Shape used by individual bones.
        /// </summary>
        public enum BoneShape
        {
            /// <summary>Bones are rendered with single lines.</summary>
            Line,

            /// <summary>Bones are rendered with pyramid shapes.</summary>
            Pyramid,

            /// <summary>Bones are rendered with box shapes.</summary>
            Box
        };

        /// <summary>Shape of the bones.</summary>
        public BoneShape boneShape = BoneShape.Pyramid;

        /// <summary>Toggles whether to render bone shapes or not.</summary>
        public bool drawBones = true;

        /// <summary>Toggles whether to draw tripods on bones or not.</summary>
        public bool drawTripods = false;

        /// <summary>Size of the bones.</summary>
        [Range(0.01f, 5.0f)] public float boneSize = 1.0f;

        /// <summary>Size of the tripod axis.</summary>
        [Range(0.01f, 5.0f)] public float tripodSize = 1.0f;

        /// <summary>Color of the bones.</summary>
        public Color boneColor = new Color(0f, 0f, 1f, 0.5f);

        [SerializeField] private Transform[] m_Transforms;

        /// <summary>Transform references in the BoneRenderer hierarchy that are used to build bones.</summary>
        public Transform[] transforms
        {
            get { return m_Transforms; }
#if UNITY_EDITOR
            set
            {
                m_Transforms = value;
                ExtractBones();
            }
#endif
        }

#if UNITY_EDITOR
        /// <summary>
        /// Bone described by two Transform references.
        /// </summary>
        public struct TransformPair
        {
            public Transform first;
            public Transform second;
        };

        private TransformPair[] m_Bones;
        private Transform[] m_Tips;

        /// <summary>Retrieves the bones isolated from the Transform references.</summary>
        /// <seealso cref="BoneRenderer.transforms"/>
        public TransformPair[] bones
        {
            get => m_Bones;
        }

        /// <summary>Retrieves the tip bones isolated from the Transform references.</summary>
        /// <seealso cref="BoneRenderer.transforms"/>
        public Transform[] tips
        {
            get => m_Tips;
        }

        /// <summary>
        /// Delegate function that covers a BoneRenderer calling OnEnable.
        /// </summary>
        /// <param name="boneRenderer">The BoneRenderer component</param>
        public delegate void OnAddBoneRendererCallback(BoneRenderer boneRenderer);

        /// <summary>
        /// Delegate function that covers a BoneRenderer calling OnDisable.
        /// </summary>
        /// <param name="boneRenderer">The BoneRenderer component</param>
        public delegate void OnRemoveBoneRendererCallback(BoneRenderer boneRenderer);

        /// <summary>
        /// Notification callback that is sent whenever a BoneRenderer calls OnEnable.
        /// </summary>
        public static OnAddBoneRendererCallback onAddBoneRenderer;

        /// <summary>
        /// Notification callback that is sent whenever a BoneRenderer calls OnDisable.
        /// </summary>
        public static OnRemoveBoneRendererCallback onRemoveBoneRenderer;

        void OnEnable()
        {
            ExtractBones();
            onAddBoneRenderer?.Invoke(this);
        }

        void OnDisable()
        {
            onRemoveBoneRenderer?.Invoke(this);
        }

        /// <summary>
        /// Invalidate and Rebuild bones and tip bones from Transform references.
        /// </summary>
        public void Invalidate()
        {
            ExtractBones();
        }

        /// <summary>
        /// Resets the BoneRenderer to default values.
        /// </summary>
        public void Reset()
        {
            ClearBones();
        }

        /// <summary>
        /// Clears bones and tip bones.
        /// </summary>
        public void ClearBones()
        {
            m_Bones = null;
            m_Tips = null;
        }

        /// <summary>
        /// Builds bones and tip bones from Transform references.
        /// </summary>
        public void ExtractBones()
        {
            if (m_Transforms == null || m_Transforms.Length == 0)
            {
                ClearBones();
                return;
            }

            var transformsHashSet = new HashSet<Transform>(m_Transforms);

            var bonesList = new List<TransformPair>(m_Transforms.Length);
            var tipsList = new List<Transform>(m_Transforms.Length);

            for (int i = 0; i < m_Transforms.Length; ++i)
            {
                bool hasValidChildren = false;

                var transform = m_Transforms[i];
                if (transform == null)
                    continue;

                if (UnityEditor.SceneVisibilityManager.instance.IsHidden(transform.gameObject, false))
                    continue;

                var mask = UnityEditor.Tools.visibleLayers;
                if ((mask & (1 << transform.gameObject.layer)) == 0)
                    continue;

                if (transform.childCount > 0)
                {
                    for (var k = 0; k < transform.childCount; ++k)
                    {
                        var childTransform = transform.GetChild(k);

                        if (transformsHashSet.Contains(childTransform))
                        {
                            bonesList.Add(new TransformPair() {first = transform, second = childTransform});
                            hasValidChildren = true;
                        }
                    }
                }

                if (!hasValidChildren)
                {
                    tipsList.Add(transform);
                }
            }

            m_Bones = bonesList.ToArray();
            m_Tips = tipsList.ToArray();
        }
#endif // UNITY_EDITOR
    }
}
