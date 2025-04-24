using System;
using Unity.DeepPose.Core;
using Unity.Mathematics;
using UnityEngine;

namespace Unity.DeepPose.Components
{
    static class ContactUtils
    {
        public static void DrawGizmos(this ContactDisplaySettings settings, float4 contacts)
        {
            if (!settings.Enabled)
                return;

            DrawContact(settings.ColorContact, settings.ColorNoContact, settings.Thickness, settings.LeftFoot, settings.LeftToes, contacts.x);
            DrawContact(settings.ColorContact, settings.ColorNoContact, settings.Thickness, settings.LeftToes, settings.LeftToesTip, contacts.y);
            DrawContact(settings.ColorContact, settings.ColorNoContact, settings.Thickness, settings.RightFoot, settings.RightToes, contacts.z);
            DrawContact(settings.ColorContact, settings.ColorNoContact, settings.Thickness, settings.RightToes, settings.RightToesTip, contacts.w);
        }

        static void DrawContact(Color contactColor, Color noContactColor, float thickness, Transform from, Transform to, float value)
        {
            if (from == null || to == null)
                return;

            // No Contact
            if (value <= 0f)
            {
                GizmoUtils.DrawLine(from.position, to.position, noContactColor, thickness);
                return;
            }

            // Contact
            GizmoUtils.DrawLine(from.position, to.position, contactColor, thickness);
        }
    }
}
