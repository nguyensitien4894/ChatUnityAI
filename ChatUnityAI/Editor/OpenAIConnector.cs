using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace ChatUnityAI.Editor
{
    /// <summary>
    /// Handles communication with OpenAI API to process natural language requests
    /// and return structured JSON commands for Unity scene manipulation
    /// </summary>
    public class OpenAIConnector
    {
        #region Constants
        private const string OPENAI_API_URL = "https://api.openai.com/v1/chat/completions";
        private const string MODEL_NAME = "gpt-3.5-turbo";
        private const int MAX_TOKENS = 1000;
        private const float TEMPERATURE = 0.7f;
        #endregion

        #region Private Fields
        private string apiKey;
        #endregion

        #region Public Methods
        /// <summary>
        /// Sets the OpenAI API key for authentication
        /// </summary>
        /// <param name="key">The OpenAI API key</param>
        public void SetApiKey(string key)
        {
            apiKey = key;
        }

        /// <summary>
        /// Processes a natural language request and returns a structured command
        /// </summary>
        /// <param name="userRequest">The user's natural language request</param>
        /// <returns>A UnityCommand object containing the structured response</returns>
        public async Task<UnityCommand> ProcessRequest(string userRequest)
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new Exception("OpenAI API key is not set");
            }

            try
            {
                var requestData = BuildRequestData(userRequest);
                var jsonResponse = await SendHttpRequest(requestData);
                var command = ParseOpenAIResponse(jsonResponse);
                return command;
            }
            catch (Exception ex)
            {
                Debug.LogError($"OpenAI API Error: {ex.Message}");
                throw;
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Builds the request data for OpenAI API
        /// </summary>
        /// <param name="userRequest">The user's request</param>
        /// <returns>JSON string for the API request</returns>
        private string BuildRequestData(string userRequest)
        {
            var systemPrompt = GetSystemPrompt();
            
            var requestObject = new
            {
                model = MODEL_NAME,
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userRequest }
                },
                max_tokens = MAX_TOKENS,
                temperature = TEMPERATURE
            };

            return JsonUtility.ToJson(requestObject);
        }

        /// <summary>
        /// Gets the system prompt that instructs GPT on how to respond
        /// </summary>
        /// <returns>The system prompt string</returns>
        private string GetSystemPrompt()
        {
            return @"You are a Unity Editor assistant that converts natural language requests into structured JSON commands for Unity scene manipulation.

IMPORTANT: Always respond with ONLY valid JSON in the following format:

{
    ""action"": ""create_object"",
    ""name"": ""ObjectName"",
    ""primitive"": ""Cube"",
    ""position"": [0, 0, 0],
    ""rotation"": [0, 0, 0],
    ""scale"": [1, 1, 1],
    ""color"": ""red"",
    ""components"": [""Rigidbody"", ""BoxCollider""],
    ""properties"": {
        ""mass"": 1.0,
        ""useGravity"": true
    }
}

RULES:
1. action: Always use 'create_object' for creating GameObjects
2. name: Generate a descriptive name for the object
3. primitive: Use Unity primitives: 'Cube', 'Sphere', 'Capsule', 'Cylinder', 'Plane', 'Quad'
4. position: Array of [x, y, z] coordinates
5. rotation: Array of [x, y, z] Euler angles in degrees
6. scale: Array of [x, y, z] scale values
7. color: Use color names like 'red', 'blue', 'green', 'yellow', 'white', 'black', 'gray', 'orange', 'purple', 'pink'
8. components: Array of Unity component names like 'Rigidbody', 'BoxCollider', 'SphereCollider', 'MeshCollider', 'CharacterController', 'Light', 'Camera'
9. properties: Object with component-specific properties

EXAMPLES:
- 'Create a red cube' → name: 'RedCube', primitive: 'Cube', color: 'red'
- 'Make a sphere that bounces' → add 'Rigidbody' component
- 'Create at position (2, 0, 0)' → position: [2, 0, 0]
- 'Scale to 2x' → scale: [2, 2, 2]

Always respond with valid JSON only. No explanations or additional text.";
        }

        /// <summary>
        /// Sends HTTP request to OpenAI API
        /// </summary>
        /// <param name="requestData">JSON request data</param>
        /// <returns>JSON response from OpenAI</returns>
        private async Task<string> SendHttpRequest(string requestData)
        {
            var request = new UnityWebRequest(OPENAI_API_URL, "POST");
            
            // Set request headers
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {apiKey}");
            
            // Set request body
            byte[] requestBytes = Encoding.UTF8.GetBytes(requestData);
            request.uploadHandler = new UploadHandlerRaw(requestBytes);
            request.downloadHandler = new DownloadHandlerBuffer();

            // Send request
            var operation = request.SendWebRequest();
            
            while (!operation.isDone)
            {
                await Task.Delay(100);
            }

            if (request.result == UnityWebRequest.Result.Success)
            {
                return request.downloadHandler.text;
            }
            else
            {
                var errorMessage = $"HTTP Error {request.responseCode}: {request.error}";
                if (!string.IsNullOrEmpty(request.downloadHandler.text))
                {
                    errorMessage += $"\nResponse: {request.downloadHandler.text}";
                }
                throw new Exception(errorMessage);
            }
        }

        /// <summary>
        /// Parses the OpenAI API response and extracts the command
        /// </summary>
        /// <param name="jsonResponse">The JSON response from OpenAI</param>
        /// <returns>A UnityCommand object</returns>
        private UnityCommand ParseOpenAIResponse(string jsonResponse)
        {
            try
            {
                var response = JsonUtility.FromJson<OpenAIResponse>(jsonResponse);
                
                if (response?.choices == null || response.choices.Length == 0)
                {
                    throw new Exception("No response from OpenAI API");
                }

                var content = response.choices[0].message.content.Trim();
                
                // Handle cases where the response might have extra text
                if (content.StartsWith("```json"))
                {
                    content = content.Substring(7);
                }
                if (content.EndsWith("```"))
                {
                    content = content.Substring(0, content.Length - 3);
                }
                
                content = content.Trim();
                
                // Parse the command JSON
                var command = JsonUtility.FromJson<UnityCommand>(content);
                
                if (command == null)
                {
                    throw new Exception("Failed to parse command from OpenAI response");
                }
                
                return command;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to parse OpenAI response: {ex.Message}");
                Debug.LogError($"Response content: {jsonResponse}");
                throw new Exception($"Failed to parse OpenAI response: {ex.Message}");
            }
        }
        #endregion

        #region Data Classes
        /// <summary>
        /// Represents the structure of OpenAI API response
        /// </summary>
        [System.Serializable]
        public class OpenAIResponse
        {
            public Choice[] choices;
            
            [System.Serializable]
            public class Choice
            {
                public Message message;
                
                [System.Serializable]
                public class Message
                {
                    public string content;
                }
            }
        }
        #endregion
    }

    /// <summary>
    /// Represents a structured command for Unity scene manipulation
    /// </summary>
    [System.Serializable]
    public class UnityCommand
    {
        public string action;
        public string name;
        public string primitive;
        public float[] position;
        public float[] rotation;
        public float[] scale;
        public string color;
        public string[] components;
        public CommandProperties properties;
        
        [System.Serializable]
        public class CommandProperties
        {
            public float mass = 1.0f;
            public bool useGravity = true;
            public bool isTrigger = false;
            public float intensity = 1.0f;
            public string lightType = "Directional";
        }
    }
} 