using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.DeepPose.Cloud;
using Unity.Muse.Animate.Usd;
using UnityEngine;

namespace Unity.Muse.Animate.Fbx
{
    static class FBXExport
    {
        public enum Status
        {
            AvailableToPerformExport,
            InProgress,
            ExportDataReady
        }

        public static Status ExportStatus { get; private set; } = Status.AvailableToPerformExport;
        static byte[] m_ExportData = null;
        static int m_ExportId;

        static readonly Dictionary<string, int> MuseAnimateToAPIInputMapping = new()
        {
            {"Pelvis", 0},
            {"Thigh.L", 55},
            {"Calf.L", 56},
            {"Foot.L", 57},
            {"Toe.L", 58},
            {"Toe.L_end", 59},
            {"Spine_0", 1},
            {"Spine_1", 2},
            {"Chest", 3},
            {"Neck", 52},
            {"Head", 53},
            {"Head_end", 54},
            {"Clavicle.L", 4},
            {"Bicep.L", 5},
            {"Forarm.L", 6},
            {"Hand.L", 7},
            {"Thumb_0.L", 24},
            {"Thumb_1.L", 25},
            {"Thumb_2.L", 26},
            {"Thumb_2.L_end", 27},
            {"Pinky_0.L", 16},
            {"Pinky_1.L", 17},
            {"Pinky_2.L", 18},
            {"Pinky_2.L_end", 19},
            {"Ring_0.L", 20},
            {"Ring_1.L", 21},
            {"Ring_2.L", 22},
            {"Ring_2.L_end", 23},
            {"Middle_0.L", 12},
            {"Middle_1.L", 13},
            {"Middle_2.L", 14},
            {"Middle_2.L_end", 15},
            {"Index_0.L", 8},
            {"Index_1.L", 9},
            {"Index_2.L", 10},
            {"Index_2.L_end", 11},
            {"Clavicle.R", 28},
            {"Bicep.R", 29},
            {"Forarm.R", 30},
            {"Hand.R", 31},
            {"Thumb_0.R", 48},
            {"Thumb_1.R", 49},
            {"Thumb_2.R", 50},
            {"Thumb_2.R_end", 51},
            {"Pinky_0.R", 40},
            {"Pinky_1.R", 41},
            {"Pinky_2.R", 42},
            {"Pinky_2.R_end", 43},
            {"Ring_0.R", 44},
            {"Ring_1.R", 45},
            {"Ring_2.R", 46},
            {"Ring_2.R_end", 47},
            {"Middle_0.R", 36},
            {"Middle_1.R", 37},
            {"Middle_2.R", 38},
            {"Middle_2.R_end", 39},
            {"Index_0.R", 32},
            {"Index_1.R", 33},
            {"Index_2.R", 34},
            {"Index_2.R_end", 35},
            {"Thigh.R", 60},
            {"Calf.R", 61},
            {"Foot.R", 62},
            {"Toe.R", 63},
            {"Toe.R_end", 64}
        };

        public static bool TryGetExportData(out byte[] data)
        {
            if (ExportStatus != Status.ExportDataReady)
            {
                data = null;
                return false;
            }

            data = m_ExportData;
            m_ExportData = null;
            ExportStatus = Status.AvailableToPerformExport;
            return true;
        }

        public static void ExportActorsToFBX(ExportData exportData, Action onSuccess, Action<string> onFail)
        {
            if (ExportStatus != Status.AvailableToPerformExport)
            {
                Debug.LogWarning($"Another export was started during an InProgress Export. Ignoring the old export and performing a new one.");
            }
            
            ExportStatus = Status.InProgress;
            
            WebAPI api = new(WebUtils.BackendUrl, JsonToFbxAPI.ApiName);

            if (exportData == null || exportData.ActorsData == null || exportData.ActorsData.Length < 1)
            {
                onFail($"Export failed because export data was incomplete: ExportDataIsNull={exportData == null}, ActorDataIsNull={exportData?.ActorsData == null}, NumActors={exportData?.ActorsData?.Length}");
                return;
            }
            
            var actor = exportData.ActorsData[0];
            List<JsonToFbxAPI.Frame> apiFrames = new();
            ArmatureDefinition armatureDef = actor.ActorDefinitionComponent.ReferenceMotionArmature.ArmatureDefinition;

            for (int i = 0; i < exportData.BackedTimeline.FramesCount; i++)
            {
                BakedFrameModel frameModel = exportData.BackedTimeline.GetFrame(i);

                if (frameModel.TryGetModel(actor.ActorModel.EntityID, out BakedArmaturePoseModel pose))
                {
                    // Get root position
                    Vector3 rootPosition = pose.LocalPose.GetPosition(0);

                    // Get frame rotations
                    NativeArray<Quaternion> rotations =
                        new NativeArray<Quaternion>(pose.NumJoints, Allocator.Temp);

                    for (int jointIdx = 0; jointIdx < pose.NumJoints; jointIdx++)
                    {
                        string jointName = armatureDef.GetJointName(jointIdx);

                        // The input for the api expects frames in a different order
                        int mappedIndex = MuseAnimateToAPIInputMapping[jointName];
                        rotations[mappedIndex] = pose.LocalPose.GetRotation(jointIdx);
                    }

                    // TODO: Change frame constructor to use Span rather than NativeArray for rotation input
                    JsonToFbxAPI.Frame frame = new(rootPosition, rotations);
                    apiFrames.Add(frame);

                    rotations.Dispose();
                }
            }

            // In order to support canceled exports, we need to allow multiple export calls to occur. This means it is
            // possible have two export calls running simultaneously. Use an id to identify when to perform callback;
            int id = ++m_ExportId;

            var request = new JsonToFbxAPI.Request("biped_v0", apiFrames);
            api.SendRequestWithAuthHeaders(request,
                fbx =>
                {
                    if (m_ExportId == id)
                    {
                        m_ExportData = PackUSDZFile(fbx);
                        ExportStatus = Status.ExportDataReady;
                        onSuccess();
                    }
                    else
                    {
                        onFail("Export completed too late. Another export was started and this one was ignored.");
                    }
                    request.Dispose();
                },
                s =>
                {
                    ExportStatus = Status.AvailableToPerformExport;
                    onFail(s.Message);
                    request.Dispose();
                }
            );
        }

        // TODO: Bit weird that we pack a usdz, but this is temp in order to use the existing infrastructure
        static byte[] PackUSDZFile(byte[] fbx)
        {
            return fbx;
        }
    }
}