# Unity AI Chat Assistant

A powerful Unity Editor extension that brings AI-powered scene creation to your fingertips. Create GameObjects, add components, and modify your scene using natural language commands powered by OpenAI's GPT API.

## 🚀 Features

- **Natural Language Processing**: Describe what you want to create in plain English
- **Intelligent GameObject Creation**: Automatically creates primitives with appropriate components
- **Smart Component Management**: Adds and configures Unity components based on context
- **Material & Color Assignment**: Automatically applies colors and materials
- **Transform Control**: Set position, rotation, and scale through natural language
- **Dockable UI**: Clean, intuitive interface that integrates seamlessly with Unity Editor
- **Undo Support**: Full undo/redo support for all operations
- **Error Handling**: Robust error handling with helpful feedback

## 📋 Requirements

- Unity 2021.3 or newer
- OpenAI API key ([Get one here](https://platform.openai.com/api-keys))
- Internet connection for API calls

## 🔧 Installation

1. **Download the Plugin**
   - Clone this repository or download the source files
   - Place the `Editor/` folder in your Unity project's `Assets/` directory

2. **Set Up OpenAI API Key**
   - Open Unity Editor
   - Go to `Tools > AI Chat Assistant`
   - Click the "Settings" button
   - Enter your OpenAI API key in the configuration field
   - The key is automatically saved in Unity's EditorPrefs

3. **Verify Installation**
   - The window should appear with a welcome message
   - Try an example prompt to test the connection

## 🎮 Usage

### Opening the Assistant

Access the AI Chat Assistant through the Unity menu:
```
Tools > AI Chat Assistant
```

### Example Prompts

Here are some example prompts to get you started:

#### Basic Object Creation
```
Create a red cube with Rigidbody and BoxCollider
```

#### Complex Objects
```
Make a blue sphere that bounces at position (2, 1, 0)
```

#### Scaled Objects
```
Create a green capsule scaled to (1, 2, 1) with MeshCollider
```

#### Multiple Components
```
Create a yellow cylinder with Rigidbody, BoxCollider, and AudioSource
```

#### Positioned Objects
```
Make a white plane at position (0, 0, 0) scaled to (5, 1, 5)
```

### Quick Examples Menu

Click the "Example" button in the interface to access pre-made prompts:
- Create a red cube with Rigidbody
- Make a blue sphere that bounces
- Create a green capsule at position (2, 0, 0)
- Make a yellow cylinder with MeshCollider
- Create a white plane scaled to (5, 1, 5)

## 🎯 Supported Features

### Primitives
- Cube
- Sphere
- Capsule
- Cylinder
- Plane
- Quad

### Components
- Rigidbody
- BoxCollider
- SphereCollider
- CapsuleCollider
- MeshCollider
- CharacterController
- Light
- Camera
- AudioSource

### Colors
- Red, Blue, Green, Yellow
- White, Black, Gray
- Orange, Purple, Pink
- Cyan, Magenta, Brown

### Transform Properties
- Position (x, y, z coordinates)
- Rotation (Euler angles)
- Scale (x, y, z scaling)

## 💡 Advanced Usage

### Component Properties

The AI assistant can set specific properties on components:

```
Create a heavy red cube with mass 5 and gravity disabled
```

```
Make a trigger sphere collider at position (0, 2, 0)
```

```
Create a bright directional light with intensity 2
```

### Batch Operations

You can describe multiple objects in one request:

```
Create a red cube and a blue sphere next to each other
```

## 🔧 Configuration

### API Key Management

The OpenAI API key is stored securely in Unity's EditorPrefs. You can:
- Update the key anytime through the Settings panel
- The key persists between Unity sessions
- Each project can have its own API key configuration

### Error Handling

The assistant provides detailed error messages for:
- Invalid API keys
- Network connectivity issues
- Malformed requests
- Unity-specific errors

## 📁 Project Structure

```
ChatUnityAI/
├── Editor/
│   ├── AIChatAssistantWindow.cs      # Main UI window
│   ├── OpenAIConnector.cs            # API communication
│   ├── CommandExecutor.cs            # Scene manipulation
│   └── ChatUnityAI.Editor.asmdef     # Assembly definition
└── README.md                         # This file
```

## 🐛 Troubleshooting

### Common Issues

**"API Key required" error**
- Solution: Enter your OpenAI API key in the Settings panel

**"Network error" or timeout**
- Check your internet connection
- Verify your API key is valid and has credits
- Try again after a few moments

**"Failed to parse response" error**
- The AI might have returned an unexpected format
- Try rephrasing your request more clearly
- Check the Unity Console for detailed error logs

**Components not being added**
- Ensure you're using supported component names
- Check the Unity Console for specific error messages
- Some components may require specific conditions to be added

### Performance Tips

- Be specific in your requests for better results
- Use the example prompts as templates
- Keep requests focused on single objects for best performance
- Check the Unity Console for detailed execution logs

## 🎨 Customization

### Extending Component Support

To add support for new components, modify the `AddComponent` method in `CommandExecutor.cs`:

```csharp
case "newcomponent":
    component = gameObject.GetComponent<NewComponent>() ?? gameObject.AddComponent<NewComponent>();
    // Set component properties
    break;
```

### Adding New Colors

Extend the `ColorMap` dictionary in `CommandExecutor.cs`:

```csharp
{ "newcolor", new Color(r, g, b) }
```

### Modifying AI Behavior

Adjust the system prompt in `OpenAIConnector.cs` to change how the AI interprets requests.

## 📝 API Reference

### UnityCommand Structure

The AI returns structured JSON in this format:

```json
{
    "action": "create_object",
    "name": "ObjectName",
    "primitive": "Cube",
    "position": [0, 0, 0],
    "rotation": [0, 0, 0],
    "scale": [1, 1, 1],
    "color": "red",
    "components": ["Rigidbody", "BoxCollider"],
    "properties": {
        "mass": 1.0,
        "useGravity": true
    }
}
```

## 🤝 Contributing

Contributions are welcome! Please feel free to submit issues, feature requests, or pull requests.

### Development Setup

1. Clone the repository
2. Open in Unity 2021.3 or newer
3. Set up your OpenAI API key
4. Test with the provided examples

## 📄 License

This project is open source. See the LICENSE file for details.

## 🔗 Links

- [OpenAI API Documentation](https://platform.openai.com/docs)
- [Unity Editor API Reference](https://docs.unity3d.com/ScriptReference/UnityEditor.html)
- [Unity Scripting API](https://docs.unity3d.com/ScriptReference/)

## 📊 Version History

### v1.0.0
- Initial release
- Basic object creation with natural language
- Component management
- Transform control
- Color/material assignment
- Undo support

---

**Made with ❤️ for the Unity community** 