using System;
using DG.Tweening;
using UnityEngine;

#region PA_Class
public class ParticleScenario : Attribute {}

[Serializable]
public struct DifferentCanvasTarget
{
    public Transform TargetTransform;
    public Vector2 TargetLocalOffset;
}

[Serializable]
public class ScenarioAction
{
    [SerializeField] public int ActionID = 0;
    [SerializeField] public Ease TweenEase = (Ease)0;
    [SerializeField] public Vector3 VectorParam_1 = Vector3.zero;
    [SerializeField] public Vector3 VectorParam_2 = Vector3.zero;
    //todo: check if we need transform parameter
    //[SerializeField] public Transform TransformParam = null;
    [SerializeField] public float FloatParam = 0.0f;
    [SerializeField] public bool BoolParam = false;
    [SerializeField] public float DurationFloatParam = 0.0f;
    [SerializeField] public Vector2 RangeFloatParam = Vector2.zero;
}

[Serializable]
public class ParticlePartData
{
    public ParticlePartData(Sprite sprite)
    {
        Sprite = sprite;
    }
    
    public Sprite Sprite{ get; set; }
}
#endregion PA_Class

public static class ParticleAttractorHelper 
{
    /// <summary>
    /// Not used
    /// </summary>
    public static float Remap( float value, float from1, float to1, float from2, float to2 )
    {
        return ( value - from1 ) / ( to1 - from1 ) * ( to2 - from2 ) + from2;
    }
}
