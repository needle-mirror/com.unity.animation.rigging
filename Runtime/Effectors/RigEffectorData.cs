using System;

namespace UnityEngine.Animations.Rigging
{
    [Serializable]
    public class RigEffectorData
    {
        [Serializable]
        public struct Style
        {
            public Mesh shape;
            public Color color;
            public float size;
            public Vector3 position;
            public Vector3 rotation;
        };

        [SerializeField] private Transform m_Transform;
        [SerializeField] private Style m_Style = new Style();
        [SerializeField] private bool m_Visible = true;

        public Transform transform { get => m_Transform; }
        public Style style { get => m_Style; }
        public bool visible { get => m_Visible; set => m_Visible = value; }

        public void Initialize(Transform transform, Style style)
        {
            m_Transform = transform;
            m_Style = style;
        }
    }
}
