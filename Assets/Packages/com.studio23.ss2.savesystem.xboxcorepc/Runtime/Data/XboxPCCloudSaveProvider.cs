using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameSave;
using Studio23.SS2.AuthSystem.XboxCorePC.Core;
using Studio23.SS2.CloudSave.Data;
using UnityEngine;
using UnityEngine.Events;
using XGamingRuntime;


namespace Studio23.SS2.SaveSystem.XboxCorePc.Data
{
    [CreateAssetMenu(fileName = "GameCorePC Cloud Save Provider", menuName = "Studio-23/SaveSystem/Cloud Provider/Game Core PC", order = 2)]
    public class XboxPCCloudSaveProvider : AbstractCloudSaveProvider
    {
        private GameSaveManager _saveManager;


        private void OnEnable()
        {
            _platformProvider = PlatformProvider.XBoxPC;
        }

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

        protected override async UniTask<int> UploadToCloud(string slotName, string key, byte[] data)
        {
            var containerName = $"{slotName}";
            var blobBufferName = $"{key}";

            var displayName = $"{key}_{DateTime.UtcNow}";


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

        protected override async UniTask<byte[]> DownloadFromCloud(string slotName, string key)
        {
            var containerName = $"{slotName}";
            var blobBufferName = $"{key}";

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

                    DownloadBlobtaskCompletionSource.TrySetResult(null);
                    return;
                }

                Debug.Log($"Loading data buffer successful. Name: {saveBlobs[0].Info.Name} - Size: {saveBlobs[0].Info.Size} bytes");
                byte[] loadedData = saveBlobs[0].Data;
                DownloadBlobtaskCompletionSource.TrySetResult(loadedData);
            };

            _saveManager.LoadGame(new[] { blobBufferName }, (result, xgamesaveblob) =>
            {
                onLoadGameCompleted(result, xgamesaveblob);
            });


            return await DownloadBlobtaskCompletionSource.Task;
        }

        protected override async UniTask<Dictionary<string, byte[]>> DownloadFromCloud(string slotName, string[] keys)
        {
            Dictionary<string, byte[]> savedBytes= new Dictionary<string, byte[]>();

            var containerName = $"{slotName}";

            Debug.Log($"Loading {keys.Length} keys, from  Container: {containerName}.");


            UniTaskCompletionSource<int> getOrCreateContainerTaskCompletionSource = new UniTaskCompletionSource<int>();

            _saveManager.GetOrCreateContainer(containerName, hresult => getOrCreateContainerTaskCompletionSource.TrySetResult(hresult));

            int result = await getOrCreateContainerTaskCompletionSource.Task;

            if (HR.FAILED(result))
            {
                return null;
            }

            UniTaskCompletionSource<Dictionary<string, byte[]>> DownloadBlobtaskCompletionSource = new UniTaskCompletionSource<Dictionary<string, byte[]>>();

            UnityAction<int, XGameSaveBlob[]> onLoadGameCompleted = (result, saveBlobs) =>
            {

                if (HR.FAILED(result))
                {
                    DownloadBlobtaskCompletionSource.TrySetResult(savedBytes);
                    return;
                }

                for (int i = 0; i < saveBlobs.Length; i++)
                {
                    savedBytes[keys[i]] = saveBlobs[i].Data;
                    Debug.Log($"Loading data buffer successful. Name: {saveBlobs[i].Info.Name} - Size: {saveBlobs[i].Info.Size} bytes");
                }

                DownloadBlobtaskCompletionSource.TrySetResult(savedBytes);
            };

            _saveManager.LoadGame(keys, (result, xgamesaveblob) =>
            {
                onLoadGameCompleted(result, xgamesaveblob);
            });


            return await DownloadBlobtaskCompletionSource.Task;
        }

        protected override async UniTask<int> DeleteSlotFromCloud(string slotName)
        {
            UniTaskCompletionSource<int> deleteContainerTaskSource = new UniTaskCompletionSource<int>();

            _saveManager.DeleteContainer(slotName, hresult => deleteContainerTaskSource.TrySetResult(hresult));

            return await deleteContainerTaskSource.Task;
            
        }
    }
}
