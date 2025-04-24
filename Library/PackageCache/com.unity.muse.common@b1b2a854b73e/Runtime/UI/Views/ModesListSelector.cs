using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Muse.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Common
{
    internal class ModesListSelector : VisualElement
    {
        public event Action<ModeStruct> OnModeSelected;

        public ModesListSelector(Model model, List<string> modes, int currentModeIndex)
        {
            styleSheets.Add(ResourceManager.Load<StyleSheet>(PackageResources.modesListSelectorStyleSheet));
            AddToClassList("bottom-gap");
            UpdateModes(model, modes, currentModeIndex);
        }
        
        public void UpdateModes(Model model, List<string> modes, int currentModeIndex) {
            Clear();
            var currentMode = ModesFactory.GetModeData(modes[currentModeIndex]).Value;

            var modesList = new List<ModeStruct>();
            foreach (var mode in modes)
            {
                var modeStruct = ModesFactory.GetModeData(mode).Value;
                if (modeStruct.group != currentMode.group)
                    continue;
                modesList.Add(modeStruct);
            }
            var sortedModes = modesList.OrderBy(m => m.group_order).ThenBy(m => m.label).ToArray();

            if (sortedModes.Length <= 1) // If there is only one mode, there is no need to show the selector
            {
                style.display = DisplayStyle.None;
                return;
            }

            style.display = DisplayStyle.Flex;
            for (var index = 0; index < sortedModes.Length; index++)
            {
                var mode = sortedModes[index];
                var modeButton = new ModeButton(model, mode, currentMode.type == mode.type);
                modeButton.clickable.clicked += () => OnModeButtonClicked(mode);

                Add(modeButton);

                if (index < sortedModes.Length - 1)
                    Add(new Spacer());
            }
        }

        private void OnModeButtonClicked(ModeStruct mode)
        {
            OnModeSelected?.Invoke(mode);
        }
    }

    internal class ModeButton : ActionButton
    {
        public ModeStruct Mode { get; }
        private readonly Model model;

        public ModeButton(Model currentModel, ModeStruct mode, bool selected)
        {
            model = currentModel;
            model.OnRefineArtifact += (_) => UpdateEnabledState();
            model.OnFinishRefineArtifact += (_) => UpdateEnabledState();

            label = mode.icon_label;
            this.selected = selected;

            var iconElement = GetIconForMode(mode);
            if (iconElement != null)
            {
                hierarchy.Add(iconElement);
            }

            UpdateEnabledState();
        }

        private static Image GetIconForMode(ModeStruct mode)
        {
            if (mode.icon_path?.StartsWith("http") == true)
                return new WebImage(mode.icon_path);

            var t = ResourceManager.Load<Texture2D>(mode.icon_path);
            if (!t)
                return null;
            return new Image {image = t};
        }

        public void UpdateEnabledState()
        {
            if (!model.isRefineMode)
            {
                SetEnabled(true);
                return;
            }

            SetEnabled(selected);
        }
    }
}