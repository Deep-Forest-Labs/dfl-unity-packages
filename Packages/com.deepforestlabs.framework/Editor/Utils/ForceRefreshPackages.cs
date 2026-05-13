using System.IO;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

namespace DeepForestLabs.Editor
{
    public static class ForceRefreshPackages
    {
        [MenuItem("Deep Forest Labs/Utility/Force Refresh Packages")]
        public static void Execute()
        {
            string lockFile = Path.Combine("Packages", "packages-lock.json");
            if (File.Exists(lockFile))
            {
                File.Delete(lockFile);
                Debug.Log("[DFL] Deleted packages-lock.json");
            }

            Client.Resolve();
            Debug.Log("[DFL] Package resolution triggered");
        }
    }
}
