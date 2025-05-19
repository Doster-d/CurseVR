using System;
using System.Threading.Tasks;
using UnityEngine;
using CurseVR.VoiceControl.Models;

namespace CurseVR.VoiceControl.Core
{
    /// <summary>
    /// Interface for the voice command service that handles communication with the ASR API
    /// </summary>
    public interface IVoiceCommandService
    {
        /// <summary>
        /// Event triggered when a command is recognized
        /// </summary>
        event Action<VoiceCommandData> OnCommandRecognized;
        
        /// <summary>
        /// Event triggered when connection status changes
        /// </summary>
        event Action<bool> OnConnectionStatusChanged;
        
        /// <summary>
        /// Initialize the service with the given configuration
        /// </summary>
        Task InitializeAsync(VoiceServiceConfig config);
        
        /// <summary>
        /// Send audio data to the service
        /// </summary>
        Task SendAudioDataAsync(byte[] audioData);
        
        /// <summary>
        /// Connect to the voice service
        /// </summary>
        Task ConnectAsync();
        
        /// <summary>
        /// Disconnect from the voice service
        /// </summary>
        Task DisconnectAsync();
        
        /// <summary>
        /// Check if the service is connected
        /// </summary>
        bool IsConnected { get; }
    }
}
