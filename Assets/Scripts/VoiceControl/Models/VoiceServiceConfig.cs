using System;
using UnityEngine;

namespace CurseVR.VoiceControl.Models
{
    /// <summary>
    /// Configuration settings for the voice command service
    /// </summary>
    [Serializable]
    public class VoiceServiceConfig
    {
        /// <summary>
        /// The base URL for the ASR WebSocket service
        /// </summary>
        public string WebSocketUrl { get; set; } = "ws://localhost:8000/ws/asr/";
        
        /// <summary>
        /// The client identifier for this connection
        /// </summary>
        public string ClientId { get; set; } = "unity_client";
        
        /// <summary>
        /// The sample rate for audio recording
        /// </summary>
        public int SampleRate { get; set; } = 16000;
        
        /// <summary>
        /// The number of audio channels (1 for mono, 2 for stereo)
        /// </summary>
        public int Channels { get; set; } = 1;
        
        /// <summary>
        /// The size of the audio buffer in samples
        /// </summary>
        public int BufferSize { get; set; } = 2048;
        
        /// <summary>
        /// Maximum reconnection attempts
        /// </summary>
        public int MaxReconnectAttempts { get; set; } = 3;
        
        /// <summary>
        /// Delay between reconnection attempts in milliseconds
        /// </summary>
        public int ReconnectDelayMs { get; set; } = 1000;
    }
} 
