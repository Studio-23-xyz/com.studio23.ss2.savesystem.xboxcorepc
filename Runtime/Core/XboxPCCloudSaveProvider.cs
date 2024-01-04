using System;
using System.IO;
using GameSave;
using Studio23.SS2.AuthSystem.XboxCorePC.Core;
using Studio23.SS2.CloudSave.Core;
using UnityEngine;
using UnityEngine.Events;
using XGamingRuntime;


namespace Studio23.SS2.SaveSystem.XboxCorePc.Core
{
    public class XboxPCCloudSaveProvider : AbstractCloudSaveProvider
    {
        private GameSaveManager _saveManager;
        private string _filePath;

        protected internal override void Initialize()
        {
            _saveManager = new GameSaveManager();
            XUserHandle currentUserHandle = GamingRuntimeManager.Instance.UserManager.m_CurrentUserData.userHandle;

            if (currentUserHandle == null)
            {
                Debug.LogError("XUserhandle not found");
            }

            string SCID = GamingRuntimeManager.Instance.SCID;

            CloudSaveManager.Instance._initializationState = API_States.Process_Started;
            _saveManager.Initialize(currentUserHandle, SCID, false, OnInitializeSaveGamesComplete);
        }

        private void OnInitializeSaveGamesComplete(int hresult)
        {
            if (HR.FAILED(hresult))
            {
                CloudSaveManager.Instance._initializationState = API_States.Failed;
                Debug.LogError($"Error when initializing XGameSave. HResult 0x{hresult:x}");
                return;
            }

            CloudSaveManager.Instance._initializationState = API_States.Success;
            Debug.Log("XGameSave initialized successfully");

        }

        protected internal override void UploadToCloud(string key, string filepath)
        {
            var containerName = $"{key}";
            var blobBufferName = $"{key}_blobBuffer";

            //Enter your display name here
            var displayName = $"{key}{DateTime.Now}";

            var data = File.ReadAllBytes(filepath);

            Debug.Log($"Saving Container: {containerName}. blob Name: {blobBufferName}");

            CloudSaveManager.Instance._uploadState = API_States.Process_Started;

            _saveManager.GetOrCreateContainer(containerName,
                (hresult) =>
                {
                    if (HR.FAILED(hresult))
                    {
                        CloudSaveManager.Instance._uploadState = API_States.Failed;
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
                CloudSaveManager.Instance._uploadState = API_States.Failed;
                Debug.LogError($"Error when Saving Game. HResult 0x{hresult:x}");
                return;
            }

            CloudSaveManager.Instance._uploadState = API_States.Success;

            Debug.Log($"Saved data successfully on container {containerName} and data buffer {blobBufferName}");
        }

        protected internal override void DownloadFromCloud(string key, string downloadLocation)
        {

            var containerName = $"{key}";
            var blobBufferName = $"{key}_blobBuffer";

            _filePath = downloadLocation;

            Debug.Log($"Loading from  Container: {containerName}. blob Name: {blobBufferName}");
            CloudSaveManager.Instance._downloadState = API_States.Process_Started;
            _saveManager.GetOrCreateContainer(containerName,
                (hresult) =>
                {
                    if (HR.FAILED(hresult))
                    {
                        CloudSaveManager.Instance._downloadState = API_States.Failed;
                        return;
                    }
                    _saveManager.LoadGame(blobBufferName, OnLoadGameCompleted);
                });
        }

        private void OnLoadGameCompleted(int hresult, XGameSaveBlob[] blobs)
        {
            if (HR.FAILED(hresult))
            {
                CloudSaveManager.Instance._downloadState = API_States.Failed;
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

            CloudSaveManager.Instance._downloadState = API_States.Success;


        }


    }
}
