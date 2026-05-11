#nullable enable
using System;
using UnityEngine;

namespace DeepForestLabs.Audio
{
    [Serializable]
    public readonly struct SoundGroupId : IEquatable<SoundGroupId>
    {
        public static readonly SoundGroupId Bgm = new("BGM");
        public static readonly SoundGroupId Sfx = new("SFX");
        public static readonly SoundGroupId Ui = new("UI");

        [SerializeField] private readonly string _name;

        public string Name => _name;

        public SoundGroupId(string name)
        {
            _name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public bool Equals(SoundGroupId other) =>
            string.Equals(_name, other._name, StringComparison.Ordinal);

        public override bool Equals(object? obj) =>
            obj is SoundGroupId other && Equals(other);

        public override int GetHashCode() =>
            _name != null ? StringComparer.Ordinal.GetHashCode(_name) : 0;

        public override string ToString() => _name ?? string.Empty;

        public static bool operator ==(SoundGroupId a, SoundGroupId b) => a.Equals(b);
        public static bool operator !=(SoundGroupId a, SoundGroupId b) => !a.Equals(b);
    }
}
#nullable disable
