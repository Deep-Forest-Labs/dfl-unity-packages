#nullable enable
using UnityEditor;
using UnityEngine;

namespace DeepForestLabs.Audio.Editor
{
    public static class AudioMenuItems
    {
        [MenuItem("Deep Forest Labs/Audio/Setup Instructions")]
        public static void ShowSetupInstructions()
        {
            EditorUtility.DisplayDialog(
                "Audio Package Setup",
                "1. Create an AudioMixer (Assets > Create > Audio Mixer) with groups:\n" +
                "   Master (exposed param: MasterVolume)\n" +
                "     BGM (exposed param: BGMVolume)\n" +
                "     SFX (exposed param: SFXVolume)\n" +
                "     UI  (exposed param: UIVolume)\n\n" +
                "2. Create an AudioMixerConfig (Assets > Create > Audio > Mixer Config)\n" +
                "3. Assign the mixer and map groups in the config inspector\n" +
                "4. Optionally create a SoundCatalog (Assets > Create > Audio > Sound Catalog)",
                "OK");
        }

    }
}
#nullable disable
