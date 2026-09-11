#if !ODIN_INSPECTOR
// -----------------------------------------------------------------------------
// Dummy Odin Inspector Stubs
// Lets code compile without Odin installed.
// -----------------------------------------------------------------------------

using System;
using UnityEngine;

namespace Sirenix.OdinInspector
{

    // ---- PROPERTY ATTRIBUTES ----

    public class PropertyOrderAttribute : Attribute { public PropertyOrderAttribute(int a) {} }
    public class LabelTextAttribute : Attribute { public LabelTextAttribute(string s) {} }
    public class TooltipAttribute : Attribute { public TooltipAttribute(string s) {} }
    public class TitleAttribute : Attribute { public TitleAttribute(string s) {} }
    public class HideLabelAttribute : Attribute {}
    public class ShowInInspector : Attribute {}
    public class ReadOnlyAttribute : Attribute {}
    public class InlinePropertyAttribute : Attribute {}
    public class InlineEditorAttribute : Attribute { public InlineEditorAttribute() {} }
    public class MultilineAttribute : Attribute { public MultilineAttribute() {} }
    public class FoldoutGroupAttribute : Attribute { public FoldoutGroupAttribute(string s) {} }
    public class TabGroupAttribute : Attribute { public TabGroupAttribute(string s) {} }


    // ---- BUTTONS ----

    public class ButtonAttribute : Attribute 
    { 
        public ButtonAttribute() {} 
        public ButtonAttribute(string s) {} 
    }


    // ---- VALIDATION ----

    public class MinValueAttribute : Attribute { public MinValueAttribute(float f) {} }
    public class MaxValueAttribute : Attribute { public MaxValueAttribute(float f) {} }
    public class RangeAttribute : Attribute { public RangeAttribute(float a, float b) {} }
    public class ValidateInputAttribute : Attribute { public ValidateInputAttribute(string s) {} }
    public class RequiredAttribute : Attribute {}


    // ---- CONDITIONAL ----

    public class ShowIfAttribute : Attribute { public ShowIfAttribute(string s) {} }
    public class HideIfAttribute : Attribute { public HideIfAttribute(string s) {} }
    public class EnableIfAttribute : Attribute { public EnableIfAttribute(string s) {} }
    public class DisableIfAttribute : Attribute { public DisableIfAttribute(string s) {} }


    // ---- SERIALIZATION ----

    public class OdinSerializeAttribute : Attribute {}

    // Subclass selector
    public class SubclassSelectorAttribute : Attribute {}

    // List drawer attributes
    public class ListDrawerSettingsAttribute : Attribute 
    { 
        public ListDrawerSettingsAttribute() {} 
        public bool DraggableItems;
        public bool ShowPaging;
        public bool Expanded;
    }

    // ---- VALUE DROPDOWNS ----

    public class ValueDropdownAttribute : Attribute 
    { 
        public ValueDropdownAttribute(string s) {} 
    }

    // ---- GROUPS ----

    public class BoxGroupAttribute : Attribute { public BoxGroupAttribute(string s) {} }
    public class VerticalGroupAttribute : Attribute { public VerticalGroupAttribute(string s) {} }
    public class HorizontalGroupAttribute : Attribute { public HorizontalGroupAttribute(string s) {} }

    // ---- MISC ----

    public class InfoBoxAttribute : Attribute { public InfoBoxAttribute(string s) {} }
    public class AssetListAttribute : Attribute {}
    public class EnumToggleButtonsAttribute : Attribute {}
}

namespace Sirenix.Serialization
{
    public class OdinSerializeAttribute : Attribute {}
}

#endif
