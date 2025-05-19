using System;
using UnityEngine;

namespace CurseVR.VoiceControl.VAD
{
    public interface IVoiceActivityDetector : IDisposable
    {
        bool IsActive { get; }
        event Action<bool> OnVoiceActivityChanged;
        void Update();
    }
} 