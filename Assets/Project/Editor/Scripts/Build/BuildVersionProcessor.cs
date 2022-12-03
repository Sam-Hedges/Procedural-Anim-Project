using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Assets.Project.Editor.Scripts.Build
{
    public class BuildVersionProcessor : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        private const string InitialVersion = "0.0";

        public void OnPreprocessBuild(BuildReport report)
        {
            string currentVersion = FindCurrentVersion();
            UpdateVersion(currentVersion);
        }

        private string FindCurrentVersion()
        {
            // Split to find string of current version
            string[] currentVersion = PlayerSettings.bundleVersion.Split('[', ']');

            // If not the correct format, start with the initial
            return currentVersion.Length == 1 ? InitialVersion : currentVersion[1];
        }

        private void UpdateVersion(string version)
        {
            // Parse out version number from string split
            if (float.TryParse(version, out float versionNumber))
            {
                // Setup new values
                float newVersion = versionNumber + 0.01f;
                string date = DateTime.Now.ToString("d");

                // Create new string and set in Player Settings
                PlayerSettings.bundleVersion = string.Format("Version [{0}] - {1}", newVersion, date);
                Debug.Log(PlayerSettings.bundleVersion);
            }
        }

    }
}
