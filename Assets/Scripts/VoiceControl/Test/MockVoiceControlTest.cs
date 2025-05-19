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
        
        private GameObject selectedObject;
        private Material originalMaterial;
        private IVoiceCommandService voiceService;
        private bool isProcessing;
        private bool isQuitting;
        
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
            
            InitializeVoiceService();
            SetupInputAction();
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

        private void SetupInputAction()
        {
            if (interactAction != null && interactAction.action != null)
            {
                interactAction.action.Enable();
                interactAction.action.performed += OnInteractPerformed;
                Debug.Log("Input action setup complete");
            }
            else
            {
                Debug.LogError("Interact action not assigned!");
            }
        }
        
        private void OnInteractPerformed(InputAction.CallbackContext context)
        {
            if (isProcessing) return;
            
            Debug.Log("OnInteractPerformed called");
            
            if (mainCamera == null)
            {
                Debug.LogError("No camera assigned!");
                return;
            }
            
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Ray ray = mainCamera.ScreenPointToRay(mousePosition);
            Debug.Log($"Mouse Position: {mousePosition}, Ray Origin: {ray.origin}, Ray Direction: {ray.direction}");
            
            Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red, 1f);
            
            RaycastHit hit;
            bool didHit = Physics.Raycast(ray, out hit, 100f, interactableLayer);
            Debug.Log($"Raycast hit something: {didHit}");
            
            if (didHit)
            {
                Debug.Log($"Hit object: {hit.collider.gameObject.name} at distance {hit.distance}");
                SelectObject(hit.collider.gameObject);
            }
            else
            {
                Debug.Log("No object hit");
                DeselectObject();
            }
        }
        
        private void SelectObject(GameObject obj)
        {
            Debug.Log($"Selecting object: {obj.name}");
            
            DeselectObject();
            
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
            if (testVoiceClip == null)
            {
                Debug.LogError("Test voice clip is not assigned!");
                yield break;
            }
            
            isProcessing = true;
            Debug.Log("Starting mock voice command process");
            
            var audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = testVoiceClip;
            audioSource.Play();
            
            Debug.Log($"Playing audio clip, duration: {testVoiceClip.length}s");
            
            yield return new WaitForSeconds(testVoiceClip.length);
            
            float[] samples = new float[testVoiceClip.samples * testVoiceClip.channels];
            testVoiceClip.GetData(samples, 0);
            
            byte[] audioData = new byte[samples.Length * 2];
            for (int i = 0; i < samples.Length; i++)
            {
                short value = (short)(samples[i] * 32767f);
                audioData[i * 2] = (byte)(value & 0xff);
                audioData[i * 2 + 1] = (byte)((value >> 8) & 0xff);
            }
            
            Debug.Log($"Sending audio data, size: {audioData.Length} bytes");
            
            yield return voiceService.SendAudioDataAsync(audioData);
            
            Destroy(audioSource);
            isProcessing = false;
            Debug.Log("Mock voice command process completed");
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
        
        private void OnDestroy()
        {
            if (interactAction != null && interactAction.action != null)
            {
                interactAction.action.performed -= OnInteractPerformed;
            }
            
            if (!isQuitting)
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
