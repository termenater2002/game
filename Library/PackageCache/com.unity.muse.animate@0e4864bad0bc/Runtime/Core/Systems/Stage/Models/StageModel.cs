using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

namespace Unity.Muse.Animate
{
    /// <summary>
    /// Represents the scene composition / entities.
    /// </summary>
    [Serializable]
    class StageModel : ICopyable<StageModel>, IEntityIDProvider, ISerializationCallbackReceiver
    {
        internal const int initialActorCapacity = 8;
        internal const int initialPropsCapacity = 8;

        public ApplicationContext ApplicationContext
        {
            get => m_ApplicationContext;
            set => SetContext(value);
        }
        
        public AuthorContext AuthorContext => ApplicationContext.AuthorContext;
        public AuthorTimelineContext TimelineContext => AuthorContext.TimelineContext;
        public AuthorMotionToTimelineContext MotionToTimelineContext => AuthorContext.MotionToTimelineContext;
        public PropRegistry PropRegistry => ApplicationContext.PropRegistry;
        public ActorRegistry ActorRegistry => ApplicationContext.ActorRegistry;
        
        public int NumActors => m_Data.Actors.Count;
        public int NumProps => m_Data.Props.Count;
        public int NumCameraViewpoints => m_Data.CameraViewpoints.Count;
        
        [SerializeField]
        StageData m_Data;
        [SerializeField]
        string m_Identifier;
        
        [NonSerialized]
        ApplicationContext m_ApplicationContext;
        [NonSerialized]
        Dictionary<ActorID, ActorDefinitionComponent> m_InstantiatedActors;
        [NonSerialized]
        Dictionary<PropID, PropDefinitionComponent> m_InstantiatedProps;
        [NonSerialized]
        public TimelineModel WorkingTimeline;
        [NonSerialized]
        public BakedTimelineModel WorkingBakedTimeline;
        [NonSerialized]
        public BakedTimelineMappingModel WorkingBakedTimelineMapping;
        
        public bool IsValid
        {
            get
            {
                if (m_Data.Actors == null || WorkingTimeline == null || WorkingBakedTimeline == null)
                    return false;

                if (!WorkingTimeline.IsValid || !WorkingBakedTimeline.IsValid)
                    return false;

                foreach (var actor in m_Data.Actors)
                {
                    if (actor == null || !actor.IsValid)
                        return false;
                }

                return true;
            }
        }

        public StageModel(string identifier = "", string name = "Unnamed")
        {
            m_Identifier = identifier;
            m_Data = new StageData
            {
                Version = StageData.DataVersion,
                NextEntityId = 0,
                Name = name,
                Actors = new List<ActorModel>(initialActorCapacity),
                Props = new List<PropModel>(initialPropsCapacity),
                CameraViewpoints = new List<CameraCoordinatesModel>()
            };

            InitializeNonSerializedData();
        }

        void InitializeNonSerializedData()
        {
            WorkingTimeline ??= new TimelineModel();
            WorkingBakedTimeline ??= new BakedTimelineModel();
            WorkingBakedTimelineMapping ??= new BakedTimelineMappingModel();

            m_InstantiatedActors = new Dictionary<ActorID, ActorDefinitionComponent>();
            m_InstantiatedProps = new Dictionary<PropID, PropDefinitionComponent>();
        }

        public StageModel(StageModel source, string identifier = "")
        {
            m_Identifier = identifier;
            InitializeNonSerializedData();
            source.CopyTo(this);
        }

        public StageModel Clone()
        {
            return new StageModel(this);
        }

