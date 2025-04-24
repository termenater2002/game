using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;
#if !UNITY_EDITOR
using UnityEngine;
#endif

namespace Unity.Muse.Common
{
    internal static class OperatorsFactory
    {
        static Dictionary<string, Type> s_AvailableOperatorTypes;

        public static bool RegisterOperator<T>() where T : IOperator, new()
        {
            var operatorType = typeof(T);
            var operatorInstance = (IOperator)Activator.CreateInstance(operatorType);

            s_AvailableOperatorTypes ??= new Dictionary<string, Type>();
            return s_AvailableOperatorTypes.TryAdd(operatorInstance.OperatorName, operatorType);
        }

        public static IOperator GetOperatorInstance(string operatorName, string key = null)
        {
            if (s_AvailableOperatorTypes == null || !s_AvailableOperatorTypes.TryGetValue(operatorName, out var operatorType))
            {
                Debug.LogError($"Operator {operatorName} not found.");
                return null;
            }

            var instance = (IOperator)Activator.CreateInstance(operatorType);
            if (!string.IsNullOrEmpty(key))
                instance.SetOperatorKey(key);
            return instance;
        }

        public static string GetOperatorKey(this IOperator op)
        {
            if (op == null)
                return null;

            var data = op.GetOperatorData();
            return string.IsNullOrEmpty(data.key) ? op.OperatorName : data.key;
        }

        public static void SetOperatorKey(this IOperator op, string key)
        {
            var operatorData = op.GetOperatorData();
            operatorData.key = key;
            op.SetOperatorData(operatorData);
        }

#if !UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
#endif
        [Preserve]
        public static void RegisterDefaultOperators()
        {
            RegisterOperator<GenerateOperator>();
            RegisterOperator<LoraOperator>();
            RegisterOperator<MaskOperator>();
            RegisterOperator<PromptOperator>();
            RegisterOperator<ReferenceOperator>();
            RegisterOperator<UpscaleOperator>();
        }
    }
}
