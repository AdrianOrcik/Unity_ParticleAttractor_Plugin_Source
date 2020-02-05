using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ParticleAttractorPool : MonoBehaviour
{
  Queue<GameObject> _pooledObjects = new Queue<GameObject>();
  Queue<GameObject> _activeObjects = new Queue<GameObject>();

  public static ParticleAttractorPool Instance{ get; private set; }

  void Awake()
  {
    Instance = this;
  }

  public Queue<GameObject> GetPooledQueue()
  {
    return _pooledObjects;
  }

  public int GetPooledObjCount()
  {
    return _pooledObjects.Count;
  }

  public GameObject GetPooledObj()
  {
    return _pooledObjects.Dequeue();
  }
  
  public void AddObjToPooledQueue(GameObject obj)
  {
    _pooledObjects.Enqueue( obj );
  }

  public Queue<GameObject> GetActiveQueue()
  {
    return _activeObjects;
  }
  
  public int GetActiveObjCount()
  {
    return _activeObjects.Count;
  }
  
  public GameObject GetActiveObj()
  {
    return _activeObjects.Dequeue();
  }
  
  public void AddObjToActiveQueue(GameObject obj)
  {
    _activeObjects.Enqueue( obj );
  }
  
  public GameObject CreateOrGetPooledObj()
  {
    var obj = _pooledObjects.Count > 0 ? _pooledObjects.Dequeue() : CreateObj();
    obj.transform.localScale = new Vector3(0.0f,0.0f,0.0f);
    return obj;
  }

  public GameObject CreateObj()
  {
    var obj = new GameObject("ParticlePart_" + (_pooledObjects.Count + _activeObjects.Count));
    obj.transform.SetParent( transform );
    obj.gameObject.SetActive( false );
    return obj;
  }
}