        public void CopyTo(StageModel target)
        {
            // Deep-Copy Serialized data over to target
            var copiedData = new StageData();
            copiedData.Version = m_Data.Version;
            copiedData.Name = m_Data.Name;
            
            // Deep copy the lists contained within StageData
            copiedData.Actors = new List<ActorModel>();
            copiedData.Props = new List<PropModel>();
            copiedData.CameraViewpoints = new List<CameraCoordinatesModel>();

            foreach (var actorModel in m_Data.Actors)
            {
                copiedData.Actors.Add(actorModel.Clone());
            }

            foreach (var propModel in m_Data.Props)
            {
                copiedData.Props.Add(propModel.Clone());
            }

            foreach (var viewpoint in m_Data.CameraViewpoints)
            {
                copiedData.CameraViewpoints.Add(viewpoint.Clone());
            }

            target.m_Data = copiedData;
            
            Log($"CopyTo({target.m_Identifier} has {target.NumActors} actors after copy)");
        }

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {
            InitializeNonSerializedData();
        }

        public string GetName()
        {
            return "StageModel";
        }

        public void SetContext(ApplicationContext applicationContext)
        {
            Log($"SetContext({applicationContext})");
            m_ApplicationContext = applicationContext;
        }

        public void LoadDefaultScene()
        {
            Log("LoadDefaultScene()");
            
            // NOTE: This is a placeholder, it ensures the scene always has a starting character
            CreateActor("biped", Vector3.zero, Quaternion.identity);

            // Set camera position
            RestoreDefaultCameraViewpoint();
            var defaultViewpoint = GetCameraViewpoint(0);
            ApplicationContext.CameraMovement.SetCoordinates(defaultViewpoint.Pivot, defaultViewpoint.CameraPosition);
        }

        public void RestoreDefaultCameraViewpoint()
        {
            m_Data.CameraViewpoints.Clear();
            AddCameraViewpoint(new Vector3(0f, 0.7f, 0f), Quaternion.Euler(13f, 128f, 0f), 3.5f);
        }

        public void CheckIntegrity()
        {
            if (m_Data.Actors.Count == 0)
            {
                LogError("CheckIntegrity()... No actors!");
            }
            else
            {
                Log("CheckIntegrity()... passed!");
            }
        }
        
        public void Load(ApplicationContext applicationContext, StageModel source)
        {
            Log($"Load({source})");
            source.CopyTo(this);
            SetContext(applicationContext);
            LoadInstances();
            CheckIntegrity();
        }

        void LoadInstances()
        {
            Log($"LoadInstances()");
            foreach (var actorModel in m_Data.Actors)
            {
                InstantiateActor(actorModel);
            }

            foreach (var propModel in m_Data.Props)
            {
                InstantiateProp(propModel);
            }
        }
        
        void InitializeKeyPoses(ActorModel actorModel)
        {
            Log($"InitializeKeyPoses({actorModel.ID})");
            for (var i = 0; i < TimelineContext.Stage.WorkingTimeline.KeyCount; i++)
            {
                var key = TimelineContext.Stage.WorkingTimeline.Keys[i];
                AuthorContext.PoseAuthoringLogic.ApplyPosingStateToKey(actorModel.EntityID, key.Key);
            }
        }

