using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Studio23.SS2.Authsystem.XboxCorePC.Core;
using UnityEngine;

 
namespace Studio23.SS2.SaveSystem.XboxCorePc.Core
{
   
    
    [CreateAssetMenu(fileName = "Save-Load Provider", menuName = "Studio-23/Save-Load Provider", order = 1)]
    public class CloudAbstractSaveLoadProvider : AbstractSaveLoadProvider
    {
        
       
        public string output;
        private PlayerSaveData playerSaveData;
      
        public override void UploadToCloud(string key, string filepath)
        {
            playerSaveData = new PlayerSaveData();
            playerSaveData.name = "Jane Doe";
            playerSaveData.level = 2;
            /*string path = Path.Combine(Application.persistentDataPath, "savedata.text");
            if (!File.Exists(path))
            {
                File.Create(path).Close();
            }*/
            
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            using (MemoryStream memoryStream = new MemoryStream())
            {
                binaryFormatter.Serialize(memoryStream, playerSaveData);
                MSGdk.Helpers.Save(memoryStream.ToArray());
               
                output = "\n Saved game data:" +
                         "\n Name: " + playerSaveData.name +
                         "\n Level: " + playerSaveData.level;
                
                /*
                using (FileStream fileStream = File.Open(path, FileMode.OpenOrCreate))
                {
                    binaryFormatter.Serialize(fileStream, playerSaveData);
                }*/
            }
        }

        public override void DownloadFromCloud(string key, string downloadLocation)
        {
            MSGdk.Helpers.OnGameSaveLoaded -= OnGameSaveLoaded;
            MSGdk.Helpers.OnGameSaveLoaded += OnGameSaveLoaded;
            MSGdk.Helpers.LoadSaveData();
        }
        
       

        private void OnGameSaveLoaded(object sender, GameSaveLoadedArgs saveData)
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            using (MemoryStream memoryStream = new MemoryStream(saveData.Data))
            {
                object playerSaveDataObj = binaryFormatter.Deserialize(memoryStream);
                playerSaveData = playerSaveDataObj as PlayerSaveData;
                
                output = "\n Loaded save game:" +
                              "\n Name: " + playerSaveData.name +
                              "\n Level: " + playerSaveData.level;
                
                string path = Path.Combine(Application.persistentDataPath, "savedata.text");
                if (!File.Exists(path))
                {
                    File.Create(path).Close();
                }
                using (FileStream fileStream = File.Open(path, FileMode.OpenOrCreate))
                {
                    binaryFormatter.Serialize(fileStream, playerSaveData);
                }
                
            }
            
          
            
        }
    }
}
