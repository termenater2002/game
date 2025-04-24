using UnityEngine;

namespace Unity.DeepPose.Core
{
    static class HumanoidUtils
    {
        public static string GetMuscleBindingName(int muscle)
        {
            var muscleName = HumanTrait.MuscleName[muscle];
            muscleName = FixHandMuscleName(muscleName);
            return muscleName;
        }

        static string FixHandMuscleName(string name)
        {
            var newName = name;
            newName = newName.Replace("Left Index ", "LeftHand.Index.");
            newName = newName.Replace("Left Middle ", "LeftHand.Middle.");
            newName = newName.Replace("Left Ring ", "LeftHand.Ring.");
            newName = newName.Replace("Left Thumb ", "LeftHand.Thumb.");
            newName = newName.Replace("Left Little ", "LeftHand.Little.");

            newName = newName.Replace("Right Index ", "RightHand.Index.");
            newName = newName.Replace("Right Middle ", "RightHand.Middle.");
            newName = newName.Replace("Right Ring ", "RightHand.Ring.");
            newName = newName.Replace("Right Thumb ", "RightHand.Thumb.");
            newName = newName.Replace("Right Little ", "RightHand.Little.");

            return newName;
        }
    }
}
