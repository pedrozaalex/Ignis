namespace CrucibleUI.Types;

/// <summary>
/// The position type determines whether a node will be positioned in-line with its siblings
/// or out-of-line / independently of its siblings.
/// </summary>
public enum PositionType
{
    /// <summary>
    /// Node is positioned relative to parent but ignores its siblings.
    /// </summary>
    Absolute,
    
    /// <summary>
    /// Node is positioned relative to parent and in-line with siblings (default).
    /// </summary>
    Relative
}
