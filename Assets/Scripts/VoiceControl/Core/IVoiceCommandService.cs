using System;
using System.Threading.Tasks;
using UnityEngine;
using CurseVR.VoiceControl.Models;

namespace CurseVR.VoiceControl.Core
{
    /// <summary>
    /// Interface for the voice command service that handles communication with the ASR API.
    /// </summary>
    /// <remarks>
    /// This interface defines the contract for communicating with an Automatic Speech Recognition (ASR) 
    /// service over WebSockets. It handles initialization, connection management, and data transmission.
    /// </remarks>
    public interface IVoiceCommandService
    {
        /// <summary>
        /// Event triggered when a voice command is recognized by the ASR service.
        /// </summary>
        /// <remarks>
        /// Subscribers will receive a VoiceCommandData object containing the recognized command details
        /// including the command text, confidence level, and any additional parameters.
        /// </remarks>
        event Action<VoiceCommandData> OnCommandRecognized;
        
        /// <summary>
        /// Event triggered when the connection status with the ASR service changes.
        /// </summary>
        /// <remarks>
        /// The boolean parameter indicates whether the connection is active (true) or inactive (false).
        /// This event should be used to update UI elements or system state based on connection status.
        /// </remarks>
        event Action<bool> OnConnectionStatusChanged;
        
        /// <summary>
        /// Initializes the voice command service with the provided configuration.
        /// </summary>
        /// <param name="config">Configuration parameters for the voice service connection</param>
        /// <returns>A task representing the asynchronous initialization operation</returns>
        /// <exception cref="ArgumentNullException">Thrown when config is null</exception>
        /// <exception cref="InvalidOperationException">Thrown when initialization fails</exception>
        Task InitializeAsync(VoiceServiceConfig config);
        
        /// <summary>
        /// Sends audio data to the ASR service for processing.
        /// </summary>
        /// <param name="audioData">Raw audio data bytes to be sent for recognition</param>
        /// <returns>A task representing the asynchronous send operation</returns>
        /// <remarks>
        /// The audio data should be in the format specified in the VoiceServiceConfig
        /// (sample rate, channels, etc.). The data will be sent over WebSocket to the ASR service.
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown when service is not connected</exception>
        Task SendAudioDataAsync(byte[] audioData);
        
        /// <summary>
        /// Establishes a connection to the voice recognition service.
        /// </summary>
        /// <returns>A task representing the asynchronous connection operation</returns>
        /// <remarks>
        /// This method should be called after initialization and before sending audio data.
        /// It establishes a WebSocket connection to the ASR service endpoint specified in the configuration.
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown when service is not initialized</exception>
        Task ConnectAsync();
        
        /// <summary>
        /// Terminates the connection to the voice recognition service.
        /// </summary>
        /// <returns>A task representing the asynchronous disconnection operation</returns>
        /// <remarks>
        /// This method should be called when the application no longer needs to process voice commands,
        /// such as during application shutdown or when switching scenes.
        /// </remarks>
        Task DisconnectAsync();
        
        /// <summary>
        /// Gets a value indicating whether the service is currently connected to the ASR endpoint.
        /// </summary>
        /// <value>True if connected, false otherwise</value>
        /// <remarks>
        /// This property should be checked before attempting to send audio data to prevent errors.
        /// </remarks>
        bool IsConnected { get; }
    }
}
