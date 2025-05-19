using UnityEngine;

namespace CurseVR.VoiceControl.Models
{
    [System.Serializable]
    public class VADParameters
    {
        [Header("Audio Settings")]
        [Tooltip("Size of the audio buffer in samples")]
        public int BufferSize = 2048;
        
        [Header("Timing")]
        [Tooltip("Maximum duration of voice activity in seconds")]
        public float MaxActiveDurationSeconds = 10f;
        
        [Tooltip("Maximum time to queue audio data before processing")]
        public float MaxQueueingTimeSeconds = 0.1f;
        
        [Tooltip("Minimum time to queue audio data before processing")]
        public float MinQueueingTimeSeconds = 0.05f;
        
        [Header("Detection")]
        [Tooltip("Volume threshold for voice activity detection")]
        [Range(0f, 1f)]
        public float ActiveVolumeThreshold = 0.1f;
        
        [Tooltip("Rate threshold for activation")]
        [Range(0f, 1f)]
        public float ActivationRateThreshold = 0.6f;
        
        [Tooltip("Rate threshold for inactivation")]
        [Range(0f, 1f)]
        public float InactivationRateThreshold = 0.4f;
        
        [Header("Intervals")]
        [Tooltip("Time interval for activation check")]
        public float ActivationIntervalSeconds = 0.1f;
        
        [Tooltip("Time interval for inactivation check")]
        public float InactivationIntervalSeconds = 0.3f;
    }
} 