        void AddEntityToSystems(
            EntityID entityID,
            ArmatureMappingComponent referencePosingArmature,
            ArmatureMappingComponent referenceViewArmature,
            ArmatureMappingComponent referencePhysicsArmature,
            ArmatureMappingComponent referenceMotionArmature,
            ArmatureMappingComponent referenceTextToMotionArmature,
            ArmatureToArmatureMapping posingToEntityArmatureMapping,
            PhysicsEntityType physicsEntityType,
            JointMask jointMask = null)
        {
            Log($"AddEntityToSystems({entityID})");
            
            AuthorContext.PoseAuthoringLogic.AddEntity(entityID,
                referencePosingArmature,
                referenceViewArmature,
                referencePhysicsArmature,
                posingToEntityArmatureMapping,
                AuthorContext.Authoring.Timeline.AskResetPose,
                AuthorContext.Authoring.Timeline.AskCopyPose,
                AuthorContext.Authoring.Timeline.AskPastePose,
                AuthorContext.CanPastePose,
                jointMask);

            TimelineContext.TimelineBakingLogic.AddEntity(entityID, referencePhysicsArmature, referenceMotionArmature, physicsEntityType);
            TimelineContext.BakedTimelineViewLogic.AddEntity(entityID, referenceViewArmature, ApplicationLayers.LayerBaking);
            TimelineContext.LoopAuthoringLogic.AddEntity(entityID, referenceViewArmature);
            ApplicationContext.ThumbnailsService.AddEntity(entityID, referenceViewArmature);

            AuthorContext.PoseAuthoringLogic.SnapPhysicsToPosing(entityID);

            if (AuthorContext.PoseAuthoringLogic.TryGetPosingLogic(entityID, out var entityPosingLogic) && entityPosingLogic.PosingModel != null)
                TimelineContext.Stage.WorkingTimeline.AddEntity(entityID, entityPosingLogic.PosingModel, referencePhysicsArmature.NumJoints);

            // Text to Motion
            ApplicationContext.TextToMotionService.AddEntity(entityID,
                null,
                referenceTextToMotionArmature,
                PhysicsEntityType.Active);

            AuthorContext.TextToMotionTakeContext.OutputBakedTimelineViewLogic.AddEntity(entityID, referenceViewArmature, ApplicationLayers.LayerBaking);

            // Video to Motion
            // Use the same armature for baking/viewing video-to-motion
            ApplicationContext.VideoToMotionService.AddEntity(entityID,
                null,
                referenceTextToMotionArmature,
                PhysicsEntityType.Active);
            AuthorContext.VideoToMotionTakeContext.OutputBakedTimelineViewLogic.AddEntity(entityID, referenceViewArmature, ApplicationLayers.LayerBaking);

            // Motion to Timeline
            MotionToTimelineContext.OutputTimelineBaking.AddEntity(entityID, referencePhysicsArmature, referenceMotionArmature, physicsEntityType);
            MotionToTimelineContext.InputBakedTimelineViewLogic.AddEntity(entityID, referenceViewArmature, ApplicationLayers.LayerBaking);
            MotionToTimelineContext.OutputBakedTimelineViewLogic.AddEntity(entityID, referenceViewArmature, ApplicationLayers.LayerBaking);
            MotionToTimelineContext.MotionToKeysSampling.AddEntity(entityID, jointMask, referenceMotionArmature);

            // Select newly added actor, alone
            TimelineContext.EntitySelection.Clear();
            TimelineContext.EntitySelection.Select(entityID);

            // Capture default pose for Reset Pose feature
            // This is essentially a hack for 26th June Release
            // TODO: serialize default poses to assets instead
            if (TryGetActorModel(entityID, out var actorModel)
                && !AuthorContext.PoseLibrary.HasDefaultPose(actorModel)
                && entityPosingLogic != null
                && AuthorContext.PoseAuthoringLogic.TryGetSolvedPoses(entityID, out var localPose, out var globalPose))
            {
                var entityKeyModel = new EntityKeyModel(entityPosingLogic.PosingModel, localPose, globalPose);
                AuthorContext.PoseLibrary.RegisterDefaultPose(actorModel, entityKeyModel);
            }
        }

        void RemoveEntityFromSystems(EntityID entityID)
        {
            Log($"RemoveEntityFromSystems({entityID})");
            
            // Remove from timeline data
            WorkingTimeline.RemoveEntity(entityID);
            WorkingBakedTimeline.RemoveEntity(entityID);
            
            // Unselect
            TimelineContext.EntitySelection.Unselect(entityID);
            
            // Remove from Sub-Systems
            TimelineContext.LoopAuthoringLogic.RemoveEntity(entityID);
            TimelineContext.PoseAuthoringLogic.RemoveEntity(entityID);
            
            // Baking & Services
            TimelineContext.TimelineBakingLogic.RemoveEntity(entityID);
            ApplicationContext.ThumbnailsService.RemoveEntity(entityID);
            ApplicationContext.TextToMotionService.RemoveEntity(entityID);
            ApplicationContext.VideoToMotionService.RemoveEntity(entityID);
            MotionToTimelineContext.MotionToKeysSampling.RemoveEntity(entityID);
            MotionToTimelineContext.OutputTimelineBaking.RemoveEntity(entityID);
            
            // View Logics
            TimelineContext.BakedTimelineViewLogic.RemoveEntity(entityID);
            MotionToTimelineContext.InputBakedTimelineViewLogic.RemoveEntity(entityID);
            MotionToTimelineContext.OutputBakedTimelineViewLogic.RemoveEntity(entityID);
            AuthorContext.TextToMotionTakeContext.OutputBakedTimelineViewLogic.RemoveEntity(entityID);
            AuthorContext.VideoToMotionTakeContext.OutputBakedTimelineViewLogic.RemoveEntity(entityID);
        }

