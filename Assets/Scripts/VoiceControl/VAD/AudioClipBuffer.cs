using System;
using UnityEngine;

namespace CurseVR.VoiceControl.VAD
{
    public class AudioClipBuffer
    {
        private readonly float[] buffer;
        private readonly int channels;
        private readonly int frequency;
        private int writePosition;
        private bool isFull;
        
        public event Action<AudioClip> OnBufferFilled;
        
        public AudioClipBuffer(int maxSampleLength, int frequency, int channels = 1)
        {
            this.buffer = new float[maxSampleLength * channels];
            this.frequency = frequency;
            this.channels = channels;
            this.writePosition = 0;
            this.isFull = false;
        }
        
        public void AddSamples(float[] samples)
        {
            if (samples == null || samples.Length == 0) return;
            
            int samplesToWrite = Math.Min(samples.Length, buffer.Length - writePosition);
            Array.Copy(samples, 0, buffer, writePosition, samplesToWrite);
            
            writePosition += samplesToWrite;
            
            if (writePosition >= buffer.Length)
            {
                isFull = true;
                CreateAndEmitAudioClip();
                Reset();
            }
        }
        
        public void Flush()
        {
            if (writePosition > 0)
            {
                CreateAndEmitAudioClip();
                Reset();
            }
        }
        
        private void CreateAndEmitAudioClip()
        {
            int sampleCount = isFull ? buffer.Length : writePosition;
            if (sampleCount == 0) return;
            
            var clip = AudioClip.Create(
                "VoiceBuffer", 
                sampleCount / channels, 
                channels, 
                frequency, 
                false);
                
            float[] data = new float[sampleCount];
            Array.Copy(buffer, data, sampleCount);
            clip.SetData(data, 0);
            
            OnBufferFilled?.Invoke(clip);
        }
        
        private void Reset()
        {
            writePosition = 0;
            isFull = false;
            Array.Clear(buffer, 0, buffer.Length);
        }
    }
} 