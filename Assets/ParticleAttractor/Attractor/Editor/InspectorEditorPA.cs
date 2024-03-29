﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DG.Tweening;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

[CustomEditor( typeof( ParticleAttractor ) )]
public class InspectorEditorPA : Editor
{
    private ReorderableList _scenarioList;
    private List<MethodInfo> _scenarioInfo = new List<MethodInfo>();

    const float X_PADDING = 10.0f;
    string _templateName;

    private ParticleAttractor ParticleAttractor => target as ParticleAttractor;
    
    void InitScenarioList()
    {
        _scenarioList = new ReorderableList(ParticleAttractor.ScenarioList,typeof(ScenarioAction), true, true, true, true);
    }
    
     void OnEnable()
     {
         InitScenarioList();

         _scenarioList.drawHeaderCallback += DrawHeader;
         _scenarioList.drawElementCallback += DrawElement;

         _scenarioList.onAddCallback += AddItem;
         _scenarioList.onRemoveCallback += RemoveItem;
         
         if(ParticleAttractor.Points == null){
             ParticleAttractor.Points = new List<Vector3>()
             {
                 ParticleAttractor.SourceTransform.GetComponent<RectTransform>().localPosition + Vector3.left,
                 ParticleAttractor.SourceTransform.GetComponent<RectTransform>().localPosition + ( Vector3.left + Vector3.up ) * 50.0f,
                 ParticleAttractor.DestinationTransform.GetComponent<RectTransform>().localPosition + ( Vector3.right + Vector3.down ) * 50.0f,
                 ParticleAttractor.DestinationTransform.GetComponent<RectTransform>().localPosition + Vector3.right,
             };
         }
     }
     
     void OnDisable()
     {
         _scenarioList.drawHeaderCallback -= DrawHeader;
         _scenarioList.drawElementCallback -= DrawElement;

         _scenarioList.onAddCallback -= AddItem;
         _scenarioList.onRemoveCallback -= RemoveItem;
     }
     
     void DrawHeader(Rect rect)
     {
         GUI.Label(rect, "Particle Scenario");
     }

     void DrawElement(Rect rect, int index, bool active, bool focused)
     {
         if( _scenarioInfo.Count == 0 ) return;
         var item = ParticleAttractor.ScenarioList[index];

         EditorGUI.BeginChangeCheck();
         var paramInfo = _scenarioInfo[item.ActionID].GetParameters();
         var maxPropertyCount = _scenarioInfo.Select( info => info.GetParameters().Length ).Concat( new[] { 0 } ).Max();

         item.ActionID = EditorGUI.Popup( new Rect( rect.x + X_PADDING, rect.y, rect.width - X_PADDING, EditorGUIUtility.singleLineHeight),
             item.ActionID, _scenarioInfo.Select( scenario => scenario.Name ).ToArray() );

         for( var i = 0; i < paramInfo.Length; i++ )
         {
             var rowID = i+1;
             var rowRect = new Rect( rect.x + X_PADDING, rect.y + ( EditorGUIUtility.singleLineHeight * (rowID) ), 
                 rect.width - X_PADDING, EditorGUIUtility.singleLineHeight);
             
             if( i >= paramInfo.Length ) break;

             if( paramInfo[i].ParameterType == typeof( Vector2 ) )
             {
                 item.RangeFloatParam =  EditorGUI.Vector2Field( rowRect,paramInfo[i].Name,item.RangeFloatParam);
             }
             
             // if( paramInfo[i].ParameterType == typeof( Transform ) )
             // {
             //     //item.TransformParam = (Transform)EditorGUI.ObjectField( rowRect,paramInfo[i].Name,  item.TransformParam, typeof(Transform), true );
             // }
             
             if( paramInfo[i].ParameterType == typeof( Vector3 ) )
             {
                 if(i==1) item.VectorParam_1 =  EditorGUI.Vector3Field( rowRect,paramInfo[i].Name,item.VectorParam_1);
                 if(i==2) item.VectorParam_2 =  EditorGUI.Vector3Field( rowRect,paramInfo[i].Name,item.VectorParam_2);
             }
             
             if( paramInfo[i].ParameterType == typeof( float ))
             {
                 item.DurationFloatParam = EditorGUI.FloatField( rowRect,paramInfo[i].Name,item.DurationFloatParam);
             }

             if( paramInfo[i].ParameterType == typeof( bool ) )
             {
                 item.BoolParam = EditorGUI.Toggle( rowRect,paramInfo[i].Name,item.BoolParam);
             }
             
             if( paramInfo[i].ParameterType == typeof( Ease ) )
             {
                 item.TweenEase = (Ease)EditorGUI.EnumPopup(rowRect, paramInfo[i].Name, item.TweenEase);
             }
         }

         if (EditorGUI.EndChangeCheck())
         {
             EditorUtility.SetDirty(target);
         }
         
         _scenarioList.elementHeight = EditorGUIUtility.singleLineHeight * (paramInfo.Length + (maxPropertyCount - paramInfo.Length) + 1);
     }

     void AddItem(ReorderableList list)
     {
         ParticleAttractor.ScenarioList.Add(new ScenarioAction());

         EditorUtility.SetDirty(target);
     }

