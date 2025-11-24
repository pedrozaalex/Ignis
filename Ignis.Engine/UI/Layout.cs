namespace Ignis.Engine.UI;
// --- Types ---

public enum LayoutType
{
    Row,
    Column,
    Grid
}

public enum PositionType
{
    Relative,
    Absolute
}

public enum UnitKind
{
    Pixels,
    Percentage,
    Stretch,
    Auto
}

public struct Units : IEquatable<Units>
{
    public UnitKind Kind;
    public float Value;

    public Units(UnitKind kind, float value)
    {
        Kind = kind;
        Value = value;
    }

    public static Units Pixels(float v)
    {
        return new Units(UnitKind.Pixels, v);
    }

    public static Units Percentage(float v)
    {
        return new Units(UnitKind.Percentage, v);
    }

    public static Units Stretch(float v)
    {
        return new Units(UnitKind.Stretch, v);
    }

    public static readonly Units Auto = new(UnitKind.Auto, 0);

    public float ToPx(float parentValue, float defaultValue)
    {
        return Kind switch
        {
            UnitKind.Pixels => Value,
            UnitKind.Percentage => Value / 100.0f * parentValue,
            _ => defaultValue
        };
    }

    public bool IsAuto => Kind == UnitKind.Auto;
    public bool IsStretch => Kind == UnitKind.Stretch;

    public bool Equals(Units other)
    {
        return Kind == other.Kind && Value.Equals(other.Value);
    }
}

public enum Alignment
{
    TopLeft,
    TopCenter,
    TopRight,
    Left,
    Center,
    Right,
    BottomLeft,
    BottomCenter,
    BottomRight
}

public struct Size
{
    public float Main;
    public float Cross;
}

public struct Rect
{
    public float PosX;
    public float PosY;
    public float Width;
    public float Height;
}

// --- Interfaces ---

/// <summary>
///     Provides layout properties and tree structure. Replaces Node, Store, and Tree from Rust.
/// </summary>
public interface ILayoutNode
{
    IEnumerable<object> GetChildren(object node);
    bool IsVisible(object node);
    LayoutType GetLayoutType(object node);
    PositionType GetPositionType(object node);
    Alignment GetAlignment(object node);

    Units GetWidth(object node);
    Units GetHeight(object node);
    Units GetMinWidth(object node);
    Units GetMinHeight(object node);
    Units GetMaxWidth(object node);
    Units GetMaxHeight(object node);

    Units GetLeft(object node);
    Units GetRight(object node);
    Units GetTop(object node);
    Units GetBottom(object node);

    Units GetPaddingLeft(object node);
    Units GetPaddingRight(object node);
    Units GetPaddingTop(object node);
    Units GetPaddingBottom(object node);

    Units GetBorderLeft(object node);
    Units GetBorderRight(object node);
    Units GetBorderTop(object node);
    Units GetBorderBottom(object node);

    Units GetChildLeft(object node); // Used for gap logic mapping
    Units GetChildTop(object node);

    Units GetRowGap(object node);
    Units GetColumnGap(object node);

    // Grid specific
    List<Units> GetGridRows(object node);
    List<Units> GetGridColumns(object node);
    int GetRowStart(object node);
    int GetRowSpan(object node);
    int GetColumnStart(object node);
    int GetColumnSpan(object node);

    // Content sizing hook
    (float width, float height)? MeasureContent(object node, float? knownWidth, float? knownHeight);
}

public interface ILayoutCache
{
    void SetBounds(object node, float posX, float posY, float width, float height);
    float GetWidth(object node);
    float GetHeight(object node);
    float GetPosX(object node);
    float GetPosY(object node);
}

// --- Algorithm ---

public static class LayoutEngine
{
    private const float DefaultMin = -float.MaxValue;
    private const float DefaultMax = float.MaxValue;

