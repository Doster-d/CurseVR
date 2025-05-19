using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using CurseVR.VoiceControl.Core;
using CurseVR.VoiceControl.Models;
using CurseVR.VoiceControl.Services;

namespace CurseVR.VoiceControl.Test
{
    public class MockVoiceControlTest : MonoBehaviour
    {
        [Header("Test Settings")]
        [SerializeField] private AudioClip testVoiceClip;
        [SerializeField] private Material highlightMaterial;
        [SerializeField] private Material defaultMaterial;
        [SerializeField] private float moveDistance = 5f;
        [SerializeField] private InputActionReference interactAction;
        [SerializeField] private Camera mainCamera;
        [SerializeField] private LayerMask interactableLayer = -1;
        
        [Header("Audio Settings")]
        [SerializeField] private AudioSource voiceAudioSource;
        [SerializeField] private bool playAudioOnSelect = true;
        [SerializeField] private float audioVolume = 1f;
        [SerializeField] private bool debugAudio = true;
        
        [Header("Debug Settings")]
        [SerializeField] private bool offlineMode = false;
        [SerializeField] private float offlineModeDelay = 1f;
        [SerializeField] private bool debugInput = true;
        
        private GameObject selectedObject;
        private Material originalMaterial;
        private IVoiceCommandService voiceService;
        private bool isProcessing;
        private bool isQuitting;
        private Key interactKey = Key.E;
        
        private void Start()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
                if (mainCamera == null)
                {
                    Debug.LogError("No main camera found! Please assign a camera in the inspector.");
                    enabled = false;
                    return;
                }
            }
            
            if (testVoiceClip == null)
            {
                Debug.LogError("Test voice clip is not assigned! Please assign an audio clip in the inspector.");
            }
            
            if (voiceAudioSource == null)
            {
                voiceAudioSource = GetComponent<AudioSource>();
                if (voiceAudioSource == null)
                {
                    Debug.LogError("No AudioSource component found! Please add an AudioSource component to this GameObject or assign one in the inspector.");
                    return;
                }
            }

            voiceAudioSource.clip = testVoiceClip;
            voiceAudioSource.playOnAwake = false;
            voiceAudioSource.volume = audioVolume;

            if (debugAudio)
            {
                Debug.Log($"Audio source configured: Volume={voiceAudioSource.volume}, Clip={testVoiceClip?.name ?? "None"}");
            }
            
            if (!offlineMode)
            {
                InitializeVoiceService();
            }
            else
            {
                Debug.Log("Running in offline mode - voice service disabled");
            }
            
