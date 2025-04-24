using System;
using Hsm;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;

namespace Unity.Muse.Animate
{
    static class HsmUtils
    {
        public static void SendPointerClickEvent(this StateMachine hsm, PointerEventData eventData)
        {
            Assert.IsFalse(eventData.used, "Event is already used");
            
            hsm.VisitInnerToOuter<State>(x =>
            {
                var pointerClickHandler = x as IPointerClickHandler;
                if (pointerClickHandler == null)
                    return true;

                pointerClickHandler.OnPointerClick(eventData);
                return !eventData.used;
            });
        }

        public static void SendKeyDownEvent(this StateMachine hsm, KeyPressEvent eventData)
        {
            Assert.IsFalse(eventData.IsUsed, "Event is already used");
            
            if (eventData.KeyCode == KeyCode.None)
                return;

            hsm.VisitInnerToOuter<State>(x =>
            {
                var keyPressHandler = x as IKeyDownHandler;
                if (keyPressHandler == null)
                    return true;

                keyPressHandler.OnKeyDown(eventData);
                return !eventData.IsUsed;
            });
        }
        
        public static void SendKeyUpEvent(this StateMachine hsm, KeyPressEvent eventData)
        {
            Assert.IsFalse(eventData.IsUsed, "Event is already used");
            if (eventData.KeyCode == KeyCode.None)
                return;

            hsm.VisitInnerToOuter<State>(x =>
            {
                var keyPressHandler = x as IKeyUpHandler;
                if (keyPressHandler == null)
                    return true;

                keyPressHandler.OnKeyUp(eventData);
                return !eventData.IsUsed;
            });
        }

        public static void SendKeyHoldEvent(this StateMachine hsm, KeyPressEvent eventData)
        {
            Assert.IsFalse(eventData.IsUsed, "Event is already used");
            if (eventData.KeyCode == KeyCode.None)
                return;

            hsm.VisitInnerToOuter<State>(x =>
            {
                var keyPressHandler = x as IKeyHoldHandler;
                if (keyPressHandler == null)
                    return true;

                keyPressHandler.OnKeyHold(eventData);
                return !eventData.IsUsed;
            });
        }
    }
}
