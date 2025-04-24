using System;
using System.Collections.Generic;
using System.Linq;

namespace Unity.Muse.Chat.BackendApi.Model
{
    internal partial class FunctionCall
    {
        public static IEnumerable<FunctionCall> Deduplicate(IEnumerable<FunctionCall> calls)
            => calls.Distinct();

        protected bool Equals(FunctionCall other)
        {
            if (Parameters.Count != other.Parameters.Count)
                return false;

            if (Function != other.Function)
                return false;

            for (var i = 0; i < Parameters.Count; i++)
            {
                if (Parameters[i] != other.Parameters[i])
                    return false;
            }

            return true;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((FunctionCall)obj);
        }

        public override int GetHashCode()
        {
            int code = Function.GetHashCode();
            foreach (var parameter in Parameters)
                code = HashCode.Combine(code, parameter);

            return code;
        }
    }
}
