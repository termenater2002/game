using System;
using System.IO;
#if ENABLE_FBX_DEPENDENCY
using Autodesk.Fbx;
#endif
using Unity.Muse.Animate.Usd;
using Unity.Mathematics;
using UnityEngine;

namespace Unity.Muse.Animate.Fbx
{
    /// <summary>
    /// This class handles the export of Muse-generated animations to FBX
    /// synchronously on-device.
    /// </summary>
    static class LocalFbxExport
    {
#if ENABLE_FBX_DEPENDENCY
        static readonly string k_BipedModelPath = "Packages/com.unity.muse.animate/Runtime/PackageResources/Entities/Actors/Biped/Biped_Puppet_DeepPose.fbx";   
        /// <summary>
        /// Exports the animation of the first actor in the export data.
        /// </summary>
        /// <param name="exportData">The scene data to export.</param>
        /// <param name="name">The name that will be used for the take..</param>
        /// <param name="filePath">The path where the exported FBX file should be saved.</param>
        public static void Export(ExportData exportData, string name, string filePath)
        {
            using var fbxManager = FbxManager.Create();
            var fbxIOSettings = FbxIOSettings.Create(fbxManager, Globals.IOSROOT);
            fbxManager.SetIOSettings(fbxIOSettings);

            var fbxExporter = SetupFBXFile(filePath, fbxManager);
            var fbxImporter = FbxImporter.Create(fbxManager, "Importer");

            var fileFormat = -1;
            fbxImporter.Initialize(Path.GetFullPath(k_BipedModelPath), fileFormat, fbxManager.GetIOSettings());

            var exportScene = CreateFBXScene(fbxManager, name);

            fbxImporter.Import(exportScene);

            var workspace = new ExportWorkspace(exportData);
            CollectHierarchyFromImportedFile(workspace, exportScene);

            var unityAxisSystem = new FbxAxisSystem(FbxAxisSystem.EUpVector.eYAxis, FbxAxisSystem.EFrontVector.eParityOddNegative, FbxAxisSystem.ECoordSystem.eRightHanded);
            unityAxisSystem.DeepConvertScene(exportScene);

            SetDefaultCamera(exportScene);

            ConfigureForExport(workspace);
            CreateAnimation(workspace, name, exportScene);
            ConfigureSceneSettings(exportScene);

            // The Maya axis system has Y up, Z forward, X to the left (right handed system with odd parity).
            // We need to export right-handed for Maya because ConvertScene (used by Maya and Max importers) can't switch handedness:
            // https://forums.autodesk.com/t5/fbx-forum/get-confused-with-fbxaxissystem-convertscene/td-p/4265472
            // This needs to be done last so that everything is converted properly.
            FbxAxisSystem.MayaYUp.DeepConvertScene(exportScene);

            // Export the scene to the file.
            fbxExporter.Export(exportScene);

            // cleanup
            exportScene.Destroy();
            fbxExporter.Destroy();
            fbxImporter.Destroy();
        }

