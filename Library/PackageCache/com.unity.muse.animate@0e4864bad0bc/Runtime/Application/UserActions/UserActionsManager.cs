using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Unity.Muse.Animate.UserActions
{
    class UserActionsManager
    {
        public static UserActionsManager Instance { get; } = new();
        public bool IsUserEditing => m_UserIsEditing;

        AuthoringModel m_AuthoringModel;
        
        List<UserAction> m_History = new();
        int m_HistoryIndex;
        KeyModel m_BackupKey;
        TransitionModel m_BackupTransition;
        
        int m_NextId;
        bool m_Locked;
        int m_CurrentGroupId;
        bool m_UserIsEditing;
        string m_UserEditDescription;
        ApplicationLibraryModel m_ApplicationLibraryModel;

        /// <summary>
        /// Constructor of the UserActionsManager.
        /// Subscribes to the Unity Undo system.
        /// </summary>
        UserActionsManager()
        {
            UnityEditor.Undo.undoRedoEvent += UndoRedoPerformed;
        }
        
        /// <summary>
        /// Initializes the manager.
        /// For now it clears all previously recorded user actions.
        /// </summary>
        /// <param name="authoringModel">The AuthoringModel that will be using this UserActions.</param>
        /// <param name="applicationLibraryModel">The ApplicationLibraryModel that will be using this UserActions.</param>
        internal void Initialize(AuthoringModel authoringModel, ApplicationLibraryModel applicationLibraryModel)
        {
            m_AuthoringModel = authoringModel;
            m_ApplicationLibraryModel = applicationLibraryModel;
            ClearAll();
        }
        
        /// <summary>
        /// Saves a copy of the provided TransitionModel.
        /// It will then be used as a "before" state when saving a timeline transition modification.
        /// </summary>
        /// <param name="transition">The TransitionModel to save a copy of.</param>
        void BackupTransitionForUndo(TransitionModel transition)
        {
            Log($"BackupTransitionForUndo({transition.ListIndex})");
            m_BackupTransition = transition.Clone();
        }
        
        /// <summary>
        /// Saves a copy of the provided KeyModel.
        /// It will then be used as a "before" state when saving a timeline key modification.
        /// </summary>
        /// <param name="key">The KeyModel to save a copy of.</param>
        void BackupKeyForUndo(KeyModel key)
        {
            Log($"BackupKeyForUndo({key.ListIndex})");
            m_BackupKey = key.Clone();
        }

        /// <summary>
        /// Records when a user starts editing a item from a library.
        /// </summary>
        /// <param name="itemBefore"></param>
        /// <param name="itemAfter"></param>
        internal void RecordEditLibraryItem(LibraryItemAsset itemBefore, LibraryItemAsset itemAfter)
        {
            BackupForUndo();
            RecordAction(new EditLibraryItem(
                GetNextId(), 
                GetNextGroupId(), 
                m_ApplicationLibraryModel, 
                m_AuthoringModel, 
                itemBefore, 
                itemAfter));
        }
        
        internal void RecordModifiedTimelineKey(TimelineModel timeline, int index, KeyModel after)
        {
            RecordAction(new ModifiedTimelineKey(GetNextId(), GetNextGroupId(), m_UserEditDescription, m_AuthoringModel, timeline, index, m_BackupKey, after));
        }
        
        internal void RecordModifiedTimelineTransition(TimelineModel timeline, int index, TransitionModel after)
        {
            RecordAction(new ModifiedTimelineTransition(GetNextId(), GetNextGroupId(), m_UserEditDescription, m_AuthoringModel, timeline, index, m_BackupTransition, after));
        }
        
        internal void RecordEditTimelineKey(TimelineModel timeline, int indexBefore, int indexAfter)
        {
            RecordAction(new EditTimelineKey(GetNextId(), GetNextGroupId(), m_AuthoringModel, timeline, indexBefore, indexAfter));
        }

        internal void RecordInsertTimelineKey(TimelineModel timeline, int atIndex, TimelineModel.SequenceKey newKey)
        {
            RecordAction(new InsertTimelineKey(GetNextId(), GetNextGroupId(), m_AuthoringModel, timeline, atIndex));
        }

        internal void RecordInsertTimelineKeyCopy(TimelineModel timeline, int previousIndex, int toIndex, KeyModel keyToPaste, TransitionModel keyOutTransitionToPaste, bool splitTransition, float splitTimePercent)
        {
            RecordAction(new InsertTimelineKeyCopy(GetNextId(), GetNextGroupId(), m_AuthoringModel, timeline, keyToPaste, keyOutTransitionToPaste, previousIndex, toIndex, splitTransition, splitTimePercent));
        }
        
        internal void RecordInsertTimelineKeyWithEffectorRecovery(TimelineModel timeline, int currentFrameIndex, int keyIndex, float transitionProgress, TimelineModel.SequenceKey newKey)
        {
            RecordAction(new InsertTimelineKeyWithEffectorRecovery(GetNextId(), GetNextGroupId(), m_AuthoringModel, timeline, currentFrameIndex, keyIndex, transitionProgress));
        }
        
        internal void RecordDeleteTimelineKey(TimelineModel timeline, KeyModel keyToDelete)
        {
            RecordAction(new DeleteTimelineKey(GetNextId(), GetNextGroupId(), m_AuthoringModel, timeline, keyToDelete));
        }
        
        internal void RecordMoveTimelineKey(TimelineModel timeline, int fromIndex, int toIndex)
        {
            RecordAction(new MoveTimelineKey(GetNextId(), GetNextGroupId(), m_AuthoringModel, timeline, fromIndex, toIndex));
        }

        internal void RecordDuplicateTimelineKey(TimelineModel timeline, TimelineModel.SequenceKey keyToDuplicate, int fromIndex, int toIndex)
        {
            RecordAction(new DuplicateTimelineKey(GetNextId(), GetNextGroupId(), m_AuthoringModel, timeline, fromIndex, toIndex));
        }
        
        internal void RecordAddTimelineKey(TimelineModel timeline)
        {
            RecordAction(new AddTimelineKey(GetNextId(), GetNextGroupId(), m_AuthoringModel, timeline, m_BackupKey.Clone()));
        }
        
        void RecordAction(UserAction action)
        {
            ClearAfter();
            m_History.Add(action);
            m_HistoryIndex = m_History.Count - 1;
            UnityEditor.Undo.RegisterCompleteObjectUndo(action.DummyObject, action.Name);
        }
        
        /// <summary>
        /// Clears all the entries after the current entry.
        /// </summary>
        public void ClearAfter()
        {
            ClearFrom(m_HistoryIndex+1);
        }
        
        /// <summary>
        /// Clears all the entries starting from the provided index to the end of the history.
        /// </summary>
        /// <param name="fromIndex">The index to start clearing from.</param>
        void ClearFrom(int fromIndex)
        {
            if (fromIndex < 0 || fromIndex >= m_History.Count)
                return;
            
            for (var i = m_History.Count-1; i >= fromIndex; i--)
            {
                UnityEditor.Undo.ClearUndo(m_History[i].DummyObject);
                m_History[i].Clear();
                m_History.RemoveAt(i);
            }
        }
        
        /// <summary>
        /// Clears all the entries of the history.
        /// </summary>
        internal void ClearAll()
        { 
            ClearFrom(0);
        }
        
        /// <summary>
        /// Clears all undo items associated with a LibraryItemAsset.
        /// </summary>
        internal void ClearAll(LibraryItemAsset asset)
        {
            using var retainedItems = TempList<UserAction>.Allocate();

            int i = 0;
            while (i < m_History.Count)
            {
                //skip until you find an EditLibraryItem
                while (i < m_History.Count && m_History[i] is not EditLibraryItem)
                {
                    retainedItems.Add(m_History[i]);
                    i++;    
                }

                //exit if we're out of elements
                if (i >= m_History.Count)
                    break;

                var item = m_History[i] as EditLibraryItem; 
                
                //If the EditLibraryItem doesn't target the asset, add it to the list and continue
                if (item.TargetItemAsset != asset)
                {
                    retainedItems.Add(m_History[i]);
                    i++;
                    continue;
                }
                
                //If we're here it means we've found an EditLibraryItem that targets our asset.
                //Clear items until the next EditLibraryItem
                do
                {
                    UnityEditor.Undo.ClearUndo(m_History[i].DummyObject);
                    i++;
                } while (i < m_History.Count && m_History[i] is not EditLibraryItem);
            }
            
            //Clear the history and replace it by the retained items
            m_History.Clear();
            m_History.AddRange(retainedItems);
        }

        /// <summary>
        /// Called when UnityEditor.Undo.undoRedoEvent is triggered.
        /// </summary>
        /// <param name="undo">Information about the undoRedo event.</param>
        void UndoRedoPerformed(in UndoRedoInfo undo)
        {
            int group = undo.undoGroup;
            var identifier = $"{undo.undoGroup} - {undo.undoName}";

            if (!undo.isRedo)
            {
                if (CanUndoGroup(group))
                {
                    Log($"UndoRedoPerformed({identifier}) -> Undo");
                    Undo();
                }
                else
                {
                    Log($"UndoRedoPerformed({identifier}) -> No matching undo for this group");
                }
            }
            else
            {
                if (CanRedoGroup(group))
                {
                    Log($"UndoRedoPerformed({identifier}) -> Redo");
                    Redo();
                }
                else
                {
                    Log($"UndoRedoPerformed({identifier}) -> No matching redo for this group");
                }
            }
        }

        bool CanUndoGroup(int groupId)
        {
            if (m_History.Count == 0)
                return false;
            
            if (m_HistoryIndex < 0)
                return false;
            
            if (m_HistoryIndex >= m_History.Count)
                return false;
            
            if (m_History[m_HistoryIndex].GroupId != groupId)
                return false;
            
            return true;
        }
        
        bool CanRedoGroup(int groupId)
        {
            if (m_History.Count == 0)
                return false;
            
            if (m_HistoryIndex+1 < 0)
                return false;

            if (m_HistoryIndex+1 >= m_History.Count)
                return false;
            
            if (m_History[m_HistoryIndex+1].GroupId != groupId)
                return false;
            
            return true;
        }
        
        void Undo()
        {
            if (m_History.Count == 0)
                return;

            var actionToUndo = m_History[m_HistoryIndex];
            m_HistoryIndex --;
            actionToUndo.Undo();
        }
        
        void Redo()
        {
            if (m_History.Count == 0)
                return;

            var actionToRedo = m_History[m_HistoryIndex+1];
            m_HistoryIndex ++;
            actionToRedo.Redo();
        }

        void Log(string message)
        {
            if (ApplicationConstants.DebugUndoRedo)
                Debug.Log($"[UserActionsManager] -> {message}");
        }
        
        /// <summary>
        /// Increments the current UnityEditor.Undo Group, then returns the new value.
        /// </summary>
        /// <returns>The incremented Undo Group.</returns>
        int GetNextGroupId()
        {
            UnityEditor.Undo.IncrementCurrentGroup();
            m_CurrentGroupId = UnityEditor.Undo.GetCurrentGroup();
            return m_CurrentGroupId;
        }
        
        /// <summary>
        /// Increments the next id, then returns it.
        /// </summary>
        /// <returns>The incremented next id.</returns>
        int GetNextId()
        {
            return m_NextId ++;
        }
        
        /// <summary>
        /// Used to declare that a user-driven edit is happening this update.
        /// </summary>
        internal void StartUserEdit(string description)
        {
            m_UserIsEditing = true;
            m_UserEditDescription = description;
            BackupForUndo();
        }
        
        internal void BackupForUndo()
        {
            Log("BackupForUndo()");

            if (m_AuthoringModel.Timeline.ActiveKey != null)
            {
                BackupKeyForUndo(m_AuthoringModel.Timeline.ActiveKey.Key);
                
                if (m_AuthoringModel.Timeline.ActiveKey.OutTransition != null)
                {
                    BackupTransitionForUndo(m_AuthoringModel.Timeline.ActiveKey.OutTransition.Transition);
                }
            }
            
            if (m_AuthoringModel.Timeline.ActiveTransition != null)
            {
                BackupTransitionForUndo(m_AuthoringModel.Timeline.ActiveTransition.Transition);
                
                if (m_AuthoringModel.Timeline.ActiveTransition.FromKey != null)
                {
                    BackupKeyForUndo(m_AuthoringModel.Timeline.ActiveTransition.FromKey.Key);
                }
            }
        }
        
        /// <summary>
        /// Ends the user-driven key edit. Must be called after the user edits have been processed.
        /// </summary>
        internal void ResetUserEdit()
        {
            m_UserIsEditing = false;
        }

    }
}
