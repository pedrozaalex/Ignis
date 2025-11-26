using Ignis.Engine.UI.Core;

namespace Ignis.Editor.UI.Inspection.Core;

public interface IInspector
{
    /// <summary>
    /// Creates the UI for the given data accessor.
    /// </summary>
    IView CreateView(IAccessor accessor);
}

