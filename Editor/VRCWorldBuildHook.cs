using System;
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransTool.Build;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace net.rs64.TexTransTool.WorldExtension
{
    public class VRCWorldBuildHook : IProcessSceneWithReport
    {
        public int callbackOrder => -2048;
        public void OnProcessScene(Scene scene, BuildReport report)
        {
            ProcessScene(scene);
        }

        private static void ProcessScene(Scene scene)
        {
            var worldRenderers = scene.GetRootGameObjects().SelectMany(s => s.GetComponentsInChildren<Renderer>(true)).ToList();

            var domains = scene.GetRootGameObjects()
                .SelectMany(g => g.GetComponentsInChildren<DomainDefinition>(true))
                .Select(c => c.gameObject)
                .Reverse().ToArray();
            foreach (var domainRoot in domains)
            {
                AvatarBuildUtils.ProcessAvatar(domainRoot, null, true);
            }
        }
    }
}
