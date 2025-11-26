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

internal class LayoutChild
{
    public object Node { get; }
    public float Main { get; set; }
    public float Cross { get; set; }
    public float GapAfter { get; set; }
    public bool Frozen { get; set; }
    public bool IsFlex { get; set; }

    public LayoutChild(object node)
    {
        Node = node;
    }
}

internal class NodeLayoutState
{
    public object Node { get; }
    public LayoutType Type { get; }
    public LayoutType ParentType { get; }
    
    public float ComputedMain { get; set; }
    public float ComputedCross { get; set; }
    
    public float MinMain { get; set; }
    public float MaxMain { get; set; }
    public float MinCross { get; set; }
    public float MaxCross { get; set; }

    public float PadMainBefore { get; set; }
    public float PadMainAfter { get; set; }
    public float PadCrossBefore { get; set; }
    public float PadCrossAfter { get; set; }
    public float BorderMainBefore { get; set; }
    public float BorderMainAfter { get; set; }
    public float BorderCrossBefore { get; set; }
    public float BorderCrossAfter { get; set; }

    public float InnerContentMain { get; set; }
    public float InnerContentCross { get; set; }

    public NodeLayoutState(object node, LayoutType type, LayoutType parentType)
    {
        Node = node;
        Type = type;
        ParentType = parentType;
    }
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

internal static class LayoutUtils
{
    private const float DefaultMin = 0f;
    private const float DefaultMax = float.MaxValue;

    public static float ResolveUnit(Units unit, float parentSize)
    {
        return unit.Kind switch
        {
            UnitKind.Pixels => unit.Value,
            UnitKind.Percentage => (float)Math.Round(parentSize * (unit.Value / 100.0f)),
            UnitKind.Stretch => parentSize,
            _ => 0.0f
        };
    }

    public static float ResolveAutoMin(Units units, bool isWidthAxis, float referenceSize, object target, ILayoutNode store)
    {
        if (!units.IsAuto) return units.ToPx(referenceSize, DefaultMin);
        
        var content = store.MeasureContent(target, null, null);
        if (content.HasValue) return isWidthAxis ? content.Value.width : content.Value.height;
        return DefaultMin;
    }

    public static (float min, float max) GetChildMainConstraints(object child, ILayoutNode store, LayoutType layoutType, float referenceSize)
    {
        var minUnits = layoutType == LayoutType.Row ? store.GetMinWidth(child) : store.GetMinHeight(child);
        var maxUnits = layoutType == LayoutType.Row ? store.GetMaxWidth(child) : store.GetMaxHeight(child);

        var min = DefaultMin;
        if (minUnits.IsAuto)
        {
            var content = store.MeasureContent(child, null, null);
            if (content.HasValue)
                min = layoutType == LayoutType.Row ? content.Value.width : content.Value.height;
        }
        else
        {
            min = minUnits.ToPx(referenceSize, DefaultMin);
        }

        var max = maxUnits.ToPx(referenceSize, DefaultMax);
        return (min, max);
    }

