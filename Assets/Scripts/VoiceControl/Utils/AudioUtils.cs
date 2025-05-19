using UnityEngine;

namespace CurseVR.VoiceControl.Utils
{
    /// <summary>
    /// Utility class for audio processing operations
    /// </summary>
    public static class AudioUtils
    {
        /// <summary>
        /// Converts an AudioClip to a byte array in 16-bit PCM format
        /// </summary>
        public static byte[] AudioClipToBytes(AudioClip clip)
        {
            if (clip == null) return null;
            
            float[] samples = new float[clip.samples * clip.channels];
            clip.GetData(samples, 0);
            
            byte[] audioData = new byte[samples.Length * 2];
            for (int i = 0; i < samples.Length; i++)
            {
                short value = (short)(samples[i] * 32767f);
                audioData[i * 2] = (byte)(value & 0xff);
                audioData[i * 2 + 1] = (byte)((value >> 8) & 0xff);
            }
            
            return audioData;
        }
        
        /// <summary>
        /// Converts raw PCM data to an AudioClip
        /// </summary>
        public static AudioClip BytesToAudioClip(byte[] audioData, int channels, int frequency)
        {
            if (audioData == null) return null;
            
            float[] samples = new float[audioData.Length / 2];
            for (int i = 0; i < samples.Length; i++)
            {
                short value = (short)((audioData[i * 2 + 1] << 8) | audioData[i * 2]);
                samples[i] = value / 32768f;
            }
            
            AudioClip clip = AudioClip.Create("ConvertedClip", samples.Length / channels, channels, frequency, false);
            clip.SetData(samples, 0);
            
            return clip;
        }
        
        /// <summary>
        /// Calculates the RMS volume of an audio buffer
        /// </summary>
        public static float CalculateRMSVolume(float[] samples)
        {
            float sum = 0f;
            for (int i = 0; i < samples.Length; i++)
            {
                sum += samples[i] * samples[i];
            }
            
            return Mathf.Sqrt(sum / samples.Length);
        }
        
        /// <summary>
        /// Applies a simple noise gate to the audio data
        /// </summary>
        public static void ApplyNoiseGate(float[] samples, float threshold)
        {
            for (int i = 0; i < samples.Length; i++)
            {
                if (Mathf.Abs(samples[i]) < threshold)
                {
                    samples[i] = 0f;
                }
            }
        }
    }
}
