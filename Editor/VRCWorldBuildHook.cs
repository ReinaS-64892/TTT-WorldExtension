using System;
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransTool.Build;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRC.SDKBase;
using VRC.SDKBase.Editor.BuildPipeline;
using static net.rs64.TexTransTool.Build.AvatarBuildUtils;

namespace net.rs64.TexTransTool.WorldExtension
{
    public class VRCWorldBuildHook : IVRCSDKBuildRequestedCallback
    {
        public int callbackOrder => -2048;

        public bool OnBuildRequested(VRCSDKRequestedBuildType requestedBuildType)
        {
            if (requestedBuildType is not VRCSDKRequestedBuildType.Scene) { return true; }

            Debug.Log("CallBy" + requestedBuildType);
            var sceneFinder = new GameObject("temp");
            try
            {
                var scene = sceneFinder.scene;
                var worldRenderers = scene.GetRootGameObjects().SelectMany(s => s.GetComponentsInChildren<Renderer>(true)).ToList();
                var findAtPhase = FindAtPhaseFromScene(scene);
                var renderersDomain = new RenderersDomain(worldRenderers, false, true);
                var session = new TexTransBuildSession(renderersDomain, findAtPhase);

                AvatarBuildUtils.ExecuteAllPhaseAndEnd(session);
                foreach (var tag in scene.GetRootGameObjects().SelectMany(s => s.GetComponentsInChildren<ITexTransToolTag>(true)).OfType<Component>())
                { UnityEngine.Object.DestroyImmediate(tag); }

                UnityEngine.Object.DestroyImmediate(sceneFinder);

                return true;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                UnityEngine.Object.DestroyImmediate(sceneFinder);
                return false;
            }
        }


        internal static List<Domain2Behavior> FindAtPhaseFromScene(Scene scene)
        {
            var rootTree = new RootBehaviorTree();
            var sceneRoots = scene.GetRootGameObjects();

            for (var i = 1; sceneRoots.Length > i; i += 1)
            {
                var root = sceneRoots[i].transform;
                if (root.GetComponent<PhaseDefinition>() != null) { continue; }
                FindDomainsTexTransBehavior(rootTree.Behaviors, rootTree.ChildeTrees, root);
            }

            var domainTreeList = new List<DomainTree>();
            foreach (var sudDomain in sceneRoots.SelectMany(g => g.GetComponentsInChildren<DomainDefinition>(true)))
            {
                var point = sudDomain.transform.parent;
                while (point?.GetComponent<DomainDefinition>() == null && point != null)
                { point = point?.parent; }

                var dt = new DomainTree();
                dt.ParentDomain = point?.gameObject;
                dt.DomainPoint = sudDomain;
                domainTreeList.Add(dt);

                FindDomainsTexTransBehavior(dt.BehaviorTree.Behaviors, dt.BehaviorTree.ChildeTrees, dt.DomainPoint.transform);
            }

            for (var i = 0; domainTreeList.Count > i; i += 1)
            {
                var sd = domainTreeList[i];

                var depth = 0;
                var wt = sd;
                while (wt?.ParentDomain != null)
                {
                    wt = domainTreeList.Find(i => i.DomainPoint.gameObject == wt.ParentDomain.gameObject);
                    depth += 1;
                }

                sd.Depth = depth;
            }

            domainTreeList.Sort((l, r) => r.Depth - l.Depth);//深いほうが先に並ぶようにする

            var domainList = new List<Domain2Behavior>();

            foreach (var domainTree in domainTreeList)
            {
                var d2b = new Domain2Behavior();

                d2b.Domain = domainTree.DomainPoint;
                RegisterDomain2Behavior(d2b, domainTree.BehaviorTree.ChildeTrees, domainTree.BehaviorTree.Behaviors);

                domainList.Add(d2b);
            }

            var rootDomain2Behavior = new Domain2Behavior();
            RegisterDomain2Behavior(rootDomain2Behavior, rootTree.ChildeTrees, rootTree.Behaviors);
            domainList.Add(rootDomain2Behavior);

            return domainList;
        }
    }
}
