using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using CurseVR.VoiceControl.Core;
using CurseVR.VoiceControl.Models;
using CurseVR.VoiceControl.Services;
using CurseVR.VoiceControl.VAD;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace CurseVR.VoiceControl.Input
{
    /// <summary>
    /// Manages voice input and integrates with Unity's Input System
    /// </summary>
    public class VoiceInputManager : MonoBehaviour
    {
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private VADParameters vadParameters;
        [SerializeField] private AudioSource debugAudioSource;
        [SerializeField] private VoiceServiceConfig serviceConfig;
        
        private IVoiceActivityDetector vad;
        private UnityMicrophoneProxy micProxy;
        private IVoiceCommandService voiceService;
        private AudioClipBuffer audioBuffer;
        private bool isProcessingVoice;
        
        // Dictionary to store action references by command type
        private Dictionary<string, InputActionReference> actionReferences = new Dictionary<string, InputActionReference>();
        
        private void Start()
        {
            InitializeVoiceService();
            InitializeVAD();
            InitializeInputActions();
        }
        
        private async void InitializeVoiceService()
        {
            voiceService = gameObject.AddComponent<VoiceCommandService>();
            await voiceService.InitializeAsync(serviceConfig);
            await voiceService.ConnectAsync();
            
            voiceService.OnCommandRecognized += HandleVoiceCommand;
            voiceService.OnConnectionStatusChanged += HandleConnectionStatus;
        }
        
        private void InitializeVAD()
        {
            micProxy = new UnityMicrophoneProxy();
            
            audioBuffer = new AudioClipBuffer(
                maxSampleLength: (int)(vadParameters.MaxActiveDurationSeconds * micProxy.SampleRate),
                frequency: micProxy.SampleRate);
            
            audioBuffer.OnBufferFilled += HandleVoiceInactive;
            
            vad = new VoiceActivityDetector(micProxy, audioBuffer, vadParameters);
                
            if (debugAudioSource != null)
            {
                debugAudioSource.clip = micProxy.AudioClip;
                debugAudioSource.loop = true;
                debugAudioSource.Play();
            }
        }
        
        private void InitializeInputActions()
        {
            var voiceActionMap = inputActions.FindActionMap("Voice");
            if (voiceActionMap != null)
            {
                voiceActionMap.Enable();
                
                // Create action references for all actions in the map
                foreach (var action in voiceActionMap)
                {
                    var actionRef = ScriptableObject.CreateInstance<InputActionReference>();
                    actionRef.Set(action);
                    actionReferences[action.name] = actionRef;
                }
            }
        }
        
        private void HandleVoiceInactive(AudioClip clip)
        {
            if (!isProcessingVoice || clip == null) return;
            
            // Convert AudioClip to byte array
            float[] samples = new float[clip.samples * clip.channels];
            clip.GetData(samples, 0);
            
            // Convert to 16-bit PCM
            byte[] audioData = new byte[samples.Length * 2];
            for (int i = 0; i < samples.Length; i++)
            {
                short value = (short)(samples[i] * 32767f);
                audioData[i * 2] = (byte)(value & 0xff);
                audioData[i * 2 + 1] = (byte)((value >> 8) & 0xff);
            }
            
            // Send to voice service
            _ = voiceService.SendAudioDataAsync(audioData);
            
            // Play debug audio if needed
            if (debugAudioSource != null)
            {
                debugAudioSource.clip = clip;
                debugAudioSource.Play();
            }
        }
        
        private void HandleVoiceCommand(VoiceCommandData commandData)
        {
            Debug.Log($"Voice command recognized: {commandData.CommandType}");
            
            if (actionReferences.TryGetValue(commandData.CommandType, out var actionRef))
            {
                var action = actionRef.action;
                if (action != null)
                {
                    Debug.Log($"Triggering input action: {action.name}");
                    
                    // Simply enable the action - this will trigger any subscribers 
                    // that are monitoring for this action's state changes
                    action.Enable();
                    
                    // This is a workaround - Unity's InputSystem doesn't provide 
                    // a public API to directly trigger an action from code
                    // We're just toggling the action's state via enable/disable
                    StartCoroutine(ToggleAction(action));
                }
            }
            else
            {
                Debug.LogWarning($"No action found for command type: {commandData.CommandType}");
            }
        }
        
        private System.Collections.IEnumerator ToggleAction(InputAction action)
        {
            // Wait a frame to ensure the enable event is processed
            yield return null;
            
            // Disable and re-enable the action to trigger another change
            action.Disable();
            yield return null;
            action.Enable();
        }
        
        private void HandleConnectionStatus(bool isConnected)
        {
            Debug.Log($"Voice service connection status: {isConnected}");
            isProcessingVoice = isConnected;
        }
        
        private void Update()
        {
            vad?.Update();
        }
        
        private void OnDestroy()
        {
            vad?.Dispose();
            micProxy?.Dispose();
            _ = voiceService?.DisconnectAsync();
            
            // Clean up action references
            foreach (var actionRef in actionReferences.Values)
            {
                if (actionRef != null)
                {
                    Destroy(actionRef);
                }
            }
            actionReferences.Clear();
        }
    }
}