    public static (float min, float max) GetChildCrossConstraints(object child, ILayoutNode store, LayoutType layoutType, float referenceSize)
    {
        var minUnits = layoutType == LayoutType.Row ? store.GetMinHeight(child) : store.GetMinWidth(child);
        var maxUnits = layoutType == LayoutType.Row ? store.GetMaxHeight(child) : store.GetMaxWidth(child);

        var min = DefaultMin;
        if (minUnits.IsAuto)
        {
            var content = store.MeasureContent(child, null, null);
            if (content.HasValue)
                min = layoutType == LayoutType.Row ? content.Value.height : content.Value.width;
        }
        else
        {
            min = minUnits.ToPx(referenceSize, DefaultMin);
        }

        var max = maxUnits.ToPx(referenceSize, DefaultMax);
        return (min, max);
    }
}

internal static class FlexSolver
{
    public static void ResolveFlexibleChildren(
        List<LayoutChild> children, 
        NodeLayoutState state, 
        ILayoutNode store, 
        float totalFlexSum, 
        float availableSpace)
    {
        if (totalFlexSum <= 0) return;

        var activeFlex = totalFlexSum;
        var freeSpace = Math.Max(0, availableSpace);
        bool constraintHit;

        do
        {
            constraintHit = false;

            foreach (var child in children)
            {
                if (!child.IsFlex || child.Frozen) continue;

                var childMainUnit = state.Type == LayoutType.Row 
                    ? store.GetWidth(child.Node) 
                    : store.GetHeight(child.Node);

                var share = activeFlex > 0 
                    ? (childMainUnit.Value / activeFlex) * freeSpace 
                    : 0;

                var (cMin, cMax) = LayoutUtils.GetChildMainConstraints(child.Node, store, state.Type, state.InnerContentMain);

                if (share < cMin)
                {
                    child.Main = cMin;
                    child.Frozen = true;
                    activeFlex -= childMainUnit.Value;
                    freeSpace -= cMin;
                    constraintHit = true;
                }
                else if (share > cMax)
                {
                    child.Main = cMax;
                    child.Frozen = true;
                    activeFlex -= childMainUnit.Value;
                    freeSpace -= cMax;
                    constraintHit = true;
                }
                else
                {
                    child.Main = share;
                }
            }
        } while (constraintHit && activeFlex > 0 && freeSpace > 0);
    }
}

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

