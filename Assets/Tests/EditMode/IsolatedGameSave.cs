using System;
using TCCGame.Core.Persistence;
using UnityEngine;

namespace TCCGame.Tests.EditMode
{
    internal sealed class IsolatedGameSave : IDisposable
    {
        private readonly string prefix = $"tcc.tests.{Guid.NewGuid():N}.";

        public string SaveKey => prefix + GameSaveRepository.SaveKey;
        public string CorruptSaveKey => prefix + GameSaveRepository.CorruptSaveKey;

        public GameSaveRepository OpenRepository() => new GameSaveRepository(prefix);

        public void Dispose()
        {
            PlayerPrefs.DeleteKey(SaveKey);
            PlayerPrefs.DeleteKey(CorruptSaveKey);
            PlayerPrefs.DeleteKey(prefix + GameSaveRepository.LegacyCitySaveKey);
            PlayerPrefs.Save();
        }
    }
}
