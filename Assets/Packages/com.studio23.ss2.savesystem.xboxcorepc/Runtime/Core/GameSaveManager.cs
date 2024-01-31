using Studio23.SS2.SaveSystem.XboxCorePc.Helpers;
using UnityEngine;
using UnityEngine.Events;
using XGamingRuntime;

namespace GameSave
{
    public class GameSaveManager
    {
        private XGameSaveProviderHandle m_GameSaveProviderHandle;
        private XGameSaveContainerHandle m_GameSaveContainerHandle;
        private XGameSaveUpdateHandle m_GameSaveContainerUpdateHandle;

        public void Initialize(XUserHandle userHandle, string scid, bool syncOnDemand, UnityAction<int> onInitializationComplete)
        {
            SDK.XGameSaveInitializeProviderAsync(
                userHandle,
                scid,
                syncOnDemand,
                (hresult, gameSaveProviderHandle) =>
                    OnSaveGameInitialized(hresult, gameSaveProviderHandle, onInitializationComplete));
        }

        private void OnSaveGameInitialized(int hresult, XGameSaveProviderHandle gameSaveProviderHandle, UnityAction<int> onInitializatioComplete)
        {
            if (HR.FAILED(hresult))
            {
                onInitializatioComplete?.Invoke(hresult);
                return;
            }

            m_GameSaveProviderHandle = gameSaveProviderHandle;
            onInitializatioComplete?.Invoke(hresult);
        }

        public void SaveGame(string displayName, string blobBufferName, byte[] blobData, UnityAction<int> onSaveGameCompleted)
        {

            if (m_GameSaveContainerHandle == null)
            {
                Debug.LogWarning("You have not created or retrieved a container. Not doing anything.");
                return;
            }

            //Prepare Container for Update
            var hresult = StartContainerUpdate(displayName);
            if (HR.FAILED(hresult))
            {
                onSaveGameCompleted?.Invoke(hresult);
                return;
            }

            //Actually Save data to the container
            hresult = SubmitDataBlobToWrite(blobBufferName, blobData);
            if (HR.FAILED(hresult))
            {
                onSaveGameCompleted?.Invoke(hresult);
                return;
            }


            SubmitGameSaveUpdate(onSaveGameCompleted);


        }

        public void GetOrCreateContainer(string containerName, UnityAction<int> onContainerCreated)
        {
            if (m_GameSaveProviderHandle == null)
            {
                Debug.LogWarning("Game Save Provider not initialized. Not doing anything.");
                onContainerCreated?.Invoke(-1);
                return;
            }

            var hresult =
                SDK.XGameSaveCreateContainer(m_GameSaveProviderHandle, containerName, out m_GameSaveContainerHandle);
            if (HR.FAILED(hresult))
            {
                Debug.LogError($"Error when creating the {containerName} container. HResult <color=white>0x{hresult:x}</color> <color=yellow>{(XSaveErrorCodes)hresult}</color>");
                onContainerCreated?.Invoke(hresult);
                return;
            }

            Debug.Log($"Container {containerName} obtained or created.");
            onContainerCreated?.Invoke(hresult);
        }

        public int StartContainerUpdate(string containerDisplayName)
        {
            var hresult = SDK.XGameSaveCreateUpdate(m_GameSaveContainerHandle, containerDisplayName,
                out m_GameSaveContainerUpdateHandle);
            if (HR.FAILED(hresult))
            {
                Debug.LogError(
                    $"Error when creating the {containerDisplayName} update process. HResult <color=white>0x{hresult:x}</color> <color=yellow>{(XSaveErrorCodes)hresult}</color>");
                return hresult;
            }

            Debug.Log($"Container {containerDisplayName} update process created.");
            return hresult;
        }

        public int SubmitDataBlobToWrite(string blobName, byte[] data)
        {
            if (m_GameSaveContainerUpdateHandle == null)
            {
                Debug.LogWarning("You have not started a Update save process. not doing anything");
                return -1;
            }

            var hresult = SDK.XGameSaveSubmitBlobWrite(m_GameSaveContainerUpdateHandle, blobName, data);
            if (HR.FAILED(hresult))
            {
                Debug.Log($"Error when submitting the blob {blobName}. HResult <color=white>0x{hresult:x}</color> <color=yellow>{(XSaveErrorCodes)hresult}</color>");
                return hresult;
            }
            Debug.Log($"Blob {blobName} submitted.");
            return hresult;
        }

