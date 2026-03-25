using System;
using System.IO;
using TTA.Core;
using TTA.Presenter;
using Unity.Plastic.Newtonsoft.Json;

namespace TTA.Game
{
    public enum GameContentLoadMode
    {
        CodeDefined = 0,
        PackageFolder = 1
    }

    public sealed class LoadedGameContent
    {
        public string rootPath = string.Empty;
        public GameDefinition definition = new();
        public PresentationPackageManifest package = new();
        public PresentationResourceManifest resources = new();
        public PresentationResourceResolver resourceResolver = new(string.Empty, new PresentationResourceManifest());
    }

    public static class GameContentLoader
    {
        public static LoadedGameContent CreateGameContent(
            GameDefinition definition,
            PresentationResourceManifest resources = null,
            string contentRootPath = "",
            PresentationPackageManifest package = null)
        {
            PresentationResourceManifest safeResources = resources ?? new PresentationResourceManifest();
            return new LoadedGameContent
            {
                rootPath = string.IsNullOrWhiteSpace(contentRootPath)
                    ? string.Empty
                    : Path.GetFullPath(contentRootPath),
                definition = definition ?? new GameDefinition(),
                package = package ?? new PresentationPackageManifest(),
                resources = safeResources,
                resourceResolver = new PresentationResourceResolver(contentRootPath, safeResources)
            };
        }

        public static LoadedGameContent LoadFromPackageDirectory(string packageRootPath)
        {
            if (string.IsNullOrWhiteSpace(packageRootPath))
                throw new ArgumentException("Package root path must not be empty.", nameof(packageRootPath));

            string rootPath = Path.GetFullPath(packageRootPath);
            if (!Directory.Exists(rootPath))
                throw new DirectoryNotFoundException($"Package directory not found: {rootPath}");

            string manifestPath = Path.Combine(rootPath, "package.json");
            PresentationPackageManifest package = File.Exists(manifestPath)
                ? LoadPackageManifest(manifestPath)
                : new PresentationPackageManifest();

            string gameFileName = string.IsNullOrWhiteSpace(package.gameFile)
                ? "game.json"
                : package.gameFile;
            string resourcesFileName = string.IsNullOrWhiteSpace(package.resourcesFile)
                ? "resources.json"
                : package.resourcesFile;

            string gamePath = Path.Combine(rootPath, gameFileName);
            if (!File.Exists(gamePath))
                throw new FileNotFoundException("Game definition file was not found.", gamePath);

            GameData data = LoadGameData(gamePath);

            string resourcesPath = Path.Combine(rootPath, resourcesFileName);
            PresentationResourceManifest resources = File.Exists(resourcesPath)
                ? LoadResources(resourcesPath)
                : new PresentationResourceManifest();

            return CreateGameContent(data, resources, rootPath, package);
        }

        public static PresentationPackageManifest LoadPackageManifest(string path)
        {
            return DeserializeJsonFile<PresentationPackageManifest>(path);
        }

        public static GameData LoadGameData(string path)
        {
            return DeserializeJsonFile<GameData>(path);
        }

        public static PresentationResourceManifest LoadResources(string path)
        {
            return DeserializeJsonFile<PresentationResourceManifest>(path);
        }

        public static string SerializeGameData(GameDefinition definition, bool prettyPrint = true)
        {
            return Serialize(definition ?? new GameDefinition(), prettyPrint);
        }

        public static string SerializePackageManifest(PresentationPackageManifest manifest, bool prettyPrint = true)
        {
            return Serialize(manifest ?? new PresentationPackageManifest(), prettyPrint);
        }

        public static string SerializeResources(PresentationResourceManifest resources, bool prettyPrint = true)
        {
            return Serialize(resources ?? new PresentationResourceManifest(), prettyPrint);
        }

        private static T DeserializeJsonFile<T>(string path) where T : class, new()
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Path must not be empty.", nameof(path));

            string json = File.ReadAllText(path);
            T value = JsonConvert.DeserializeObject<T>(json);
            return value ?? new T();
        }

        private static string Serialize<T>(T value, bool prettyPrint)
        {
            return JsonConvert.SerializeObject(
                value,
                prettyPrint ? Formatting.Indented : Formatting.None);
        }
    }
}
