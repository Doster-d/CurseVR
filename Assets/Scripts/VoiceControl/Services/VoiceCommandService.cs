using System;
using System.Threading.Tasks;
using UnityEngine;
using NativeWebSocket;
using CurseVR.VoiceControl.Core;
using CurseVR.VoiceControl.Models;
using Newtonsoft.Json;

namespace CurseVR.VoiceControl.Services
{
    /// <summary>
    /// Implementation of the voice command service using WebSocket communication
    /// </summary>
    public class VoiceCommandService : MonoBehaviour, IVoiceCommandService
    {
        private WebSocket webSocket;
        private VoiceServiceConfig config;
        private bool isInitialized;
        private int reconnectAttempts;
        
        public event Action<VoiceCommandData> OnCommandRecognized;
        public event Action<bool> OnConnectionStatusChanged;
        
        public bool IsConnected => webSocket?.State == WebSocketState.Open;

        public async Task InitializeAsync(VoiceServiceConfig config)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            
            string fullUrl = $"{config.WebSocketUrl}{config.ClientId}";
            webSocket = new WebSocket(fullUrl);
            
            var tcs = new TaskCompletionSource<bool>();
            
            webSocket.OnOpen += () =>
            {
                Debug.Log("Connected to ASR service");
                OnConnectionStatusChanged?.Invoke(true);
                reconnectAttempts = 0;
                tcs.TrySetResult(true);
            };
            
            webSocket.OnClose += (e) =>
            {
                Debug.Log($"Disconnected from ASR service: {e}");
                OnConnectionStatusChanged?.Invoke(false);
                TryReconnect();
                tcs.TrySetResult(false);
            };
            
            webSocket.OnMessage += (bytes) =>
            {
                var message = System.Text.Encoding.UTF8.GetString(bytes);
                ProcessWebSocketMessage(message);
            };
            
            webSocket.OnError += (e) =>
            {
                Debug.LogError($"WebSocket error: {e}");
                tcs.TrySetException(new Exception($"WebSocket error: {e}"));
            };
            
            try
            {
                await webSocket.Connect();
                await tcs.Task;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to initialize WebSocket: {ex.Message}");
                throw;
            }
            
            isInitialized = true;
        }

        public async Task ConnectAsync()
        {
            if (!isInitialized)
            {
                throw new InvalidOperationException("Service must be initialized before connecting");
            }
            
            if (webSocket.State == WebSocketState.Open)
            {
                return;
            }
            
            await webSocket.Connect();
        }

        public async Task DisconnectAsync()
        {
            if (webSocket != null && webSocket.State == WebSocketState.Open)
            {
                await webSocket.Close();
            }
        }

        public async Task SendAudioDataAsync(byte[] audioData)
        {
            if (!IsConnected)
            {
                Debug.LogWarning("Cannot send audio data: WebSocket is not connected");
                return;
            }
            
            await webSocket.Send(audioData);
        }
        
        private async void TryReconnect()
        {
            if (reconnectAttempts >= config.MaxReconnectAttempts)
            {
                Debug.LogError("Max reconnection attempts reached");
                return;
            }
            
            reconnectAttempts++;
            Debug.Log($"Attempting to reconnect ({reconnectAttempts}/{config.MaxReconnectAttempts})...");
            
            await Task.Delay(config.ReconnectDelayMs);
            await ConnectAsync();
        }
        
        private void ProcessWebSocketMessage(string message)
        {
            try
            {
                var commandData = JsonConvert.DeserializeObject<VoiceCommandData>(message);
                if (commandData != null)
                {
                    OnCommandRecognized?.Invoke(commandData);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error processing WebSocket message: {e.Message}");
            }
        }
        
        private void Update()
        {
            if (webSocket != null)
            {
                #if !UNITY_WEBGL || UNITY_EDITOR
                webSocket.DispatchMessageQueue();
                #endif
            }
        }
        
        private void OnDestroy()
        {
            DisconnectAsync().GetAwaiter().GetResult();
        }
    }
}
