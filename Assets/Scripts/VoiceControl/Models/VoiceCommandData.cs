using System;
using System.Collections.Generic;
using UnityEngine;

namespace CurseVR.VoiceControl.Models
{
    /// <summary>
    /// Data structure for voice commands received from the API
    /// </summary>
    [Serializable]
    public class VoiceCommandData
    {
        /// <summary>
        /// The type of command recognized
        /// </summary>
        public string CommandType { get; set; }
        
        /// <summary>
        /// The raw text transcribed from speech
        /// </summary>
        public string RawText { get; set; }
        
        /// <summary>
        /// The confidence score of the recognition
        /// </summary>
        public float Confidence { get; set; }
        
        /// <summary>
        /// Additional parameters associated with the command
        /// </summary>
        public Dictionary<string, object> Parameters { get; set; }
        
        /// <summary>
        /// Timestamp when the command was recognized
        /// </summary>
        public DateTime Timestamp { get; set; }
        
        public VoiceCommandData()
        {
            Parameters = new Dictionary<string, object>();
            Timestamp = DateTime.UtcNow;
        }
    }
}
