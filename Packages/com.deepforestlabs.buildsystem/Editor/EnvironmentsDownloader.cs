#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ZLinq;
using DeepForestLabs.Logger;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace DeepForestLabs.BuildSystems
{
    public static class EnvironmentsDownloader
    {
        // TODO: Set this to your project's environment list endpoint
        public const string DEFAULT_URL = "https://example.com/envlist";
        private static List<EnvironmentBuildSettings> _cached = new();
        private static string? _cachedUrl = null;
        
        public static IReadOnlyList<EnvironmentBuildSettings> Refresh(string url)
        {
            _cached.Clear();
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
                using (UnityWebRequest request = UnityWebRequest.Get(url))
                {
                    request.SetRequestHeader("Content-Type", "application/json");
                    UnityWebRequestAsyncOperation op = request.SendWebRequest();
                    while (!op.isDone) {} // Block
                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        EnvironmentResponse? environmentResponse = 
                            JsonUtility.FromJson<EnvironmentResponse>(request.downloadHandler.text);
                        if (environmentResponse == null)
                        {
                            throw new BuildException("Failed to get environment response.");
                        }
                        List<EnvironmentBuildSettings> result = environmentResponse.Envs.AsValueEnumerable().ToList();
                        _cached = result;
                        _cachedUrl = url;
                    }
                    else
                    {
                        _cached = new List<EnvironmentBuildSettings>() { new EnvironmentBuildSettings("Download Error") };
                        _cachedUrl = url;
                        throw BuildException.FromFormat("Failed to receive environment settings from url '{0}' with '{1}'.", url,
                                request.result);
                    }
                }
            }
            catch (Exception e)
            {
                throw new BuildException(e.ToString(), e);
            }

            return _cached;
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