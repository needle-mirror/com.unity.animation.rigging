using System.Collections.Generic;
using UnityEngine.Playables;
using UnityEngine.Experimental.Animations;

namespace UnityEngine.Animations.Rigging
{
    [RequireComponent(typeof(Animator))]
    [DisallowMultipleComponent, ExecuteInEditMode, AddComponentMenu("Animation Rigging/Setup/Rig Builder")]
    [HelpURL("https://docs.unity3d.com/Packages/com.unity.animation.rigging@0.3?preview=1&subfolder=/manual/index.html")]
    public class RigBuilder : MonoBehaviour, IAnimationWindowPreview, IRigEffectorHolder
    {
        [SerializeField] private List<RigLayer> m_RigLayers;

        private IRigLayer[] m_RuntimeRigLayers;
        private SyncSceneToStreamLayer m_SyncSceneToStreamLayer;

#if UNITY_EDITOR
        [SerializeField] private List<RigEffectorData> m_Effectors = new List<RigEffectorData>();
        public IEnumerable<RigEffectorData> effectors { get => m_Effectors; }
#endif

        public delegate void OnAddRigBuilderCallback(RigBuilder rigBuilder);
        public delegate void OnRemoveRigBuilderCallback(RigBuilder rigBuilder);

        public static OnAddRigBuilderCallback onAddRigBuilder;
        public static OnRemoveRigBuilderCallback onRemoveRigBuilder;

        void OnEnable()
        {
            // Build runtime data.
            if (Application.isPlaying)
                Build();

            onAddRigBuilder?.Invoke(this);
        }

        void OnDisable()
        {
            // Clear runtime data.
            if (Application.isPlaying)
                Clear();

            onRemoveRigBuilder?.Invoke(this);
        }

        void OnDestroy()
        {
            Clear();
        }

        void Update()
        {
            if (!graph.IsValid())
                return;

            syncSceneToStreamLayer.Update(m_RuntimeRigLayers);

            for (int i = 0, count = m_RuntimeRigLayers.Length; i < count; ++i)
            {
                if (m_RuntimeRigLayers[i].IsValid() && m_RuntimeRigLayers[i].active)
                    m_RuntimeRigLayers[i].Update();
            }
        }

        public bool Build()
        {
            Clear();

            var animator = GetComponent<Animator>();
            if (animator == null || layers.Count == 0)
                return false;

            // Make a copy of the layers list.
            m_RuntimeRigLayers = layers.ToArray();

            graph = RigBuilderUtils.BuildPlayableGraph(animator, m_RuntimeRigLayers, syncSceneToStreamLayer);

            if (!graph.IsValid())
                return false;

            graph.Play();

            return true;
        }

        public void Clear()
        {
            if (graph.IsValid())
                graph.Destroy();

            if (m_RuntimeRigLayers != null)
            {
                foreach (var layer in m_RuntimeRigLayers)
                    layer.Reset();

                m_RuntimeRigLayers = null;
            }

            syncSceneToStreamLayer.Reset();
        }

        //
        // IAnimationWindowPreview methods implementation
        //

        public void StartPreview()
        {
            if (!enabled)
                return;

            // Make a copy of the layer list if it doesn't already exist.
            if (m_RuntimeRigLayers == null)
                m_RuntimeRigLayers = layers.ToArray();

            var animator = GetComponent<Animator>();
            if (animator != null)
            {
                foreach (var layer in m_RuntimeRigLayers)
                {
                    layer.Initialize(animator);
                }
            }
        }

        public void StopPreview()
        {
            if (!enabled)
                return;

            if (Application.isPlaying)
                return;

            Clear();
        }

        public void UpdatePreviewGraph(PlayableGraph graph)
        {
            if (!enabled)
                return;

            if (!graph.IsValid())
                return;

            syncSceneToStreamLayer.Update(m_RuntimeRigLayers);

            foreach (var layer in m_RuntimeRigLayers)
            {
                if (layer.IsValid() && layer.active)
                    layer.Update();
            }
        }

        public Playable BuildPreviewGraph(PlayableGraph graph, Playable inputPlayable)
        {
            if (!enabled)
                return inputPlayable;

            var animator = GetComponent<Animator>();
            if (animator == null || m_RuntimeRigLayers.Length == 0)
                return inputPlayable;

            var playableChains = RigBuilderUtils.BuildPlayables(animator, graph, m_RuntimeRigLayers, syncSceneToStreamLayer);
            foreach(var chain in playableChains)
            {
                if (chain.playables == null || chain.playables.Length == 0)
                    continue;

                chain.playables[0].AddInput(inputPlayable, 0, 1);
                inputPlayable = chain.playables[chain.playables.Length - 1];
            }

            return inputPlayable;
        }

#if UNITY_EDITOR
        public void AddEffector(Transform transform, RigEffectorData.Style style)
        {
            var effector = new RigEffectorData();
            effector.Initialize(transform, style);

            m_Effectors.Add(effector);
        }

        public void RemoveEffector(Transform transform)
        {
            m_Effectors.RemoveAll((RigEffectorData data) => data.transform == transform);
        }

        public bool ContainsEffector(Transform transform)
        {
            return m_Effectors.Exists((RigEffectorData data) => data.transform == transform);
        }
#endif

        public List<RigLayer> layers
        {
            get
            {
                if (m_RigLayers == null)
                    m_RigLayers = new List<RigLayer>();

                return m_RigLayers;
            }

            set => m_RigLayers = value;
        }

        private SyncSceneToStreamLayer syncSceneToStreamLayer
        {
            get
            {
                if (m_SyncSceneToStreamLayer == null)
                    m_SyncSceneToStreamLayer = new SyncSceneToStreamLayer();

                return m_SyncSceneToStreamLayer;
            }

            set => m_SyncSceneToStreamLayer = value;
        }

        public PlayableGraph graph { get; private set; }
    }
}
