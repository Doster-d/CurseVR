using System;
using UnityEngine;
using CurseVR.VoiceControl.Models;

namespace CurseVR.VoiceControl.VAD
{
    public class VoiceActivityDetector : IVoiceActivityDetector
    {
        private readonly UnityMicrophoneProxy micProxy;
        private readonly AudioClipBuffer audioBuffer;
        private readonly VADParameters parameters;
        private readonly float[] sampleBuffer;
        
        private float lastActiveTime;
        private float lastInactiveTime;
        private bool isActive;
        private int lastReadPosition;
        
        public bool IsActive => isActive;
        public event Action<bool> OnVoiceActivityChanged;
        
        public VoiceActivityDetector(UnityMicrophoneProxy micProxy, AudioClipBuffer audioBuffer, VADParameters parameters)
        {
            this.micProxy = micProxy ?? throw new ArgumentNullException(nameof(micProxy));
            this.audioBuffer = audioBuffer ?? throw new ArgumentNullException(nameof(audioBuffer));
            this.parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
            
            this.sampleBuffer = new float[parameters.BufferSize];
            this.lastActiveTime = -parameters.ActivationIntervalSeconds;
            this.lastInactiveTime = -parameters.InactivationIntervalSeconds;
        }
        
        public void Update()
        {
            if (micProxy.AudioClip == null) return;
            
            int currentPosition = Microphone.GetPosition(null);
            if (currentPosition < 0) return;
            
            if (currentPosition == lastReadPosition) return;
            
            int samplesToRead = GetSamplesToRead(currentPosition);
            if (samplesToRead <= 0) return;
            
            micProxy.AudioClip.GetData(sampleBuffer, 0);
            ProcessAudioData(sampleBuffer, samplesToRead);
            
            lastReadPosition = currentPosition;
        }
        
        private int GetSamplesToRead(int currentPosition)
        {
            if (currentPosition < lastReadPosition)
            {
                return (micProxy.AudioClip.samples - lastReadPosition) + currentPosition;
            }
            
            return currentPosition - lastReadPosition;
        }
        
        private void ProcessAudioData(float[] samples, int sampleCount)
        {
            float volume = CalculateVolume(samples, sampleCount);
            float time = Time.time;
            
            if (!isActive && volume > parameters.ActiveVolumeThreshold)
            {
                if (time - lastActiveTime >= parameters.ActivationIntervalSeconds)
                {
                    isActive = true;
                    lastActiveTime = time;
                    OnVoiceActivityChanged?.Invoke(true);
                }
            }
            else if (isActive && volume < parameters.ActiveVolumeThreshold)
            {
                if (time - lastInactiveTime >= parameters.InactivationIntervalSeconds)
                {
                    isActive = false;
                    lastInactiveTime = time;
                    OnVoiceActivityChanged?.Invoke(false);
                    audioBuffer.Flush();
                }
            }
            
            if (isActive)
            {
                float[] activeData = new float[sampleCount];
                Array.Copy(samples, activeData, sampleCount);
                audioBuffer.AddSamples(activeData);
            }
        }
        
        private float CalculateVolume(float[] samples, int sampleCount)
        {
            float sum = 0f;
            for (int i = 0; i < sampleCount; i++)
            {
                sum += samples[i] * samples[i];
            }
            
            return Mathf.Sqrt(sum / sampleCount);
        }
        
        public void Dispose()
        {
            micProxy?.Dispose();
        }
    }
} 