using System;
using UnityEngine;

namespace CurseVR.VoiceControl.VAD
{
    public class UnityMicrophoneProxy : IDisposable
    {
        private readonly string deviceName;
        private AudioClip audioClip;
        private readonly int frequency;
        private readonly int sampleRate;
        
        public AudioClip AudioClip => audioClip;
        public int SampleRate => sampleRate;
        
        public UnityMicrophoneProxy(string deviceName = null, int frequency = 44100)
        {
            this.deviceName = deviceName ?? Microphone.devices[0];
            this.frequency = frequency;
            this.sampleRate = frequency;
            
            InitializeMicrophone();
        }
        
        private void InitializeMicrophone()
        {
            if (Microphone.IsRecording(deviceName))
            {
                Microphone.End(deviceName);
            }
            
            audioClip = Microphone.Start(deviceName, true, 1, frequency);
            
            while (!(Microphone.GetPosition(deviceName) > 0)) { }
        }
        
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