using System;
using System.Collections.Generic;
using UnityEngine;

namespace CurseVR.VoiceControl.Models
{
    /// <summary>
    /// Data structure for voice commands received from the ASR service.
    /// </summary>
    /// <remarks>
    /// This class represents a processed voice command after recognition by the ASR service.
    /// It contains the recognized command type, the original transcribed text, confidence level,
    /// and any parameters extracted from the command. This object is passed to subscribers
    /// of the OnCommandRecognized event in the voice command service.
    /// </remarks>
    [Serializable]
    public class VoiceCommandData
    {
        /// <summary>
        /// Gets or sets the type of command recognized.
        /// </summary>
        /// <value>A string identifier for the command category (e.g., "move", "select", "open")</value>
        /// <remarks>
        /// This field represents the classified intent of the voice command after natural language
        /// processing. It is used to determine which action should be performed in response to
        /// the command.
        /// </remarks>
        public string CommandType { get; set; }
        
        /// <summary>
        /// Gets or sets the raw text transcribed from speech.
        /// </summary>
        /// <value>The unprocessed text as transcribed by the ASR service</value>
        /// <remarks>
        /// This is the direct output from the speech-to-text process before any intent
        /// recognition or parameter extraction. Useful for debugging or displaying exactly
        /// what was heard.
        /// </remarks>
        public string RawText { get; set; }
        
        /// <summary>
        /// Gets or sets the confidence score of the recognition.
        /// </summary>
        /// <value>A float between 0.0 and 1.0 representing recognition confidence</value>
        /// <remarks>
        /// Higher values indicate greater confidence in the accuracy of the transcription.
        /// This can be used to filter out potentially misheard commands or to request
        /// confirmation from the user when confidence is low.
        /// </remarks>
        public float Confidence { get; set; }
        
        /// <summary>
        /// Gets or sets additional parameters associated with the command.
        /// </summary>
        /// <value>A dictionary mapping parameter names to their values</value>
        /// <remarks>
        /// These parameters are extracted from the speech by the natural language understanding
        /// component. For example, in "move forward three meters", "direction" might be "forward"
        /// and "distance" might be 3.0. Parameter values can be of various types (string, number, boolean).
        /// </remarks>
        public Dictionary<string, object> Parameters { get; set; }
        
        /// <summary>
        /// Gets or sets the timestamp when the command was recognized.
        /// </summary>
        /// <value>DateTime indicating when the command was processed</value>
        /// <remarks>
        /// This timestamp can be used to implement command timeout or to synchronize 
        /// commands with other events in the application. It is set to UTC time
        /// when the object is created.
        /// </remarks>
        public DateTime Timestamp { get; set; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="VoiceCommandData"/> class.
        /// </summary>
        /// <remarks>
        /// Creates a new voice command data object with an empty parameter dictionary
        /// and sets the timestamp to the current UTC time.
        /// </remarks>
        public VoiceCommandData()
        {
            Parameters = new Dictionary<string, object>();
            Timestamp = DateTime.UtcNow;
        }
    }
}
