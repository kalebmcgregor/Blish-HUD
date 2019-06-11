﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Blish_HUD {

    public class DirectoriesManager {

        private readonly HashSet<string>            _directoryNames;
        private readonly Dictionary<string, string> _directoryPaths;

        public IReadOnlyList<string> RegisteredDirectories => _directoryNames.ToList();

        public DirectoriesManager(IEnumerable<string> directoryNames) {
            _directoryNames = new HashSet<string>(directoryNames, StringComparer.OrdinalIgnoreCase);
            _directoryPaths = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            PrepareDirectories();
        }

        private void PrepareDirectories() {
            foreach (string directoryName in _directoryNames) {
                string fullDirectoryPath = Path.Combine(GameService.Directory.BasePath, directoryName);

                Directory.CreateDirectory(fullDirectoryPath);

                _directoryPaths.Add(directoryName, fullDirectoryPath);
            }
        }

        public string GetFullDirectoryPath(string directoryName) {
            if (!_directoryNames.Contains(directoryName)) return null;

            return _directoryPaths[directoryName];
        }

    }

    public class DirectoryService : GameService {

        private const string TEST_WRITE = "test-write";

        private const string ADDON_DIR = @"Guild Wars 2\addons\blishhud";

        public string BasePath { get; private set; } = System.IO.Directory.GetCurrentDirectory();

        private bool CanWriteToDir(string dir) {
            try {
                string testFilePath = Path.Combine(dir, Guid.NewGuid().ToString() + ".tmp");

                File.WriteAllText(testFilePath, TEST_WRITE);
                string testContents = File.ReadAllText(testFilePath);
                File.Delete(testFilePath);

                return (testContents == TEST_WRITE);
            } catch (Exception ex) {
                GameService.Debug.WriteWarningLine($"Was unable to write to directory '{dir}'. {ex.Message}");
                return false;
            }
        }

        protected override void Initialize() {
            // Need to determine where we will be saving things here first since services
            // will start to make their file operations starting in `Load()`

            DetermineWritePath();
        }

        public DirectoriesManager RegisterDirectories(IEnumerable<string> directories) {
            return new DirectoriesManager(directories ?? new List<string>(0));
        }

        private void DetermineWritePath() {
            // Prepare user documents directory
            this.BasePath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments,
                                                                          Environment.SpecialFolderOption.DoNotVerify),
                                         ADDON_DIR);
            System.IO.Directory.CreateDirectory(this.BasePath);
            
            string settingsPath = Path.Combine(this.BasePath, "settings.json");

            // Move existing settings, if upgrading from an older version
            if (!File.Exists(settingsPath)) {
                try {
                    string oldSettingsPath = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "settings.json");

                    if (File.Exists(oldSettingsPath)) {
                        File.Copy(oldSettingsPath, settingsPath);
                    }
                } catch (Exception ex) { /* NOOP */ }
            }
        }

        protected override void Load() { /* NOOP */ }
        protected override void Unload() { /* NOOP */ }

        protected override void Update(GameTime gameTime) { /* NOOP */ }

    }
}