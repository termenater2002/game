using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Unity.Muse.Agent.Dynamic
{
#if CODE_LIBRARY_INSTALLED
    public
#else
    internal
#endif
    class CommandAttachment
    {
        List<Object> m_Attachments;

        public CommandAttachment(List<Object> objectAttachment)
        {
            m_Attachments = objectAttachment;
        }

        public T Get<T>(string name) where T : UnityEngine.Object
        {
            var result = m_Attachments.FirstOrDefault(o => o.name == name && o is T);
            if (result == null)
            {
                // If the attached object is not found, but only one of the requested type is present
                // Take a guess to prevent post-generation renaming or typo issue in the script
                var compatibleObjects = m_Attachments.Where(o => o is T);
                if (compatibleObjects.Count() == 1)
                    return (T)compatibleObjects.First();

                return null;
            }


            return (T)result;
        }

        public List<T> GetAll<T>() where T : UnityEngine.Object
        {
            return m_Attachments.OfType<T>().ToList();
        }
    }
}
