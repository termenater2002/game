using System;
using UnityEngine;

namespace Unity.Muse.Animate
{
    class ClipboardService : IDisposable
    {
        RigidTransformModel m_RigidTransformModel = new RigidTransformModel();
        EntityKeyModel m_EntityKeyModel = null;
        KeyModel m_KeyModel = new KeyModel();
        TransitionModel m_TransitionModel = new TransitionModel();

        public bool CanPaste(RigidTransformModel destination)
        {
            return destination != null && m_RigidTransformModel != null;
        }

        public void Paste(RigidTransformModel destination)
        {
            if (!CanPaste(destination))
                return;

            m_RigidTransformModel.CopyTo(destination);
        }

        public void Copy(RigidTransformModel source)
        {
            if (source == null)
                return;

            source.CopyTo(m_RigidTransformModel);
        }

        public bool CanPaste(EntityKeyModel destination)
        {
            if (m_EntityKeyModel == null || destination == null)
                return false;

            return m_EntityKeyModel.IsCompatibleWith(destination);
        }

        public void Paste(EntityKeyModel destination, bool keepHorizontalRootOffset = false)
        {
            if (!CanPaste(destination))
                return;

            var prevRootPosition = destination.GlobalPose.GetPosition(0);
            m_EntityKeyModel.CopyTo(destination);

            if (keepHorizontalRootOffset)
            {
                var newRootPosition = destination.GlobalPose.GetPosition(0);
                var horizontalOffset = new Vector3(newRootPosition.x - prevRootPosition.x, 0f, newRootPosition.z - prevRootPosition.z);

                m_EntityKeyModel.Translate(horizontalOffset);
            }
        }

        public void Copy(EntityKeyModel source)
        {
            if (source == null)
                return;

            m_EntityKeyModel = source.Clone();
        }

        public bool CanPaste(KeyModel destination)
        {
            if (m_KeyModel == null || destination == null)
                return false;

            return m_KeyModel.HasSameEntitiesAs(destination);
        }

        public void Paste(KeyModel destination)
        {
            if (!CanPaste(destination))
                return;

            m_KeyModel.CopyTo(destination);
        }

        public void Copy(KeyModel source)
        {
            if (source == null)
                return;

            source.CopyTo(m_KeyModel);
        }

        public bool CanPaste(TransitionModel destination)
        {
            return destination != null && m_TransitionModel != null;
        }

        public void Paste(TransitionModel destination)
        {
            if (!CanPaste(destination))
                return;

            m_TransitionModel.CopyTo(destination);
        }

        public void Copy(TransitionModel source)
        {
            if (source == null)
                return;

            source.CopyTo(m_TransitionModel);
        }

        public void Dispose()
        {
            m_KeyModel = null;
            m_EntityKeyModel = null;
            m_RigidTransformModel = null;
            m_TransitionModel = null;
        }

        public bool CanPaste(LibraryItemModel targetTarget)
        {
            return false;
        }
    }
}
