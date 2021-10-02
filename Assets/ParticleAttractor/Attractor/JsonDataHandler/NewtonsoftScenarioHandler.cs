using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public class NewtonsoftScenarioHandler : IScenarioHandler
{
    private const string FOLER_NAME = "ParticleAttractorScenarioData";
    
    public void Save( string fileName, object saveData, bool isPersistent = false)
    {
        if(isPersistent){
            SaveScenarioPersistent(fileName, saveData);
        }else{
            SaveScenario(fileName, saveData);
        }
    }

    public T Load<T>(string filename, bool isPersistent = false)
    {
        return isPersistent ? LoadScenarioPersistent<T>(filename) : LoadScenario<T>(filename);
    }
    
    void SaveScenario(string fileName, object saveData)
    {
        Directory.CreateDirectory(FOLER_NAME);
        string json = JsonConvert.SerializeObject(saveData, Formatting.Indented, new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        });
        File.WriteAllText(FOLER_NAME + "/" + fileName + ".json", json);
    } 
    
    void SaveScenarioPersistent(string fileName, object saveData)
    {
        string json = JsonConvert.SerializeObject(saveData, Formatting.Indented, new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        });
        File.WriteAllText(Application.persistentDataPath + "/" + fileName + ".json", json);
    }
    
    T LoadScenario<T>(string fileName)
    {
        string fileNameWithExtension = fileName.Contains(".json") ? fileName : fileName + ".json"; 
        string json = File.ReadAllText(FOLER_NAME + "/" + fileNameWithExtension);
        return JsonConvert.DeserializeObject<T>(json);
    }
    
    T LoadScenarioPersistent<T>(string fileName)
    {
        string fileNameWithExtension = fileName.Contains(".json") ? fileName : fileName + ".json"; 
        string json = File.ReadAllText(Application.persistentDataPath + "/" + fileNameWithExtension);
        return JsonConvert.DeserializeObject<T>(json);
    }
    
}
