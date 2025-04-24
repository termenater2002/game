using System.Collections.Generic;

namespace Unity.Muse.Animate
{
    class PoseLibraryService
    {
        Dictionary<string, EntityKeyModel> m_DefaultPoses = new();

        public bool HasDefaultPose(ActorModel actorModel)
        {
            return m_DefaultPoses.ContainsKey(actorModel.PrefabID);
        }

        public void RegisterDefaultPose(ActorModel actorModel, EntityKeyModel entityKeyModel)
        {
            if (m_DefaultPoses.TryGetValue(actorModel.PrefabID, out var registeredKey))
            {
                entityKeyModel.CopyTo(registeredKey);
            }
            else
            {
                registeredKey = entityKeyModel.Clone();
                m_DefaultPoses[actorModel.PrefabID] = registeredKey;
            }
        }

        public bool TryGetDefaultPose(ActorModel actorModel, out EntityKeyModel entityKeyModel)
        {
            return m_DefaultPoses.TryGetValue(actorModel.PrefabID, out entityKeyModel);
        }
    }
}