        public void RemoveEntity(EntityID entityID)
        {
            Log($"RemoveEntity({entityID})");
            TryGetActorModel(entityID, out var actorModel);
            TryGetPropModel(entityID, out var propModel);

            if (actorModel == null && propModel == null)
                throw new ArgumentOutOfRangeException($"Cannot find entity: {entityID}");

            if (actorModel != null)
            {
                Assert.IsNull(propModel, "Entity cannot be both actor and prop");
                RemoveActor(actorModel.ID);
            }

            if (propModel != null)
            {
                Assert.IsNull(actorModel, "Entity cannot be both actor and prop");
                RemoveProp(propModel.ID);
            }
        }

        public ActorID CreateActor(string prefabID, Vector3 position, Quaternion rotation)
        {
            Log($"CreateActor({prefabID}, {position}, {rotation})");
            
            // Create actor model
            var actorID = ActorID.Generate(this);
            var actorModel = new ActorModel(actorID, prefabID, position, rotation);
            Assert.IsTrue(actorModel.IsValid, "Actor Model is invalid");
            
            // Register actor model
            Assert.IsFalse(m_Data.Actors.Contains(actorModel), "Actor Model was already added");
            m_Data.Actors.Add(actorModel);
            
            // Instantiate the actors prefab
            InstantiateActor(actorModel);
            
            return actorID;
        }

        public void RemoveActor(ActorID actorID)
        {
            Log($"RemoveActor({actorID})");
            var success = TryGetActorModel(actorID, out var actorModel);
            if (!success)
                AssertUtils.Fail($"Could not find actor with ID: {actorID.ToString()}");

            // UnInstantiate the prefab
            UnInstantiateActor(actorModel);
            
            // Unregister the model
            m_Data.Actors.Remove(actorModel);
        }

        void UnInstantiateActor(ActorModel actorModel)
        { 
            if (!m_InstantiatedActors.ContainsKey(actorModel.ID))
                AssertUtils.Fail($"Actor instance not found for: {actorModel.ID}");
            
            RemoveEntityFromSystems(actorModel.EntityID);
            
            // Destroy instance
            GameObjectUtils.Destroy(m_InstantiatedActors[actorModel.ID].gameObject);
        }
        
        public void RemoveAllActors()
        {
            Log($"RemoveAllActors()");
            using var tmpList = TempList<ActorModel>.Allocate();

            foreach (var actorModel in m_Data.Actors)
            {
                tmpList.Add(actorModel);
            }

            foreach (var actorModel in tmpList.List)
            {
                RemoveActor(actorModel.ID);
            }
        }

        public ActorID GetActorID(int actorIndex)
        {
            var actorModel = GetActorModel(actorIndex);
            return actorModel.ID;
        }

        public ActorModel GetActorModel(int actorIndex)
        {
            if (actorIndex < 0 || actorIndex > NumActors)
                AssertUtils.Fail($"Invalid actor index: {actorIndex.ToString()}");
            var actorModel = m_Data.Actors[actorIndex];
            return actorModel;
        }

        public bool TryGetActorModel(EntityID entityID, out ActorModel model)
        {
            // TODO: fast lookup
            foreach (var actorModel in m_Data.Actors)
            {
                if (actorModel.EntityID == entityID)
                {
                    model = actorModel;
                    return true;
                }
            }

            model = null;
            return false;
        }

