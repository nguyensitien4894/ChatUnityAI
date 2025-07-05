using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace ChatUnityAI.Editor
{
    /// <summary>
    /// Executes structured commands in Unity Editor to create and modify GameObjects
    /// </summary>
    public class CommandExecutor
    {
        #region Data Classes
        /// <summary>
        /// Represents the result of a command execution
        /// </summary>
        public class ExecutionResult
        {
            public bool success;
            public string message;
            public GameObject createdObject;

            public ExecutionResult(bool success, string message, GameObject createdObject = null)
            {
                this.success = success;
                this.message = message;
                this.createdObject = createdObject;
            }
        }
        #endregion

        #region Color Mapping
        private static readonly Dictionary<string, Color> ColorMap = new Dictionary<string, Color>
        {
            { "red", Color.red },
            { "blue", Color.blue },
            { "green", Color.green },
            { "yellow", Color.yellow },
            { "white", Color.white },
            { "black", Color.black },
            { "gray", Color.gray },
            { "grey", Color.gray },
            { "orange", new Color(1f, 0.5f, 0f) },
            { "purple", new Color(0.5f, 0f, 1f) },
            { "pink", new Color(1f, 0.5f, 0.5f) },
            { "cyan", Color.cyan },
            { "magenta", Color.magenta },
            { "brown", new Color(0.6f, 0.3f, 0.1f) }
        };
        #endregion

        #region Public Methods
        /// <summary>
        /// Executes a Unity command and returns the result
        /// </summary>
        /// <param name="command">The command to execute</param>
        /// <returns>Execution result</returns>
        public ExecutionResult ExecuteCommand(UnityCommand command)
        {
            if (command == null)
            {
                return new ExecutionResult(false, "Command is null");
            }

            try
            {
                switch (command.action?.ToLower())
                {
                    case "create_object":
                        return CreateObject(command);
                    default:
                        return new ExecutionResult(false, $"Unknown action: {command.action}");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Command execution error: {ex.Message}");
                return new ExecutionResult(false, $"Execution error: {ex.Message}");
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Creates a GameObject based on the command specifications
        /// </summary>
        /// <param name="command">The creation command</param>
        /// <returns>Execution result</returns>
        private ExecutionResult CreateObject(UnityCommand command)
        {
            try
            {
                // Create primitive GameObject
                GameObject gameObject = CreatePrimitive(command.primitive);
                
                if (gameObject == null)
                {
                    return new ExecutionResult(false, $"Failed to create primitive: {command.primitive}");
                }

                // Set object name
                if (!string.IsNullOrEmpty(command.name))
                {
                    gameObject.name = command.name;
                }

                // Set transform properties
                SetTransformProperties(gameObject, command);

                // Set color/material
                SetObjectColor(gameObject, command.color);

                // Add components
                AddComponents(gameObject, command.components, command.properties);

                // Register undo operation
                Undo.RegisterCreatedObjectUndo(gameObject, "Create AI Object");

                // Select the created object
                Selection.activeGameObject = gameObject;

                // Focus on the object in Scene view
                SceneView.FrameLastActiveSceneView();

                var successMessage = $"Successfully created {gameObject.name}";
                if (command.components != null && command.components.Length > 0)
                {
                    successMessage += $" with components: {string.Join(", ", command.components)}";
                }

                return new ExecutionResult(true, successMessage, gameObject);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to create object: {ex.Message}");
                return new ExecutionResult(false, $"Failed to create object: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates a primitive GameObject
        /// </summary>
        /// <param name="primitiveType">The type of primitive to create</param>
        /// <returns>Created GameObject</returns>
        private GameObject CreatePrimitive(string primitiveType)
        {
            if (string.IsNullOrEmpty(primitiveType))
            {
                primitiveType = "Cube";
            }

            switch (primitiveType.ToLower())
            {
                case "cube":
                    return GameObject.CreatePrimitive(PrimitiveType.Cube);
                case "sphere":
                    return GameObject.CreatePrimitive(PrimitiveType.Sphere);
                case "capsule":
                    return GameObject.CreatePrimitive(PrimitiveType.Capsule);
                case "cylinder":
                    return GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                case "plane":
                    return GameObject.CreatePrimitive(PrimitiveType.Plane);
                case "quad":
                    return GameObject.CreatePrimitive(PrimitiveType.Quad);
                default:
                    Debug.LogWarning($"Unknown primitive type: {primitiveType}. Using Cube instead.");
                    return GameObject.CreatePrimitive(PrimitiveType.Cube);
            }
        }

        /// <summary>
        /// Sets the transform properties of a GameObject
        /// </summary>
        /// <param name="gameObject">The GameObject to modify</param>
        /// <param name="command">The command containing transform data</param>
        private void SetTransformProperties(GameObject gameObject, UnityCommand command)
        {
            var transform = gameObject.transform;

            // Set position
            if (command.position != null && command.position.Length >= 3)
            {
                transform.position = new Vector3(
                    command.position[0],
                    command.position[1],
                    command.position[2]
                );
            }

            // Set rotation
            if (command.rotation != null && command.rotation.Length >= 3)
            {
                transform.rotation = Quaternion.Euler(
                    command.rotation[0],
                    command.rotation[1],
                    command.rotation[2]
                );
            }

            // Set scale
            if (command.scale != null && command.scale.Length >= 3)
            {
                transform.localScale = new Vector3(
                    command.scale[0],
                    command.scale[1],
                    command.scale[2]
                );
            }
        }

        /// <summary>
        /// Sets the color/material of a GameObject
        /// </summary>
        /// <param name="gameObject">The GameObject to modify</param>
        /// <param name="colorName">The name of the color to apply</param>
        private void SetObjectColor(GameObject gameObject, string colorName)
        {
            if (string.IsNullOrEmpty(colorName))
                return;

            var renderer = gameObject.GetComponent<Renderer>();
            if (renderer == null)
                return;

            // Get color from mapping
            Color color = Color.white;
            if (ColorMap.ContainsKey(colorName.ToLower()))
            {
                color = ColorMap[colorName.ToLower()];
            }
            else
            {
                Debug.LogWarning($"Unknown color: {colorName}. Using white instead.");
            }

            // Create new material with the specified color
            var material = new Material(Shader.Find("Standard"))
            {
                color = color,
                name = $"{colorName}Material"
            };

            renderer.material = material;
        }

        /// <summary>
        /// Adds components to a GameObject
        /// </summary>
        /// <param name="gameObject">The GameObject to modify</param>
        /// <param name="components">Array of component names to add</param>
        /// <param name="properties">Properties to set on components</param>
        private void AddComponents(GameObject gameObject, string[] components, UnityCommand.CommandProperties properties)
        {
            if (components == null || components.Length == 0)
                return;

            foreach (var componentName in components)
            {
                AddComponent(gameObject, componentName, properties);
            }
        }

        /// <summary>
        /// Adds a single component to a GameObject
        /// </summary>
        /// <param name="gameObject">The GameObject to modify</param>
        /// <param name="componentName">The name of the component to add</param>
        /// <param name="properties">Properties to set on the component</param>
        private void AddComponent(GameObject gameObject, string componentName, UnityCommand.CommandProperties properties)
        {
            if (string.IsNullOrEmpty(componentName))
                return;

            try
            {
                Component component = null;

                switch (componentName.ToLower())
                {
                    case "rigidbody":
                        component = gameObject.GetComponent<Rigidbody>() ?? gameObject.AddComponent<Rigidbody>();
                        SetRigidbodyProperties(component as Rigidbody, properties);
                        break;

                    case "boxcollider":
                        component = gameObject.GetComponent<BoxCollider>() ?? gameObject.AddComponent<BoxCollider>();
                        SetColliderProperties(component as Collider, properties);
                        break;

                    case "spherecollider":
                        component = gameObject.GetComponent<SphereCollider>() ?? gameObject.AddComponent<SphereCollider>();
                        SetColliderProperties(component as Collider, properties);
                        break;

                    case "capsulecollider":
                        component = gameObject.GetComponent<CapsuleCollider>() ?? gameObject.AddComponent<CapsuleCollider>();
                        SetColliderProperties(component as Collider, properties);
                        break;

                    case "meshcollider":
                        component = gameObject.GetComponent<MeshCollider>() ?? gameObject.AddComponent<MeshCollider>();
                        SetColliderProperties(component as Collider, properties);
                        break;

                    case "charactercontroller":
                        component = gameObject.GetComponent<CharacterController>() ?? gameObject.AddComponent<CharacterController>();
                        break;

                    case "light":
                        component = gameObject.GetComponent<Light>() ?? gameObject.AddComponent<Light>();
                        SetLightProperties(component as Light, properties);
                        break;

                    case "camera":
                        component = gameObject.GetComponent<Camera>() ?? gameObject.AddComponent<Camera>();
                        break;

                    case "audiosource":
                        component = gameObject.GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
                        break;

                    default:
                        Debug.LogWarning($"Unknown component: {componentName}");
                        break;
                }

                if (component != null)
                {
                    Debug.Log($"Added component: {componentName} to {gameObject.name}");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to add component {componentName}: {ex.Message}");
            }
        }

        /// <summary>
        /// Sets properties on a Rigidbody component
        /// </summary>
        /// <param name="rigidbody">The Rigidbody component</param>
        /// <param name="properties">Properties to set</param>
        private void SetRigidbodyProperties(Rigidbody rigidbody, UnityCommand.CommandProperties properties)
        {
            if (rigidbody == null || properties == null)
                return;

            rigidbody.mass = properties.mass;
            rigidbody.useGravity = properties.useGravity;
        }

        /// <summary>
        /// Sets properties on a Collider component
        /// </summary>
        /// <param name="collider">The Collider component</param>
        /// <param name="properties">Properties to set</param>
        private void SetColliderProperties(Collider collider, UnityCommand.CommandProperties properties)
        {
            if (collider == null || properties == null)
                return;

            collider.isTrigger = properties.isTrigger;
        }

        /// <summary>
        /// Sets properties on a Light component
        /// </summary>
        /// <param name="light">The Light component</param>
        /// <param name="properties">Properties to set</param>
        private void SetLightProperties(Light light, UnityCommand.CommandProperties properties)
        {
            if (light == null || properties == null)
                return;

            light.intensity = properties.intensity;
            
            switch (properties.lightType?.ToLower())
            {
                case "directional":
                    light.type = LightType.Directional;
                    break;
                case "point":
                    light.type = LightType.Point;
                    break;
                case "spot":
                    light.type = LightType.Spot;
                    break;
                default:
                    light.type = LightType.Directional;
                    break;
            }
        }
        #endregion
    }
} 