    public static void Layout(object root, ILayoutNode store, ILayoutCache cache, float viewportWidth = 0,
        float viewportHeight = 0)
    {
        var widthUnits = store.GetWidth(root);
        var heightUnits = store.GetHeight(root);

        // Use viewport dimensions for stretch/percentage calculations
        var w = widthUnits.Kind switch
        {
            UnitKind.Pixels => widthUnits.Value,
            UnitKind.Percentage => viewportWidth * (widthUnits.Value / 100f),
            UnitKind.Stretch => viewportWidth,
            _ => viewportWidth // Auto uses viewport as constraint
        };

        var h = heightUnits.Kind switch
        {
            UnitKind.Pixels => heightUnits.Value,
            UnitKind.Percentage => viewportHeight * (heightUnits.Value / 100f),
            UnitKind.Stretch => viewportHeight,
            _ => viewportHeight // Auto uses viewport as constraint
        };

        cache.SetBounds(root, 0, 0, w, h);
        var result = Compute(root, LayoutType.Column, h, w, store, cache);

        // Update root bounds if auto-sized (Compute returns Main=height, Cross=width for Column layout)
        if (widthUnits.IsAuto || heightUnits.IsAuto)
        {
            var resolvedW = widthUnits.IsAuto ? result.Cross : w;
            var resolvedH = heightUnits.IsAuto ? result.Main : h;
            cache.SetBounds(root, 0, 0, resolvedW, resolvedH);
        }
    }