        /// <summary>
        /// Creates all the animation information relative to a single Actor
        /// </summary>
        /// <param name="exportWorkspace">The current export workspace.</param>
        /// <param name="fbxScene">The FBX scene in which the Animation data will be exported</param>
        /// <param name="takeName">The name to use for the exported animation take.</param>
        /// <exception cref="InvalidOperationException">Throws InvalidOperationException if the data is malformed and an actor is not found.</exception>
        static void CreateAnimation(ExportWorkspace exportWorkspace, string takeName, FbxScene fbxScene)
        {
            // setup anim stack
            var fbxAnimStack = FbxAnimStack.Create(fbxScene, takeName);
            fbxAnimStack.Description.Set("Animation Take: " + fbxAnimStack.Description);

            // add one mandatory animation layer
            FbxAnimLayer fbxAnimLayer = FbxAnimLayer.Create(fbxScene, "Animation Base Layer");
            fbxAnimStack.AddMember(fbxAnimLayer);

            //set time span
            var startTime = FbxTime.FromSecondDouble(0);
            var endTime = FbxTime.FromFrame(exportWorkspace.TimelineModel.FramesCount, FbxTime.EMode.eFrames30);
            fbxAnimStack.SetLocalTimeSpan(new FbxTimeSpan(startTime, endTime));

            //Acquire curves for all FBXNodes
            for (var i = 0; i < exportWorkspace.NumJoints; i++)
            {
                var fbxNode = exportWorkspace.FbxNodes[i];
                exportWorkspace.FbxAnimCurves[i * 6 + 0] = fbxNode.LclTranslation.GetCurve(fbxAnimLayer, Globals.FBXSDK_CURVENODE_COMPONENT_X, true);
                exportWorkspace.FbxAnimCurves[i * 6 + 1] = fbxNode.LclTranslation.GetCurve(fbxAnimLayer, Globals.FBXSDK_CURVENODE_COMPONENT_Y, true);
                exportWorkspace.FbxAnimCurves[i * 6 + 2] = fbxNode.LclTranslation.GetCurve(fbxAnimLayer, Globals.FBXSDK_CURVENODE_COMPONENT_Z, true);
                exportWorkspace.FbxAnimCurves[i * 6 + 3] = fbxNode.LclRotation.GetCurve(fbxAnimLayer, Globals.FBXSDK_CURVENODE_COMPONENT_X, true);
                exportWorkspace.FbxAnimCurves[i * 6 + 4] = fbxNode.LclRotation.GetCurve(fbxAnimLayer, Globals.FBXSDK_CURVENODE_COMPONENT_Y, true);
                exportWorkspace.FbxAnimCurves[i * 6 + 5] = fbxNode.LclRotation.GetCurve(fbxAnimLayer, Globals.FBXSDK_CURVENODE_COMPONENT_Z, true);
            }

            //Export all frames
            for (var i = 0; i < exportWorkspace.TimelineModel.FramesCount; i++)
            {
                var frame = exportWorkspace.TimelineModel.GetFrame(i);
                if (!frame.TryGetModel(exportWorkspace.EntityID, out BakedArmaturePoseModel poseModel))
                    throw new InvalidOperationException("Actor not found");

                for (var j = 0; j < exportWorkspace.NumJoints; j++)
                {
                    var position = poseModel.LocalPose.GetPosition(j);
                    var rotation = poseModel.LocalPose.GetRotation(j).eulerAngles;

                    var txCurve = exportWorkspace.FbxAnimCurves[j * 6 + 0];
                    var tyCurve = exportWorkspace.FbxAnimCurves[j * 6 + 1];
                    var tzCurve = exportWorkspace.FbxAnimCurves[j * 6 + 2];

                    var rxCurve = exportWorkspace.FbxAnimCurves[j * 6 + 3];
                    var ryCurve = exportWorkspace.FbxAnimCurves[j * 6 + 4];
                    var rzCurve = exportWorkspace.FbxAnimCurves[j * 6 + 5];

                    txCurve.KeyAdd(FbxTime.FromFrame(i, FbxTime.EMode.eFrames30));
                    txCurve.KeySetValue(i, position.x);

                    tyCurve.KeyAdd(FbxTime.FromFrame(i, FbxTime.EMode.eFrames30));
                    tyCurve.KeySetValue(i, position.y);

                    tzCurve.KeyAdd(FbxTime.FromFrame(i, FbxTime.EMode.eFrames30));
                    tzCurve.KeySetValue(i, position.z);

                    rxCurve.KeyAdd(FbxTime.FromFrame(i, FbxTime.EMode.eFrames30));
                    rxCurve.KeySetValue(i, rotation.x);

                    ryCurve.KeyAdd(FbxTime.FromFrame(i, FbxTime.EMode.eFrames30));
                    ryCurve.KeySetValue(i, rotation.y);

                    rzCurve.KeyAdd(FbxTime.FromFrame(i, FbxTime.EMode.eFrames30));
                    rzCurve.KeySetValue(i, rotation.z);
                }
            }
            //Unroll Euler curves
            for (var i = 0; i < exportWorkspace.NumJoints; i++)
            {
                var rxCurve = exportWorkspace.FbxAnimCurves[i * 6 + 3];
                var ryCurve = exportWorkspace.FbxAnimCurves[i * 6 + 4];
                var rzCurve = exportWorkspace.FbxAnimCurves[i * 6 + 5];

                var x = rxCurve.KeyGetValue(0);
                var y = ryCurve.KeyGetValue(0);
                var z = rzCurve.KeyGetValue(0);
                var prev = new float3(x, y, z);

                for (var j = 1; j < exportWorkspace.TimelineModel.FramesCount; j++)
                {
                    x = rxCurve.KeyGetValue(j);
                    y = ryCurve.KeyGetValue(j);
                    z = rzCurve.KeyGetValue(j);

                    var current =
                        new float3(rxCurve.KeyGetValue(j),
                            ryCurve.KeyGetValue(j),
                            rzCurve.KeyGetValue(j));

                    current = math.degrees(EulerUtils.ClosestEuler(math.radians(current), math.radians(prev)));
                    rxCurve.KeySetValue(j, current.x);
                    ryCurve.KeySetValue(j, current.y);
                    rzCurve.KeySetValue(j, current.z);

                    prev = current;
                }
            }
        }


