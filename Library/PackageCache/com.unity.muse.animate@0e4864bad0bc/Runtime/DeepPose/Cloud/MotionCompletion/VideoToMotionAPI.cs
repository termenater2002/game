using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Unity.Muse.Common.Account;
using UnityEngine;

namespace Unity.DeepPose.Cloud
{
    static partial class VideoToMotionAPI
    {
        public static string ApiName => $"/api/v2/animate/organizations/{AccountInfo.Instance.Organization.Id}/video_to_motion";
        
        public class Request : ISerializableAsForm
        {
            public byte[] VideoData { get; set; }
            public string VideoName { get; set; }
            public int? StartFrame { get; set; }
            public int? FrameCount { get; set; }
            
            public WWWForm Serialize()
            {
                var form = new WWWForm();
                form.AddBinaryData("video", VideoData, VideoName, "video/mp4");
                form.AddField("video_name", VideoName);
                if (StartFrame.HasValue)
                {
                    form.AddField("start_frame", StartFrame.Value.ToString());
                }
                if (FrameCount.HasValue)
                {
                    form.AddField("frame_count", FrameCount.Value.ToString());
                }

                return form;
            }
        }
        
        public class Response : IDisposable, IDeserializable<JObject>
        {
            public List<Frame> Frames => m_Frames;
            public float FramesPerSecond => m_FramesPerSecond;

            List<Frame> m_Frames;
            float m_FramesPerSecond;

            public Response()
            {
                m_Frames = new List<Frame>();
            }

            public void Dispose()
            {
                foreach (var frame in m_Frames)
                {
                    frame.Dispose();
                }
                m_Frames.Clear();
            }

            public void Deserialize(JObject data)
            {
                Dispose();
                
                var serializedResponse = data.ToObject<SerializedResponse>();
                
                m_Frames = new List<Frame>(serializedResponse.Frames.Length);
                for (var i = 0; i < serializedResponse.Frames.Length; i++)
                {
                    var serializedFrame = serializedResponse.Frames[i];
                    var frame = new Frame(serializedFrame);

                    m_Frames.Add(frame);
                }
                
                m_FramesPerSecond = serializedResponse.FPS;
            }
        }
    }
}
