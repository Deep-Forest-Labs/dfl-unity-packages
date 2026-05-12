#nullable enable
using System.Collections.Generic;
using System.Diagnostics;
using DeepForestLabs.Logger;
using ZLinq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Build.DataBuilders;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace DeepForestLabs.BuildSystems.AddressablesBuildScripts
{
    [CreateAssetMenu(fileName = nameof(GroupsBuildMode), menuName = "Addressables/Content Builders/Deep Forest Labs/Groups Build Mode")]
    public sealed class GroupsBuildMode : BuildScriptBase
    {
        public override string Name => "Build Groups";

        public override bool CanBuildData<T>() =>
            BuildSystemSettings.SettingsExist &&
            typeof(T).IsAssignableFrom(typeof(AddressablesPlayerBuildResult));
        
        protected override TResult BuildDataImplementation<TResult>(AddressablesDataBuilderInput builderInput)
        {
            BuildLog.Info("Groups {0} Started.", Name);
            Stopwatch timer = new();
            timer.Start();

            // Set and run the folder importer
            int committedGroupCount = builderInput.AddressableSettings.groups.Count;
            AddressableImporter.FolderImporter.ReimportFolders(new[] {"Assets"});

            TResult result = AddressableAssetBuildResult.CreateResult<TResult>(null, 0, null);
            if (BuildSystemEntryPoint.ReadArgs().IsCommandLineBuild &&
                committedGroupCount != builderInput.AddressableSettings.groups.Count)
            {
                result = AddressableAssetBuildResult.CreateResult<TResult>(builderInput.AddressableSettings.AssetPath,
                    result.LocationCount,
                    "Group count miss match.  Please run \"Rebuild Groups\" build job and commit changes.");
            }
            else
            {
                string buildTime = string.Format("{0:00}:{1:00}:{2:00}", timer.Elapsed.Hours, timer.Elapsed.Minutes,
                    timer.Elapsed.Seconds);
                BuildLog.Info("Addressables {0} Complete. Time to Build: {1}", Name, buildTime);
            }

            RemoveEmptyGroups();

            result.Duration = timer.Elapsed.Seconds;
            return result;
        }
        
        public static void RemoveEmptyGroups()
        {
            AddressableAssetSettings? settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                BuildLog.Error("[GroupsBuildScript] AddressableAssetSettings not found.");
                return;
            }

            // Collect candidates
            List<AddressableAssetGroup> toRemove = new List<AddressableAssetGroup>();
            foreach (AddressableAssetGroup? g in settings.groups.AsValueEnumerable().ToList())
            {
                if (g == null)
                {
                    continue;
                }

                // Don’t touch built-in or default groups
                if (g == settings.DefaultGroup)
                {
                    continue;
                }

                if (g.Name == "Built In Data")
                {
                    continue;
                }

                // Some groups can be marked read-only by tooling; skip if so
                if (g.ReadOnly)
                {
                    continue;
                }

                // Empty if no entries; (entries is public in 1.20–1.21)
                // If your version hides it, use reflection or check via CountAssetsInGroup() helper below.
                if (g.entries == null || g.entries.Count == 0)
                {
                    toRemove.Add(g);
                }
            }

            if (toRemove.Count == 0)
            {
                BuildLog.Info("[GroupsBuildScript] No empty groups found.");
                return;
            }

            foreach (AddressableAssetGroup? g in toRemove)
            {
                BuildLog.Info("[GroupsBuildScript] Removing empty group: {0}", g.Name);
                settings.RemoveGroup(g);
            }
            AssetDatabase.SaveAssets();
            BuildLog.Info("[GroupsBuildScript] Removed {0} empty group(s).", toRemove.Count);
        }
    }
}
#nullable disable