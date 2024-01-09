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
          
            _saveManager = new GameSaveManager();
            XUserHandle currentUserHandle = GamingRuntimeManager.Instance.UserManager.m_CurrentUserData.userHandle;
            
            if (currentUserHandle == null)
            {
                Debug.LogError("XUserhandle not found");
            }
            
            string SCID = GamingRuntimeManager.Instance.SCID;
            
            UniTaskCompletionSource<int> cloudSaveTaskCompletionSource= new UniTaskCompletionSource<int>();
            _saveManager.Initialize(currentUserHandle, SCID, false, result =>
            {
                cloudSaveTaskCompletionSource.TrySetResult(result);
            });


            return cloudSaveTaskCompletionSource.Task;
        }

        
        protected override async UniTask<int> UploadToCloud(string key, byte[] data)
        {
            var containerName = $"{key}";
            var blobBufferName = $"{key}_blobBuffer";

            //Enter your display name here
            var displayName = $"{key}_{DateTime.Now}";


            Debug.Log($"Saving Container: {containerName}. blob Name: {blobBufferName}");



            UniTaskCompletionSource<int> getOrCreateContainerTaskCompletionSource = new UniTaskCompletionSource<int>();

            _saveManager.GetOrCreateContainer(containerName, hresult => getOrCreateContainerTaskCompletionSource.TrySetResult(hresult));

            int result = await getOrCreateContainerTaskCompletionSource.Task;

            if (HR.FAILED(result))
            {
                return result;
            }

            UniTaskCompletionSource<int> saveBlobTaskCompletionSource = new UniTaskCompletionSource<int>();


            _saveManager.SaveGame(displayName,
                blobBufferName,
                data,
                hresult => saveBlobTaskCompletionSource.TrySetResult(hresult));

            return await saveBlobTaskCompletionSource.Task;
        }
        
        protected override async UniTask<byte[]> DownloadFromCloud(string key)
        {
            var containerName = $"{key}";
            var blobBufferName = $"{key}_blobBuffer";

            Debug.Log($"Loading from  Container: {containerName}. blob Name: {blobBufferName}");


            UniTaskCompletionSource<int> getOrCreateContainerTaskCompletionSource = new UniTaskCompletionSource<int>();

            _saveManager.GetOrCreateContainer(containerName, hresult => getOrCreateContainerTaskCompletionSource.TrySetResult(hresult));

            int result = await getOrCreateContainerTaskCompletionSource.Task;

            if (HR.FAILED(result))
            {
                return null;
            }

            UniTaskCompletionSource<byte[]> DownloadBlobtaskCompletionSource = new UniTaskCompletionSource<byte[]>();

            UnityAction<int, XGameSaveBlob[]> onLoadGameCompleted = (result, saveBlobs) =>
            {

                if (HR.FAILED(result))
                {
                    Debug.Log($"Error when loading GameSave. HResult 0x{result:x}");
                    DownloadBlobtaskCompletionSource.TrySetResult(Array.Empty<byte>());
                    return;
                }

                Debug.Log($"Loading data buffer successful. Name: {saveBlobs[0].Info.Name} - Size: {saveBlobs[0].Info.Size} bytes");
                byte[] loadedData = saveBlobs[0].Data;
                DownloadBlobtaskCompletionSource.TrySetResult(loadedData);
            };

            _saveManager.LoadGame(blobBufferName, (result, xgamesaveblob) =>
            {
                onLoadGameCompleted(result, xgamesaveblob);
            });


            return await DownloadBlobtaskCompletionSource.Task;



        }
    }
}
