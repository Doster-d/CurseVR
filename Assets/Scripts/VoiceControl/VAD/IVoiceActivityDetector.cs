using System;
using UnityEngine;

namespace CurseVR.VoiceControl.VAD
{
    /// <summary>
    /// Interface for voice activity detection functionality.
    /// </summary>
    /// <remarks>
    /// This interface defines the contract for components that detect when a user is speaking
    /// by analyzing audio input. Implementations should handle microphone data processing
    /// and determine speaking/silence transitions based on volume thresholds and timing parameters.
    /// </remarks>
    public interface IVoiceActivityDetector : IDisposable
    {
        /// <summary>
        /// Gets a value indicating whether voice activity is currently detected.
        /// </summary>
        /// <value>True when voice is active, false when silent</value>
        bool IsActive { get; }
        
        /// <summary>
        /// Event triggered when voice activity state changes.
        /// </summary>
        /// <remarks>
        /// The boolean parameter indicates the new state: true when voice becomes active,
        /// false when voice becomes inactive. Subscribers can use this to start/stop
        /// recording or processing audio based on detected speech.
        /// </remarks>
        event Action<bool> OnVoiceActivityChanged;
        
        /// <summary>
        /// Updates the voice activity detection state.
        /// </summary>
        /// <remarks>
        /// This method should be called regularly (typically once per frame) to process
        /// new audio data from the microphone and update the activity state. It handles
        /// reading new microphone data, analyzing volume levels, and triggering state
        /// change events when appropriate.
        /// </remarks>
        void Update();
    }
} 