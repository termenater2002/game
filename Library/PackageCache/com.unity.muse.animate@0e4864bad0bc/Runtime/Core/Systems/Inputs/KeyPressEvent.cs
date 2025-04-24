using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Pool;

namespace Unity.Muse.Animate
{
    class KeyPressEvent
    {
        public bool IsUsed { get; private set; }
        public KeyCode KeyCode { get; set; }
        public bool IsControlOrCommand { get; set; }
        public bool IsShift { get; set; }
        public bool IsAlt { get; set; }
        
        public static ObjectPool<KeyPressEvent> Pool => s_Pool;

        static ObjectPool<KeyPressEvent> s_Pool = new (CreatePooledItem, OnTakeFromPool, OnReturnedToPool, OnDestroyPoolObject, true, 10, 100);

        protected KeyPressEvent()
        {
            Reset();
        }

        public void Use()
        {
            Assert.IsFalse(IsUsed, "Event already used");
            IsUsed = true;
        }

        void Reset()
        {
            IsUsed = false;
            KeyCode = KeyCode.None;
        }

        public void Release()
        {
            Pool.Release(this);
        }
        
        static KeyPressEvent CreatePooledItem()
        {
            var ev = new KeyPressEvent();
            return ev;
        }

        static void OnReturnedToPool(KeyPressEvent ev)
        {
            // Do nothing
        }

        static void OnTakeFromPool(KeyPressEvent ev)
        {
            ev.Reset();
        }

        static void OnDestroyPoolObject(KeyPressEvent ev)
        {
            // Do nothing
        }
    }
}
