#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;

namespace Mandible.Core
{
    [CustomPropertyDrawer(typeof(SerializeAsEnumAttribute))]
    public class SerializeAsEnumDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var attr = (SerializeAsEnumAttribute)attribute;

            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.LabelField(position, label.text, "Use [SerializeAsEnum] on a string field.");
                return;
            }

            string current = property.stringValue;
            string[] names = Enum.GetNames(attr.EnumType);

            int index = Mathf.Max(0, Array.IndexOf(names, current));

            index = EditorGUI.Popup(position, label.text, index, names);

            property.stringValue = names[index];
        }
    }
}

#endif