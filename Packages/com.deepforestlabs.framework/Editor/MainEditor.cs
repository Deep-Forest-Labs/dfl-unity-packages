#nullable enable
using DeepForestLabs;
using UnityEditor;
using UnityEditor.Compilation;

namespace AnalyticsSDK.Core
{
    [InitializeOnLoad]
    public static class MainEditor
    {
        static MainEditor()
        {
            CompilationPipeline.compilationStarted += OnCompilationStarted;
        }

        private static void OnCompilationStarted(object obj)
        {
            Main.EditorExit();
        }
    }
}
#nullable disable