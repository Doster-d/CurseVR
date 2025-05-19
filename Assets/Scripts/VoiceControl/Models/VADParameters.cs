using UnityEngine;

namespace CurseVR.VoiceControl.Models
{
    /// <summary>
    /// Parameters for the Voice Activity Detection (VAD) system.
    /// </summary>
    /// <remarks>
    /// This class contains all configuration parameters for the voice activity detection system.
    /// It controls audio buffer sizes, thresholds for detecting when speech begins and ends,
    /// timing parameters, and other settings that affect VAD sensitivity and responsiveness.
    /// These values can be adjusted in the Unity Inspector to fine-tune voice detection for
    /// different environments and microphones.
    /// </remarks>
    [System.Serializable]
    public class VADParameters
    {
        /// <summary>
        /// Size of the audio buffer in samples.
        /// </summary>
        /// <remarks>
        /// Determines how much audio data is processed at once. Larger values may improve
        /// detection accuracy but increase latency. At typical speech sample rates (16kHz),
        /// a value of 2048 represents about 128ms of audio.
        /// </remarks>
        [Header("Audio Settings")]
        [Tooltip("Size of the audio buffer in samples")]
        public int BufferSize = 2048;
        
        /// <summary>
        /// Maximum duration of continuous voice activity in seconds.
        /// </summary>
        /// <remarks>
        /// Sets a limit on how long a single voice activation can last before being
        /// forcibly terminated. This prevents runaway recordings in noisy environments
        /// or when the deactivation threshold isn't met.
        /// </remarks>
        [Header("Timing")]
        [Tooltip("Maximum duration of voice activity in seconds")]
        public float MaxActiveDurationSeconds = 10f;
        
        /// <summary>
        /// Maximum time to queue audio data before processing in seconds.
        /// </summary>
        /// <remarks>
        /// The upper limit on how long audio data will be queued before being processed.
        /// Helps ensure that even in difficult detection scenarios, data will eventually
        /// be processed.
        /// </remarks>
        [Tooltip("Maximum time to queue audio data before processing")]
        public float MaxQueueingTimeSeconds = 0.1f;
        
        /// <summary>
        /// Minimum time to queue audio data before processing in seconds.
        /// </summary>
        /// <remarks>
        /// The lower limit on how long audio data must be queued before processing starts.
        /// This helps prevent processing very small chunks of audio which could reduce
        /// recognition accuracy.
        /// </remarks>
        [Tooltip("Minimum time to queue audio data before processing")]
        public float MinQueueingTimeSeconds = 0.05f;
        
        /// <summary>
        /// Volume threshold for voice activity detection.
        /// </summary>
        /// <remarks>
        /// The RMS (Root Mean Square) volume level that must be exceeded for audio to be
        /// considered speech rather than background noise. Higher values require louder
        /// speech but reduce false activations. This should be calibrated based on
        /// the microphone and ambient noise levels.
        /// </remarks>
        [Header("Detection")]
        [Tooltip("Volume threshold for voice activity detection")]
        [Range(0f, 1f)]
        public float ActiveVolumeThreshold = 0.1f;
        
        /// <summary>
        /// Rate threshold for activation.
        /// </summary>
        /// <remarks>
        /// The proportion of audio frames that must exceed the volume threshold within a
        /// detection window to trigger voice activation. Higher values make activation
        /// more resistant to brief spikes in volume.
        /// </remarks>
        [Tooltip("Rate threshold for activation")]
        [Range(0f, 1f)]
        public float ActivationRateThreshold = 0.6f;
        
        /// <summary>
        /// Rate threshold for inactivation.
        /// </summary>
        /// <remarks>
        /// The proportion of audio frames that must fall below the volume threshold within
        /// a detection window to trigger voice deactivation. Lower values make the system
        /// more responsive to pauses in speech.
        /// </remarks>
        [Tooltip("Rate threshold for inactivation")]
        [Range(0f, 1f)]
        public float InactivationRateThreshold = 0.4f;
        
        /// <summary>
        /// Time interval for activation check in seconds.
        /// </summary>
        /// <remarks>
        /// The minimum time that must elapse between voice activation events. This prevents
        /// rapid toggling of the active state due to fluctuating audio levels near the threshold.
        /// </remarks>
        [Header("Intervals")]
        [Tooltip("Time interval for activation check")]
        public float ActivationIntervalSeconds = 0.1f;
        
        /// <summary>
        /// Time interval for inactivation check in seconds.
        /// </summary>
        /// <remarks>
        /// The minimum time that must elapse between voice deactivation events. A higher value
        /// makes the system more tolerant of brief pauses in speech, preventing a single
        /// utterance from being split into multiple activations.
        /// </remarks>
        [Tooltip("Time interval for inactivation check")]
        public float InactivationIntervalSeconds = 0.3f;
    }
} 