using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

using Unity.Muse.Common.Account;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Unity.DeepPose.Cloud {
    static partial class JobAPI {
        public static string ApiStatusName => $"/api/v2/animate/organizations/{AccountInfo.Instance.Organization.Id}/job/";

        static readonly string k_ApiResultName = "/result";
        public static string ApiResultName => k_ApiResultName;

        public enum JobStatus {
            waiting,
            working,
            done,
            failed
		}

        public class Request : ISerializable {
            public Request() {}

            SerializedRequest SerializeImpl() {
                var serializedRequest = new SerializedRequest {};

                return serializedRequest;
            }

            public object Serialize() => SerializeImpl();
        }

        [Serializable]
        public class Response : IDisposable, IDeserializable<JObject> {
            public string Guid => m_Guid;
            public string Status => m_Status;
            public bool Success => m_Success;
            public string Error => m_Error;

            [SerializeField]
            string m_Guid;

            [SerializeField]
            string m_Status;

            [SerializeField]
            bool m_Success;

            [SerializeField]
            string m_Error;

            public Response() {
            }

            public void Dispose() {}

            public void Deserialize(JObject data) {
                Dispose();

                var serializedResponse = data.ToObject<SerializedResponse>();

                m_Guid = serializedResponse.Guid;
                m_Status = serializedResponse.Status;
                m_Success = serializedResponse.Success;
                m_Error = serializedResponse.Error;
            }
        }
    }
}