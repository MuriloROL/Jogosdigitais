using System;
using System.IO;
using MCPForUnity.Editor.Services;
using UnityEditor;
using UnityEngine;

namespace ProjetoJogosDigitais.Editor
{
    /// <summary>
    /// Keeps the project-scoped Codex integration available whenever this project is open.
    /// </summary>
    [InitializeOnLoad]
    public static class UnityMcpAutoStart
    {
        private const string UseHttpTransportKey = "MCPForUnity.UseHttpTransport";
        private const string SetupCompletedKey = "MCPForUnity.SetupCompleted";
        private const string SetupDismissedKey = "MCPForUnity.SetupDismissed";
        private const string UvxPathKey = "MCPForUnity.UvxPath";
        private const string TelemetryDisabledKey = "MCPForUnity.TelemetryDisabled";

        static UnityMcpAutoStart()
        {
            EditorPrefs.SetBool(UseHttpTransportKey, false);
            EditorPrefs.SetBool(SetupCompletedKey, true);
            EditorPrefs.SetBool(SetupDismissedKey, true);
            EditorPrefs.SetBool(TelemetryDisabledKey, true);

            // Keep valid user overrides; otherwise let the plugin discover uvx locally.
            string uvxPath = EditorPrefs.GetString(UvxPathKey, string.Empty);
            if (!string.IsNullOrEmpty(uvxPath) && !File.Exists(uvxPath))
            {
                EditorPrefs.DeleteKey(UvxPathKey);
            }

            // The plugin may have cached HTTP before this initializer was called.
            EditorConfigurationCache.Instance.Refresh();

            EditorApplication.delayCall += StartBridge;
        }

        private static async void StartBridge()
        {
            bool allowHeadlessBridge = string.Equals(
                Environment.GetEnvironmentVariable("UNITY_MCP_HEADLESS"),
                "1",
                StringComparison.Ordinal);

            if (Application.isBatchMode && !allowHeadlessBridge)
            {
                return;
            }

            // Other plugin initializers can change transport during domain reload.
            EditorConfigurationCache.Instance.SetUseHttpTransport(false);
            if (MCPServiceLocator.Bridge.IsRunning)
            {
                return;
            }

            bool started = await MCPServiceLocator.Bridge.StartAsync();
            if (started)
            {
                Debug.Log("[Unity MCP] Project bridge started in stdio mode.");
            }
            else
            {
                Debug.LogWarning("[Unity MCP] Could not start the project bridge.");
            }
        }
    }
}
