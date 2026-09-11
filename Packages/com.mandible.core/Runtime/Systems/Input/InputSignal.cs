using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mandible.Core.Data
{
    public struct InputSignal
    {
        public bool Pressed;
        public bool Held;
        public bool Released;

        internal bool[] consumed;
        internal const int SIGNAL_TYPE_COUNT = 3;

        public void Initialize()
        {
            if (consumed == null)
                consumed = new bool[SIGNAL_TYPE_COUNT];
        }

        public bool IsConsumed(InputType type)
        {
            Initialize();
            return consumed[(int)type];
        }

        public void Consume(InputType type)
        {
            Initialize();
            consumed[(int)type] = true;
        }
    }

    public enum InputType
    {
        Pressed,
        Held,
        Released
    }
}