using System;
using System.Collections.Generic;
using StyleTrainer.Backend;
using Unity.Muse.Sprite.Common.Backend;
using Unity.Muse.StyleTrainer.Debug;

namespace Unity.Muse.StyleTrainer
{
    class CheckPointStatusCheckTask
    {
        class CheckPointDataLoadRequest
        {
            public CheckPointData checkPointData;
            public Action<CheckPointData, string, bool> onDoneCallback;
        }

        List<CheckPointDataLoadRequest> m_CheckPoints = new ();
        string m_ProjectID;
        GetCheckPointStatusRestCall m_GetCheckPointStatusRestCall;
        public CheckPointStatusCheckTask(string projectID)
        {
            m_ProjectID = projectID;
        }

        public void AddCheckPoint(IList<CheckPointData> checkPointData, Action<CheckPointData, string, bool> callback)
        {
            for (int j = 0; j < checkPointData.Count; ++j)
            {
                int i = 0;
                for (; i < m_CheckPoints.Count; ++i)
                {
                    if (m_CheckPoints[i].checkPointData.guid == checkPointData[j].guid)
                    {
                        m_CheckPoints[i].onDoneCallback += callback;
                        break;
                    }
                }

                if( i >= m_CheckPoints.Count)
                {
                    var dataRequest = new CheckPointDataLoadRequest()
                    {
                        checkPointData = checkPointData[j],
                    };
                    dataRequest.onDoneCallback += callback;
                    m_CheckPoints.Add(dataRequest);
                }
            }

            if (m_GetCheckPointStatusRestCall == null)
                GetCheckPointStatusRestCall();
        }

        void GetCheckPointStatusRestCall()
        {
            if (m_CheckPoints.Count > 0)
            {
                if (m_GetCheckPointStatusRestCall == null)
                {
                    var request = new GetCheckPointStatusRequest
                    {
                        guid = m_ProjectID,
                    };
                    m_GetCheckPointStatusRestCall = new GetCheckPointStatusRestCall(ServerConfig.serverConfig, request);
                    m_GetCheckPointStatusRestCall.RegisterOnSuccess(OnGetCheckPointSuccess);
                    m_GetCheckPointStatusRestCall.RegisterOnFailure(OnGetCheckPointFailure);
                }

                m_GetCheckPointStatusRestCall.request.guids = m_CheckPoints.ConvertAll(x => x.checkPointData.guid).ToArray();
                m_GetCheckPointStatusRestCall.SendRequest();
            }
        }

        void OnGetCheckPointFailure(GetCheckPointStatusRestCall obj)
        {
            if (obj.retriesFailed)
            {
                SendFailStatusMessage(obj.request);
                m_GetCheckPointStatusRestCall.Dispose();
                m_GetCheckPointStatusRestCall = null;
                m_CheckPoints.Clear();
            }
        }

        void OnGetCheckPointSuccess(GetCheckPointStatusRestCall arg1, GetCheckPointStatusResponse arg2)
        {
            if (arg2.success)
            {
                for (int i = 0; i < arg2.results.Length; ++i)
                {
                    var statusResult = arg2.results[i];
                    for (int j = 0; j < m_CheckPoints.Count; ++j)
                    {
                        if (m_CheckPoints[j].checkPointData.guid == statusResult.guid)
                        {
                            m_CheckPoints[j].onDoneCallback(m_CheckPoints[j].checkPointData, statusResult.status, true);
                            if (statusResult.status != GetCheckPointResponse.Status.working)
                            {
                                m_CheckPoints.RemoveAt(j);
                                --j;
                            }
                            break;
                        }
                    }
                }
            }
            else
            {
                StyleTrainerDebug.LogWarning($"Get Checkpoint status call succeeded but response was not successful. {arg2.error}");
                SendFailStatusMessage(arg1.request);
            }

            GetCheckPointStatusRestCall();
        }

        void SendFailStatusMessage(GetCheckPointStatusRequest request)
        {
            for (int i = 0; i < request.guids.Length; ++i)
            {
                var guid = request.guids[i];
                for (int j = 0; j < m_CheckPoints.Count; ++j)
                {
                    if (m_CheckPoints[j].checkPointData.guid == guid)
                    {
                        m_CheckPoints[j].onDoneCallback(m_CheckPoints[j].checkPointData, GetCheckPointResponse.Status.failed, false);
                    }
                }
            }
        }
    }
}