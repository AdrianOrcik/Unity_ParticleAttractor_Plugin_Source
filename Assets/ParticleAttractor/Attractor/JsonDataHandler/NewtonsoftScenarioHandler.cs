using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public class NewtonsoftScenarioHandler : ScenarioAbstractHandler
{
    private const string FOLER_NAME = "ParticleAttractorScenarioData";
    
    public override void Save( string fileName, object saveData, bool isPersistent = false)
    {
        if(isPersistent){
            SaveScenarioPersistent(fileName, saveData);
        }else{
            SaveScenario(fileName, saveData);
        }
    }

    public override T Load<T>(string filename, bool isPersistent = false)
    {
        return isPersistent ? LoadScenarioPersistent<T>(filename) : LoadScenario<T>(filename);
    }
    
    void SaveScenario(string fileName, object saveData)
    {
        if (!IsFolderValid(FOLER_NAME))
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
        string json = File.ReadAllText(FOLER_NAME + "/" + fileName + ".json");
        return JsonConvert.DeserializeObject<T>(json);
    }
    
    T LoadScenarioPersistent<T>(string fileName)
    {
        string json = File.ReadAllText(Application.persistentDataPath + "/" + fileName + ".json");
        return JsonConvert.DeserializeObject<T>(json);
    }
    
}