        public bool TryGetActorModel(ActorID actorID, out ActorModel model)
        {
            // TODO: fast lookup
            foreach (var actorModel in m_Data.Actors)
            {
                if (actorModel.ID == actorID)
                {
                    model = actorModel;
                    return true;
                }
            }

            model = null;
            return false;
        }

        public ActorDefinitionComponent GetActorInstance(ActorID actorID)
        {
            if (!m_InstantiatedActors.TryGetValue(actorID, out var actorDefinitionComponent))
                AssertUtils.Fail($"No instance found for actor: {actorID}");

            return actorDefinitionComponent;
        }

        public PropID CreateProp(string prefabID, Vector3 position, Quaternion rotation)
        {
            // Create model
            var propID = PropID.Generate(this);
            var propModel = new PropModel(propID, prefabID, position, rotation);
            Assert.IsTrue(propModel.IsValid, "Prop Model is invalid");

            // Register model
            Assert.IsFalse(m_Data.Props.Contains(propModel), "Prop Model was already added");
            m_Data.Props.Add(propModel);

            InstantiateProp(propModel);

            return propID;
        }

        public void RemoveProp(PropID propID)
        {
            var success = TryGetPropModel(propID, out var propModel);
            if (!success)
                AssertUtils.Fail($"Could not find prop with ID: {propID.ToString()}");

            if (!m_InstantiatedProps.ContainsKey(propID))
                AssertUtils.Fail($"Prop instance not found for: {propID}");

            // Remove from timeline data
            RemoveEntityFromSystems(propID.EntityID);

            var propComponent = m_InstantiatedProps[propID];
            Assert.IsNotNull(propComponent, "Got null prop instance");

            // Destroy instance
            var propInstance = propComponent.gameObject;
            GameObjectUtils.Destroy(propInstance);

            // Unregister actor model
            m_Data.Props.Remove(propModel);
        }

        public void RemoveAllProps()
        {
            using var tmpList = TempList<PropModel>.Allocate();

            foreach (var propModel in m_Data.Props)
            {
                tmpList.Add(propModel);
            }

            foreach (var propModel in tmpList.List)
            {
                RemoveProp(propModel.ID);
            }
        }

        public PropID GetPropID(int propIndex)
        {
            var propModel = GetPropModel(propIndex);
            return propModel.ID;
        }

        public PropModel GetPropModel(int propIndex)
        {
            if (propIndex < 0 || propIndex > NumProps)
                AssertUtils.Fail($"Invalid prop index: {propIndex.ToString()}");
            var propModel = m_Data.Props[propIndex];
            return propModel;
        }

        public bool TryGetPropModel(PropID propID, out PropModel model)
        {
            // TODO: fast lookup
            foreach (var propModel in m_Data.Props)
            {
                if (propModel.ID == propID)
                {
                    model = propModel;
                    return true;
                }
            }

            model = null;
            return false;
        }

        public bool TryGetPropModel(EntityID entityID, out PropModel model)
        {
            // TODO: fast lookup
            foreach (var propModel in m_Data.Props)
            {
                if (propModel.EntityID == entityID)
                {
                    model = propModel;
                    return true;
                }
            }

            model = null;
            return false;
        }

        public PropDefinitionComponent GetPropInstance(PropID propID)
        {
            if (!m_InstantiatedProps.TryGetValue(propID, out var propDefinitionComponent))
                AssertUtils.Fail($"No instance found for prop: {propID}");

            return propDefinitionComponent;
        }

        public void Unload()
        {
            Log($"Unload()");
            
            RemoveAllActors();
            RemoveAllProps();
        }

        public CameraCoordinatesModel GetCameraViewpoint(int idx)
        {
            if (idx < 0 || idx >= m_Data.CameraViewpoints.Count)
                AssertUtils.Fail($"Invalid camera viewpoint index: {idx}");

            return m_Data.CameraViewpoints[idx];
        }

        public CameraCoordinatesModel AddCameraViewpoint()
        {
            return AddCameraViewpoint(Vector3.zero, Quaternion.identity, 2f);
        }

