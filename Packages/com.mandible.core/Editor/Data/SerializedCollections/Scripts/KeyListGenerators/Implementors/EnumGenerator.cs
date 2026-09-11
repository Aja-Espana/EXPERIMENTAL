using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mandible.Core.Data.KeysGenerators
{
    [KeyListGenerator("Populate Enum", typeof(System.Enum), false)]
    public class EnumGenerator : KeyListGenerator
    {
        public override IEnumerable GetKeys(System.Type type)
        {
            return System.Enum.GetValues(type);
        }
    }
}