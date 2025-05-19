using System;
using UnityEngine;

namespace CurseVR.VoiceControl.Models
{
    /// <summary>
    /// Configuration settings for the voice command service.
    /// </summary>
    /// <remarks>
    /// This serializable class contains all the connection and audio parameters needed
    /// to establish and maintain a connection with an Automatic Speech Recognition (ASR)
    /// WebSocket service. It can be configured in the Unity Inspector and saved as a
    /// ScriptableObject or directly in a MonoBehaviour component.
    /// </remarks>
    [Serializable]
    public class VoiceServiceConfig
    {
        /// <summary>
        /// Gets or sets the base URL for the ASR WebSocket service.
        /// </summary>
        /// <value>WebSocket URL in the format "ws://hostname:port/path"</value>
        /// <remarks>
        /// For local development, the default "ws://localhost:8000/ws/asr/" connects to a
        /// service running on the same machine. For production, this should be changed
        /// to the actual server address.
        /// </remarks>
        public string WebSocketUrl { get; set; } = "ws://localhost:8000/ws/asr/";
        
        /// <summary>
        /// Gets or sets the client identifier for this connection.
        /// </summary>
        /// <value>A unique string identifying this client instance</value>
        /// <remarks>
        /// The client ID can be used by the server to track different connections and
        /// maintain session state. In multi-user environments, this should be unique per user.
        /// </remarks>
        public string ClientId { get; set; } = "unity_client";
        
        /// <summary>
        /// Gets or sets the sample rate for audio recording.
        /// </summary>
        /// <value>Sample rate in Hz (samples per second)</value>
        /// <remarks>
        /// 16000 Hz (16 kHz) is a common sample rate for speech recognition systems.
        /// This must match the expected input format of the ASR service.
        /// </remarks>
        public int SampleRate { get; set; } = 16000;
        
        /// <summary>
        /// Gets or sets the number of audio channels.
        /// </summary>
        /// <value>1 for mono, 2 for stereo</value>
        /// <remarks>
        /// For voice recognition, mono (1 channel) is typically sufficient and reduces
        /// bandwidth requirements. The ASR service must support the specified channel count.
        /// </remarks>
        public int Channels { get; set; } = 1;
        
        /// <summary>
        /// Gets or sets the size of the audio buffer in samples.
        /// </summary>
        /// <value>Buffer size in samples (per channel)</value>
        /// <remarks>
        /// This determines how much audio data is collected before sending to the ASR service.
        /// Larger buffers introduce more latency but may improve recognition accuracy.
        /// A value of 2048 at 16 kHz represents about 128ms of audio.
        /// </remarks>
        public int BufferSize { get; set; } = 2048;
        
        /// <summary>
        /// Gets or sets the maximum reconnection attempts.
        /// </summary>
        /// <value>Number of reconnection attempts before giving up</value>
        /// <remarks>
        /// If the WebSocket connection is lost, the system will automatically attempt
        /// to reconnect this many times before requiring manual intervention.
        /// </remarks>
        public int MaxReconnectAttempts { get; set; } = 3;
        
        /// <summary>
        /// Gets or sets the delay between reconnection attempts in milliseconds.
        /// </summary>
        /// <value>Delay time in milliseconds</value>
        /// <remarks>
        /// After a failed connection attempt, the system will wait this many milliseconds
        /// before trying again. This helps prevent overloading the server with rapid
        /// connection requests.
        /// </remarks>
        public int ReconnectDelayMs { get; set; } = 1000;
    }
} 
