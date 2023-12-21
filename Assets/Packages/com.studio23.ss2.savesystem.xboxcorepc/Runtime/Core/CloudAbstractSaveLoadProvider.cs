using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Studio23.SS2.Authsystem.XboxCorePC.Core;
using UnityEngine;

 
namespace Studio23.SS2.SaveSystem.XboxCorePc.Core
{
   
    
    [CreateAssetMenu(fileName = "XboxCorePcSaveLoadProvider", menuName = "Studio-23/Save System/XboxCorePc SaveLoad Provider", order = 1)]
    public class CloudAbstractSaveLoadProvider : AbstractSaveLoadProvider
    {
        public override void UploadToCloud(string filePath)
        {
            if (!File.Exists(filePath))
            {
               Debug.LogError($" File path is not found!");
               return;
            }
            var playerSaveData = File.ReadAllBytes(filePath);
            
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            using (MemoryStream memoryStream = new MemoryStream())
            {
                binaryFormatter.Serialize(memoryStream, playerSaveData);
                MSGdk.Helpers.Save(memoryStream.ToArray());
                memoryStream.Close();
                Debug.Log($"Game data uploaded!");
                OnUploadSuccess?.Invoke();
            }
            
        }
        public override void DownloadFromCloud(string filePath)
        {
            _filePath = filePath;
            MSGdk.Helpers.OnGameSaveLoaded -= OnGameSaveLoaded;
            MSGdk.Helpers.OnGameSaveLoaded += OnGameSaveLoaded;
            MSGdk.Helpers.LoadSaveData();
        }

        private string _filePath;

        private void OnGameSaveLoaded(object sender, GameSaveLoadedArgs saveData)
        {
            if (saveData.Data.Length == 0  ||  saveData.Data == Array.Empty<byte>())
            {
                Debug.Log($"Game data empty or null {_filePath}");
                OnDownloadSuccess?.Invoke();
            }
            else
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                using (MemoryStream memoryStream = new MemoryStream(saveData.Data))
                {
                    byte[] playerSaveData = (byte[])binaryFormatter.Deserialize(memoryStream);
                
                    File.WriteAllBytes(_filePath, playerSaveData);
                    memoryStream.Close();
                
                    Debug.Log($"Game data downloaded and saved to {_filePath}");
                    OnDownloadSuccess?.Invoke();
                }
            }
            
           
        }
    }
}
