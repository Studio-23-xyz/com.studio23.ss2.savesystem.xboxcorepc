using System;
using UnityEngine;

namespace Studio23.SS2.SaveSystem.XboxCorePc.Core
{
    
   
    public abstract class AbstractSaveLoadProvider : ScriptableObject
{
    public delegate void CloudSaveEvent();
    public CloudSaveEvent OnUploadSuccess;
    public CloudSaveEvent OnDownloadSuccess;

    /// <summary>
    /// You should fire OnUploadSuccess in the implementation.
    /// </summary>
    /// <param name="filepath"></param>
    public abstract void UploadToCloud(string filepath);

    /// <summary>
    /// You should fire OnDownloadSuccess in the implementation
    /// </summary>
    /// <param name="downloadLocation"></param>
    public abstract void DownloadFromCloud(string downloadLocation);
}
}