            if (interactAction != null && interactAction.action != null)
            {
                interactAction.action.Enable();
                Debug.Log($"Input action enabled: {interactAction.action.name}");
            }
            else
            {
                Debug.LogError("Interact action reference is not assigned!");
            }
        }

        private void InitializeVoiceService()
        {
            voiceService = gameObject.AddComponent<VoiceCommandService>();
            var config = new VoiceServiceConfig
            {
                WebSocketUrl = "ws://localhost:8000/ws/asr/",
                ClientId = "unity_test_client",
                SampleRate = 16000,
                Channels = 1
            };
            
            voiceService.InitializeAsync(config).ContinueWith(_ =>
            {
                if (!isQuitting)
                {
                    voiceService.ConnectAsync();
                }
            });
            
            voiceService.OnCommandRecognized += HandleVoiceCommand;
        }

        private void Update()
        {
            // Check for input using both methods for reliability
            bool shouldInteract = (Keyboard.current != null && Keyboard.current[interactKey].wasPressedThisFrame) ||
                                (interactAction != null && interactAction.action != null && interactAction.action.triggered);
            
            if (shouldInteract)
            {
                if (debugInput) Debug.Log("Interact key was pressed this frame");
                HandleInteraction();
            }
        }

        private void HandleInteraction()
        {
            if (isProcessing)
            {
                if (debugInput) Debug.Log("Interaction skipped - still processing previous command");
                return;
            }
            
            if (mainCamera == null)
            {
                Debug.LogError("No camera assigned!");
                return;
            }
            
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Ray ray = mainCamera.ScreenPointToRay(mousePosition);
            
            if (debugInput)
            {
                Debug.Log($"Casting ray from: {ray.origin} in direction: {ray.direction}");
                Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red, 1f);
            }
            
            RaycastHit hit;
            bool didHit = Physics.Raycast(ray, out hit, 100f, interactableLayer);
            
            if (debugInput) Debug.Log($"Raycast hit something: {didHit}" + (didHit ? $", Object: {hit.collider.gameObject.name}, Distance: {hit.distance}" : ""));
            
            if (didHit)
            {
                SelectObject(hit.collider.gameObject);
            }
            else
            {
                DeselectObject();
            }
        }
        
        private void SelectObject(GameObject obj)
        {
            if (debugAudio) Debug.Log($"Selecting object: {obj.name}");
            
            // Deselect previous object if any
            DeselectObject();
            
            // Select new object
            selectedObject = obj;
            var renderer = selectedObject.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                originalMaterial = renderer.material;
                renderer.material = highlightMaterial;
                Debug.Log("Applied highlight material");
            }
            else
            {
                Debug.LogWarning("Selected object has no MeshRenderer component!");
            }
            
            // Start mock voice command process
            StartCoroutine(ProcessMockVoiceCommand());
        }
        
        private void DeselectObject()
        {
            if (selectedObject != null)
            {
                Debug.Log($"Deselecting object: {selectedObject.name}");
                var renderer = selectedObject.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    renderer.material = originalMaterial;
                }
                selectedObject = null;
            }
        }
        
        private IEnumerator ProcessMockVoiceCommand()
        {
            if (testVoiceClip == null && !offlineMode)
            {
                Debug.LogError("Test voice clip is not assigned!");
                yield break;
            }
            
            isProcessing = true;
            Debug.Log("Starting mock voice command process");
            
            if (offlineMode && playAudioOnSelect)
            {
                if (voiceAudioSource != null && testVoiceClip != null)
                {
                    if (debugAudio) Debug.Log($"Starting audio playback: {testVoiceClip.name}, length: {testVoiceClip.length}s");
                    
                    voiceAudioSource.time = 0;
                    
                    voiceAudioSource.Play();
                    
                    if (debugAudio) 
                    {
                        Debug.Log($"Audio state: isPlaying={voiceAudioSource.isPlaying}, time={voiceAudioSource.time}, volume={voiceAudioSource.volume}");
                        StartCoroutine(MonitorAudioPlayback());
                    }
                    
                    while (voiceAudioSource.isPlaying)
                    {
                        yield return null;
                    }
                    
                    if (debugAudio) Debug.Log("Audio playback completed");
                }
                else
                {
                    Debug.LogError($"Cannot play audio: AudioSource={voiceAudioSource != null}, Clip={testVoiceClip != null}");
                    yield return new WaitForSeconds(1f);
                }
                
                if (testVoiceClip != null)
                {
                    // Convert AudioClip to byte array and send to service
                    float[] samples = new float[testVoiceClip.samples * testVoiceClip.channels];
                    testVoiceClip.GetData(samples, 0);
                    
                    byte[] audioData = new byte[samples.Length * 2];
                    for (int i = 0; i < samples.Length; i++)
                    {
                        short value = (short)(samples[i] * 32767f);
                        audioData[i * 2] = (byte)(value & 0xff);
                        audioData[i * 2 + 1] = (byte)((value >> 8) & 0xff);
                    }
                    
                    if (debugAudio) Debug.Log($"Sending audio data, size: {audioData.Length} bytes");
                    yield return voiceService.SendAudioDataAsync(audioData);
                }
            }
            else if (offlineMode)
            {
                Debug.Log($"Offline mode: simulating voice command processing for {offlineModeDelay} seconds");
                yield return new WaitForSeconds(offlineModeDelay);
                
                var mockCommand = new VoiceCommandData
                {
                    CommandType = "move_right",
                    RawText = "передвинуть этот предмет на 5 метров вправо",
                    Confidence = 1.0f
                };
                
                HandleVoiceCommand(mockCommand);
            }
            
            isProcessing = false;
            Debug.Log("Mock voice command process completed");
        }
        
        private IEnumerator MonitorAudioPlayback()
        {
            float startTime = Time.time;
            while (voiceAudioSource != null && voiceAudioSource.isPlaying)
            {
                Debug.Log($"Audio playing: time={voiceAudioSource.time:F2}/{testVoiceClip.length:F2}, volume={voiceAudioSource.volume}");
                yield return new WaitForSeconds(0.1f);
                
                // Timeout after twice the clip length
                if (Time.time - startTime > testVoiceClip.length * 2)
                {
                    Debug.LogWarning("Audio playback timeout!");
                    break;
                }
            }
        }
        
        private void HandleVoiceCommand(VoiceCommandData commandData)
        {
            if (selectedObject == null) return;
            
            Debug.Log($"Received command: {commandData.CommandType}, Raw text: {commandData.RawText}");
            
            if (commandData.RawText.ToLower().Contains("передвинуть") && 
                commandData.RawText.ToLower().Contains("вправо"))
            {
                StartCoroutine(MoveObjectRight());
            }
        }
        
        private IEnumerator MoveObjectRight()
        {
            if (selectedObject == null) yield break;
            
            Debug.Log($"Moving object {selectedObject.name} right by {moveDistance} units");
            
            Vector3 startPos = selectedObject.transform.position;
            Vector3 endPos = startPos + Vector3.right * moveDistance;
            float duration = 1f;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                selectedObject.transform.position = Vector3.Lerp(startPos, endPos, t);
                yield return null;
            }
            
            selectedObject.transform.position = endPos;
            Debug.Log("Movement completed");
            DeselectObject();
        }

        private void OnApplicationQuit()
        {
            isQuitting = true;
            CleanupVoiceService();
        }
        
        private void OnEnable()
        {
            if (interactAction != null && interactAction.action != null)
            {
                interactAction.action.Enable();
                if (debugInput) Debug.Log($"Input action enabled in OnEnable: {interactAction.action.name}");
            }
        }

        private void OnDisable()
        {
            if (interactAction != null && interactAction.action != null)
            {
                interactAction.action.Disable();
                if (debugInput) Debug.Log($"Input action disabled in OnDisable: {interactAction.action.name}");
            }
        }

        private void OnDestroy()
        {
            if (!isQuitting && !offlineMode)
            {
                CleanupVoiceService();
            }
        }

        private void CleanupVoiceService()
        {
            Debug.Log("Cleaning up voice service");
            if (voiceService != null)
            {
                try
                {
                    voiceService.DisconnectAsync().GetAwaiter().GetResult();
                    Debug.Log("Voice service disconnected successfully");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error disconnecting voice service: {e}");
                }
            }
        }
    }
}
