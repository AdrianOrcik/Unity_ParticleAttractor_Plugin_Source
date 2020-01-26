using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class ParticleAttractor : MonoBehaviour
{
    #region PA_Events
    /// <summary>
    /// Before process callback
    /// Note: Invoked before spawn first particle
    /// </summary>
    public Action OnBeforePlay;
    
    /// <summary>
    /// After particle lifetime is zero callback
    /// Note: Invoked after first particle process is done
    /// </summary>
    public Action OnParticleDone;
    
    /// <summary>
    /// After process is done callback
    /// Note: Invoked after all particles are spawned
    /// </summary>
    public Action OnAfterPlay;
    #endregion PA_Events
    
    Sequence Sequence_1;
    Sequence Sequence_2;
    
    enum ParticleScenarioType{DoScale = 0, DoScaleUpDown = 1, DoMove = 2, 
        DoMoveSetTarget = 3, DoRotate = 4, DoFade = 5, DoBoom = 6, DoDelayInRange = 7, DoDrawMove = 8}
    public enum ParticleSpawnMode{SeveralFrames = 0, OneFrame = 1}
    
    //Particle management settings
    [HideInInspector] public ParticleSpawnMode SpawnMode;
    [HideInInspector] public bool IsParticleVisible;
    [HideInInspector] public bool IsDrawEnable;

    //Attractor target in same canvas
    [HideInInspector] public Transform SourceTransform;
    [HideInInspector] public Transform DestinationTransform;
    
    //Attractor target from different canvas
    [HideInInspector] public Transform DestinationTargetTransform;
    [HideInInspector] public Vector2 DestinationTargetOffset;
    
    //Parent canvas of attraction particles
    [HideInInspector] public Canvas RenderCanvas;
    
    //Particle behavior settings
    [HideInInspector] public Sprite SpawnImage;
    [HideInInspector] public int SpawnAmount;
    [HideInInspector] public float SpawnRange;
    [HideInInspector] public float DurationRandomRange;
    
    //Attractor data structures
    [HideInInspector] public List<ScenarioAction> ScenarioList;
    [HideInInspector] public string SelectedTemplateKey;
    [HideInInspector] public List<Vector3> Points;
    [HideInInspector] public bool IsSetupAttractor;
    [HideInInspector] public bool IsActiveProcess;
    Queue<GameObject> _pooledObjects = new Queue<GameObject>();
    Queue<GameObject> _activeObjects = new Queue<GameObject>();
    GameObject _particleObj;
    MethodInfo[] _scenarioInfos;

    void Start()
    {
        _scenarioInfos =  typeof( ParticleAttractor ).GetMethods( BindingFlags.NonPublic |
            BindingFlags.Public |
            BindingFlags.Instance ).Where( p => p.IsDefined( typeof( ParticleScenario ), true ) ).ToArray();
        
        if(SpawnMode == ParticleSpawnMode.OneFrame)
            StartCoroutine( SpawnParticlesToPool( SpawnAmount ) );
    }
    
    #region Builder_API
    public ParticleAttractor SetSourceTransform( Transform source )
    {
        SourceTransform = source;
        return this;
    }

    public ParticleAttractor SetDestinationTransform( Transform destination )
    {
        DestinationTransform = destination;
        return this;
    }

    public ParticleAttractor SetSpawnImage( Sprite image )
    {
        SpawnImage = image;
        return this;
    }

    public ParticleAttractor SetSpawnAmount( int amount )
    {
        SpawnAmount = amount;
        return this;
    }

    public ParticleAttractor InvokeSpawn()
    {
        Spawn();
        return this;
    }
    
    public ParticleAttractor InvokeSafeSpawn(Transform sourcePosition)
    {
        SafeSpawn(sourcePosition);
        return this;
    }

    public ParticleAttractor InvokeSafeSpawn()
    {
        SafeSpawn();
        return this;
    }

    public ParticleAttractor SetAttractorSetupState( bool value)
    {
        IsSetupAttractor = value;
        return this;
    }

    /// <summary>
    /// Invoke attractor process 
    /// </summary>
    public ParticleAttractor SetAsActive()
    {
        IsActiveProcess = true;
        return this;
    }
    
    /// <summary>
    /// Kill attractor process
    /// </summary>
    public ParticleAttractor SetAsDeactivated()
    {
        IsActiveProcess = false;
        return this;
    }
    
    #endregion Builder_API
    
    [ContextMenu( ( "Test Spawn" ) )]
    void Spawn()
    {
        StartCoroutine( SpawnMode == ParticleSpawnMode.OneFrame ? 
            SpawnParticlesInOneFrame( SpawnAmount ) : SpawnParticlesInSeveralFrames( SpawnAmount ) );
    }
    
    void SafeSpawn()
    {
        if( !IsSetupAttractor ) return;
        IsSetupAttractor = false;
        StartCoroutine( SpawnMode == ParticleSpawnMode.OneFrame ? 
            SpawnParticlesInOneFrame( SpawnAmount ) : SpawnParticlesInSeveralFrames( SpawnAmount ) );
    }
    
    void SafeSpawn( Transform sourceTransform )
    {
        if( !IsSetupAttractor ) return;
        IsSetupAttractor = false;
        SourceTransform = sourceTransform;
        StartCoroutine( SpawnMode == ParticleSpawnMode.OneFrame ? 
            SpawnParticlesInOneFrame( SpawnAmount ) : SpawnParticlesInSeveralFrames( SpawnAmount ) );
    }

    public void Debug_Spawn()
    {
        Debug.Log( "Debug_Spawn Invoked" );
    }
    
    public IEnumerator SpawnParticlesToPool( int spawnAmount )
    {
        for( var i = 0; i < spawnAmount; i++ )
        {
            _pooledObjects.Enqueue( CreateObj() );
            yield return typeof( WaitForEndOfFrame );
        }
    }
    
    public IEnumerator SpawnParticlesInOneFrame(int spawnAmount) 
    {
        SetAsActive();
        OnBeforePlay?.Invoke();
        for(var i = 0; i < spawnAmount; i++){

            Sequence_1 = DOTween.Sequence();
            Sequence_2 = DOTween.Sequence();
            
            _particleObj = CreateOrGetPooledObj();
            _particleObj.transform.position = GenerateSpawnPositionRange();
            _activeObjects.Enqueue( _particleObj );

            GenerateSequences();
            Sequence_1.Append( DOTween.Sequence() );
            Sequence_2.Append( DOTween.Sequence() ).OnComplete( DoComplete );
        }
        
        yield return typeof( WaitForEndOfFrame );
        foreach( var t in _activeObjects )
        {
            t.gameObject.SetActive( true );
        }
        OnAfterPlay?.Invoke();
        OnAfterPlay = null;
        SetAsDeactivated();
    }
    
    
    public IEnumerator SpawnParticlesInSeveralFrames(int spawnAmount) 
    {
        SetAsActive();
        OnBeforePlay?.Invoke();
        for(var i = 0; i < spawnAmount; i++){

            Sequence_1 = DOTween.Sequence();
            Sequence_2 = DOTween.Sequence();
            
            _particleObj = CreateOrGetPooledObj();
            _particleObj.transform.position = GenerateSpawnPositionRange();
            _particleObj.SetActive( true );
            _activeObjects.Enqueue( _particleObj );

            GenerateSequences();
            Sequence_1.Append( DOTween.Sequence() );
            Sequence_2.Append( DOTween.Sequence() ).OnComplete( DoComplete ); 
            yield return typeof( WaitForEndOfFrame );
        }
        OnAfterPlay?.Invoke();
        OnAfterPlay = null;
        SetAsDeactivated();
    }
    
    void GenerateSequences()
    {
        foreach( var action in ScenarioList )
        {
            switch( action.ActionID )
            {
                case (int)ParticleScenarioType.DoScale:
                    Sequence_2.Append( (Tween) _scenarioInfos[action.ActionID].Invoke( this, new object[]{action.VectorParam_1, GenerateInRange(action.DurationFloatParam,DurationRandomRange)} ));
                    break;
                case (int)ParticleScenarioType.DoScaleUpDown:
                    Sequence_2.Append( (Tween)_scenarioInfos[(int)ParticleScenarioType.DoScale].Invoke( this, new object[]{action.VectorParam_1, GenerateInRange(action.DurationFloatParam,DurationRandomRange)} ));
                    Sequence_2.Append( (Tween)_scenarioInfos[(int)ParticleScenarioType.DoScale].Invoke( this, new object[]{action.VectorParam_2, GenerateInRange(action.DurationFloatParam,DurationRandomRange)} ));
                    break;
                case (int)ParticleScenarioType.DoMove:
                    Sequence_2.Append( (Tween)_scenarioInfos[action.ActionID].Invoke( this, new object[]{GenerateInRange(action.DurationFloatParam,DurationRandomRange)}));
                    break;
                case (int)ParticleScenarioType.DoMoveSetTarget:
                case (int)ParticleScenarioType.DoRotate:
                    Sequence_2.Append( (Tween)_scenarioInfos[action.ActionID].Invoke( this, new object[]{action.TransformParam, GenerateInRange(action.DurationFloatParam,DurationRandomRange)}));
                    break;
                case (int)ParticleScenarioType.DoFade:
                    Sequence_2.Append( (Tween)_scenarioInfos[action.ActionID].Invoke( this, new object[]{action.FloatParam, GenerateInRange(action.DurationFloatParam,DurationRandomRange)}));
                    break;
                case (int)ParticleScenarioType.DoBoom: 
                    Sequence_1.Append( Transform_DoScale( action.VectorParam_1, action.DurationFloatParam * 0.2f ) );
                    Sequence_1.Append( Transform_DoScale( action.VectorParam_1 * 0.3f, action.DurationFloatParam * 0.8f ) );
                    Sequence_2.Append( Transform_DoRotate( new Vector3(0,0,0), 0.15f ) );
                    var x = _particleObj.transform.position.x + Random.Range( -( action.VectorParam_2.x * 100f ), ( action.VectorParam_2.x * 100f ) );
                    var y = _particleObj.transform.position.y + Random.Range( -( action.VectorParam_2.y * 100f ), ( action.VectorParam_2.y * 100f ) );
                    Sequence_2.Append( Transform_DoMove_SetPosition( new Vector3(x,y), action.DurationFloatParam * 0.7f));
                    break;
                case (int)ParticleScenarioType.DoDelayInRange:
                    Sequence_2.AppendInterval( Sequence_DoDelayInRange( action.RangeFloatParam ));
                    break;
                case (int)ParticleScenarioType.DoDrawMove:
                    Sequence_2.Append( Transform_DODrawMove(action.DurationFloatParam) );
                    break;
            }
        } 
    }
    
    #region PA_Tweens
    [ParticleScenario]
    public Tweener Transform_DoScale(Vector3 toScale, float duration)
    {
        return _particleObj.transform.DOScale( toScale, duration );
    }
    
    [ParticleScenario]
    public Tweener Transform_DoScale_UpDown(Vector3 toScaleUp,Vector3 toScaleDown, float duration)
    {
        return null; //Note: used as definition of method
    }
    
    [ParticleScenario]
    public Tweener Transform_DoMove(float duration)
    {
//        //Transform world to camera point movement
//        //Note: used when target is in another canvas
//        if(DestinationTargetTransform != null )
//        {
//            var screenPosition = Camera.main.WorldToScreenPoint( DestinationTargetTransform.position);
//            return _particleObj.transform.DOMove( new Vector3 (screenPosition.x + DestinationTargetOffset.x,
//                screenPosition.y+DestinationTargetOffset.y)  , duration);
//        }
        
        //Basic to world position movement
        return _particleObj.transform.DOMove( DestinationTransform.position, duration);
    }
    
    [ParticleScenario]
    public Tweener Transform_DoMove_SetTarget(Transform toTransform, float duration)
    {
        return _particleObj.transform.DOMove( toTransform.position, duration);
    }
    
    [ParticleScenario]
    public Tweener Transform_DoRotate(Vector3 toRotate, float duration)
    {
        return _particleObj.transform.DORotate( toRotate, duration);
    }
    
    [ParticleScenario]
    public Tweener SpriteRenderer_DoFade(float toFade, float duration)
    {
        return _particleObj.GetComponent<SpriteRenderer>().DOFade( toFade, duration );
    }
    
    [ParticleScenario]
    public Tweener Effect_DoBoom(Vector3 explosionScaleUp, Vector3 explosionMoveRange, float duration)
    {
        //Note: used as definition of method
        return null;
    }
    
    [ParticleScenario]
    public float Sequence_DoDelayInRange(Vector2 rangeFloatParam)
    {
        return Random.Range( rangeFloatParam.x,rangeFloatParam.y );
    }
    
    [ParticleScenario]
    public Tweener Transform_DODrawMove(float duration)
    {
        return _particleObj.transform.DOPath(Points.ToArray(),duration,PathType.CatmullRom,PathMode.Sidescroller2D);
    }
    
    public Tweener Transform_DoMove_SetPosition(Vector3 toPosition, float duration)
    {
        return _particleObj.transform.DOMove( toPosition, duration);
    }
    #endregion PA_Tweens
    
    void DoComplete()
    {
        var o = _activeObjects.Dequeue();
        o.SetActive( false );
        _pooledObjects.Enqueue( o );
        
        OnParticleDone?.Invoke();
        OnParticleDone = null;
    }

    GameObject CreateOrGetPooledObj()
    {
        GameObject obj;
        if(_pooledObjects.Count > 0)
        {
            obj = _pooledObjects.Dequeue();
            obj.GetComponent<Image>().sprite = SpawnImage;
        }
        else
        {
            obj = CreateObj();
        }
        
        obj.transform.localScale = new Vector3(0.0f,0.0f,0.0f);
        return obj;
    }

    GameObject CreateObj()
    {
        GameObject obj;
        obj = new GameObject("ParticlePart");
        obj.AddComponent<Image>().sprite = SpawnImage;
        obj.transform.SetParent( this.transform );
        obj.gameObject.SetActive( false );
        if(!IsParticleVisible)obj.hideFlags = HideFlags.HideInHierarchy;
        return obj;
    }
    
    float GenerateInRange( float value, float byRange )
    {
        if( (int)byRange == 0 ) return value;
        return Random.Range( value, value * 2f);
    }

    Vector3 GenerateSpawnPositionRange()
    {
        var xOffset = Random.Range( -SpawnRange,SpawnRange );
        var yOffset = Random.Range( -SpawnRange,SpawnRange );
            
        var sourcePosition = SourceTransform.position;
        return new Vector3(sourcePosition.x + xOffset,
            sourcePosition.y + yOffset, sourcePosition.z);
    }
}
