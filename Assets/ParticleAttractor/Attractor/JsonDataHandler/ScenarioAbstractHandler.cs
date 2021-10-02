using System.IO;
using UnityEngine;

public abstract class ScenarioAbstractHandler
{
    public abstract void Save(string fileName, object saveData, bool isPersistent = false);
    public abstract T Load<T>(string filename, bool isPersistent = false);

    public bool IsPathValid(string fileName, string prefix)
    {
        var isExist = File.Exists(prefix + "/" + fileName + ".json");
        if (!isExist)
            Debug.Log($"{fileName} doesnt not exist");
        return isExist;
    }

    public bool IsFolderValid(string prefix)
    {
        var isExist = File.Exists(prefix);
        if (!isExist)
            Debug.Log($"{prefix} doesnt not exist");
        return isExist;
    }
}
