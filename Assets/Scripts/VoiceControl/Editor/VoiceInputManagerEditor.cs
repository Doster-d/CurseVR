using UnityEngine;
using UnityEditor;
using CurseVR.VoiceControl.Input;

namespace CurseVR.VoiceControl.Editor
{
    [CustomEditor(typeof(VoiceInputManager))]
    public class VoiceInputManagerEditor : UnityEditor.Editor
    {
        private bool showDebugSettings = false;
        private bool showServiceConfig = false;
        private bool showVADSettings = false;
        
        public override void OnInspectorGUI()
        {
            var manager = (VoiceInputManager)target;
            
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Voice Input Manager Settings", EditorStyles.boldLabel);
            
            // Input Actions
            EditorGUILayout.PropertyField(serializedObject.FindProperty("inputActions"));
            
            // Service Configuration
            showServiceConfig = EditorGUILayout.Foldout(showServiceConfig, "Service Configuration");
            if (showServiceConfig)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("serviceConfig"));
                EditorGUI.indentLevel--;
            }
            
            // VAD Settings
            showVADSettings = EditorGUILayout.Foldout(showVADSettings, "Voice Activity Detection Settings");
            if (showVADSettings)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("vadParameters"));
                EditorGUI.indentLevel--;
            }
            
            // Debug Settings
            showDebugSettings = EditorGUILayout.Foldout(showDebugSettings, "Debug Settings");
            if (showDebugSettings)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("debugAudioSource"));
                EditorGUI.indentLevel--;
            }
            
            serializedObject.ApplyModifiedProperties();
            
            // Add test buttons in debug mode
            if (showDebugSettings)
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("Debug Tools", EditorStyles.boldLabel);
                
                if (GUILayout.Button("Test Microphone"))
                {
                    TestMicrophone();
                }
                
                if (GUILayout.Button("Test WebSocket Connection"))
                {
                    TestWebSocketConnection();
                }
            }
        }
        
        private void TestMicrophone()
        {
            var devices = Microphone.devices;
            if (devices.Length == 0)
            {
                EditorUtility.DisplayDialog("Microphone Test", 
                    "No microphone devices found!", "OK");
                return;
            }
            
            string deviceList = "Available Microphones:\n\n";
            foreach (var device in devices)
            {
                deviceList += $"- {device}\n";
            }
            
            EditorUtility.DisplayDialog("Microphone Test", deviceList, "OK");
        }
        
        private void TestWebSocketConnection()
        {
            // This would need to be implemented to actually test the connection
            EditorUtility.DisplayDialog("WebSocket Test",
                "WebSocket connection test not implemented in editor.", "OK");
        }
    }
}