        /// <summary>
        /// Initializes the FBX camera to a decent default
        /// </summary>
        /// <param name="fbxScene">The scene for which the camera settings will be initialized.</param>
        static void SetDefaultCamera(FbxScene fbxScene)
        {
            fbxScene.GetGlobalSettings().SetDefaultCamera(Globals.FBXSDK_CAMERA_PERSPECTIVE);
        }

        /// <summary>
        /// Utility class used to simplify the handling of the export process.
        /// </summary>
        class ExportWorkspace
        {
            public readonly FbxNode[] FbxNodes;
            public readonly FbxAnimCurve[] FbxAnimCurves;
            ArmatureMappingComponent Armature => m_ExportData.ActorsData[0].PosingArmature;
            public ActorDefinitionComponent Actor => m_ExportData.ActorsData[0].ActorDefinitionComponent;
            public int NumJoints => m_ExportData.ActorsData[0].PosingArmature.NumJoints;

            readonly ExportData m_ExportData;
            public BakedTimelineModel TimelineModel => m_ExportData.BackedTimeline;

            public Transform[] Transforms => Armature.ArmatureMappingData.Transforms;

            public EntityID EntityID => m_ExportData.ActorsData[0].ActorModel.EntityID;

            public ExportWorkspace(ExportData exportData)
            {
                m_ExportData = exportData;
                FbxNodes = new FbxNode[NumJoints];
                FbxAnimCurves = new FbxAnimCurve[NumJoints * 6];
            }
        }

        static FbxNode GetNodeWithNameRecursive(FbxNode node, string name)
        {
            if (node.GetName() == name)
                return node;

            for (var i = 0; i < node.GetChildCount(); i++)
                if (GetNodeWithNameRecursive(node.GetChild(i), name) is { } foundNode)
                    return foundNode;

            return null;
        }

        static void CollectHierarchyFromImportedFile(ExportWorkspace workspace, FbxScene fbxScene)
        {
            var rootName = workspace.Actor.ReferenceViewArmature.transform.GetChild(0).name;
            var rootNode = GetNodeWithNameRecursive(fbxScene.GetRootNode(), rootName);

            for (var i = 0; i < workspace.NumJoints; i++)
            {
                workspace.FbxNodes[i] = GetNodeWithNameRecursive(rootNode, workspace.Transforms[i].name);
            }
        }

