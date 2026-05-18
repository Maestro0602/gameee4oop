using System;
using System.Collections.Generic;
using System.Reflection;

public class BoolFieldAccessOptimizer<T>
{
    private readonly Dictionary<string, FieldInfo> fieldCache = new Dictionary<string, FieldInfo>(StringComparer.Ordinal);

    public bool GetField(T target, string fieldName)
    {
        FieldInfo field = GetFieldInfo(fieldName);
        if (field == null)
        {
            return false;
        }

        return (bool)field.GetValue(target);
    }

    public void SetField(T target, string fieldName, bool value)
    {
        FieldInfo field = GetFieldInfo(fieldName);
        if (field == null)
        {
            return;
        }

        field.SetValue(target, value);
    }

    public bool FieldExists(Type type, string fieldName)
    {
        return GetFieldInfo(fieldName, type) != null;
    }

    private FieldInfo GetFieldInfo(string fieldName)
    {
        return GetFieldInfo(fieldName, typeof(T));
    }

    private FieldInfo GetFieldInfo(string fieldName, Type type)
    {
        if (fieldCache.TryGetValue(fieldName, out FieldInfo cachedField))
        {
            return cachedField;
        }

        FieldInfo field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        fieldCache[fieldName] = field;
        return field;
    }
}
