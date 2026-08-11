#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using ZLinq;
using DeepForestLabs.Logger;
using UnityEngine;
using UnityEngine.Networking;

namespace DeepForestLabs.BuildSystems
{
    public static class EnvironmentsDownloader
    {
        // TODO: Set this to your project's live environment list endpoint when available
        public const string DEFAULT_URL = "https://example.com/envlist";
        private static List<EnvironmentBuildSettings> _cached = new();
        private static string? _cachedUrl;

        public static IReadOnlyList<EnvironmentBuildSettings> Refresh(string url)
        {
            _cached.Clear();
            _cachedUrl = null;
            return GetEnvironments(url);
        }

        public static IReadOnlyList<EnvironmentBuildSettings> GetEnvironments(string url = DEFAULT_URL)
        {
            if (_cachedUrl == url && _cached.Count > 0)
            {
                return _cached;
            }

            try
            {
                if (TryResolveLocalPath(url, out string localPath))
                {
                    return LoadFromFile(localPath, url);
                }

                return LoadFromHttp(url);
            }
            catch (BuildException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new BuildException(e.ToString(), e);
            }
        }

        private static IReadOnlyList<EnvironmentBuildSettings> LoadFromFile(string path, string cacheKey)
        {
            BuildLog.Info("EnvironmentsDownloader - Loading envlist from file '{0}'", path);
            string text = File.ReadAllText(path);
            return ParseAndCache(text, cacheKey);
        }

        private static IReadOnlyList<EnvironmentBuildSettings> LoadFromHttp(string url)
        {
            using UnityWebRequest request = UnityWebRequest.Get(url);
            request.SetRequestHeader("Content-Type", "application/json");
            UnityWebRequestAsyncOperation op = request.SendWebRequest();
            while (!op.isDone)
            {
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                _cached = new List<EnvironmentBuildSettings> { new("Download Error") };
                _cachedUrl = url;
                throw BuildException.FromFormat(
                    "Failed to receive environment settings from url '{0}' with '{1}'.",
                    url,
                    request.result);
            }

            return ParseAndCache(request.downloadHandler.text, url);
        }

        private static IReadOnlyList<EnvironmentBuildSettings> ParseAndCache(string json, string cacheKey)
        {
            EnvironmentResponse? environmentResponse = JsonUtility.FromJson<EnvironmentResponse>(json);
            if (environmentResponse == null)
            {
                throw new BuildException("Failed to get environment response.");
            }

            List<EnvironmentBuildSettings> result = environmentResponse.Envs.AsValueEnumerable().ToList();
            _cached = result;
            _cachedUrl = cacheKey;
            return _cached;
        }

        /// <summary>
        /// Accepts file:// URIs, absolute paths, or project-relative paths (e.g. ci/envlist.json).
        /// </summary>
        internal static bool TryResolveLocalPath(string urlOrPath, out string resolvedPath)
        {
            resolvedPath = string.Empty;
            if (string.IsNullOrWhiteSpace(urlOrPath))
            {
                return false;
            }

            string candidate = urlOrPath.Trim();
            if (candidate.StartsWith("file:", StringComparison.OrdinalIgnoreCase))
            {
                candidate = new Uri(candidate).LocalPath;
            }
            else if (candidate.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                     || candidate.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (File.Exists(candidate))
            {
                resolvedPath = Path.GetFullPath(candidate);
                return true;
            }

            string fromProject = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), candidate));
            if (File.Exists(fromProject))
            {
                resolvedPath = fromProject;
                return true;
            }

            string fromData = Path.GetFullPath(Path.Combine(Application.dataPath, "..", candidate));
            if (File.Exists(fromData))
            {
                resolvedPath = fromData;
                return true;
            }

            // Relative path that does not exist yet — still treat non-http as local so CI fails clearly
            if (candidate.IndexOf("://", StringComparison.Ordinal) < 0)
            {
                throw BuildException.FromFormat(
                    "Local environment list file not found at '{0}' (cwd '{1}').",
                    candidate,
                    Directory.GetCurrentDirectory());
            }

            return false;
        }

        [Serializable]
        internal sealed class EnvironmentResponse
        {
            // ReSharper disable once InconsistentNaming
            [SerializeField] public List<EnvironmentBuildSettings> envs = null!; // must be envs without underscore

            internal IEnumerable<EnvironmentBuildSettings> Envs => envs;
        }
    }
}
#nullable disable
