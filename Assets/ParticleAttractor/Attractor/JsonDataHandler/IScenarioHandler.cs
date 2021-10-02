using System.IO;
using UnityEngine;

public interface IScenarioHandler
{
    public void Save(string fileName, object saveData, bool isPersistent = false);
    public T Load<T>(string filename, bool isPersistent = false);
}
