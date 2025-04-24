using System.Collections.Generic;
using System.Linq;

namespace Unity.Muse.Common
{
    internal static class ArtifactExtensions
    {
        /// <summary>
        /// Utility method to validate an artifact.
        /// </summary>
        /// <param name="artifact">Artifact to validate</param>
        /// <returns><c>true</c> if the artifact is valid, <c>false</c> otherwise</returns>
        public static bool IsValid(this Artifact artifact)
        {
            return artifact != null && !string.IsNullOrEmpty(artifact?.Guid);
        }

        /// <summary>
        /// Utility method to get operators of a certain type from an artifact.
        /// </summary>
        /// <param name="artifact">Artifact to get operators from.</param>
        /// <typeparam name="T">Type of operators to get.</typeparam>
        /// <returns></returns>
        public static T GetOperator<T>(this Artifact artifact) where T: class, IOperator
        {
            return artifact == null ? null : artifact.GetOperators().GetOperator<T>();
        }

        /// <summary>
        /// Utility method to get operators of a certain type from a list of operators.
        /// </summary>
        /// <param name="operators">The operators to filter.</param>
        /// <typeparam name="T">Type of operators to get.</typeparam>
        /// <returns></returns>
        public static T GetOperator<T>(this IEnumerable<IOperator> operators) where T: class, IOperator
        {
            return operators?.FirstOrDefault(x => x != null && (x.GetType() == typeof(T) || x.GetType().IsSubclassOf(typeof(T)))) as T;
        }

        /// <summary>
        /// Utility method to get operators of a certain type with a certain key from a list of operators.
        /// </summary>
        /// <param name="operators">The operators to filter.</param>
        /// <param name="key">The operator key being searched</param>
        /// <typeparam name="T">Type of operators to get.</typeparam>
        /// <returns></returns>
        public static T GetOperatorByKey<T>(this IEnumerable<IOperator> operators, string key) where T: class, IOperator
        {
            return operators?.FirstOrDefault(x => (x.GetType() == typeof(T) || x.GetType().IsSubclassOf(typeof(T)))
                && x.GetOperatorKey() == key) as T;
        }

        /// <summary>
        /// Utility method to remove operators of a certain type from an artifact.
        /// </summary>
        /// <param name="artifact">Artifact to get operators from.</param>
        /// <typeparam name="T">Type of operators to get.</typeparam>
        /// <returns></returns>
        public static bool RemoveOperator<T>(this Artifact artifact) where T: class, IOperator
        {
            T op =artifact?.GetOperators().GetOperator<T>();
            if (op != null)
            {
                artifact.GetOperators().Remove(op);
                return true;
            }

            return false;
        }
    }
}
