using UnityEngine;
using UnityEditor;
using System.IO;
using Studio23.SS2.SaveSystem.XboxCorePc.Data;


namespace Studio23.SS2.CloudSave.Editor
{

    public class XBoxPCProviderPCEditor : EditorWindow
    {
        [MenuItem("Studio-23/Save System/CloudProviders/Create GameCore PC Provider")]
        static void CreateDefaultProvider()
        {
            XboxPCCloudSaveProvider providerSettings = ScriptableObject.CreateInstance<XboxPCCloudSaveProvider>();

            // Create the resource folder path
            string resourceFolderPath = "Assets/Resources/SaveSystem/CloudProviders";

            if (!Directory.Exists(resourceFolderPath))
            {
                Directory.CreateDirectory(resourceFolderPath);
            }

            // Create the ScriptableObject asset in the resource folder
            string assetPath = resourceFolderPath + "/CloudSaveProvider.asset";
            AssetDatabase.CreateAsset(providerSettings, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("Default Cloud Provider created at: " + assetPath);
        }
    }

}
