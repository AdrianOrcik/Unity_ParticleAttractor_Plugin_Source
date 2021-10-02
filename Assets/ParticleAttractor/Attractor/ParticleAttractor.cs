using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Random = UnityEngine.Random;

//todo: split this script by logical scripts 
//todo: implement scenario management (save, load, remove) + inspector
//todo: implement 
public class ParticleAttractor : MonoBehaviour
{
    #region PA_Events
    /// <summary>
    /// Before process callback
    /// Note: Invoked before spawn first particle
    /// </summary>
    public Action OnBeforePlay_Action;
    
    /// <summary>
    /// After particle lifetime is zero callback
    /// Note: Invoked after first particle process is done
    /// </summary>
    public Action OnParticleDone_Action;

    /// <summary>
    /// After process is done callback
    /// Note: Invoked after all particles are spawned
    /// </summary>
    public Action OnAfterPlay_Action;
    #endregion PA_Events
    
    Sequence Sequence_1;

    enum ParticleScenarioType{DoScale = 0, DoScaleUpDown = 1, DoMove = 2, 
        DoMoveSetTarget = 3, DoRotate = 4, DoFade = 5, DoDelayInRange = 6, DoDrawMove = 7}
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
    public List<Vector3> Points;
    [HideInInspector] public bool IsSetupAttractor;
    [HideInInspector] public bool IsActiveProcess;
    ParticleAttractorPool _particleAttractorPool;
    ParticlePartData _particlePartData;
    GameObject _particleObj;
    private MethodInfo[] _scenarioInfos;
    
    //Editor based attraction events
    public UnityEvent OnBeforePlay;
    public UnityEvent OnParticleDone;
    public UnityEvent OnAfterPlay;

    private ScenarioAbstractHandler _scenarioHandler;

    //todo: finalize it
    public class PAData
    {
        public int One = 0;
        public int Two = 0;
    }

    [ContextMenu("SaveJson")]
    public void TestSaveJson()
    {
        var testData = new PAData
        {
            One = 11,
            Two = 12
        };

        _scenarioHandler = new NewtonsoftScenarioHandler();
        _scenarioHandler.Save("scenario", testData);
    }
    
    [ContextMenu("LoadJson")]
    public void TestLoadJson()
    {
        _scenarioHandler = new NewtonsoftScenarioHandler();
        var testData = _scenarioHandler.Load<PAData>("scenario");
    }
    
    void Awake()
    {
        InitParticlePool();
    }
    
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
    
    public ParticleAttractor SetOnBeforePlayEvent( Action action)
    {
        OnBeforePlay_Action = action;
        return this;
    }
    
    public ParticleAttractor SetOnParticleDoneEvent( Action action)
    {
        OnParticleDone_Action = action;
        return this;
    }
    
