namespace UnityEditor.Animations.Rigging
{
    /// <summary>
    /// Class to easily store a foldout state for a custom editor.
    /// </summary>
    class FoldoutState
    {
        bool m_Initialized;
        bool m_Value;
        string m_Name;

        public bool value
        {
            get
            {
                if (!m_Initialized)
                {
                    m_Value = EditorPrefs.GetBool(m_Name, m_Value);
                    m_Initialized = true;
                }
                return m_Value;
            }
            set
            {
                if (m_Value != value)
                    EditorPrefs.SetBool(m_Name, m_Value = value);
            }
        }

        FoldoutState() {}

        public static FoldoutState Create<T>(string name, bool value) =>
            new FoldoutState
            {
                m_Name = $"{typeof(T)}.{name}",
                m_Value = value
            };

        public static FoldoutState ForSettings<T>() => Create<T>("Settings", false);

        public static FoldoutState ForSourceObjects<T>() => Create<T>("SourceObjects", true);
    }
}
