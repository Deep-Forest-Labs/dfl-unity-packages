using System.IO;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

namespace DeepForestLabs.EditorTools
{
    public static class ForceRefreshPackages
    {
        private const string PackagePrefix = "com.deepforestlabs.";

        [MenuItem("Deep Forest Labs/Utility/Force Refresh Packages")]
        public static void Execute()
        {
            string lockFile = Path.Combine("Packages", "packages-lock.json");
            if (File.Exists(lockFile))
            {
                File.Delete(lockFile);
                Debug.Log("[DFL] Deleted packages-lock.json");
            }

            string cacheDir = Path.Combine("Library", "PackageCache");
            if (Directory.Exists(cacheDir))
            {
                foreach (string dir in Directory.GetDirectories(cacheDir))
                {
                    string folderName = Path.GetFileName(dir);
                    if (folderName.StartsWith(PackagePrefix))
                    {
                        Directory.Delete(dir, true);
                        Debug.Log($"[DFL] Deleted cached package: {folderName}");
                    }
                }
            }

            Client.Resolve();
            Debug.Log("[DFL] Package resolution triggered — Unity will re-fetch DFL packages");
        }
    }
}
