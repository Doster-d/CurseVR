using System;
using UnityEngine;

namespace CurseVR.VoiceControl.VAD
{
    /// <summary>
    /// Buffers audio samples and creates AudioClips when the buffer is filled or flushed.
    /// </summary>
    /// <remarks>
    /// This class provides a mechanism to collect audio samples over time and create
    /// AudioClip instances when specific conditions are met (buffer full or manually flushed).
    /// It's primarily used to collect voice data detected by the voice activity detection
    /// system for further processing.
    /// </remarks>
    public class AudioClipBuffer
    {
        private readonly float[] buffer;
        private readonly int channels;
        private readonly int frequency;
        private int writePosition;
        private bool isFull;
        
        /// <summary>
        /// Event triggered when the buffer is filled or flushed with sufficient data to create an AudioClip.
        /// </summary>
        /// <remarks>
        /// Subscribers will receive the newly created AudioClip containing the buffered audio data.
        /// This is typically used to process voice data for recognition or transmission.
        /// </remarks>
        public event Action<AudioClip> OnBufferFilled;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="AudioClipBuffer"/> class.
        /// </summary>
        /// <param name="maxSampleLength">Maximum number of samples (per channel) to buffer</param>
        /// <param name="frequency">Sample rate in Hz (e.g., 16000 for 16kHz audio)</param>
        /// <param name="channels">Number of audio channels (1 for mono, 2 for stereo)</param>
        /// <remarks>
        /// The total buffer size will be maxSampleLength * channels floating-point values.
        /// For voice recognition, typical values might be 16000 samples at 16kHz for 1 second of audio.
        /// </remarks>
        public AudioClipBuffer(int maxSampleLength, int frequency, int channels = 1)
        {
            this.buffer = new float[maxSampleLength * channels];
            this.frequency = frequency;
            this.channels = channels;
            this.writePosition = 0;
            this.isFull = false;
        }
        
        /// <summary>
        /// Adds audio samples to the buffer.
        /// </summary>
        /// <param name="samples">Array of audio samples to add</param>
        /// <remarks>
        /// This method adds audio data to the internal buffer. If the buffer becomes full,
        /// it automatically creates an AudioClip, fires the OnBufferFilled event, and resets the buffer.
        /// The samples should be in the correct format (interleaved if multi-channel) matching
        /// the buffer's configuration.
        /// </remarks>
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
        
        /// <summary>
        /// Flushes the buffer, creating an AudioClip with current data regardless of buffer fullness.
        /// </summary>
        /// <remarks>
        /// This is typically called when voice activity ends to process any remaining audio
        /// that hasn't filled the buffer yet. If the buffer is empty (writePosition == 0),
        /// no AudioClip will be created.
        /// </remarks>
        public void Flush()
        {
            if (writePosition > 0)
            {
                CreateAndEmitAudioClip();
                Reset();
            }
        }
        
        /// <summary>
        /// Creates an AudioClip from the current buffer content and triggers the OnBufferFilled event.
        /// </summary>
        /// <remarks>
        /// Creates a new Unity AudioClip with the appropriate sample rate and channel count,
        /// copies the buffered data to it, and notifies subscribers via the OnBufferFilled event.
        /// </remarks>
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
        
        /// <summary>
        /// Resets the buffer to its initial empty state.
        /// </summary>
        /// <remarks>
        /// Clears all data in the buffer, resets the write position, and marks the buffer as not full.
        /// Called automatically after an AudioClip is created and the OnBufferFilled event is triggered.
        /// </remarks>
        private void Reset()
        {
            writePosition = 0;
            isFull = false;
            Array.Clear(buffer, 0, buffer.Length);
        }
    }
} 