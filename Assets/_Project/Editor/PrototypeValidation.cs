using System;
using System.IO;
using ACaldeira.Core;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ACaldeira.Editor
{
    public static partial class PrototypeBuilder
    {
        [MenuItem("Tools/A Caldeira/Validate Open Bootstrap")]
        public static void ValidatePrototype()
        {
            var scene = SceneManager.GetActiveScene();
            if (scene.name != "00_Bootstrap") throw new InvalidOperationException("Open 00_Bootstrap to validate.");
            int managers = 0, checkedReferences = 0;
            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var behaviour in root.GetComponentsInChildren<MonoBehaviour>(true))
                {
                    if (behaviour == null) throw new InvalidOperationException("Missing script in generated hierarchy.");
                    if (behaviour is GameManager) managers++;
                    if (behaviour.GetType().Namespace == null || !behaviour.GetType().Namespace.StartsWith("ACaldeira.")) continue;
                    var so = new SerializedObject(behaviour); var p = so.GetIterator();
                    while (p.NextVisible(true))
                        if (p.propertyType == SerializedPropertyType.ObjectReference)
                        {
                            checkedReferences++;
                            if (p.objectReferenceValue == null) throw new InvalidOperationException(behaviour.name + ": missing " + p.propertyPath);
                        }
                }
            }
            if (managers != 1) throw new InvalidOperationException("Expected exactly one GameManager.");
            foreach (var buildScene in EditorBuildSettings.scenes)
                if (buildScene.enabled && !File.Exists(buildScene.path)) throw new InvalidOperationException("Missing build scene: " + buildScene.path);
            Debug.Log("Bootstrap references validated: " + checkedReferences);
        }
        [MenuItem("Tools/A Caldeira/Build Windows Development")]
        public static void BuildWindows()
        {
            ValidatePrototype(); Directory.CreateDirectory("Builds/Windows");
            var scenePaths = new System.Collections.Generic.List<string>();
            foreach (var s in EditorBuildSettings.scenes) if (s.enabled) scenePaths.Add(s.path);
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = scenePaths.ToArray(), locationPathName = "Builds/Windows/ACaldeira.exe",
                target = BuildTarget.StandaloneWindows64, options = BuildOptions.Development
            });
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Windows build failed: " + report.summary.result);
        }
    }
}
