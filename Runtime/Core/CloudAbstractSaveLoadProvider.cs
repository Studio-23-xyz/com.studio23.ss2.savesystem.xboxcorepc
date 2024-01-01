using System;
using System.IO;
using System.Text;
using Studio23.SS2.Authsystem.XboxCorePC.Core;
using Studio23.SS2.CloudSave.Core;
using UnityEngine;
 
 
namespace Studio23.SS2.SaveSystem.XboxCorePc.Core
{
   

   

    [CreateAssetMenu(fileName = "XboxCorePcSaveLoadProvider",
        menuName = "Studio-23/Save System/XboxCorePc SaveLoad Provider", order = 1)]
    [Serializable]
    public class Employee
    {
        public string Name;
        public int Age;
    }
    public class CloudAbstractSaveLoadProvider : AbstractCloudSaveProvider
    {
       

        private string _filePath;

       

        public override void InitializePlatformService()
        {
            /* Automatically call after sign in*/
          // MSGdk.Helpers.InitializeGameSaves();
        }

      
        public override void UploadToCloud(string key, string filepath)
        {

            if (!File.Exists(filepath))
            {
                Debug.LogError($" File path is not found!");
                return;
            }
 
            var readAllBytes = File.ReadAllBytes(filepath);
            if (readAllBytes.Length > 0)
            {
                MSGdk.Helpers.Save(key, readAllBytes);  
                Debug.Log($"Game data uploaded! {Encoding.UTF8.GetString(readAllBytes)}");
                OnUploadSuccess?.Invoke();
            }
            else
            {
                Debug.LogError("Error reading file: Not all bytes were read.");
            }

             
        }

        public override void DownloadFromCloud(string key, string downloadLocation)
        {
            
            _filePath = downloadLocation;
            MSGdk.Helpers.OnGameSaveLoaded -= OnGameSaveLoaded;
            MSGdk.Helpers.OnGameSaveLoaded += OnGameSaveLoaded;
            MSGdk.Helpers.LoadSaveData(key);
        }
        
        
        private void OnGameSaveLoaded(object sender, GameSaveLoadedArgs saveData)
        {
            
            if (saveData.Data == null || saveData.Data.Length == 0)
            {
                if (File.Exists(_filePath))
                {
                    File.Delete(_filePath);
                    Debug.LogError($"Old saved deleted");
                }
                Debug.Log($"Online data is null or 0. Do nothing.");
                OnDownloadSuccess?.Invoke();
            }
            else if( saveData.Data.Length > 0)
            {
                
                File.WriteAllBytes(_filePath, saveData.Data); // Write the JSON string to the file
                Debug.Log($"Downloaded from online: Successful, Saved to {Encoding.UTF8.GetString(saveData.Data)}");
                OnDownloadSuccess?.Invoke();
            }
            
        }
        
    }
}
