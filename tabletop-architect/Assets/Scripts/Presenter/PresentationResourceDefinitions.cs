using System;
using System.Collections.Generic;
using System.IO;

namespace TTA.Presenter
{
    [Serializable]
    public enum PresentationResourceKind
    {
        Unknown = 0,
        Texture = 1,
        Audio = 2,
        Model = 3
    }

    [Serializable]
    public sealed class PresentationPackageManifest
    {
        public int schemaVersion = 1;
        public string packageId = string.Empty;
        public string version = string.Empty;
        public string gameFile = "game.json";
        public string resourcesFile = "resources.json";
        public string minAppVersion = string.Empty;
    }

    [Serializable]
    public sealed class PresentationResourceEntry
    {
        public string key = string.Empty;
        public PresentationResourceKind kind = PresentationResourceKind.Texture;
        public string path = string.Empty;
    }

    [Serializable]
    public sealed class PresentationResourceManifest
    {
        public PresentationResourceEntry[] entries = Array.Empty<PresentationResourceEntry>();
    }

    public sealed class PresentationResourceResolver
    {
        private readonly string _rootPath;
        private readonly Dictionary<string, PresentationResourceEntry> _entriesByKey;

        public PresentationResourceResolver(string rootPath, PresentationResourceManifest manifest)
        {
            _rootPath = NormalizeRootPath(rootPath);
            _entriesByKey = new Dictionary<string, PresentationResourceEntry>(StringComparer.Ordinal);

            PresentationResourceManifest safeManifest = manifest ?? new PresentationResourceManifest();
            for (int index = 0; index < safeManifest.entries.Length; index++)
            {
                PresentationResourceEntry entry = safeManifest.entries[index];
                if (entry == null || string.IsNullOrWhiteSpace(entry.key))
                    continue;

                _entriesByKey[entry.key] = entry;
            }
        }

        public string RootPath => _rootPath;

        public bool TryGetEntry(string key, out PresentationResourceEntry entry)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                entry = null;
                return false;
            }

            return _entriesByKey.TryGetValue(key, out entry);
        }

        public bool TryResolveResourcePath(string key, PresentationResourceKind expectedKind, out string resolvedPath)
        {
            resolvedPath = string.Empty;

            if (!TryGetEntry(key, out PresentationResourceEntry entry))
                return false;

            if (expectedKind != PresentationResourceKind.Unknown &&
                entry.kind != PresentationResourceKind.Unknown &&
                entry.kind != expectedKind)
            {
                return false;
            }

            return TryResolveEntryPath(entry, out resolvedPath);
        }

        private bool TryResolveEntryPath(PresentationResourceEntry entry, out string resolvedPath)
        {
            resolvedPath = string.Empty;
            if (entry == null || string.IsNullOrWhiteSpace(entry.path))
                return false;

            string candidatePath = string.IsNullOrWhiteSpace(_rootPath)
                ? Path.GetFullPath(entry.path)
                : Path.GetFullPath(Path.Combine(_rootPath, entry.path));

            if (!string.IsNullOrWhiteSpace(_rootPath) && !IsPathWithinRoot(candidatePath, _rootPath))
                return false;

            if (!File.Exists(candidatePath))
                return false;

            resolvedPath = candidatePath;
            return true;
        }

        private static string NormalizeRootPath(string rootPath)
        {
            return string.IsNullOrWhiteSpace(rootPath)
                ? string.Empty
                : Path.GetFullPath(rootPath);
        }

        private static bool IsPathWithinRoot(string fullPath, string rootPath)
        {
            if (string.IsNullOrWhiteSpace(rootPath))
                return true;

            string normalizedRoot = rootPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            string normalizedPath = fullPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            string rootWithDirectorySeparator = normalizedRoot + Path.DirectorySeparatorChar;
            string rootWithAltSeparator = normalizedRoot + Path.AltDirectorySeparatorChar;

            return string.Equals(normalizedPath, normalizedRoot, StringComparison.OrdinalIgnoreCase) ||
                normalizedPath.StartsWith(rootWithDirectorySeparator, StringComparison.OrdinalIgnoreCase) ||
                normalizedPath.StartsWith(rootWithAltSeparator, StringComparison.OrdinalIgnoreCase);
        }
    }
}
