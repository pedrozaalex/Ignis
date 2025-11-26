namespace Ignis.Editor.UI.Inspection.Core;

public static class InspectorRegistry
{
    private static readonly Dictionary<Type, IInspector> _inspectors = new();
    
    public static IInspector Composite { get; set; } = null!;
    public static IInspector Fallback { get; set; } = null!;

    public static void Register<T>(IInspector inspector)
    {
        _inspectors[typeof(T)] = inspector;
    }

    public static IInspector GetInspector(Type type)
    {
        if (_inspectors.TryGetValue(type, out var inspector))
            return inspector;

        if (type.IsEnum)
            return Fallback;

        if (!type.IsPrimitive && type != typeof(string))
            return Composite;

        return Fallback;
    }
}

