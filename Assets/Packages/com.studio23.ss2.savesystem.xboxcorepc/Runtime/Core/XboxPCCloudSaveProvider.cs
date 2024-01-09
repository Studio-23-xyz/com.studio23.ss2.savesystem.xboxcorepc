using System;
using System.IO;
using Cysharp.Threading.Tasks;
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

        protected override UniTask<int> Initialize()
        {
            UniTaskCompletionSource<int> cloudSaveTaskCompletionSource= new UniTaskCompletionSource<int>();
            _saveManager = new GameSaveManager();
            XUserHandle currentUserHandle = GamingRuntimeManager.Instance.UserManager.m_CurrentUserData.userHandle;

            if (currentUserHandle == null)
            {
                Debug.LogError("XUserhandle not found");
            }
            
            string SCID = GamingRuntimeManager.Instance.SCID;
            
            _saveManager.Initialize(currentUserHandle, SCID, false, result =>
            {
                cloudSaveTaskCompletionSource.TrySetResult(result);
            });


            return cloudSaveTaskCompletionSource.Task;
        }

        

        protected override async UniTask<int> UploadToCloud(string key, byte[] data)
        {
            UniTaskCompletionSource<int> uploadedToCloudTaskCompletionSource = new UniTaskCompletionSource<int>(); 
            var containerName = $"{key}";
            var blobBufferName = $"{key}_blobBuffer";
            var displayName = $"{key}{DateTime.Now}";
            
            Debug.Log($"Saving Container: {containerName}. blob Name: {blobBufferName}");

         

            _saveManager.GetOrCreateContainer(containerName,
                result =>
                {
                    uploadedToCloudTaskCompletionSource.TrySetResult(result);
                    if (HR.FAILED(result))
                    {
                        Debug.Log($"Error when GetOrCreateContainer HResult 0x{result:x}");
                        return;
                    } 
                    _saveManager.SaveGame(displayName,
                        blobBufferName,
                        data,
                        result =>
                        {
                            uploadedToCloudTaskCompletionSource.TrySetResult(result);
                        });
                });

            return await uploadedToCloudTaskCompletionSource.Task;
        }
        protected override async UniTask<byte[]> DownloadFromCloud(string key)
        
        {
            var containerName = $"{key}";
            var blobBufferName = $"{key}_blobBuffer";
            UniTaskCompletionSource<int> getOrCreateContainerTaskCompletionSource  = new UniTaskCompletionSource<int>(); 
            UniTaskCompletionSource<byte[]> downloadBlobTaskCompletionSource = new UniTaskCompletionSource<byte[]>();
            
            Debug.Log($"Loading from  Container: {containerName}. blob Name: {blobBufferName}");
           
            _saveManager.GetOrCreateContainer(containerName,
                result =>
                {
                    getOrCreateContainerTaskCompletionSource.TrySetResult(result);
                    if (HR.FAILED(result))
                    {
                        Debug.Log($"Error when GetOrCreateContainer HResult 0x{result:x}");
                        return;
                    } 
                    
                    _saveManager.LoadGame(blobBufferName, (hresult, blobs) =>
                    {
                        getOrCreateContainerTaskCompletionSource.TrySetResult(hresult);
                        downloadBlobTaskCompletionSource.TrySetResult(Array.Empty<byte>());
                        if (HR.FAILED(result))
                        {
                            Debug.Log($"Error when loading GameSave. HResult 0x{result:x}");
                            return;
                        } 
                        Debug.Log($"Loading data buffer successful. Name: {blobs[0].Info.Name} - Size: {blobs[0].Info.Size} bytes");
                        byte[] loadedData = blobs[0].Data;
                        downloadBlobTaskCompletionSource.TrySetResult(loadedData);
                    });
                });

           
            
            return await downloadBlobTaskCompletionSource.Task;
        }
    }
}
