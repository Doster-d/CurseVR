using System;
using UnityEngine;
using CurseVR.VoiceControl.Models;

namespace CurseVR.VoiceControl.VAD
{
    /// <summary>
    /// Implements voice activity detection by analyzing microphone input volume.
    /// </summary>
    /// <remarks>
    /// This class monitors audio input from a microphone to detect when a user starts and stops speaking.
    /// It uses volume thresholds and timing parameters to determine state transitions and prevent
    /// rapid toggling between active and inactive states. When voice activity is detected,
    /// audio data is captured and stored in a buffer for further processing.
    /// </remarks>
    public class VoiceActivityDetector : IVoiceActivityDetector
    {
        private readonly UnityMicrophoneProxy micProxy;
        private readonly AudioClipBuffer audioBuffer;
        private readonly VADParameters parameters;
        private readonly float[] sampleBuffer;
        
        private float lastActiveTime;
        private float lastInactiveTime;
        private bool isActive;
        private int lastReadPosition;
        
        /// <inheritdoc/>
        public bool IsActive => isActive;
        
        /// <inheritdoc/>
        public event Action<bool> OnVoiceActivityChanged;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="VoiceActivityDetector"/> class.
        /// </summary>
        /// <param name="micProxy">Proxy for the Unity microphone system</param>
        /// <param name="audioBuffer">Buffer to store captured audio samples</param>
        /// <param name="parameters">Configuration parameters for voice activity detection</param>
        /// <exception cref="ArgumentNullException">Thrown if any parameter is null</exception>
        /// <remarks>
        /// The constructor initializes internal state and prepares the detector for processing.
        /// Initial timestamps are set to ensure proper debouncing at startup.
        /// </remarks>
        public VoiceActivityDetector(UnityMicrophoneProxy micProxy, AudioClipBuffer audioBuffer, VADParameters parameters)
        {
            this.micProxy = micProxy ?? throw new ArgumentNullException(nameof(micProxy));
            this.audioBuffer = audioBuffer ?? throw new ArgumentNullException(nameof(audioBuffer));
            this.parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
            
            this.sampleBuffer = new float[parameters.BufferSize];
            this.lastActiveTime = -parameters.ActivationIntervalSeconds;
            this.lastInactiveTime = -parameters.InactivationIntervalSeconds;
        }
        
        /// <inheritdoc/>
        public void Update()
        {
            if (micProxy.AudioClip == null) return;
            
            int currentPosition = Microphone.GetPosition(null);
            if (currentPosition < 0) return;
            
            if (currentPosition == lastReadPosition) return;
            
            int samplesToRead = GetSamplesToRead(currentPosition);
            if (samplesToRead <= 0) return;
            
            micProxy.AudioClip.GetData(sampleBuffer, 0);
            ProcessAudioData(sampleBuffer, samplesToRead);
            
            lastReadPosition = currentPosition;
        }
        
        /// <summary>
        /// Calculates how many new audio samples should be read from the microphone.
        /// </summary>
        /// <param name="currentPosition">Current position in the microphone buffer</param>
        /// <returns>Number of samples to read</returns>
        /// <remarks>
        /// Handles the circular buffer wrapping that occurs in Unity's microphone system.
        /// </remarks>
        private int GetSamplesToRead(int currentPosition)
        {
            if (currentPosition < lastReadPosition)
            {
                return (micProxy.AudioClip.samples - lastReadPosition) + currentPosition;
            }
            
            return currentPosition - lastReadPosition;
        }
        
        /// <summary>
        /// Processes a batch of audio data to detect voice activity.
        /// </summary>
        /// <param name="samples">Array of audio samples</param>
        /// <param name="sampleCount">Number of samples to process</param>
        /// <remarks>
        /// This method analyzes volume levels to determine if voice is active, handles
        /// state transitions with appropriate timing intervals, and stores active audio
        /// in the buffer for later processing.
        /// </remarks>
        private void ProcessAudioData(float[] samples, int sampleCount)
        {
            float volume = CalculateVolume(samples, sampleCount);
            float time = Time.time;
            
            if (!isActive && volume > parameters.ActiveVolumeThreshold)
            {
                if (time - lastActiveTime >= parameters.ActivationIntervalSeconds)
                {
                    isActive = true;
                    lastActiveTime = time;
                    OnVoiceActivityChanged?.Invoke(true);
                }
            }
            else if (isActive && volume < parameters.ActiveVolumeThreshold)
            {
                if (time - lastInactiveTime >= parameters.InactivationIntervalSeconds)
                {
                    isActive = false;
                    lastInactiveTime = time;
                    OnVoiceActivityChanged?.Invoke(false);
                    audioBuffer.Flush();
                }
            }
            
            if (isActive)
            {
                float[] activeData = new float[sampleCount];
                Array.Copy(samples, activeData, sampleCount);
                audioBuffer.AddSamples(activeData);
            }
        }
        
        /// <summary>
        /// Calculates the Root Mean Square (RMS) volume level of an audio sample batch.
        /// </summary>
        /// <param name="samples">Array of audio samples</param>
        /// <param name="sampleCount">Number of samples to analyze</param>
        /// <returns>RMS volume level, typically between 0 and 1</returns>
        private float CalculateVolume(float[] samples, int sampleCount)
        {
            float sum = 0f;
            for (int i = 0; i < sampleCount; i++)
            {
                sum += samples[i] * samples[i];
            }
            
            return Mathf.Sqrt(sum / sampleCount);
        }
        
        /// <inheritdoc/>
        public void Dispose()
        {
            micProxy?.Dispose();
        }
    }
} 