     void RemoveItem(ReorderableList list)
     {
         ParticleAttractor.ScenarioList.RemoveAt(list.index);

         EditorUtility.SetDirty(target);
     }

     public override void OnInspectorGUI()
     {


         if( GUILayout.Button( "Spawn (PlayMode)" ) )
         {
             ParticleAttractor.TestSpawn();
         }
         
         GUILayout.BeginVertical("HelpBox");
             EditorGUILayout.LabelField( "Particle Management", EditorStyles.boldLabel );
             ParticleAttractor.SpawnMode = (ParticleAttractor.ParticleSpawnMode)EditorGUILayout.EnumPopup( "Spawn Mode: ",  ParticleAttractor.SpawnMode );
             ParticleAttractor.IsDrawEnable = EditorGUILayout.Toggle( "Draw Line: ", ParticleAttractor.IsDrawEnable );
             if( ParticleAttractor.IsDrawEnable )
             {
                 if(GUILayout.Button( "Assign main nodes" ))
                 {
                     ParticleAttractor.Points[0] = ParticleAttractor.SourceTransform.position;
                     ParticleAttractor.Points[ParticleAttractor.Points.Count - 1] = ParticleAttractor.DestinationTransform.position;
                 }
             }
         GUILayout.EndVertical ();
         

         GUILayout.BeginVertical("HelpBox");
             EditorGUILayout.LabelField( "Component Settings",EditorStyles.boldLabel );
             ParticleAttractor.SourceTransform =  (Transform)EditorGUILayout.ObjectField( "Source Transform", ParticleAttractor.SourceTransform, typeof( Transform ), true );
             ParticleAttractor.DestinationTransform = (Transform)EditorGUILayout.ObjectField( "Destination Transform", ParticleAttractor.DestinationTransform, typeof( Transform ), true );
             ParticleAttractor.RenderCanvas = (Canvas)EditorGUILayout.ObjectField( "RenderCanvas", ParticleAttractor.RenderCanvas, typeof( Canvas ), true );
         GUILayout.EndVertical ();
         

         GUILayout.BeginVertical("HelpBox");
             EditorGUILayout.LabelField( "Particles Settings",EditorStyles.boldLabel );
             ParticleAttractor.SpawnAmount = EditorGUILayout.IntField( "Spawn Amount", ParticleAttractor.SpawnAmount );
             ParticleAttractor.SpawnRange = EditorGUILayout.FloatField( "Spawn Position Range", ParticleAttractor.SpawnRange );
             ParticleAttractor.DurationRandomRange = EditorGUILayout.FloatField( "Duration Random (%)", ParticleAttractor.DurationRandomRange );
             ParticleAttractor.SpawnImage = (Sprite)EditorGUILayout.ObjectField( "Sprite", ParticleAttractor.SpawnImage, typeof( Sprite ), true );
         GUILayout.EndVertical ();
         
         if( _scenarioInfo == null ) InitScenarioList();
         _scenarioList.DoLayoutList();
         _scenarioInfo = typeof( ParticleAttractor ).GetMethods(BindingFlags.NonPublic | 
             BindingFlags.Public | 
             BindingFlags.Instance).Where( p=>p.IsDefined( typeof(ParticleScenario ),true)).ToList();
         
         GUILayout.BeginVertical();
             GUILayout.BeginHorizontal();
                 
                 if(GUILayout.Button( "Save" ))
                 {
                   SaveWindowPA.SetScenarioData(ParticleAttractor.ScenarioList);
                   SaveWindowPA.Init();
                 }
                 if(GUILayout.Button( "Load" ))
                 {
                    string path = EditorUtility.OpenFilePanel("Load Scenario", "ParticleAttractorScenarioData", "*.*");
                    string[] filteredPath = path.Split('/');
                    IScenarioHandler scenarioHandler = new NewtonsoftScenarioHandler();
                    ParticleAttractor.ScenarioList = scenarioHandler.Load<List<ScenarioAction>>(filteredPath.Last());
                 }
             GUILayout.EndHorizontal();
         GUILayout.EndVertical ();
         
         base.OnInspectorGUI();
         
     }
     
     void OnSceneGUI()
     {
         if( !ParticleAttractor.IsDrawEnable ) return;
         if(ParticleAttractor.SourceTransform != null && ParticleAttractor.DestinationTransform != null){
             Handles.color = Color.green;
             for( int i = 0; i < ParticleAttractor.Points.Count-1; i++ )
                 Handles.DrawLine( ParticleAttractor.Points[i], ParticleAttractor.Points[i+1]);
         }
         
         for (int i = 0; i < ParticleAttractor.Points.Count; i++) {
             if(i==0 || i == ParticleAttractor.Points.Count-1)Handles.color = Color.red;
             else Handles.color = Color.black;
             Vector2 newPos = Handles.FreeMoveHandle(ParticleAttractor.Points[i], Quaternion.identity, 20f, Vector2.zero, Handles.CylinderHandleCap);
             ParticleAttractor.Points[i] = newPos;
         }
     }
}