    private static Size Compute(object node, LayoutType parentLayoutType, float parentMain, float parentCross,
        ILayoutNode store, ILayoutCache cache)
    {
        var layoutType = store.GetLayoutType(node);

        // Helper to swap main/cross based on layout type
        // float Main(Units u) => parentLayoutType == LayoutType.Column ? u.ToPx(parentMain, 0) : u.ToPx(parentMain, 0); 

        var width = store.GetWidth(node);
        var height = store.GetHeight(node);
        (float width, float height)? intrinsicSize = null;
        var widthReference = parentLayoutType == LayoutType.Row ? parentMain : parentCross;
        var heightReference = parentLayoutType == LayoutType.Column ? parentMain : parentCross;

        float ResolveAutoMin(Units units, bool isWidthAxis, float referenceSize, object target)
        {
            if (!units.IsAuto) return units.ToPx(referenceSize, DefaultMin);
            var content = target == node
                ? intrinsicSize ??= store.MeasureContent(node, null, null)
                : store.MeasureContent(target, null, null);
            if (content.HasValue) return isWidthAxis ? content.Value.width : content.Value.height;
            return DefaultMin;
        }

        // Determine main/cross units based on parent direction
        var main = parentLayoutType is LayoutType.Row or LayoutType.Grid ? width : height;
        var cross = parentLayoutType is LayoutType.Row or LayoutType.Grid ? height : width;

        var computedMain = main.Kind switch
        {
            UnitKind.Pixels => main.Value,
            UnitKind.Percentage => (float)Math.Round(parentMain * (main.Value / 100.0f)),
            UnitKind.Stretch => parentMain,
            _ => 0.0f
        };

        var computedCross = cross.Kind switch
        {
            UnitKind.Pixels => cross.Value,
            UnitKind.Percentage => (float)Math.Round(parentCross * (cross.Value / 100.0f)),
            UnitKind.Stretch => parentCross,
            _ => 0.0f
        };

        // Constraints
        var minW = ResolveAutoMin(store.GetMinWidth(node), true, widthReference, node);
        var maxW = store.GetMaxWidth(node).ToPx(widthReference, DefaultMax);
        var minH = ResolveAutoMin(store.GetMinHeight(node), false, heightReference, node);
        var maxH = store.GetMaxHeight(node).ToPx(heightReference, DefaultMax);

        var minMain = parentLayoutType is LayoutType.Row or LayoutType.Grid ? minW : minH;
        var maxMain = parentLayoutType is LayoutType.Row or LayoutType.Grid ? maxW : maxH;
        var minCross = parentLayoutType is LayoutType.Row or LayoutType.Grid ? minH : minW;
        var maxCross = parentLayoutType is LayoutType.Row or LayoutType.Grid ? maxH : maxW;

        // Check for relative children to see if we can auto-size
        var visibleChildren = store.GetChildren(node).Where(c => store.IsVisible(c)).ToList();
        var relativeChildren =
            visibleChildren.Where(c => store.GetPositionType(c) == PositionType.Relative).ToList();

        // Content Sizing
        if ((main.IsAuto || cross.IsAuto) && relativeChildren.Count == 0)
        {
            var pW = width.IsAuto
                ? (float?)null
                : parentLayoutType is LayoutType.Row or LayoutType.Grid
                    ? computedMain
                    : computedCross;
            var pH = height.IsAuto
                ? (float?)null
                : parentLayoutType is LayoutType.Column
                    ? computedMain
                    : computedCross;

            var content = store.MeasureContent(node, pW, pH);
            if (content.HasValue)
            {
                if (main.IsAuto)
                    computedMain = parentLayoutType is LayoutType.Row or LayoutType.Grid
                        ? content.Value.width
                        : content.Value.height;
                if (cross.IsAuto)
                    computedCross = parentLayoutType is LayoutType.Row or LayoutType.Grid
                        ? content.Value.height
                        : content.Value.width;
            }
        }

        computedMain = Math.Clamp(computedMain, minMain, maxMain);
        computedCross = Math.Clamp(computedCross, minCross, maxCross);

        // Grid Layout Branch
        if (layoutType == LayoutType.Grid) return LayoutGrid(node, computedMain, computedCross, store, cache);

        // Determine available space for children
        var childParentMain = layoutType == parentLayoutType ? computedMain : computedCross;
        var childParentCross = layoutType == parentLayoutType ? computedCross : computedMain;

        // Padding
        var padLeft = store.GetPaddingLeft(node).ToPx(childParentMain, 0);
        var padRight = store.GetPaddingRight(node).ToPx(childParentMain, 0);
        var padTop = store.GetPaddingTop(node).ToPx(childParentCross, 0);
        var padBottom = store.GetPaddingBottom(node).ToPx(childParentCross, 0);

        // Borders
        var borderLeft = store.GetBorderLeft(node).ToPx(childParentMain, 0);
        var borderRight = store.GetBorderRight(node).ToPx(childParentMain, 0);
        var borderTop = store.GetBorderTop(node).ToPx(childParentCross, 0);
        var borderBottom = store.GetBorderBottom(node).ToPx(childParentCross, 0);

        var padMainBefore = layoutType == LayoutType.Row ? padLeft : padTop;
        var padMainAfter = layoutType == LayoutType.Row ? padRight : padBottom;
        var padCrossBefore = layoutType == LayoutType.Row ? padTop : padLeft;
        var padCrossAfter = layoutType == LayoutType.Row ? padBottom : padRight;

        var borderMainBefore = layoutType == LayoutType.Row ? borderLeft : borderTop;
        var borderMainAfter = layoutType == LayoutType.Row ? borderRight : borderBottom;
        var borderCrossBefore = layoutType == LayoutType.Row ? borderTop : borderLeft;
        var borderCrossAfter = layoutType == LayoutType.Row ? borderBottom : borderRight;

        childParentMain -= padMainBefore + padMainAfter + borderMainBefore + borderMainAfter;
        childParentCross -= padCrossBefore + padCrossAfter + borderCrossBefore + borderCrossAfter;

        // Layout Children
        float mainFlexSum = 0;
        var childrenData = new List<(object node, float main, float cross, float mainAfter, bool frozen)>();

        // First Pass: Non-flexible Relative Children & collection of flex factors
        foreach (var child in relativeChildren)
        {
            var childW = store.GetWidth(child);
            var childH = store.GetHeight(child);

            var childMain = layoutType == LayoutType.Row ? childW : childH;
            var childCross = layoutType == LayoutType.Row ? childH : childW;

            float childComputedMain = 0;
            float childComputedCross = 0;
            float childMainAfter = 0; // Gap

            // Gap logic
            if (child != relativeChildren.Last())
            {
                var gap = layoutType == LayoutType.Row ? store.GetColumnGap(node) : store.GetRowGap(node);
                if (gap.Kind == UnitKind.Stretch) mainFlexSum += gap.Value;
                else childMainAfter = gap.ToPx(childParentMain, 0);
            }

            if (childMain.Kind == UnitKind.Stretch)
            {
                mainFlexSum += childMain.Value;
            }
            else
            {
                // Fixed size on main axis, layout immediately if cross is not stretch
                if (!childCross.IsStretch)
                {
                    var size = Compute(child, layoutType, childParentMain, childParentCross, store, cache);
                    childComputedMain = size.Main;
                    childComputedCross = size.Cross;
                }
                else
                {
                    // Cross stretch, main fixed. 
                    childComputedMain = childMain.ToPx(childParentMain, 0);

                    // Get constraints - handle Auto properly
                    var minMainUnits = layoutType == LayoutType.Row
                        ? store.GetMinWidth(child)
                        : store.GetMinHeight(child);
                    var cMin = DefaultMin;
                    if (minMainUnits.IsAuto)
                    {
                        // MinAuto means content size
                        var childWidth = layoutType == LayoutType.Row ? (float?)null : childComputedMain;
                        var childHeight = layoutType == LayoutType.Column ? (float?)null : childComputedMain;
                        var content = store.MeasureContent(child, childWidth, childHeight);
                        if (content.HasValue)
                            cMin = layoutType == LayoutType.Row ? content.Value.width : content.Value.height;
                    }
                    else
                    {
                        cMin = minMainUnits.ToPx(childParentMain, DefaultMin);
                    }

                    var cMax = (layoutType == LayoutType.Row ? store.GetMaxWidth(child) : store.GetMaxHeight(child))
                        .ToPx(childParentMain, DefaultMax);
                    childComputedMain = Math.Clamp(childComputedMain, cMin, cMax);

                    // If parent's cross is Auto, we need to measure child's intrinsic cross size
                    if (cross.IsAuto)
                    {
                        // Measure content directly - child wants to stretch but we need its intrinsic size
                        var childWidth = layoutType == LayoutType.Row ? childComputedMain : (float?)null;
                        var childHeight = layoutType == LayoutType.Column ? childComputedMain : (float?)null;
                        var content = store.MeasureContent(child, childWidth, childHeight);
                        if (content.HasValue)
                            childComputedCross = layoutType == LayoutType.Row
                                ? content.Value.height
                                : content.Value.width;
                    }
                }
            }

            childrenData.Add((child, childComputedMain, childComputedCross, childMainAfter, false));
        }

        // Calculate Auto Size (First Pass)
        if (relativeChildren.Count > 0)
        {
            var mainSum = childrenData.Sum(c => c.main + c.mainAfter);
            var crossMax = childrenData.Max(c => c.cross);

            // Auto-sizing: map children's dimensions based on THIS node's layout type
            // Children's main/cross are in the node's coordinate system (based on node's layoutType)
            // But we need to set computedMain/computedCross which are in parent's coordinate system
            if (layoutType == parentLayoutType)
            {
                // Same layout direction: main maps to main, cross maps to cross
                if (main.IsAuto)
                    computedMain =
                        Math.Clamp(mainSum + padMainBefore + padMainAfter + borderMainBefore + borderMainAfter,
                            minMain, maxMain);
                if (cross.IsAuto)
                    computedCross =
                        Math.Clamp(crossMax + padCrossBefore + padCrossAfter + borderCrossBefore + borderCrossAfter,
                            minCross, maxCross);
            }
            else
            {
                // Perpendicular layout: main maps to cross, cross maps to main
                if (main.IsAuto)
                    computedMain =
                        Math.Clamp(crossMax + padCrossBefore + padCrossAfter + borderCrossBefore + borderCrossAfter,
                            minMain, maxMain);
                if (cross.IsAuto)
                    computedCross =
                        Math.Clamp(mainSum + padMainBefore + padMainAfter + borderMainBefore + borderMainAfter,
                            minCross, maxCross);
            }

            // Re-update parents
            childParentMain = (layoutType == parentLayoutType ? computedMain : computedCross) - padMainBefore -
                              padMainAfter - borderMainBefore - borderMainAfter;
            childParentCross = (layoutType == parentLayoutType ? computedCross : computedMain) - padCrossBefore -
                               padCrossAfter - borderCrossBefore - borderCrossAfter;
        }

        // Second Pass: Resolve Cross Stretch
        for (var i = 0; i < childrenData.Count; i++)
        {
            var d = childrenData[i];
            var child = d.node;
            var childCross = layoutType == LayoutType.Row ? store.GetHeight(child) : store.GetWidth(child);

            if (childCross.IsStretch)
            {
                // Handle MinAuto properly for cross axis
                var minCrossUnits = layoutType == LayoutType.Row
                    ? store.GetMinHeight(child)
                    : store.GetMinWidth(child);
                var cMin = DefaultMin;
                if (minCrossUnits.IsAuto)
                {
                    // MinAuto means content size
                    var content = store.MeasureContent(child, null, null);
                    if (content.HasValue)
                        cMin = layoutType == LayoutType.Row ? content.Value.height : content.Value.width;
                }
                else
                {
                    cMin = minCrossUnits.ToPx(childParentCross, DefaultMin);
                }

                var cMax = (layoutType == LayoutType.Row ? store.GetMaxHeight(child) : store.GetMaxWidth(child))
                    .ToPx(childParentCross, DefaultMax);
                d.cross = Math.Clamp(childParentCross, cMin, cMax);

                // Layout child if Main is not Stretch (Main Stretch is handled in Third Pass)
                var childMain = layoutType == LayoutType.Row ? store.GetWidth(child) : store.GetHeight(child);
                if (!childMain.IsStretch)
                {
                    var size = Compute(child, layoutType, childParentMain, d.cross, store, cache);
                    d.main = size.Main;
                    d.cross = size.Cross;
                }

                childrenData[i] = d; // struct update
            }
        }

        // Third Pass: Resolve Main Stretch (Flex)
        if (mainFlexSum > 0)
        {
            // If parent is auto-sized, measure stretch children at their content size first
            var nodeMainAxis = layoutType == LayoutType.Row ? width : height;
            if (nodeMainAxis.IsAuto)
            {
                for (var i = 0; i < childrenData.Count; i++)
                {
                    var d = childrenData[i];
                    var childMain = layoutType == LayoutType.Row ? store.GetWidth(d.node) : store.GetHeight(d.node);

                    if (childMain.Kind == UnitKind.Stretch)
                    {
                        // Measure at content size (use 0 as available space to get minimum size)
                        var size = Compute(d.node, layoutType, 0, d.cross, store, cache);
                        d.main = size.Main;
                        d.cross = size.Cross;
                        childrenData[i] = d;
                    }
                }
            }
            else
            {
                // Standard flex distribution with constraint freezing
                var freeSpace = Math.Max(0, childParentMain - childrenData.Sum(c => c.main + c.mainAfter));
                var activeFlex = mainFlexSum;
                bool constraintHit;

                // Loop until no more constraints are hit
                do
                {
                    constraintHit = false;

                    for (var i = 0; i < childrenData.Count; i++)
                    {
                        var d = childrenData[i];
                        if (d.frozen) continue;

                        var childMain = layoutType == LayoutType.Row
                            ? store.GetWidth(d.node)
                            : store.GetHeight(d.node);

                        if (childMain.Kind == UnitKind.Stretch)
                        {
                            var share = activeFlex > 0 ? childMain.Value / activeFlex * freeSpace : 0;

                            // Get constraints - handle MinAuto properly
                            var minMainUnits = layoutType == LayoutType.Row
                                ? store.GetMinWidth(d.node)
                                : store.GetMinHeight(d.node);
                            var cMin = DefaultMin;
                            if (minMainUnits.IsAuto)
                            {
                                // MinAuto means content size
                                var content = store.MeasureContent(d.node, null, null);
                                if (content.HasValue)
                                    cMin = layoutType == LayoutType.Row
                                        ? content.Value.width
                                        : content.Value.height;
                            }
                            else
                            {
                                cMin = minMainUnits.ToPx(childParentMain, DefaultMin);
                            }

                            var cMax = (layoutType == LayoutType.Row
                                ? store.GetMaxWidth(d.node)
                                : store.GetMaxHeight(d.node)).ToPx(childParentMain, DefaultMax);

                            // Check if share violates constraints
                            if (share < cMin)
                            {
                                d.main = cMin;
                                d.frozen = true;
                                activeFlex -= childMain.Value;
                                freeSpace -= cMin; // Can go negative for overflow
                                constraintHit = true;
                            }
                            else if (share > cMax)
                            {
                                d.main = cMax;
                                d.frozen = true;
                                activeFlex -= childMain.Value;
                                freeSpace -= cMax;
                                constraintHit = true;
                            }
                            else
                            {
                                d.main = share;
                            }

                            childrenData[i] = d;
                        }
                    }
                } while (constraintHit && activeFlex > 0 && freeSpace > 0); // Stop if no free space left

                // Now compute the final layout for each stretched child
                for (var i = 0; i < childrenData.Count; i++)
                {
                    var d = childrenData[i];
                    var childMain = layoutType == LayoutType.Row ? store.GetWidth(d.node) : store.GetHeight(d.node);

                    if (childMain.Kind == UnitKind.Stretch)
                    {
                        var size = Compute(d.node, layoutType, d.main, d.cross, store, cache);
                        d.main = size.Main;
                        d.cross = size.Cross;
                        childrenData[i] = d;
                    }
                }
            }
        }

        // Auto Size Final Pass
        if (relativeChildren.Count > 0)
        {
            var mainSum = childrenData.Sum(c => c.main + c.mainAfter);
            var crossMax = childrenData.Max(c => c.cross);

            if (layoutType == parentLayoutType)
            {
                if (main.IsAuto)
                    computedMain =
                        Math.Clamp(mainSum + padMainBefore + padMainAfter + borderMainBefore + borderMainAfter,
                            minMain, maxMain);
                if (cross.IsAuto)
                    computedCross =
                        Math.Clamp(crossMax + padCrossBefore + padCrossAfter + borderCrossBefore + borderCrossAfter,
                            minCross, maxCross);
            }
            else
            {
                if (main.IsAuto)
                    computedMain =
                        Math.Clamp(crossMax + padCrossBefore + padCrossAfter + borderCrossBefore + borderCrossAfter,
                            minMain, maxMain);
                if (cross.IsAuto)
                    computedCross =
                        Math.Clamp(mainSum + padMainBefore + padMainAfter + borderMainBefore + borderMainAfter,
                            minCross, maxCross);
            }
        }

        computedMain = Math.Clamp(computedMain, minMain, maxMain);
        computedCross = Math.Clamp(computedCross, minCross, maxCross);

        // Position Children
        var currentMainPos = padMainBefore + borderMainBefore;
        var alignment = store.GetAlignment(node);

        // Alignment Offset
        var mainSumFinal = childrenData.Sum(c => c.main + c.mainAfter);
        var extraMain = childParentMain + padMainBefore + padMainAfter + borderMainBefore + borderMainAfter -
                        (mainSumFinal + padMainBefore + padMainAfter + borderMainBefore +
                         borderMainAfter); // Roughly
        // Simpler:
        var contentMain = mainSumFinal;
        var freeMain = childParentMain - contentMain;

        if (freeMain > 0)
        {
            if (alignment is Alignment.Center or Alignment.TopCenter or Alignment.BottomCenter)
                currentMainPos += freeMain * 0.5f;
            else if (alignment is Alignment.Right or Alignment.TopRight or Alignment.BottomRight)
                currentMainPos += freeMain;
            // Note: This assumes Row layout logic for main axis alignment mapping. 
            // A full mapping of Alignment enum to Flex-Start/End/Center per axis is needed for exact parity.
        }

        for (var i = 0; i < childrenData.Count; i++)
        {
            var d = childrenData[i];
            var crossPos = padCrossBefore + borderCrossBefore;

            // Cross alignment
            var freeCross = childParentCross - d.cross;
            if (freeCross > 0)
            {
                // Simple mapping for demo
                if (alignment == Alignment.Center || alignment == Alignment.Left || alignment == Alignment.Right)
                    crossPos += freeCross * 0.5f;
                if (alignment == Alignment.BottomCenter || alignment == Alignment.BottomLeft ||
                    alignment == Alignment.BottomRight) crossPos += freeCross;
            }

            float x, y, w, h;
            if (layoutType == LayoutType.Row)
            {
                x = currentMainPos;
                y = crossPos;
                w = d.main;
                h = d.cross;
            }
            else
            {
                x = crossPos;
                y = currentMainPos;
                w = d.cross;
                h = d.main;
            }

            // Apply relative positioning offsets (Left/Top/Right/Bottom) - these shift the node visually without affecting layout flow
            var leftOffset = store.GetLeft(d.node)
                .ToPx(layoutType == LayoutType.Row ? computedMain : computedCross, 0);
            var topOffset = store.GetTop(d.node)
                .ToPx(layoutType == LayoutType.Column ? computedMain : computedCross, 0);

            cache.SetBounds(d.node, x + cache.GetPosX(node) + leftOffset, y + cache.GetPosY(node) + topOffset, w,
                h);
            currentMainPos += d.main + d.mainAfter;
        }

        // Absolute Children
        foreach (var child in visibleChildren.Where(c => store.GetPositionType(c) == PositionType.Absolute))
        {
            var cMinW = store.GetMinWidth(child).ToPx(computedMain, DefaultMin);
            var cMaxW = store.GetMaxWidth(child).ToPx(computedMain, DefaultMax);
            var cMinH = store.GetMinHeight(child).ToPx(computedCross, DefaultMin);
            var cMaxH = store.GetMaxHeight(child).ToPx(computedCross, DefaultMax);

            var childW = store.GetWidth(child);
            var childH = store.GetHeight(child);

            var cw = childW.IsStretch ? computedMain : childW.ToPx(computedMain, 0);
            var ch = childH.IsStretch ? computedCross : childH.ToPx(computedCross, 0);

            // Recursive layout for absolute child content
            var size = Compute(child, layoutType, cw, ch, store, cache);
            cw = size.Main;
            ch = size.Cross;

            cw = Math.Clamp(cw, cMinW, cMaxW);
            ch = Math.Clamp(ch, cMinH, cMaxH);

            var left = store.GetLeft(child).ToPx(computedMain, 0);
            var top = store.GetTop(child).ToPx(computedCross, 0);

            // Handle Stretch/Auto positioning logic simplified
            cache.SetBounds(child, cache.GetPosX(node) + left, cache.GetPosY(node) + top, cw, ch);
        }

        return new Size { Main = computedMain, Cross = computedCross };
    }

