# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- `AudioMixerConfig.GroupMapping.Priority` (default 128) and `SoundEntry` priority override (off by default). The pooled `AudioSource.priority` is set from them when a sound plays. `SoundParams.Priority` overrides both for one play.
- `AudioMixerConfig.GroupMapping.WhenFull` (`GroupFullPolicy`, default `RejectNew`). `StealOldest` stops the group's oldest voice when the group is at `MaxVoices`.
- `AudioMixerConfig.SizePoolToRealVoices` (default off) sizes the pool to `AudioSettings.GetConfiguration().numRealVoices` instead of 64.
- When the pool is full, a new sound takes the voice of the least important playing sound that is strictly less important than itself. Voices inside another group's reservation are never taken.
- The `AudioMixerConfig` inspector shows the pool size against Unity's real voices and the sum of group caps.
- `DuckingProfile` **Also Ducks**: more groups ducked by the same amount and timing. `DuckingProfile.TargetGroups` lists the target first. Profiles saved before this duck only their target.

### Changed
- The pool's initial capacity no longer exceeds its max capacity.
- Ducks fade on the profile's attack and release times (unscaled). Before, the volume jumped to the ducked level and back, and both times were ignored. Set both to 0 to keep the old jump.

With every new voice field at its default, playback behaves as before: priority 128, a full group or pool rejects the new sound, and the pool holds 64 voices.
