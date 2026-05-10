using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class QuickCreateAssetAttribute : PropertyAttribute
{
    public QuickCreateAssetAttribute()
    {
    }

    public QuickCreateAssetAttribute(string menuPath)
    {
    }

    public QuickCreateAssetAttribute(string menuPath, string nameField, string valueField)
    {
    }
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
public class EnumPickerBitmaskAttribute : PropertyAttribute
{
    public EnumPickerBitmaskAttribute()
    {
    }
}