    private static Size LayoutGrid(object node, float width, float height, ILayoutNode store, ILayoutCache cache)
    {
        // Minimal Grid Implementation for test compatibility
        // In a full port, this duplicates the track sizing logic from layout.rs

        // Account for padding and borders
        var padLeft = store.GetPaddingLeft(node).ToPx(width, 0);
        var padRight = store.GetPaddingRight(node).ToPx(width, 0);
        var padTop = store.GetPaddingTop(node).ToPx(height, 0);
        var padBottom = store.GetPaddingBottom(node).ToPx(height, 0);

        var borderLeft = store.GetBorderLeft(node).ToPx(width, 0);
        var borderRight = store.GetBorderRight(node).ToPx(width, 0);
        var borderTop = store.GetBorderTop(node).ToPx(height, 0);
        var borderBottom = store.GetBorderBottom(node).ToPx(height, 0);

        var availableWidth = width - padLeft - padRight - borderLeft - borderRight;
        var availableHeight = height - padTop - padBottom - borderTop - borderBottom;
        var offsetX = padLeft + borderLeft;
        var offsetY = padTop + borderTop;

        var colTracks = store.GetGridColumns(node);
        var rowTracks = store.GetGridRows(node);

        // Resolve tracks (simplified: px and stretch only)
        var colSizes = new float[colTracks.Count];
        float usedW = 0;
        float flexColSum = 0;
        for (var i = 0; i < colTracks.Count; i++)
            if (colTracks[i].IsStretch)
            {
                flexColSum += colTracks[i].Value;
            }
            else
            {
                colSizes[i] = colTracks[i].ToPx(availableWidth, 0);
                usedW += colSizes[i];
            }

        if (flexColSum > 0)
        {
            var free = Math.Max(0, availableWidth - usedW);
            for (var i = 0; i < colTracks.Count; i++)
                if (colTracks[i].IsStretch)
                    colSizes[i] = colTracks[i].Value / flexColSum * free;
        }

        var rowSizes = new float[rowTracks.Count];
        float usedH = 0;
        float flexRowSum = 0;
        for (var i = 0; i < rowTracks.Count; i++)
            if (rowTracks[i].IsStretch)
            {
                flexRowSum += rowTracks[i].Value;
            }
            else
            {
                rowSizes[i] = rowTracks[i].ToPx(availableHeight, 0);
                usedH += rowSizes[i];
            }

        if (flexRowSum > 0)
        {
            var free = Math.Max(0, availableHeight - usedH);
            for (var i = 0; i < rowTracks.Count; i++)
                if (rowTracks[i].IsStretch)
                    rowSizes[i] = rowTracks[i].Value / flexRowSum * free;
        }

        // Calculate positions
        var colPos = new float[colSizes.Length + 1];
        for (var i = 0; i < colSizes.Length; i++) colPos[i + 1] = colPos[i] + colSizes[i];

        var rowPos = new float[rowSizes.Length + 1];
        for (var i = 0; i < rowSizes.Length; i++) rowPos[i + 1] = rowPos[i] + rowSizes[i];

        // Layout Children
        foreach (var child in store.GetChildren(node).Where(c => store.IsVisible(c)))
        {
            var cStart = store.GetColumnStart(child);
            var cSpan = store.GetColumnSpan(child);
            var rStart = store.GetRowStart(child);
            var rSpan = store.GetRowSpan(child);

            // Safe indices
            cStart = Math.Clamp(cStart, 0, colSizes.Length - 1);
            rStart = Math.Clamp(rStart, 0, rowSizes.Length - 1);

            var cx = colPos[cStart];
            var cy = rowPos[rStart];
            var cw = colPos[Math.Min(cStart + cSpan, colPos.Length - 1)] - cx;
            var ch = rowPos[Math.Min(rStart + rSpan, rowPos.Length - 1)] - cy;

            Compute(child, LayoutType.Row, cw, ch, store, cache);
            cache.SetBounds(child, cache.GetPosX(node) + cx + offsetX, cache.GetPosY(node) + cy + offsetY, cw, ch);
        }

        return new Size { Main = width, Cross = height };
    }
}