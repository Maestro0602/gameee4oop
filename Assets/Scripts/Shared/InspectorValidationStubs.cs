using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
public class InspectorValidationAttribute : PropertyAttribute
{
    public InspectorValidationAttribute()
    {
    }
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
public class ModifiablePropertyAttribute : PropertyAttribute
{
    public ModifiablePropertyAttribute()
    {
    }

    public ModifiablePropertyAttribute(string label)
    {
    }
}
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
public class ConditionalAttribute : PropertyAttribute
{
    public ConditionalAttribute()
    {
    }

    public ConditionalAttribute(string conditionalSource)
    {
    }

    public ConditionalAttribute(string conditionalSource, int compareValue, bool invert, bool showIfMissing)
    {
    }

    public ConditionalAttribute(string conditionalSource, bool compareValue, bool invert, bool showIfMissing)
    {
    }

    public ConditionalAttribute(string conditionalSource, bool compareValue)
    {
    }

    public ConditionalAttribute(string conditionalSource, int compareValue)
    {
    }
}
