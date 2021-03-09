#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FollowTarget))]
public class FollowTargetEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        FollowTarget followTargetScript = (FollowTarget) target;

        if (GUILayout.Button("Calculate Offset"))
        {
            followTargetScript.CalculateOffset();
        }
    }
}

#endif