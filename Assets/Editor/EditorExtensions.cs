using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public static class EditorExtensions
{
    [MenuItem("GameObject/Create Other/Container")]
    public static void CreateAndChild(MenuCommand menuCommand)
    {
        // Executes the command only once
        if (Selection.objects.Length > 1 && menuCommand.context != null)
        {
            if (menuCommand.context != Selection.objects[0])
            {
                return;
            }
        }

        // Create a Container gameobject
        GameObject go = new GameObject("Container");

        // Parent every selected transform
        Transform[] transforms = Selection.transforms;
        foreach (Transform t in transforms)
        {
            t.parent = go.transform;
        }

        // Select the created object and his children
        List<Object> objs = new List<Object>(Selection.objects) { go };
        Selection.objects = objs.ToArray();
    }
}