using System;
using UnityEngine;

namespace CurseVR.VoiceControl.VAD
{
    /// <summary>
    /// Provides an abstraction over Unity's Microphone API for simplified access to audio input.
    /// </summary>
    /// <remarks>
    /// This class encapsulates the initialization, management, and cleanup of Unity's microphone system.
    /// It creates a continuous recording AudioClip that can be accessed for real-time audio processing.
    /// The proxy pattern allows for easier testing and a more controlled interface to the Unity API.
    /// </remarks>
    public class UnityMicrophoneProxy : IDisposable
    {
        private readonly string deviceName;
        private AudioClip audioClip;
        private readonly int frequency;
        private readonly int sampleRate;
        
        /// <summary>
        /// Gets the AudioClip containing the microphone input data.
        /// </summary>
        /// <value>The AudioClip recording from the microphone</value>
        /// <remarks>
        /// This is a continuous recording in a circular buffer. Use Microphone.GetPosition 
        /// to determine the current recording position within the clip.
        /// </remarks>
        public AudioClip AudioClip => audioClip;
        
        /// <summary>
        /// Gets the sample rate of the audio recording.
        /// </summary>
        /// <value>Sample rate in Hz</value>
        public int SampleRate => sampleRate;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="UnityMicrophoneProxy"/> class.
        /// </summary>
        /// <param name="deviceName">Name of the microphone device to use, or null for default device</param>
        /// <param name="frequency">Requested sample rate for recording in Hz</param>
        /// <remarks>
        /// If deviceName is null, the first available microphone device will be used.
        /// The default frequency of 44100Hz is CD quality, but can be reduced for voice recognition
        /// (e.g., 16000Hz is common for speech processing).
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown when no microphone devices are available</exception>
        public UnityMicrophoneProxy(string deviceName = null, int frequency = 44100)
        {
            if (Microphone.devices.Length == 0)
            {
                throw new InvalidOperationException("No microphone devices available");
            }
            
            this.deviceName = deviceName ?? Microphone.devices[0];
            this.frequency = frequency;
            this.sampleRate = frequency;
            
            InitializeMicrophone();
        }
        
        /// <summary>
        /// Initializes the microphone and begins recording.
        /// </summary>
        /// <remarks>
        /// This method stops any existing recording, creates a new AudioClip for recording,
        /// and starts the microphone in loop mode. It waits until recording has actually started
        /// before returning.
        /// </remarks>
        private void InitializeMicrophone()
        {
            if (Microphone.IsRecording(deviceName))
            {
                Microphone.End(deviceName);
            }
            
            audioClip = Microphone.Start(deviceName, true, 1, frequency);
            
            while (!(Microphone.GetPosition(deviceName) > 0)) { }
        }
        
        /// <summary>
        /// Disposes of resources used by the microphone proxy.
        /// </summary>
        /// <remarks>
        /// Stops any active recording and destroys the AudioClip to prevent memory leaks.
        /// This should be called when the proxy is no longer needed, typically during
        /// application cleanup or scene transitions.
        /// </remarks>
        public void Dispose()
        {
            if (Microphone.IsRecording(deviceName))
            {
                Microphone.End(deviceName);
            }
            
            if (audioClip != null)
            {
                UnityEngine.Object.Destroy(audioClip);
                audioClip = null;
            }
        }
    }
} 