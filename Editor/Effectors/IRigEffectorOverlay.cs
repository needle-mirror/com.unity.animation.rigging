using System;

namespace UnityEditor.Animations.Rigging
{
    interface IRigEffectorOverlay : IDisposable
    {
        bool IsValid();
        void OnSceneGUIOverlay();
    }
}
