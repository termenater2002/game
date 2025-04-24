using System;

namespace Unity.Muse.Agent.Dynamic
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
#if CODE_LIBRARY_INSTALLED
    public
#else
    internal
#endif
    class CommandParameterAttribute : Attribute
    {
    }
}
