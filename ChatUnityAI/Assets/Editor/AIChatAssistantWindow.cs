using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChatUnityAI.Editor
{
    public class AIChatAssistantWindow : EditorWindow
    {
        #region Private Fields
        private string userInput = "";
        private string apiKey = "";
        private bool isProcessing = false;
        private Vector2 scrollPosition;
        private List<ChatMessage> chatHistory = new List<ChatMessage>();
        private OpenAIConnector openAIConnector;
        private CommandExecutor commandExecutor;
        private GUIStyle chatMessageStyle;
        private GUIStyle inputFieldStyle;
        private bool showApiKeyField = false;
        #endregion

        #region Constants
        private const string API_KEY_PREF = "ChatUnityAI_OpenAI_Key";
        private const string WINDOW_TITLE = "Unity AI Assistant";
        private const float MIN_WINDOW_WIDTH = 400f;
        private const float MIN_WINDOW_HEIGHT = 300f;
        #endregion

        #region Chat Message Class
        [System.Serializable]
        public class ChatMessage
        {
            public string sender;
            public string message;
            public bool isError;
            public System.DateTime timestamp;

            public ChatMessage(string sender, string message, bool isError = false)
            {
                this.sender = sender;
                this.message = message;
                this.isError = isError;
                this.timestamp = System.DateTime.Now;
            }
        }
        #endregion

        #region Menu Item
        [MenuItem("Tools/AI Chat Assistant")]
        public static void ShowWindow()
        {
            var window = GetWindow<AIChatAssistantWindow>(WINDOW_TITLE);
            window.minSize = new Vector2(MIN_WINDOW_WIDTH, MIN_WINDOW_HEIGHT);
            window.Show();
        }
        #endregion

        #region Unity Editor Callbacks
        private void OnEnable()
        {
            Initialize();
        }

        private void OnGUI()
        {
            SetupStyles();
            DrawHeader();
            DrawChatHistory();
            DrawInputArea();
            DrawFooter();
        }

        private void OnDestroy()
        {
            SavePreferences();
        }
        #endregion

        #region Initialization
        private void Initialize()
        {
            openAIConnector = new OpenAIConnector();
            commandExecutor = new CommandExecutor();
            LoadPreferences();
            
            // Add welcome message
            if (chatHistory.Count == 0)
            {
                chatHistory.Add(new ChatMessage("Assistant", 
                    "Welcome to Unity AI Assistant! I can help you create GameObjects, add components, and modify your scene. " +
                    "Try asking me to 'Create a red cube with Rigidbody and BoxCollider' or 'Make a blue sphere that bounces'."));
            }
        }

        private void LoadPreferences()
        {
            apiKey = EditorPrefs.GetString(API_KEY_PREF, "");
        }

        private void SavePreferences()
        {
            EditorPrefs.SetString(API_KEY_PREF, apiKey);
        }
        #endregion

        #region GUI Drawing
        private void SetupStyles()
        {
            if (chatMessageStyle == null)
            {
                chatMessageStyle = new GUIStyle(EditorStyles.helpBox)
                {
                    wordWrap = true,
                    padding = new RectOffset(10, 10, 10, 10),
                    margin = new RectOffset(5, 5, 5, 5)
                };
            }

            if (inputFieldStyle == null)
            {
                inputFieldStyle = new GUIStyle(EditorStyles.textField)
                {
                    wordWrap = true
                };
            }
        }

        private void DrawHeader()
        {
            EditorGUILayout.BeginHorizontal();
            
            GUILayout.Label("Unity AI Assistant", EditorStyles.boldLabel);
            
            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button("Settings", EditorStyles.miniButton, GUILayout.Width(60)))
            {
                showApiKeyField = !showApiKeyField;
            }
            
            if (GUILayout.Button("Clear", EditorStyles.miniButton, GUILayout.Width(50)))
            {
                ClearChat();
            }
            
            EditorGUILayout.EndHorizontal();
            
            if (showApiKeyField)
            {
                DrawApiKeyField();
            }
            
            EditorGUILayout.Space();
        }

        private void DrawApiKeyField()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("OpenAI API Configuration", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("API Key:", GUILayout.Width(60));
            apiKey = EditorGUILayout.PasswordField(apiKey);
            EditorGUILayout.EndHorizontal();
            
            if (string.IsNullOrEmpty(apiKey))
            {
                EditorGUILayout.HelpBox("Please enter your OpenAI API key to use the AI assistant.", MessageType.Warning);
            }
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        private void DrawChatHistory()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            foreach (var message in chatHistory)
            {
                DrawChatMessage(message);
            }
            
            EditorGUILayout.EndScrollView();
        }

        private void DrawChatMessage(ChatMessage message)
        {
            var style = new GUIStyle(chatMessageStyle);
            
            if (message.sender == "User")
            {
                style.normal.textColor = new Color(0.3f, 0.6f, 1f);
            }
            else if (message.isError)
            {
                style.normal.textColor = Color.red;
            }
            else
            {
                style.normal.textColor = new Color(0.2f, 0.8f, 0.2f);
            }

            EditorGUILayout.BeginVertical(style);
            
            // Message header
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"{message.sender}", EditorStyles.boldLabel, GUILayout.Width(80));
            EditorGUILayout.LabelField(message.timestamp.ToString("HH:mm:ss"), EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();
            
            // Message content
            EditorGUILayout.LabelField(message.message, EditorStyles.wordWrappedLabel);
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(2);
        }

        private void DrawInputArea()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.LabelField("Enter your request:", EditorStyles.boldLabel);
            
            // Input field
            userInput = EditorGUILayout.TextArea(userInput, inputFieldStyle, GUILayout.MinHeight(60));
            
            EditorGUILayout.BeginHorizontal();
            
            // Send button
            GUI.enabled = !isProcessing && !string.IsNullOrEmpty(userInput.Trim()) && !string.IsNullOrEmpty(apiKey);
            if (GUILayout.Button(isProcessing ? "Processing..." : "Send", GUILayout.Height(30)))
            {
                ProcessUserInput();
            }
            GUI.enabled = true;
            
            // Example button
            if (GUILayout.Button("Example", GUILayout.Width(70), GUILayout.Height(30)))
            {
                ShowExamplePrompts();
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }

        private void DrawFooter()
        {
            EditorGUILayout.BeginHorizontal();
            
            if (string.IsNullOrEmpty(apiKey))
            {
                EditorGUILayout.HelpBox("API Key required", MessageType.Warning);
            }
            else if (isProcessing)
            {
                EditorGUILayout.HelpBox("Processing request...", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("Ready to assist", MessageType.Info);
            }
            
            EditorGUILayout.EndHorizontal();
        }
        #endregion

        #region User Input Processing
        private async void ProcessUserInput()
        {
            if (string.IsNullOrEmpty(userInput.Trim()) || string.IsNullOrEmpty(apiKey))
                return;

            var inputMessage = userInput.Trim();
            userInput = "";
            isProcessing = true;

            // Add user message to chat
            chatHistory.Add(new ChatMessage("User", inputMessage));
            
            // Scroll to bottom
            scrollPosition.y = float.MaxValue;
            Repaint();

            try
            {
                // Call OpenAI API
                openAIConnector.SetApiKey(apiKey);
                var response = await openAIConnector.ProcessRequest(inputMessage);
                
                if (response != null)
                {
                    // Execute the command
                    var result = commandExecutor.ExecuteCommand(response);
                    
                    if (result.success)
                    {
                        chatHistory.Add(new ChatMessage("Assistant", result.message));
                    }
                    else
                    {
                        chatHistory.Add(new ChatMessage("Assistant", $"Error: {result.message}", true));
                    }
                }
                else
                {
                    chatHistory.Add(new ChatMessage("Assistant", "I couldn't process your request. Please try again.", true));
                }
            }
            catch (System.Exception ex)
            {
                chatHistory.Add(new ChatMessage("Assistant", $"Error: {ex.Message}", true));
                Debug.LogError($"AI Chat Assistant Error: {ex.Message}");
            }
            finally
            {
                isProcessing = false;
                Repaint();
            }
        }

        private void ShowExamplePrompts()
        {
            var menu = new GenericMenu();
            
            menu.AddItem(new GUIContent("Create a red cube with Rigidbody"), false, () => 
                userInput = "Create a red cube with Rigidbody and BoxCollider");
            
            menu.AddItem(new GUIContent("Make a blue sphere that bounces"), false, () => 
                userInput = "Create a blue sphere with Rigidbody that bounces");
            
            menu.AddItem(new GUIContent("Create a green capsule at position (2, 0, 0)"), false, () => 
                userInput = "Create a green capsule at position (2, 0, 0)");
            
            menu.AddItem(new GUIContent("Make a yellow cylinder with MeshCollider"), false, () => 
                userInput = "Create a yellow cylinder with MeshCollider");
            
            menu.AddItem(new GUIContent("Create a white plane scaled to (5, 1, 5)"), false, () => 
                userInput = "Create a white plane scaled to (5, 1, 5)");
            
            menu.ShowAsContext();
        }

        private void ClearChat()
        {
            chatHistory.Clear();
            chatHistory.Add(new ChatMessage("Assistant", 
                "Chat cleared. How can I help you create something in Unity?"));
            Repaint();
        }
        #endregion
    }
} 