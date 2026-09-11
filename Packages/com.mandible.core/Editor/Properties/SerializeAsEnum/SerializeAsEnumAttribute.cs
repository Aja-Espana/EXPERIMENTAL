using System;
using UnityEngine;

namespace Mandible.Core
{
    [AttributeUsage(AttributeTargets.Field)]
    public class SerializeAsEnumAttribute : PropertyAttribute
    {
        public Type EnumType { get; }

        public SerializeAsEnumAttribute(Type enumType)
        {
            if (!enumType.IsEnum)
                throw new ArgumentException("Type must be an enum.");
            EnumType = enumType;
        }
    }
}