        public void SubmitGameSaveUpdate(UnityAction<int> onSubmitGameSaveComplete)
        {
            if (m_GameSaveContainerUpdateHandle == null)
            {
                Debug.LogWarning("You have not started a Update save process. not doing anything");
                onSubmitGameSaveComplete?.Invoke(-1);
                return;
            }

            SDK.XGameSaveSubmitUpdateAsync(
                m_GameSaveContainerUpdateHandle,
                hresult => OnSubmitUpdateCompleted(hresult, onSubmitGameSaveComplete));
        }

        private void OnSubmitUpdateCompleted(int hresult, UnityAction<int> onSubmitGameSaveComplete)
        {
            if (HR.FAILED(hresult))
            {
                Debug.LogError($"Error when submitting container updated process. HResult: 0x{hresult:x}");
                onSubmitGameSaveComplete?.Invoke(hresult);
                return;
            }

            Debug.Log("Update process submitted. Closing Update handle and container.");
            SDK.XGameSaveCloseUpdateHandle(m_GameSaveContainerUpdateHandle);
            SDK.XGameSaveCloseContainer(m_GameSaveContainerHandle);
            onSubmitGameSaveComplete?.Invoke(hresult);
        }

        public void LoadGame(string[] blobBufferNames, UnityAction<int, XGameSaveBlob[]> onLoadGameCompleted)
        {
            if (m_GameSaveContainerHandle == null)
            {
                Debug.LogWarning("You have not created or retrieved a container. Not doing aything.");
                return;
            }

            SDK.XGameSaveReadBlobDataAsync(m_GameSaveContainerHandle,
                blobBufferNames, (hresult, blobs) => OnLoadSaveGameCompleted(hresult, blobs, onLoadGameCompleted));
        }

        private void OnLoadSaveGameCompleted(int hresult, XGameSaveBlob[] blobs,UnityAction<int, XGameSaveBlob[]> onLoadGameCompleted)
        {
            if (HR.FAILED(hresult))
            {
                Debug.Log($"Error when loading GameSave. HResult <color=white>0x{hresult:x}</color> <color=yellow>{(XSaveErrorCodes)hresult}</color>");
                onLoadGameCompleted?.Invoke(hresult, null);
                return;
            }

            onLoadGameCompleted?.Invoke(hresult, blobs);
        }

        public void QuerySpaceQuota(UnityAction<int, long> onSpaceQuotaRequested)
        {
            
            SDK.XGameSaveGetRemainingQuotaAsync(m_GameSaveProviderHandle,
                (hresult, remainingQuota) => onSpaceQuotaRequested?.Invoke(hresult, remainingQuota));
        }

        public void DeleteContainer(string containerName, UnityAction<int> onDeleteContainercomplete)
        {
            SDK.XGameSaveDeleteContainerAsync(m_GameSaveProviderHandle, containerName,
                hresult =>
                {
                    if (HR.FAILED(hresult))
                    {
                        Debug.Log($"Error when deleting Container. HResult <color=white>0x{hresult:x}</color> <color=yellow>{(XSaveErrorCodes)hresult}</color>");
                    }

                    onDeleteContainercomplete?.Invoke(hresult);
                });
        }

        public void CloseGameSaveHandles()
        {
            if (m_GameSaveContainerHandle != null)
            {
                Debug.Log("Closing Container handle");
                SDK.XGameSaveCloseContainer(m_GameSaveContainerHandle);
                m_GameSaveContainerHandle = null;
            }

            if (m_GameSaveContainerUpdateHandle != null)
            {
                Debug.Log("Closing Update handle");
                SDK.XGameSaveCloseUpdateHandle(m_GameSaveContainerUpdateHandle);
                m_GameSaveContainerUpdateHandle = null;
            }

            if (m_GameSaveProviderHandle != null)
            {
                Debug.Log("Closing Game Save provider handle");
                SDK.XGameSaveCloseProvider(m_GameSaveProviderHandle);
                m_GameSaveProviderHandle = null;
            }
        }
    }
}