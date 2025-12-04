namespace CrucibleUI.Core.Types;

/// <summary>
/// The layout type determines how the nodes will position its parent-directed children.
/// </summary>
public enum LayoutType
{
    /// <summary>
    /// Stack child elements horizontally.
    /// </summary>
    Row,
    
    /// <summary>
    /// Stack child elements vertically (default).
    /// </summary>
    Column,
    
    /// <summary>
    /// Place child elements in a grid.
    /// </summary>
    Grid
}
