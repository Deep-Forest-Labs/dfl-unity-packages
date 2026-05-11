# Default AudioMixer Setup

The audio package expects a Unity AudioMixer with the following structure:

## Mixer Group Hierarchy

- **Master** (exposed param: `MasterVolume`)
  - **BGM** (exposed param: `BGMVolume`)
  - **SFX** (exposed param: `SFXVolume`)
  - **UI** (exposed param: `UIVolume`)

## Setup Instructions

1. Use the editor menu: **Audio > Create Default AudioMixer** to auto-generate this structure
2. Or manually create an AudioMixer in your project with the groups/params listed above
3. Reference it in your `AudioMixerConfig` ScriptableObject asset

Each group should have its volume parameter exposed so the `AudioMixerProvider` can control it at runtime.
