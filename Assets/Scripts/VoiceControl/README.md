# Voice Control System for Unity

A voice control system that integrates with Unity's new Input System, using Voice Activity Detection (VAD) and a WebSocket-based ASR service.

## Features

- Real-time voice activity detection
- WebSocket-based communication with ASR service
- Integration with Unity's new Input System
- Debug tools and visualization
- Custom editor interface
- Audio utilities for processing voice data

## Setup

1. Install required dependencies:
   - Voice Activity Detection (VAD) library
   - NativeWebSocket
   - UniRx
   - Newtonsoft.Json

2. Configure the Input System:
   - Create a new Input Action Asset if you haven't already
   - Add a new Action Map named "Voice"
   - Add actions for each voice command you want to support

3. Set up the Voice Input Manager:
   - Create an empty GameObject in your scene
   - Add the `VoiceInputManager` component
   - Assign your Input Action Asset
   - Configure the VAD Parameters
   - Set up the Voice Service Configuration
   - (Optional) Add an Audio Source for debug playback

## Configuration

### Voice Service Config
```json
{
    "WebSocketUrl": "ws://localhost:8000/ws/asr/",
    "ClientId": "unity_client",
    "SampleRate": 16000,
    "Channels": 1,
    "BufferSize": 2048,
    "MaxReconnectAttempts": 3,
    "ReconnectDelayMs": 1000
}
```

### VAD Parameters
Configure these in the Unity Inspector:
- Max/Min Queueing Time
- Volume Threshold
- Activation/Inactivation Rates
- Intervals
- Max Active Duration

## Usage

1. Add voice commands to your Input Action Asset:
```csharp
// Example in code
var voiceActionMap = inputActions.FindActionMap("Voice");
var moveCommand = voiceActionMap.AddAction("move");
var interactCommand = voiceActionMap.AddAction("interact");
```

2. Subscribe to voice commands:
```csharp
voiceActionMap["move"].performed += ctx => HandleMoveCommand(ctx);
voiceActionMap["interact"].performed += ctx => HandleInteractCommand(ctx);
```

3. The VoiceInputManager will automatically:
   - Detect voice activity
   - Send audio to the ASR service
   - Receive command recognition results
   - Trigger appropriate Input System actions

## Debugging

The custom editor provides several debugging tools:
- Microphone testing
- WebSocket connection testing
- Audio playback visualization
- Voice activity monitoring

## Project Structure

```
Assets/Scripts/VoiceControl/
├── Core/
│   └── IVoiceCommandService.cs
├── Models/
│   ├── VoiceCommandData.cs
│   └── VoiceServiceConfig.cs
├── Services/
│   └── VoiceCommandService.cs
├── Input/
│   └── VoiceInputManager.cs
├── Utils/
│   └── AudioUtils.cs
└── Editor/
    └── VoiceInputManagerEditor.cs
```

## Best Practices

1. Always check the connection status before sending audio data
2. Use the debug audio source in development
3. Configure VAD parameters based on your environment
4. Test with various microphones and noise levels
5. Handle connection errors gracefully

## Known Issues

- WebSocket connection may require reconnection after long periods
- VAD might need tuning for different environments
- Debug audio playback may have latency

## Contributing

Feel free to contribute to this project by:
1. Reporting issues
2. Suggesting improvements
3. Creating pull requests

## License

This project is licensed under the MIT License - see the LICENSE file for details 