        // Layout root. 0,0 is relative to context (absolute).
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
        ILayoutNode store, ILayoutCache cache, bool forceMainAuto = false, bool forceCrossAuto = false)
    {
        var layoutType = store.GetLayoutType(node);

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

        var effectiveMainKind = (forceMainAuto && main.Kind == UnitKind.Stretch) ? UnitKind.Auto : main.Kind;
        var effectiveCrossKind = (forceCrossAuto && cross.Kind == UnitKind.Stretch) ? UnitKind.Auto : cross.Kind;

        var computedMain = effectiveMainKind switch
        {
            UnitKind.Pixels => main.Value,
            UnitKind.Percentage => (float)Math.Round(parentMain * (main.Value / 100.0f)),
            UnitKind.Stretch => parentMain,
            _ => 0.0f
        };

        var computedCross = effectiveCrossKind switch
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

        var isMainAuto = effectiveMainKind == UnitKind.Auto;
        var isCrossAuto = effectiveCrossKind == UnitKind.Auto;

        // Content Sizing
        if ((isMainAuto || isCrossAuto) && relativeChildren.Count == 0)
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
                if (isMainAuto)
                    computedMain = parentLayoutType is LayoutType.Row or LayoutType.Grid
                        ? content.Value.width
                        : content.Value.height;
                if (isCrossAuto)
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
        var childrenData = new List<LayoutChild>();

        // Calculate total gap space first (needed for correct percentage calculations)
        var totalGapSpace = 0f;
        if (relativeChildren.Count > 1)
        {
            var gap = layoutType == LayoutType.Row ? store.GetColumnGap(node) : store.GetRowGap(node);
            if (gap.Kind != UnitKind.Stretch)
            {
                totalGapSpace = gap.ToPx(childParentMain, 0) * (relativeChildren.Count - 1);
            }
        }
        
        // Available space for children after accounting for gaps
        var availableForChildren = childParentMain - totalGapSpace;

        // First Pass: Non-flexible Relative Children & collection of flex factors
        foreach (var child in relativeChildren)
        {
            var childW = store.GetWidth(child);
            var childH = store.GetHeight(child);

            var childMain = layoutType == LayoutType.Row ? childW : childH;
            var childCross = layoutType == LayoutType.Row ? childH : childW;

            var childData = new LayoutChild(child);

            if (child != relativeChildren.Last())
            {
                var gap = layoutType == LayoutType.Row ? store.GetColumnGap(node) : store.GetRowGap(node);
                if (gap.Kind == UnitKind.Stretch) mainFlexSum += gap.Value;
                else childData.GapAfter = gap.ToPx(childParentMain, 0);
            }

            if (childMain.Kind == UnitKind.Stretch)
            {
                mainFlexSum += childMain.Value;
                childData.IsFlex = true;
            }
            else
            {
                if (!childCross.IsStretch)
                {
                    var size = Compute(child, layoutType, availableForChildren, childParentCross, store, cache);
                    childData.Main = size.Main;
                    childData.Cross = size.Cross;
                }
                else
                {
                    // Use availableForChildren for percentage/pixel calculations to account for gaps
                    childData.Main = childMain.ToPx(availableForChildren, 0);

                    // Get constraints - handle Auto properly
                    var minMainUnits = layoutType == LayoutType.Row
                        ? store.GetMinWidth(child)
                        : store.GetMinHeight(child);
                    var cMin = DefaultMin;
                    if (minMainUnits.IsAuto)
                    {
                        // MinAuto means content size
                        var childWidth = layoutType == LayoutType.Row ? (float?)null : childData.Main;
                        var childHeight = layoutType == LayoutType.Column ? (float?)null : childData.Main;
                        var content = store.MeasureContent(child, childWidth, childHeight);
                        if (content.HasValue)
                            cMin = layoutType == LayoutType.Row ? content.Value.width : content.Value.height;
                    }
                    else
                    {
                        cMin = minMainUnits.ToPx(availableForChildren, DefaultMin);
                    }

                    var cMax = (layoutType == LayoutType.Row ? store.GetMaxWidth(child) : store.GetMaxHeight(child))
                        .ToPx(availableForChildren, DefaultMax);
                    childData.Main = Math.Clamp(childData.Main, cMin, cMax);

                    // Check if THIS node's cross-axis is Auto (need to measure child's intrinsic size)
                    // For Row layout: cross is height
                    // For Column layout: cross is width
                    var nodeCrossAxisIsAuto = layoutType == LayoutType.Row ? height.IsAuto : width.IsAuto;
                    if (nodeCrossAxisIsAuto)
                    {
                        var size = Compute(child, layoutType, childData.Main, childParentCross, store, cache, 
                            forceMainAuto: false, forceCrossAuto: true);
                        childData.Cross = size.Cross;
                    }
                }
            }

            childrenData.Add(childData);
        }

        // Calculate Auto Size (First Pass)
        if (relativeChildren.Count > 0)
        {
            var mainSum = childrenData.Sum(c => c.Main + c.GapAfter);
            var crossMax = childrenData.Max(c => c.Cross);

            // Auto-sizing: map children's dimensions based on THIS node's layout type
            // Children's main/cross are in the node's coordinate system (based on node's layoutType)
            // But we need to set computedMain/computedCross which are in parent's coordinate system
            if (layoutType == parentLayoutType)
            {
                // Same layout direction: main maps to main, cross maps to cross
                if (isMainAuto)
                    computedMain =
                        Math.Clamp(mainSum + padMainBefore + padMainAfter + borderMainBefore + borderMainAfter,
                            minMain, maxMain);
                if (isCrossAuto)
                    computedCross =
                        Math.Clamp(crossMax + padCrossBefore + padCrossAfter + borderCrossBefore + borderCrossAfter,
                            minCross, maxCross);
            }
            else
            {
                // Perpendicular layout: main maps to cross, cross maps to main
                if (isMainAuto)
                    computedMain =
                        Math.Clamp(crossMax + padCrossBefore + padCrossAfter + borderCrossBefore + borderCrossAfter,
                            minMain, maxMain);
                if (isCrossAuto)
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
        foreach (var d in childrenData)
        {
            var child = d.Node;
            var childCross = layoutType == LayoutType.Row ? store.GetHeight(child) : store.GetWidth(child);

            if (childCross.IsStretch)
            {
                var minCrossUnits = layoutType == LayoutType.Row
                    ? store.GetMinHeight(child)
                    : store.GetMinWidth(child);
                var cMin = DefaultMin;
                if (minCrossUnits.IsAuto)
                {
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

                var nodeCrossAxisIsAuto = layoutType == LayoutType.Row ? height.IsAuto : width.IsAuto;
                if (nodeCrossAxisIsAuto)
                {
                    var size = Compute(child, layoutType, childParentMain, childParentCross, store, cache,
                        forceMainAuto: false, forceCrossAuto: true);
                    d.Cross = Math.Clamp(size.Cross, cMin, cMax);
                }
                else
                {
                    d.Cross = Math.Clamp(childParentCross, cMin, cMax);
                }

                var childMain = layoutType == LayoutType.Row ? store.GetWidth(child) : store.GetHeight(child);
                if (!childMain.IsStretch)
                {
                    // Use availableForChildren to maintain gap-corrected sizing
                    var size = Compute(child, layoutType, availableForChildren, d.Cross, store, cache);
                    d.Main = size.Main;
                    d.Cross = size.Cross;
                }
            }
        }

        // Third Pass: Resolve Main Stretch (Flex)
        if (mainFlexSum > 0)
        {
            var nodeMainAxis = layoutType == LayoutType.Row ? width : height;
            if (nodeMainAxis.IsAuto)
            {
                foreach (var d in childrenData.Where(c => c.IsFlex))
                {
                    var size = Compute(d.Node, layoutType, 0, d.Cross, store, cache);
                    d.Main = size.Main;
                    d.Cross = size.Cross;
                }
            }
            else
            {
                var state = new NodeLayoutState(node, layoutType, parentLayoutType)
                {
                    InnerContentMain = childParentMain
                };
                
                var mainUsed = childrenData.Sum(c => c.Main + c.GapAfter);
                FlexSolver.ResolveFlexibleChildren(childrenData, state, store, mainFlexSum, childParentMain - mainUsed);

                foreach (var d in childrenData.Where(c => c.IsFlex))
                {
                    var size = Compute(d.Node, layoutType, d.Main, d.Cross, store, cache);
                    d.Main = size.Main;
                    d.Cross = size.Cross;
                }
            }
        }

        // Auto Size Final Pass
        if (relativeChildren.Count > 0)
        {
            var mainSum = childrenData.Sum(c => c.Main + c.GapAfter);
            var crossMax = childrenData.Max(c => c.Cross);

            if (layoutType == parentLayoutType)
            {
                if (isMainAuto)
                    computedMain =
                        Math.Clamp(mainSum + padMainBefore + padMainAfter + borderMainBefore + borderMainAfter,
                            minMain, maxMain);
                if (isCrossAuto)
                    computedCross =
                        Math.Clamp(crossMax + padCrossBefore + padCrossAfter + borderCrossBefore + borderCrossAfter,
                            minCross, maxCross);
            }
            else
            {
                if (isMainAuto)
                    computedMain =
                        Math.Clamp(crossMax + padCrossBefore + padCrossAfter + borderCrossBefore + borderCrossAfter,
                            minMain, maxMain);
                if (isCrossAuto)
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

        var mainSumFinal = childrenData.Sum(c => c.Main + c.GapAfter);
        var freeMain = childParentMain - mainSumFinal;

        if (freeMain > 0)
        {
            if (alignment is Alignment.Center or Alignment.TopCenter or Alignment.BottomCenter)
                currentMainPos += freeMain * 0.5f;
            else if (alignment is Alignment.Right or Alignment.TopRight or Alignment.BottomRight)
                currentMainPos += freeMain;
        }

        foreach (var d in childrenData)
        {
            var crossPos = padCrossBefore + borderCrossBefore;

            var freeCross = childParentCross - d.Cross;
            if (freeCross > 0)
            {
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
                w = d.Main;
                h = d.Cross;
            }
            else
            {
                x = crossPos;
                y = currentMainPos;
                w = d.Cross;
                h = d.Main;
            }

            var leftOffset = store.GetLeft(d.Node)
                .ToPx(layoutType == LayoutType.Row ? computedMain : computedCross, 0);
            var topOffset = store.GetTop(d.Node)
                .ToPx(layoutType == LayoutType.Column ? computedMain : computedCross, 0);

            cache.SetBounds(d.Node, x + leftOffset, y + topOffset, w, h);
            currentMainPos += d.Main + d.GapAfter;
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

            // FIX: Store RELATIVE position
            cache.SetBounds(child, left, top, cw, ch);
        }

        return new Size { Main = computedMain, Cross = computedCross };
    }

    private static Size LayoutGrid(object node, float width, float height, ILayoutNode store, ILayoutCache cache)
    {
        // Minimal Grid Implementation
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
            // FIX: Relative position
            cache.SetBounds(child, cx + offsetX, cy + offsetY, cw, ch);
        }

        return new Size { Main = width, Cross = height };
    }
}