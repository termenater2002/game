using System.Collections.Generic;
using Unity.Muse.Common.Utils;
using UnityEditor;
using UnityEngine.UIElements;

namespace Unity.Muse.Chat.UI.Components
{
    partial class MuseChatView
    {
        readonly List<MuseChatContextEntry> k_SelectedContext = new();

        bool m_DelayedUpdateContextElements;

        bool AddContextFromDraggedObject(object droppedObject)
        {
            if (droppedObject is not UnityEngine.Object unityObject)
            {
                return false;
            }

            if (unityObject == null)
            {
                return false;
            }

            if (!IsSupportedAsset(unityObject))
            {
                return false;
            }

            var contextEntry = unityObject.GetContextEntry();
            if (k_SelectedContext.Contains(contextEntry))
            {
                return false;
            }

            k_SelectedContext.Add(contextEntry);
            return true;
        }

        void CheckContextForDeletedAssets(string[] paths)
        {
            if (m_DelayedUpdateContextElements)
            {
                return;
            }

            var pathHash = new HashSet<string>(paths);
            for (var i = 0; i < k_SelectedContext.Count; i++)
            {
                var entry = k_SelectedContext[i];
                switch (entry.EntryType)
                {
                    case MuseChatContextType.HierarchyObject:
                    {
                        string assetPath = AssetDatabase.GUIDToAssetPath(entry.Value);
                        if (pathHash.Contains(assetPath))
                        {
                            m_DelayedUpdateContextElements = true;
                            EditorApplication.delayCall += () => UpdateContextSelectionElements(true);
                            return;
                        }

                        break;
                    }
                }
            }
        }

        void SyncContextSelection(List<UnityEngine.Object> objectList, List<LogData> consoleList)
        {
            k_SelectedContext.Clear();

            if (objectList != null)
            {
                for (var i = 0; i < objectList.Count; i++)
                {
                    var entry = objectList[i].GetContextEntry();
                    if (k_SelectedContext.Contains(entry))
                    {
                        continue;
                    }

                    k_SelectedContext.Add(entry);
                }
            }

            if (consoleList != null)
            {
                for (var i = 0; i < consoleList.Count; i++)
                {
                    var entry = consoleList[i].GetContextEntry();
                    if (k_SelectedContext.Contains(entry))
                    {
                        continue;
                    }

                    k_SelectedContext.Add(entry);
                }
            }
        }

        void RemoveInvalidContextEntries()
        {
            IList<MuseChatContextEntry> deleteList = new List<MuseChatContextEntry>();
            for (var i = 0; i < k_SelectedContext.Count; i++)
            {
                var entry = k_SelectedContext[i];
                switch (entry.EntryType)
                {
                    case MuseChatContextType.HierarchyObject:
                    case MuseChatContextType.SceneObject:
                    {
                        if (entry.GetTargetObject() == null)
                        {
                            deleteList.Add(entry);
                        }

                        break;
                    }

                    case MuseChatContextType.Component:
                    {
                        if (entry.GetComponent() == null)
                        {
                            deleteList.Add(entry);
                        }

                        break;
                    }
                }
            }

            for (var i = 0; i < deleteList.Count; i++)
            {
                k_SelectedContext.Remove(deleteList[i]);
            }
        }

        void UpdateContextSelectionElements(bool updatePopup = false)
        {
            if (updatePopup && m_SelectionPopup.visible)
            {
                m_SelectionPopup.SetSelectionFromContext(k_SelectedContext, false);
                m_SelectionPopup.PopulateListView();
            }

            RemoveInvalidContextEntries();

            m_SelectedContextScrollView.Clear();

            for (var i = 0; i < k_SelectedContext.Count; i++)
            {
                var entry = k_SelectedContext[i];
                var newElement = new ContextElement();
                newElement.Initialize();
                newElement.SetData(entry, true, OnRemoveContextEntry);

                m_SelectedContextScrollView.Add(newElement);
            }

            m_SelectedContextScrollView.MarkDirtyRepaint();

            UpdateSelectedContextWarning();
            UpdateMuseEditorDriverContext();
            UpdateClearContextButton();
        }

        void OnRemoveContextEntry(MuseChatContextEntry entry)
        {
            k_SelectedContext.Remove(entry);
            UpdateContextSelectionElements();
        }

        void UpdateSelectedContextWarning(MouseEnterEvent evt = null)
        {
            var contextBuilder = new ContextBuilder();
            Assistant.instance.GetAttachedContextString(ref contextBuilder, true);
            if (contextBuilder.PredictedLength > MuseChatConstants.PromptContextLimit)
            {
                m_ExceedingSelectedConsoleMessageLimitRoot.style.display = DisplayStyle.Flex;
            }
            else
            {
                m_ExceedingSelectedConsoleMessageLimitRoot.style.display = DisplayStyle.None;
            }
        }

        void UpdateClearContextButton()
        {
            m_ClearContextButton.SetDisplay(k_SelectedContext.Count > 0);
        }

        internal void ClearContext(PointerUpEvent evt)
        {
            k_SelectedContext.Clear();

            Assistant.instance.k_ObjectAttachments.Clear();
            Assistant.instance.k_ConsoleAttachments.Clear();

            UpdateContextSelectionElements();
        }

        void UpdateMuseEditorDriverContext()
        {
            Assistant.instance.k_ObjectAttachments.Clear();
            Assistant.instance.k_ConsoleAttachments.Clear();

            for (var i = 0; i < k_SelectedContext.Count; i++)
            {
                var entry = k_SelectedContext[i];
                switch (entry.EntryType)
                {
                    case MuseChatContextType.ConsoleMessage:
                    {
                        Assistant.instance.k_ConsoleAttachments.Add(entry.GetLogData());
                        break;
                    }

                    case MuseChatContextType.Component:
                    {
                        Assistant.instance.k_ObjectAttachments.Add(entry.GetComponent());
                        break;
                    }

                    case MuseChatContextType.HierarchyObject:
                    case MuseChatContextType.SceneObject:
                    {
                        Assistant.instance.k_ObjectAttachments.Add(entry.GetTargetObject());
                        break;
                    }
                }
            }
        }
    }
}
