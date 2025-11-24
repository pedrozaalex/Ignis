namespace Ignis.Engine.ECS;

/// <summary>
///     System group identifiers for organizing system execution order
/// </summary>
public enum SystemGroup
{
    /// <summary>
    ///     Input processing
    /// </summary>
    Input,

    /// <summary>
    ///     Physics simulation (Future)
    /// </summary>
    Physics,

    /// <summary>
    ///     Transform hierarchy update
    /// </summary>
    Transform,

    /// <summary>
    ///     Game logic update
    /// </summary>
    Logic,

    /// <summary>
    ///     Animation update (Future)
    /// </summary>
    Animation,

    /// <summary>
    ///     Late update for final adjustments
    /// </summary>
    LateUpdate
}