        public CameraCoordinatesModel AddCameraViewpoint(Vector3 position, Quaternion rotation, float distanceToPivot)
        {
            var cameraCoordinates = new CameraCoordinatesModel(position, rotation, distanceToPivot);
            m_Data.CameraViewpoints.Add(cameraCoordinates);

            return cameraCoordinates;
        }

        void InstantiateActor(ActorModel actorModel)
        {
            Log($"InstantiateActor({actorModel.ID})");
            
            Assert.IsTrue(actorModel.IsValid, "Actor Model ("+actorModel.ID+") is invalid");
            Assert.IsTrue(m_Data.Actors.Contains(actorModel), "Actor Model was not registered");

            var actorID = actorModel.ID;
            var prefabID = actorModel.PrefabID;

            if (!ActorRegistry.TryGetActorDefinition(prefabID, out var actorDefinition))
                AssertUtils.Fail($"Could not find prefab in registry: {prefabID}");

            // Instantiate actor instance
            var actorPrefab = actorDefinition.Prefab.gameObject;

            if (!Locator.TryGet<IRootObjectSpawner<GameObject>>(out var spawner))
                AssertUtils.Fail("Runtime object spawner not found");

            var actorInstance = spawner.Instantiate(actorPrefab, actorModel.SpawnPosition, actorModel.SpawnRotation);
            var actorComponent = actorInstance.GetComponent<ActorDefinitionComponent>();
            Assert.IsNotNull(actorComponent, "Actor must have an ActorDefinitionComponent");

            // Register actor instance
            m_InstantiatedActors[actorID] = actorComponent;

            AddEntityToSystems(
                actorModel.EntityID,
                actorComponent.ReferencePosingArmature,
                actorComponent.ReferenceViewArmature,
                actorComponent.ReferencePhysicsArmature,
                actorComponent.ReferenceMotionArmature,
                actorComponent.ReferenceTextToMotionArmature,
                actorComponent.PosingToCharacterArmatureMapping,
                PhysicsEntityType.Active,
                actorComponent.EvaluationJointMask);

            InitializeKeyPoses(actorModel);
        }

        void InstantiateProp(PropModel propModel)
        {
            Assert.IsTrue(propModel.IsValid, "Prop Model is invalid");
            Assert.IsTrue(m_Data.Props.Contains(propModel), "Prop Model was not registered");

            var propID = propModel.ID;
            var prefabID = propModel.PrefabID;

            if (!PropRegistry.TryGetPropInfo(prefabID, out var propDefinition))
                AssertUtils.Fail($"Could not find prefab in registry: {prefabID}");

            // Instantiate actor instance
            var propPrefab = propDefinition.Prefab.gameObject;

            if (!Locator.TryGet<IRootObjectSpawner<GameObject>>(out var spawner))
                AssertUtils.Fail("Runtime object spawner not found");

            var propInstance = spawner.Instantiate(propPrefab, propModel.SpawnPosition, propModel.SpawnRotation);

            // propInstance.hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSaveInEditor;
            var propComponent = propInstance.GetComponent<PropDefinitionComponent>();
            Assert.IsNotNull(propComponent, "Actor must have a PropDefinitionComponent");

            // Register actor instance
            m_InstantiatedProps[propID] = propComponent;

            AddEntityToSystems(
                propModel.EntityID,
                propComponent.ReferencePosingArmature,
                propComponent.ReferenceViewArmature,
                propComponent.ReferencePhysicsArmature,
                null,
                null,
                null,
                PhysicsEntityType.Kinematic);
        }

        public int GetNextEntityId()
        {
            var id = m_Data.NextEntityId;
            m_Data.NextEntityId ++;
            return id;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Log(string msg)
        {
            DevLogger.LogSeverity(TraceLevel.Info, $"{GetType().Name}({m_Identifier}) -> {msg}");
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void LogError(string msg)
        {
            DevLogger.LogError(GetType().Name + " -> " + msg);
        }
    }
}
