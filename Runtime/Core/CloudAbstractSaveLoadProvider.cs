using System;
using System.IO;
<<<<<<< Updated upstream
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
       
=======
using GameSave;
using Studio23.SS2.AuthSystem.XboxCorePC.Core;
using Studio23.SS2.CloudSave.Core;
using UnityEngine;
using XGamingRuntime;


namespace Studio23.SS2.SaveSystem.XboxCorePc.Core
{
    public class CloudAbstractSaveLoadProvider : AbstractCloudSaveProvider
    {
        #region Opu_vai_prev_code
>>>>>>> Stashed changes

        //public override void UploadToCloud(string filePath)
        //{
        //    if (!File.Exists(filePath))
        //    {
        //       Debug.LogError($" File path is not found!");
        //       return;
        //    }
        //    var playerSaveData = File.ReadAllBytes(filePath);

        //    BinaryFormatter binaryFormatter = new BinaryFormatter();
        //    using (MemoryStream memoryStream = new MemoryStream())
        //    {
        //        binaryFormatter.Serialize(memoryStream, playerSaveData);
        //        MSGdk.Helpers.Save(memoryStream.ToArray());
        //        memoryStream.Close();
        //        Debug.Log($"Game data uploaded!");
        //        OnUploadSuccess?.Invoke();
        //    }

        //}
        //public override void DownloadFromCloud(string filePath)
        //{
        //    _filePath = filePath;
        //    MSGdk.Helpers.OnGameSaveLoaded -= OnGameSaveLoaded;
        //    MSGdk.Helpers.OnGameSaveLoaded += OnGameSaveLoaded;
        //    MSGdk.Helpers.LoadSaveData();
        //}

        //private string _filePath;

        //private void OnGameSaveLoaded(object sender, GameSaveLoadedArgs saveData)
        //{
        //    if (saveData.Data.Length == 0  ||  saveData.Data == Array.Empty<byte>())
        //    {
        //        Debug.Log($"Game data empty or null {_filePath}");
        //        OnDownloadSuccess?.Invoke();
        //    }
        //    else
        //    {
        //        BinaryFormatter binaryFormatter = new BinaryFormatter();
        //        using (MemoryStream memoryStream = new MemoryStream(saveData.Data))
        //        {
        //            byte[] playerSaveData = (byte[])binaryFormatter.Deserialize(memoryStream);

        //            File.WriteAllBytes(_filePath, playerSaveData);
        //            memoryStream.Close();

        //            Debug.Log($"Game data downloaded and saved to {_filePath}");
        //            OnDownloadSuccess?.Invoke();
        //        }
        //    }

        #endregion

        private GameSaveManager _saveManager;
        private string _filePath;

<<<<<<< Updated upstream
       

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
        
=======
        public override void Start()
        {
            base.Start();
        }

        public override void InitializePlatformService()
        {
            _saveManager = new GameSaveManager();
            XUserHandle currentUserHandle = GamingRuntimeManager.Instance.UserManager.m_CurrentUserData.userHandle;

            if (currentUserHandle == null)
            {
                Debug.LogError("XUserhandle not found");
            }

            string SCID = GamingRuntimeManager.Instance.SCID;

            _saveManager.Initialize(currentUserHandle, SCID, false, OnInitializeSaveGamesComplete);
        }

        private void OnInitializeSaveGamesComplete(int hresult)
        {
            if (HR.FAILED(hresult))
            {
                Debug.LogError($"Error when initializing XGameSave. HResult 0x{hresult:x}");
                return;
            }
            OnInitializationSucess?.Invoke();
            Debug.Log("XGameSave initialized succesfully");
        }
        public override void UploadToCloud(string key, string filepath)
        {
            var containerName = $"{key}";
            var blobBufferName = $"{key}_blobBuffer";

            //Enter your display name here
            var displayName = $"{key}{DateTime.Now}";

            var data = File.ReadAllBytes(filepath);

            Debug.Log($"Saving Container: {containerName}. blob Name: {blobBufferName}");

            _saveManager.GetOrCreateContainer(containerName,
                (hresult) =>
                {
                    if (HR.FAILED(hresult))
                    {
                        return;
                    }

                    _saveManager.SaveGame(displayName,
                        blobBufferName,
                        data,
                        (hresult) => OnSaveGameCompleted(hresult, containerName, blobBufferName));
                });
        }

        private void OnSaveGameCompleted(int hresult, string containerName, string blobBufferName)
        {
            if (HR.FAILED(hresult))
            {
                Debug.LogError($"Error when Saving Game. HResult 0x{hresult:x}");
                return;
            }
            OnUploadSuccess?.Invoke();
            Debug.Log($"Saved data successfully on container {containerName} and data bufer {blobBufferName}");
        }
        public override void DownloadFromCloud(string key, string downloadLocation)
        {
            var containerName = $"{key}";
            var blobBufferName = $"{key}_blobBuffer";

            _filePath = downloadLocation;

            Debug.Log($"Loading from  Container: {containerName}. blob Name: {blobBufferName}");

            _saveManager.GetOrCreateContainer(containerName,
                (hresult) =>
                {
                    if (HR.FAILED(hresult))
                    {
                        return;
                    }
                    _saveManager.LoadGame(blobBufferName, OnLoadGameCompleted);
                });
        }
        private void OnLoadGameCompleted(int hresult, XGameSaveBlob[] blobs)
        {
            if (HR.FAILED(hresult))
            {
                Debug.LogError($"Error when loading GameSave. HResult 0x{hresult:x}");
                return;
            }
            // For the effects of this sample, we only expect one blob
            if (blobs.Length > 0)
            {
                Debug.Log($"Loading data buffer successful. Name: {blobs[0].Info.Name} - Size: {blobs[0].Info.Size} bytes");

                // Same as save, you will get a byte array (byte[]).

                File.WriteAllBytes(_filePath, blobs[0].Data);
            }
            OnDownloadSuccess?.Invoke();
        }

        public void QuerySaveContainers()
        {
            Debug.Log("Querying slot containers");
            var containers = _saveManager.QueryContainers("cloud");
            Debug.Log($"Found {containers.Length} containers");

            for (int i = 0; i < containers.Length; i++)
            {
                Debug.Log($"Container {containers[i].Name}.\n    Display Name: {containers[i].DisplayName}. Size: {containers[i].TotalSize} bytes. Blob count: {containers[i].BlobCount}. Last modified {containers[i].LastModifiedTime}.");


            }
        }


>>>>>>> Stashed changes
    }

}