        static void ConfigureForExport(ExportWorkspace workspace)
        {
            for (var i = 0; i < workspace.NumJoints; i++)
            {
                // Fbx rotation order is XYZ, but Unity rotation order is ZXY.
                // Also, DeepConvert does not convert the rotation order (assumes XYZ), unless RotationActive is true.
                workspace.FbxNodes[i].SetRotationOrder(FbxNode.EPivotSet.eSourcePivot, FbxEuler.EOrder.eOrderZXY);
                workspace.FbxNodes[i].SetRotationOrder(FbxNode.EPivotSet.eDestinationPivot, FbxEuler.EOrder.eOrderXYZ);
                workspace.FbxNodes[i].SetRotationActive(true);
            }
        }

        /// <summary>
        /// Initializes the FBX importer with valid default options/
        /// </summary>
        /// <param name="filePath">The path where the file will be saved.</param>
        /// <param name="fbxManager">The FBXManagers which owns the exporter</param>
        /// <returns>A new FBX exporter.</returns>
        /// <exception cref="InvalidOperationException">Can throw if the FBX exporter initialization is unsuccessful.</exception>
        static FbxExporter SetupFBXFile(string filePath, FbxManager fbxManager)
        {
            // Create the exporter
            var fbxExporter = FbxExporter.Create(fbxManager, "Exporter");

            var fileFormat = -1;
#if UNITY_MUSE_ANIMATE_FBX_EXPORT_DEBUG_EXPORT_FILE
            fileFormat = fbxManager.GetIOPluginRegistry().FindWriterIDByDescription("FBX ascii (*.fbx)");
#endif
            var status = fbxExporter.Initialize(filePath, fileFormat, fbxManager.GetIOSettings());

            if (!status)
                throw new InvalidOperationException("Unknown error initializing the FBX exporter.");

            return fbxExporter;
        }

        /// <summary>
        /// Configures the scene with good default settings.
        /// Defaults were copied from the Unity FBX Exporter package.
        /// </summary>
        /// <param name="fbxScene">The scene to configure.</param>
        static void ConfigureSceneSettings(FbxScene fbxScene)
        {
            // Set up the axes (Y up, Z forward, X to the right) and units (centimeters)
            // Exporting in centimeters as this is the default unit for FBX files, and easiest
            // to work with when importing into Maya or Max
            var fbxSettings = fbxScene.GetGlobalSettings();
            fbxSettings.SetSystemUnit(FbxSystemUnit.m);

            //Muse Animate always generates at 30 frames per second
            fbxScene.GetGlobalSettings().SetTimeMode(FbxTime.EMode.eFrames30);
        }

        /// <summary>
        /// Creates a FBX scene for export.
        /// </summary>
        /// <param name="fbxManager">The FBX manager which will own the scene.</param>
        /// <param name="name">The name of the take to use as title.</param>
        /// <returns></returns>
        static FbxScene CreateFBXScene(FbxManager fbxManager, string name)
        {
            var fbxScene = FbxScene.Create(fbxManager, "Scene");

            // set up the scene info
            var fbxSceneInfo = FbxDocumentInfo.Create(fbxManager, "SceneInfo");
            fbxSceneInfo.mTitle = name;
            fbxSceneInfo.mSubject = "Unity Muse-generated Animation";
            fbxSceneInfo.mAuthor = "Unity Technologies";
            fbxSceneInfo.mRevision = "1.0";
            fbxSceneInfo.Original_ApplicationName.Set("Unity Muse Animate");

            // set last saved to be the same as original, as this is a new file.
            fbxSceneInfo.LastSaved_ApplicationName.Set(fbxSceneInfo.Original_ApplicationName.Get());
            fbxScene.SetSceneInfo(fbxSceneInfo);
            return fbxScene;
        }

#else
        public static void Export(ExportData exportData, string name, string filePath)
        {
            Debug.Log("Fbx Export requires FBX package to be installed");
        }
#endif
    }
}