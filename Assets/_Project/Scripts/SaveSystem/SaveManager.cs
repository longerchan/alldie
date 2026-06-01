using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using FrostShelter.Core;

namespace FrostShelter.SaveSystem
{
    public class SaveManager : IService
    {
        private const string PASSWORD = "FrostShelter2024!Secure";

        private string SaveDirectory => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            Constants.SAVE_DIR);

        public event Action<SaveData> OnSaveCompleted;
        public event Action<SaveData> OnLoadCompleted;

        public void Initialize()
        {
            EnsureSaveDirectory();
        }

        public void Shutdown() { }

        private void EnsureSaveDirectory()
        {
            if (!Directory.Exists(SaveDirectory))
            {
                Directory.CreateDirectory(SaveDirectory);
            }
        }

        public string GetSlotPath(int slotIndex)
        {
            return Path.Combine(SaveDirectory, $"slot_{slotIndex}{Constants.SAVE_FILE_EXTENSION}");
        }

        public string GetHashPath(int slotIndex)
        {
            return Path.Combine(SaveDirectory, $"slot_{slotIndex}.hash");
        }

        public bool SaveToSlot(int slotIndex, SaveData data)
        {
            try
            {
                data.lastSaveUnixSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                data.saveTime = DateTime.UtcNow.ToString("O");
                data.dataVersion = Constants.SAVE_DATA_VERSION;

                var json = JsonConvert.SerializeObject(data, Formatting.Indented);
                var encrypted = CryptoHelper.Encrypt(json, PASSWORD);

                var slotPath = GetSlotPath(slotIndex);
                File.WriteAllText(slotPath, encrypted);

                // Save hash for integrity verification
                var hash = CryptoHelper.ComputeHash(json);
                File.WriteAllText(GetHashPath(slotIndex), hash);

                OnSaveCompleted?.Invoke(data);
                return true;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"Save to slot {slotIndex} failed: {ex.Message}");
                return false;
            }
        }

        public SaveData LoadFromSlot(int slotIndex)
        {
            try
            {
                var slotPath = GetSlotPath(slotIndex);
                if (!File.Exists(slotPath)) return null;

                var encrypted = File.ReadAllText(slotPath);
                var json = CryptoHelper.Decrypt(encrypted, PASSWORD);

                if (string.IsNullOrEmpty(json))
                {
                    UnityEngine.Debug.LogWarning($"Slot {slotIndex}: decryption failed, possible corruption.");
                    return null;
                }

                // Verify integrity
                var hashPath = GetHashPath(slotIndex);
                if (File.Exists(hashPath))
                {
                    var storedHash = File.ReadAllText(hashPath);
                    if (!CryptoHelper.VerifyIntegrity(json, storedHash))
                    {
                        UnityEngine.Debug.LogWarning($"Slot {slotIndex}: integrity check failed. " +
                            "Save data may have been tampered with.");
                    }
                }

                var data = JsonConvert.DeserializeObject<SaveData>(json);

                if (data != null)
                {
                    OnLoadCompleted?.Invoke(data);
                }

                return data;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"Load from slot {slotIndex} failed: {ex.Message}");
                return null;
            }
        }

        public bool DeleteSlot(int slotIndex)
        {
            try
            {
                var slotPath = GetSlotPath(slotIndex);
                var hashPath = GetHashPath(slotIndex);

                if (File.Exists(slotPath)) File.Delete(slotPath);
                if (File.Exists(hashPath)) File.Delete(hashPath);

                return true;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"Delete slot {slotIndex} failed: {ex.Message}");
                return false;
            }
        }

        public bool SlotExists(int slotIndex)
        {
            return File.Exists(GetSlotPath(slotIndex));
        }

        public DateTime GetSlotSaveTime(int slotIndex)
        {
            var data = LoadFromSlot(slotIndex);
            if (data != null && DateTime.TryParse(data.saveTime, out var time))
            {
                return time;
            }
            return DateTime.MinValue;
        }

        public SaveSlotInfo[] GetAllSlotInfo()
        {
            var result = new SaveSlotInfo[Constants.MAX_MANUAL_SLOTS + 1]; // +1 for auto
            for (int i = 0; i <= Constants.MAX_MANUAL_SLOTS; i++)
            {
                result[i] = new SaveSlotInfo
                {
                    slotIndex = i,
                    exists = SlotExists(i),
                    saveTime = GetSlotSaveTime(i),
                };
                if (result[i].exists)
                {
                    var data = LoadFromSlot(i);
                    result[i].furnaceLevel = data?.player.furnaceLevel ?? 0;
                    result[i].playTimeHours = data?.playTimeHours ?? 0f;
                }
            }
            return result;
        }
    }

    public struct SaveSlotInfo
    {
        public int slotIndex;
        public bool exists;
        public DateTime saveTime;
        public int furnaceLevel;
        public float playTimeHours;
    }
}