    public ParticleAttractor SetOnAfterPlayEvent( Action action)
    {
        OnAfterPlay_Action = action;
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

    [ContextMenu( ( "Spawn" ) )]
    public void TestSpawn()
    {
        _scenarioInfos =  typeof( ParticleAttractor ).GetMethods( BindingFlags.NonPublic |
              BindingFlags.Public |
              BindingFlags.Instance ).Where( p => p.IsDefined( typeof( ParticleScenario ), true ) ).ToArray();
        
        StartCoroutine( SpawnMode == ParticleSpawnMode.OneFrame ? 
            SpawnParticlesInOneFrame( SpawnAmount ) : SpawnParticlesInSeveralFrames( SpawnAmount ) );
    }
    
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
    
    public IEnumerator SpawnParticlesToPool( int spawnAmount )
    {
        for( var i = 0; i < spawnAmount; i++ )
        {
            if( _particleAttractorPool.GetPooledObjCount() >= spawnAmount ) yield break;
            _particleAttractorPool.AddObjToPooledQueue( _particleAttractorPool.CreateObj() );
            yield return typeof( WaitForEndOfFrame );
        }
    }
    
    public IEnumerator SpawnParticlesInOneFrame(int spawnAmount) 
    {
        SetAsActive();
        OnBeforePlay_Action?.Invoke();
        OnBeforePlay?.Invoke();
        for(var i = 0; i < spawnAmount; i++){

            Sequence_1 = DOTween.Sequence();
            
            _particleObj = _particleAttractorPool.CreateOrGetPooledObj();
            InitParticleObject( ref _particleObj, ref _particlePartData );
            _particleAttractorPool.AddObjToActiveQueue( _particleObj );

            GenerateSequences();
            Sequence_1.Append( DOTween.Sequence() ).OnComplete( DoComplete );
        }
        
        yield return typeof( WaitForEndOfFrame );
        foreach( var t in _particleAttractorPool.GetActiveQueue() )
        {
            t.gameObject.SetActive( true );
        }
        OnAfterPlay_Action?.Invoke();
        OnAfterPlay?.Invoke();
        
        OnAfterPlay = null;
        OnAfterPlay_Action = null;
        SetAsDeactivated();
    }
    
    
    public IEnumerator SpawnParticlesInSeveralFrames(int spawnAmount) 
    {
        SetAsActive();
        OnBeforePlay_Action?.Invoke();
        for(var i = 0; i < spawnAmount; i++){

            Sequence_1 = DOTween.Sequence();

            _particleObj = _particleAttractorPool.CreateOrGetPooledObj();
            
            InitParticleObject( ref _particleObj, ref _particlePartData );
            _particleObj.SetActive( true );
            _particleAttractorPool.AddObjToActiveQueue( _particleObj );

            GenerateSequences();
            Sequence_1.Append( DOTween.Sequence() ).OnComplete( DoComplete ); 
            yield return typeof( WaitForEndOfFrame );
        }
        OnAfterPlay_Action?.Invoke();
        OnAfterPlay?.Invoke();
        
        OnAfterPlay = null;
        OnAfterPlay_Action = null;
        SetAsDeactivated();
    }
    
    //todo: is possible to redesign this?
    void GenerateSequences()
    {
        foreach( var action in ScenarioList )
        {
            switch( action.ActionID )
            {
                case (int)ParticleScenarioType.DoScale:
                    Sequence_1.Append( (Tween) _scenarioInfos[action.ActionID].Invoke( this, new object[]{action.TweenEase,action.VectorParam_1, GenerateInRange(action.DurationFloatParam,DurationRandomRange)} ));
                    break;
                case (int)ParticleScenarioType.DoScaleUpDown:
                    Sequence_1.Append( (Tween)_scenarioInfos[(int)ParticleScenarioType.DoScale].Invoke( this, new object[]{action.TweenEase,action.VectorParam_1, GenerateInRange(action.DurationFloatParam,DurationRandomRange)} ));
                    Sequence_1.Append( (Tween)_scenarioInfos[(int)ParticleScenarioType.DoScale].Invoke( this, new object[]{action.TweenEase,action.VectorParam_2, GenerateInRange(action.DurationFloatParam,DurationRandomRange)} ));
                    break;
                case (int)ParticleScenarioType.DoMove:
                    Sequence_1.Append( (Tween)_scenarioInfos[action.ActionID].Invoke( this, new object[]{action.TweenEase,GenerateInRange(action.DurationFloatParam,DurationRandomRange)}));
                    break;
                case (int)ParticleScenarioType.DoMoveSetTarget:
                case (int)ParticleScenarioType.DoRotate:
                    Sequence_1.Append( (Tween)_scenarioInfos[action.ActionID].Invoke( this, new object[]{action.TweenEase,action.TransformParam, GenerateInRange(action.DurationFloatParam,DurationRandomRange)}));
                    break;
                case (int)ParticleScenarioType.DoFade:
                    Sequence_1.Append( (Tween)_scenarioInfos[action.ActionID].Invoke( this, new object[]{action.TweenEase,action.FloatParam, GenerateInRange(action.DurationFloatParam,DurationRandomRange)}));
                    break;
                case (int)ParticleScenarioType.DoDelayInRange:
                    Sequence_1.AppendInterval( Sequence_DoDelayInRange( action.RangeFloatParam ));
                    break;
                case (int)ParticleScenarioType.DoDrawMove:
                    Sequence_1.Append( Transform_DODrawMove(action.TweenEase,action.DurationFloatParam) );
                    break;
            }
        } 
    }
    
    //todo: review all tween methods + check own tween lib and implement 
    #region PA_Tweens
    [ParticleScenario]
    public Tweener Transform_DoScale(Ease tweenEase,Vector3 toScale, float duration)
    {
        return _particleObj.transform.DOScale( toScale, duration ).SetEase(tweenEase);
    }
    
    [ParticleScenario]
    public Tweener Transform_DoScale_UpDown(Ease tweenEase,Vector3 toScaleUp,Vector3 toScaleDown, float duration)
    {
        return null; //Note: used as definition of method
    }
    
    [ParticleScenario]
    public Tweener Transform_DoMove(Ease tweenEase,float duration)
    {
        //Basic to world position movement
        return _particleObj.transform.DOMove( DestinationTransform.position, duration).SetEase(tweenEase);
    }
    
    [ParticleScenario]
    public Tweener Transform_DoMove_SetTarget(Ease tweenEase,Transform toTransform, float duration)
    {
        return _particleObj.transform.DOMove( toTransform.position, duration).SetEase(tweenEase);
    }
    
    [ParticleScenario]
    public Tweener Transform_DoRotate(Ease tweenEase,Vector3 toRotate, float duration)
    {
        return _particleObj.transform.DORotate( toRotate, duration).SetEase(tweenEase);
    }
    
    [ParticleScenario]
    public Tweener SpriteRenderer_DoFade(Ease tweenEase,float toFade, float duration)
    {
        return _particleObj.GetComponent<SpriteRenderer>().DOFade( toFade, duration ).SetEase(tweenEase);
    }

    [ParticleScenario]
    public float Sequence_DoDelayInRange(Vector2 rangeFloatParam)
    {
        return Random.Range( rangeFloatParam.x,rangeFloatParam.y );
    }
    
    [ParticleScenario]
    public Tweener Transform_DODrawMove(Ease tweenEase,float duration)
    {
        return _particleObj.transform.DOPath(Points.ToArray(),duration,PathType.CatmullRom,PathMode.Sidescroller2D).SetEase(tweenEase);
    }
    
    public Tweener Transform_DoMove_SetPosition(Ease tweenEase,Vector3 toPosition, float duration)
    {
        return _particleObj.transform.DOMove( toPosition, duration).SetEase(tweenEase);
    }
    #endregion PA_Tweens
    
    void DoComplete()
    {
        var o = _particleAttractorPool.GetActiveObj();
        o.SetActive( false );
        _particleAttractorPool.AddObjToPooledQueue( o );
        
        OnParticleDone_Action?.Invoke();
        OnParticleDone?.Invoke();
        
        OnParticleDone = null;
        OnParticleDone_Action = null;
    }
    
    float GenerateInRange( float value, float byRange )
    {
        return (int)byRange == 0 ? value : Random.Range( value, value * 2f);
    }

    Vector3 GenerateSpawnPositionRange()
    {
        var xOffset = Random.Range( -SpawnRange,SpawnRange );
        var yOffset = Random.Range( -SpawnRange,SpawnRange );
            
        var sourcePosition = SourceTransform.position;
        return new Vector3(sourcePosition.x + xOffset,
            sourcePosition.y + yOffset, sourcePosition.z);
    }
    
    void InitParticleObject( ref GameObject obj , ref ParticlePartData data)
    {
        var particleImage = obj.GetComponent<Image>();
        if( !particleImage ) particleImage = obj.AddComponent<Image>();
        particleImage.sprite = data.Sprite;
        obj.transform.position = GenerateSpawnPositionRange();
    }
    
    void InitParticlePool()
    {
        _particlePartData = new ParticlePartData(SpawnImage);
        
        if(!ParticleAttractorPool.Instance){
            var obj = new GameObject("UIParticleAttractor_Pool",typeof(RectTransform));
            obj.transform.SetParent( transform.parent );
            obj.transform.localScale = new Vector3(1.0f,1.0f,1.0f);
            obj.transform.localPosition = transform.localPosition;
            obj.AddComponent<ParticleAttractorPool>();
        }
        
        _particleAttractorPool = ParticleAttractorPool.Instance;
    }
}
