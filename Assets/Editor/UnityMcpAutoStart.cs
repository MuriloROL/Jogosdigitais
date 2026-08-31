using System;
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
            EditorPrefs.SetString(UvxPathKey, "/home/murilo-lima/.local/bin/uvx");
            EditorPrefs.SetBool(TelemetryDisabledKey, true);

            EditorApplication.delayCall += StartBridge;
        }

        private static async void StartBridge()
        {
            bool allowHeadlessBridge = string.Equals(
                Environment.GetEnvironmentVariable("UNITY_MCP_HEADLESS"),
                "1",
                StringComparison.Ordinal);

            if ((Application.isBatchMode && !allowHeadlessBridge) || MCPServiceLocator.Bridge.IsRunning)
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
