using UnityEngine;

namespace ChatUnityAI.Examples
{
    /// <summary>
    /// Example custom component that can be added through the AI Chat Assistant
    /// This demonstrates how to create components that work with the AI system
    /// </summary>
    public class CustomComponentExample : MonoBehaviour
    {
        [Header("AI Assistant Compatible Properties")]
        [Tooltip("This property can be set through AI commands")]
        public float customValue = 1.0f;
        
        [Tooltip("This property can be controlled via AI")]
        public bool isEnabled = true;
        
        [Tooltip("Color property that can be set through AI")]
        public Color customColor = Color.white;
        
        [Header("Behavior Settings")]
        [Tooltip("How fast this component updates")]
        public float updateSpeed = 1.0f;
        
        [Tooltip("Whether this component should auto-start")]
        public bool autoStart = true;

        private void Start()
        {
            if (autoStart)
            {
                Initialize();
            }
        }

        private void Update()
        {
            if (isEnabled)
            {
                // Custom update logic here
                UpdateBehavior();
            }
        }

        /// <summary>
        /// Initialize the component
        /// </summary>
        public void Initialize()
        {
            Debug.Log($"CustomComponentExample initialized with value: {customValue}");
        }

        /// <summary>
        /// Custom update behavior
        /// </summary>
        private void UpdateBehavior()
        {
            // Example behavior - rotate the object
            transform.Rotate(Vector3.up * updateSpeed * Time.deltaTime);
        }

        /// <summary>
        /// Method to set properties that can be called by AI system
        /// </summary>
        /// <param name="value">The value to set</param>
        public void SetCustomValue(float value)
        {
            customValue = value;
            Debug.Log($"Custom value set to: {value}");
        }

        /// <summary>
        /// Method to set the enabled state
        /// </summary>
        /// <param name="enabled">Whether the component should be enabled</param>
        public void SetEnabled(bool enabled)
        {
            isEnabled = enabled;
            Debug.Log($"Component enabled: {enabled}");
        }

        /// <summary>
        /// Method to set the custom color
        /// </summary>
        /// <param name="color">The color to set</param>
        public void SetCustomColor(Color color)
        {
            customColor = color;
            
            // Apply color to renderer if available
            var renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = color;
            }
            
            Debug.Log($"Custom color set to: {color}");
        }
    }
}

/*
 * TO INTEGRATE THIS WITH THE AI CHAT ASSISTANT:
 * 
 * 1. Add this case to the AddComponent method in CommandExecutor.cs:
 * 
 *    case "customcomponentexample":
 *        component = gameObject.GetComponent<CustomComponentExample>() ?? gameObject.AddComponent<CustomComponentExample>();
 *        SetCustomComponentProperties(component as CustomComponentExample, properties);
 *        break;
 * 
 * 2. Add this method to CommandExecutor.cs:
 * 
 *    private void SetCustomComponentProperties(CustomComponentExample customComponent, UnityCommand.CommandProperties properties)
 *    {
 *        if (customComponent == null || properties == null)
 *            return;
 * 
 *        // Set properties based on AI command
 *        if (properties.customValue != 0)
 *            customComponent.SetCustomValue(properties.customValue);
 *            
 *        customComponent.SetEnabled(properties.isEnabled);
 *    }
 * 
 * 3. Extend the CommandProperties class in OpenAIConnector.cs to include:
 * 
 *    public float customValue = 1.0f;
 *    public bool isEnabled = true;
 * 
 * 4. Update the system prompt in OpenAIConnector.cs to include information about your custom component:
 * 
 *    "CustomComponentExample: A custom component with customValue, isEnabled properties"
 * 
 * EXAMPLE AI PROMPTS:
 * - "Create a red cube with CustomComponentExample"
 * - "Make a sphere with CustomComponentExample and customValue 5"
 * - "Create a blue cylinder with CustomComponentExample enabled"
 */ 