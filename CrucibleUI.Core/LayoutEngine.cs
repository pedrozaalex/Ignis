using CrucibleUI.Core.Extensions;
using CrucibleUI.Core.Interfaces;
using CrucibleUI.Core.Types;

namespace CrucibleUI.Core;

public static class LayoutEngine
{
    // -------------------------------------------------------------------------
    // Internal State Types
    // -------------------------------------------------------------------------

    private record struct StretchItem(
        int Index,
        float Factor,
        float Min,
        float Max,
        float ComputedSize = 0,
        bool Frozen = false
    );

    private ref struct LayoutContext<TTree, TSubLayout, TCacheKey, TCache>
        where TCache : ICache<TCacheKey>
        where TCacheKey : notnull
    {
        public readonly TTree Tree;
        public readonly TCache Cache;
        public readonly ref TSubLayout SubLayout;

        public LayoutContext(TTree tree, TCache cache, ref TSubLayout subLayout)
        {
            Tree = tree;
            Cache = cache;
            SubLayout = ref subLayout;
        }
    }

    private readonly record struct AxisValues(float Main, float Cross);

    private readonly record struct BoxEdges(float Left, float Top, float Right, float Bottom)
    {
        public float Horizontal => Left + Right;
        public float Vertical => Top + Bottom;
    }

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    public static void Compute<TNode, TTree, TSubLayout, TCacheKey, TCache>(
        TNode node,
        TCache cache,
        TTree tree,
        ref TSubLayout subLayout)
        where TNode : INode<TTree, TSubLayout, TCacheKey>
        where TCache : ICache<TCacheKey>
        where TCacheKey : notnull
    {
        var ctx = new LayoutContext<TTree, TSubLayout, TCacheKey, TCache>(tree, cache, ref subLayout);

        // Reset root
        ctx.Cache.SetBounds(node.Key, 0, 0, 0, 0);

        // Kick off recursive layout
        LayoutNodeRecursive(node, ctx, parentWidth: null, parentHeight: null);
    }

    // -------------------------------------------------------------------------
    // Core Logic
    // -------------------------------------------------------------------------

    private static void LayoutNodeRecursive<TTree, TSubLayout, TCacheKey, TCache>(
        INode<TTree, TSubLayout, TCacheKey> node,
        LayoutContext<TTree, TSubLayout, TCacheKey, TCache> ctx,
        float? parentWidth,
        float? parentHeight,
        float? overrideWidth = null,
        float? overrideHeight = null)
        where TCache : ICache<TCacheKey>
        where TCacheKey : notnull
    {
        // 1. Fast Path: Invisible
        if (node is { Visible: false })
        {
            SetInvisibleRecursive(node, ctx);
            return;
        }

        var layoutType = node.LayoutType ?? LayoutType.Column;
        var key = node.Key;

        // 2. Grid Layout
        if (layoutType is LayoutType.Grid)
        {
            LayoutGridNode(node, ctx, parentWidth, parentHeight, overrideWidth, overrideHeight);
            return;
        }

        // 3. Setup Layout Orientation
        var isRow = layoutType is LayoutType.Row;

        // Helper to swap dimensions based on orientation
        (float Main, float Cross) ToAxis(float w, float h) => isRow ? (w, h) : (h, w);
        (float X, float Y) FromAxis(float main, float cross) => isRow ? (main, cross) : (cross, main);

        // 4. Resolve Constraints
        var explicitW = overrideWidth ?? ResolveSize(node.Width, node.MinWidth, node.MaxWidth, parentWidth);
        var explicitH = overrideHeight ?? ResolveSize(node.Height, node.MinHeight, node.MaxHeight, parentHeight);

        var baseW = explicitW ?? parentWidth ?? 0;
        var baseH = explicitH ?? parentHeight ?? 0;

        var padding = ResolveEdges(node, baseW, baseH, isBorder: false);
        var border = ResolveEdges(node, baseW, baseH, isBorder: true);

        var totalEdgeH = padding.Horizontal + border.Horizontal;
        var totalEdgeV = padding.Vertical + border.Vertical;
        var (edgeMain, edgeCross) = ToAxis(totalEdgeH, totalEdgeV);

        // 5. Categorize Children
        var allChildren = node.Children(ctx.Tree).ToList();
        var flexChildren = new List<INode<TTree, TSubLayout, TCacheKey>>(allChildren.Count);
        var absoluteChildren = new List<INode<TTree, TSubLayout, TCacheKey>>(allChildren.Count);

        foreach (var child in allChildren)
        {
            if (!child.Visible)
            {
                SetInvisibleRecursive(child, ctx);
                continue;
            }

            if ((child.PositionType ?? PositionType.Relative) is PositionType.Absolute)
                absoluteChildren.Add(child);
            else
                flexChildren.Add(child);
        }

        // 6. Setup Flex Variables
        var innerW = Math.Max(0, (explicitW ?? baseW) - totalEdgeH);
        var innerH = Math.Max(0, (explicitH ?? baseH) - totalEdgeV);
        var (innerMain, _) = ToAxis(innerW, innerH);

        // Determine Gap
        var gapDef = isRow ? node.HorizontalGap ?? default : node.VerticalGap ?? default;
        var minGapDef = isRow ? node.MinHorizontalGap ?? default : node.MinVerticalGap ?? default;
        var maxGapDef = isRow ? node.MaxHorizontalGap ?? default : node.MaxVerticalGap ?? default;
        var isStretchGap = gapDef.Kind is UnitsKind.Stretch;
        var fixedGap = isStretchGap ? 0f : gapDef.ToPx(innerMain, 0);
        var totalFixedGap = Math.Max(0, flexChildren.Count - 1) * fixedGap;

        // -------------------------------------------------------------
        // PHASE 1: Measurement (Determine size of flex children)
        // -------------------------------------------------------------

        var childMainSizes = new float?[flexChildren.Count];
        var childCrossSizes = new float?[flexChildren.Count];
        var stretchItems = new List<StretchItem>();

        float totalDefinedMain = 0;
        float maxChildCross = 0;

        var knownMain = isRow ? explicitW : explicitH;
        var knownCross = isRow ? explicitH : explicitW;

        // Override dimensions respect layout orientation
        if (overrideWidth.HasValue || overrideHeight.HasValue)
        {
            var (overrideMain, overrideCross) = ToAxis(overrideWidth ?? innerW, overrideHeight ?? innerH);
            if (isRow ? overrideWidth.HasValue : overrideHeight.HasValue)
                knownMain = overrideMain;
            if (isRow ? overrideHeight.HasValue : overrideWidth.HasValue)
                knownCross = overrideCross;
        }

        for (var i = 0; i < flexChildren.Count; i++)
        {
            var child = flexChildren[i];

            // Extract Child Constraints
            var (cMain, cCross) = (child.Main(layoutType), child.Cross(layoutType));
            var minMainUnit = child.MinMain(layoutType);
            var minCrossUnit = child.MinCross(layoutType);

            var minMain = minMainUnit.ToPx(knownMain ?? 0, 0);
            var maxMain = child.MaxMain(layoutType).ToPx(knownMain ?? 0, float.MaxValue);
            var minCross = minCrossUnit.ToPx(knownCross ?? 0, 0);
            var maxCross = child.MaxCross(layoutType).ToPx(knownCross ?? 0, float.MaxValue);

            // Calculate Initial Sizes (if not auto)
            var mainSize = ResolveUnit(cMain, knownMain);
            if (mainSize.HasValue) mainSize = Math.Clamp(mainSize.Value, minMain, maxMain);

            var crossSize = ResolveUnit(cCross, knownCross);
            if (crossSize.HasValue) crossSize = Math.Clamp(crossSize.Value, minCross, maxCross);

            // Recursive Measurement if needed
            var needMain = cMain.Kind is not UnitsKind.Stretch && !mainSize.HasValue;
            var needCross = cCross.Kind is UnitsKind.Auto && !crossSize.HasValue;

            // If MinMain or MinCross is Auto, we need to compute content size to get the minimum
            var needContentForMinMain = minMainUnit.Kind is UnitsKind.Auto && cMain.Kind is UnitsKind.Stretch;
            var needContentForMinCross = minCrossUnit.Kind is UnitsKind.Auto && cCross.Kind is UnitsKind.Stretch;

            // If Main is Stretch, we can't reliably resolve Auto Cross derived from it yet 
            // because the Stretch distribution hasn't happened.
            if (cMain.Kind is UnitsKind.Stretch && !needContentForMinMain && !needContentForMinCross)
            {
                needMain = false;
                needCross = false;
            }

            if (needMain || needCross || needContentForMinMain || needContentForMinCross)
            {
                var contentSize = ComputeChildContentSize(child, ctx, layoutType, knownMain ?? 0, knownCross ?? 0);
                if (needMain) mainSize = Math.Clamp(contentSize.Main, minMain, maxMain);
                if (needCross) crossSize = Math.Clamp(contentSize.Cross, minCross, maxCross);
                // Use content size as minimum when MinMain/MinCross is Auto
                if (needContentForMinMain) minMain = contentSize.Main;
                if (needContentForMinCross) minCross = contentSize.Cross;
            }

            // Register Stretch or Accumulate Size
            if (cMain.Kind is UnitsKind.Stretch)
                stretchItems.Add(new StretchItem(i, Math.Max(cMain.Value, 1f), minMain, maxMain));
            else if (mainSize.HasValue)
                totalDefinedMain += mainSize.Value;

            // Note: MainBefore/MainAfter on children are NOT used for relative positioning in morphorm
            // They are only used for absolute children

            childMainSizes[i] = mainSize;
            childCrossSizes[i] = crossSize;
            if (crossSize.HasValue) maxChildCross = Math.Max(maxChildCross, crossSize.Value);
        }

        // -------------------------------------------------------------
        // PHASE 2: Resolution (Determine container size & stretch)
        // -------------------------------------------------------------

        var finalMain = DetermineFinalSize(knownMain ?? totalDefinedMain + totalFixedGap + edgeMain,
            isRow ? node.MinWidth : node.MinHeight,
            isRow ? node.MaxWidth : node.MaxHeight,
            parentWidth ?? parentHeight ?? 0);

        var finalCross = DetermineFinalSize(knownCross ?? maxChildCross + edgeCross,
            isRow ? node.MinHeight : node.MinWidth,
            isRow ? node.MaxHeight : node.MaxWidth,
            parentHeight ?? parentWidth ?? 0);

        // Add stretch gap items if gap is stretch
        // Gap items use negative indices (-1 for gap after child 0, -2 for gap after child 1, etc.)
        var numGaps = Math.Max(0, flexChildren.Count - 1);
        var gapSizes = new float[numGaps];
        
        if (isStretchGap && numGaps > 0)
        {
            var gapFactor = Math.Max(gapDef.Value, 1f);
            var minGap = minGapDef.ToPx(finalMain - edgeMain, 0);
            var maxGap = maxGapDef.ToPx(finalMain - edgeMain, float.MaxValue);
            
            // Add one stretch item per gap (index encoded as -(gapIndex + 1))
            for (var g = 0; g < numGaps; g++)
            {
                stretchItems.Add(new StretchItem(-(g + 1), gapFactor, minGap, maxGap));
            }
        }
        else
        {
            // Fixed gaps - fill the gapSizes array
            for (var g = 0; g < numGaps; g++)
                gapSizes[g] = fixedGap;
        }

        // Solve Stretch (both sizes and gaps together)
        var availableForStretch = finalMain - edgeMain - totalDefinedMain - totalFixedGap;
        DistributeStretchSpace(stretchItems, availableForStretch,
            (idx, v) =>
            {
                if (idx >= 0)
                    childMainSizes[idx] = v;
                else
                    gapSizes[-(idx + 1)] = v;
            });

        // Resolve derived cross sizes
        ResolveDerivedCrossSizes(flexChildren, ctx, layoutType, childMainSizes, childCrossSizes, finalMain, finalCross,
            edgeCross);

        // -------------------------------------------------------------
        // PHASE 3: Positioning (Set Bounds)
        // -------------------------------------------------------------

        var (startMain, startCross) = ToAxis(border.Left + padding.Left, border.Top + padding.Top);
        
        // Calculate total main size of all flex children (for main-axis alignment)
        var totalChildMain = flexChildren.Select((_, i) => childMainSizes[i] ?? 0).Sum();
        var totalGapsMain = gapSizes.Sum();
        var mainSum = totalChildMain + totalGapsMain;
        
        // Main-axis alignment offset
        var mainAlignOff = GetMainAlignmentOffset(node.Alignment ?? Alignment.TopLeft, 
            finalMain - edgeMain, mainSum, isRow);
        var currentMainPos = startMain + mainAlignOff;

        // Flex Children
        for (var i = 0; i < flexChildren.Count; i++)
        {
            var child = flexChildren[i];
            var sizeM = childMainSizes[i] ?? 0;
            var sizeC = childCrossSizes[i] ?? 0;

            var alignOff = GetCrossAlignmentOffset(node.Alignment ?? Alignment.TopLeft, 
                finalCross - edgeCross, sizeC, isRow);
            var posCross = startCross + alignOff;

            var (x, y) = FromAxis(currentMainPos, posCross);
            var (w, h) = FromAxis(sizeM, sizeC);

            ctx.Cache.SetBounds(child.Key, x, y, w, h);
            LayoutNodeRecursive(child, ctx, null, null, w, h);

            currentMainPos += sizeM;
            if (i < flexChildren.Count - 1) currentMainPos += gapSizes[i];
        }

        // Absolute Children
        var (nodeW, nodeH) = FromAxis(finalMain, finalCross);
        foreach (var abs in absoluteChildren)
        {
            LayoutAbsoluteChild(abs, node, ctx, nodeW, nodeH);
        }

        // Final Bounds (unless overridden by parent recursion)
        if (!overrideWidth.HasValue && !overrideHeight.HasValue)
        {
            var current = ctx.Cache.Bounds(key);
            ctx.Cache.SetBounds(key, current?.PosX ?? 0, current?.PosY ?? 0, nodeW, nodeH);
        }
    }

    // -------------------------------------------------------------------------
    // Helper Methods & Logic Extraction
    // -------------------------------------------------------------------------

    private static void ResolveDerivedCrossSizes<TTree, TSubLayout, TCacheKey, TCache>(
        List<INode<TTree, TSubLayout, TCacheKey>> children,
        LayoutContext<TTree, TSubLayout, TCacheKey, TCache> ctx,
        LayoutType type,
        float?[] mainSizes,
        float?[] crossSizes,
        float containerMain,
        float containerCross,
        float edgeCross)
        where TCache : ICache<TCacheKey>
        where TCacheKey : notnull
    {
        for (var i = 0; i < children.Count; i++)
        {
            var child = children[i];
            var knownM = mainSizes[i];
            var knownC = crossSizes[i];
            var availableCross = containerCross - edgeCross;

            // Check if MinCross is explicitly Auto (content-based minimum)
            var minCrossIsAuto = child.MinCross(type).Kind == UnitsKind.Auto;

            // Case 1: Stretch Cross - need content size if MinCross is Auto
            if (!knownC.HasValue && child.Cross(type).Kind is UnitsKind.Stretch)
            {
                float min;
                if (minCrossIsAuto)
                {
                    // Compute content size for Auto minimum
                    var content = child.ContentSizing(ref ctx.SubLayout, type, knownM, null);
                    if (!content.HasValue)
                    {
                        var computed = ComputeChildContentSize(child, ctx, type, containerMain, containerCross);
                        content = (computed.Main, computed.Cross);
                    }
                    min = content?.Cross ?? 0;
                }
                else
                {
                    min = child.MinCross(type).ToPx(containerCross, 0);
                }
                
                var max = child.MaxCross(type).ToPx(containerCross, float.MaxValue);
                crossSizes[i] = Math.Clamp(availableCross, min, max);
                knownC = crossSizes[i]; // Update local for use in Case 2
            }

            // Case 2: Content Sizing needed
            if (!knownC.HasValue || !knownM.HasValue)
            {
                var content = child.ContentSizing(ref ctx.SubLayout, type, knownM, knownC);

                if (!content.HasValue)
                {
                    var computed = ComputeChildContentSize(child, ctx, type, containerMain, containerCross);
                    content = (computed.Main, computed.Cross);
                }

                if (content.HasValue)
                {
                    if (!knownC.HasValue)
                    {
                        var min = child.MinCross(type).ToPx(containerCross, 0);
                        var max = child.MaxCross(type).ToPx(containerCross, float.MaxValue);
                        crossSizes[i] = Math.Clamp(content.Value.Cross, min, max);
                    }

                    if (!knownM.HasValue)
                    {
                        var min = child.MinMain(type).ToPx(containerMain, 0);
                        var max = child.MaxMain(type).ToPx(containerMain, float.MaxValue);
                        mainSizes[i] = Math.Clamp(content.Value.Main, min, max);
                    }
                }
            }

            // Fallback
            if (!crossSizes[i].HasValue)
            {
                var min = child.MinCross(type).ToPx(containerCross, 0);
                var max = child.MaxCross(type).ToPx(containerCross, float.MaxValue);
                crossSizes[i] = Math.Clamp(availableCross, min, max);
            }
        }
    }


    private static float DetermineFinalSize(float proposed, Units? min, Units? max, float parentBase)
    {
        var mn = min?.ToPx(parentBase, 0) ?? 0;
        var mx = max?.ToPx(parentBase, float.MaxValue) ?? float.MaxValue;
        return Math.Clamp(proposed, mn, mx);
    }

    private static float GetAlignmentOffset(Alignment align, float availableSpace, float childSize)
    {
        var factor = align switch
        {
            Alignment.Center or Alignment.TopCenter or Alignment.BottomCenter => 0.5f,
            Alignment.Right or Alignment.TopRight or Alignment.BottomRight => 1.0f,
            _ => 0.0f
        };
        return factor * (availableSpace - childSize);
    }
    
    private static float GetMainAlignmentOffset(Alignment align, float availableSpace, float totalChildrenSize, bool isRow)
    {
        // For Row: main axis is horizontal, alignment horizontal component matters
        // For Column: main axis is vertical, alignment vertical component matters
        var (mainFactor, _) = GetAlignmentFactors(align);
        if (isRow)
        {
            // For Row, swap because our factors are (vertical, horizontal)
            (_, mainFactor) = GetAlignmentFactors(align);
        }
        return mainFactor * (availableSpace - totalChildrenSize);
    }
    
    private static float GetCrossAlignmentOffset(Alignment align, float availableSpace, float childSize, bool isRow)
    {
        // For Row: cross axis is vertical
        // For Column: cross axis is horizontal
        var (crossFactor, _) = GetAlignmentFactors(align);
        if (!isRow)
        {
            // For Column, cross is horizontal, so use horizontal factor
            (_, crossFactor) = GetAlignmentFactors(align);
        }
        return crossFactor * (availableSpace - childSize);
    }
    
    private static (float Vertical, float Horizontal) GetAlignmentFactors(Alignment align)
    {
        return align switch
        {
            Alignment.TopLeft => (0.0f, 0.0f),
            Alignment.TopCenter => (0.0f, 0.5f),
            Alignment.TopRight => (0.0f, 1.0f),
            Alignment.Left => (0.5f, 0.0f),
            Alignment.Center => (0.5f, 0.5f),
            Alignment.Right => (0.5f, 1.0f),
            Alignment.BottomLeft => (1.0f, 0.0f),
            Alignment.BottomCenter => (1.0f, 0.5f),
            Alignment.BottomRight => (1.0f, 1.0f),
            _ => (0.0f, 0.0f)
        };
    }

    private static void LayoutAbsoluteChild<TTree, TSubLayout, TCacheKey, TCache>(
        INode<TTree, TSubLayout, TCacheKey> child,
        INode<TTree, TSubLayout, TCacheKey> parent,
        LayoutContext<TTree, TSubLayout, TCacheKey, TCache> ctx,
        float parentW, float parentH)
        where TCache : ICache<TCacheKey>
        where TCacheKey : notnull
    {
        // Compute parent's inner dimensions (for stretch sizing)
        var parentPadding = ResolveEdges(parent, parentW, parentH, isBorder: false);
        var parentBorder = ResolveEdges(parent, parentW, parentH, isBorder: true);
        var innerW = parentW - parentPadding.Horizontal - parentBorder.Horizontal;
        var innerH = parentH - parentPadding.Vertical - parentBorder.Vertical;
        
        // Content area for positioning (inside border, includes padding)
        var contentW = parentW - parentBorder.Horizontal;
        var contentH = parentH - parentBorder.Vertical;

        // 1. Resolve Edges (relative to content area for positioning)
        var leftU = child.Left ?? Units.Auto;
        var rightU = child.Right ?? Units.Auto;
        var topU = child.Top ?? Units.Auto;
        var bottomU = child.Bottom ?? Units.Auto;

        var left = leftU.ToPx(contentW, 0);
        var right = rightU.ToPx(contentW, 0);
        var top = topU.ToPx(contentH, 0);
        var bottom = bottomU.ToPx(contentH, 0);

        // Check if we need content sizing for Auto min constraints
        // Note: null MinWidth/MinHeight means "not set" (defaults to 0), not Auto
        var minWidthIsAuto = child.MinWidth?.Kind == UnitsKind.Auto;
        var minHeightIsAuto = child.MinHeight?.Kind == UnitsKind.Auto;
        var widthIsStretch = (child.Width ?? Units.Auto).Kind is UnitsKind.Stretch;
        var heightIsStretch = (child.Height ?? Units.Auto).Kind is UnitsKind.Stretch;

        // Pre-compute content size if needed for Auto min constraints
        float? childContentW = null;
        float? childContentH = null;
        if ((minWidthIsAuto && widthIsStretch) || (minHeightIsAuto && heightIsStretch) || 
            (child.Width ?? Units.Auto).Kind is UnitsKind.Auto || 
            (child.Height ?? Units.Auto).Kind is UnitsKind.Auto)
        {
            var lType = child.LayoutType ?? LayoutType.Column;
            var content = ComputeChildContentSize(child, ctx, lType, innerW, innerH);

            (childContentW, childContentH) = lType is LayoutType.Row
                ? (content.Main, content.Cross)
                : (content.Cross, content.Main);
        }

        // 2. Resolve Size using INNER dimensions for stretch, with content-based minimums for Auto
        var w = ResolveAbsoluteSizeWithContentMin(child.Width, child.MinWidth, child.MaxWidth, innerW, left, right, leftU.IsAuto,
            rightU.IsAuto, childContentW);
        var h = ResolveAbsoluteSizeWithContentMin(child.Height, child.MinHeight, child.MaxHeight, innerH, top, bottom, topU.IsAuto,
            bottomU.IsAuto, childContentH);

        // 3. Fallback to Content Sizing if still unknown
        w ??= childContentW ?? 0;
        h ??= childContentH ?? 0;

        // 4. Clamp & Position (use content-based min for Auto)
        var finalMinW = minWidthIsAuto ? (childContentW ?? 0) : (child.MinWidth?.ToPx(innerW, 0) ?? 0);
        var finalMinH = minHeightIsAuto ? (childContentH ?? 0) : (child.MinHeight?.ToPx(innerH, 0) ?? 0);
        var finalMaxW = child.MaxWidth?.ToPx(innerW, float.MaxValue) ?? float.MaxValue;
        var finalMaxH = child.MaxHeight?.ToPx(innerH, float.MaxValue) ?? float.MaxValue;

        var finalW = Math.Clamp(w.Value, finalMinW, finalMaxW);
        var finalH = Math.Clamp(h.Value, finalMinH, finalMaxH);

        // Position within content area (use contentW/contentH which includes padding)
        var x = (leftU.IsAuto, rightU.IsAuto) switch
        {
            (false, _) => left + parentBorder.Left,
            (true, false) => contentW - right - finalW + parentBorder.Left,
            _ => parentBorder.Left
        };

        var y = (topU.IsAuto, bottomU.IsAuto) switch
        {
            (false, _) => top + parentBorder.Top,
            (true, false) => contentH - bottom - finalH + parentBorder.Top,
            _ => parentBorder.Top
        };

        ctx.Cache.SetBounds(child.Key, x, y, finalW, finalH);
        LayoutNodeRecursive(child, ctx, null, null, finalW, finalH);
    }

    private static AxisValues ComputeChildContentSize<TTree, TSubLayout, TCacheKey, TCache>(
        INode<TTree, TSubLayout, TCacheKey> child,
        LayoutContext<TTree, TSubLayout, TCacheKey, TCache> ctx,
        LayoutType parentLayout,
        float parentMain,
        float parentCross)
        where TCache : ICache<TCacheKey>
        where TCacheKey : notnull
    {
        var childLayout = child.LayoutType ?? LayoutType.Column;
        var isRow = parentLayout is LayoutType.Row;

        var (pW, pH) = isRow ? (parentMain, parentCross) : (parentCross, parentMain);

        // 1. Try Explicit (including Stretch which fills the parent)
        var w = ResolveSize(child.Width, child.MinWidth, child.MaxWidth, pW);
        var h = ResolveSize(child.Height, child.MinHeight, child.MaxHeight, pH);

        // For content sizing, Stretch should resolve to parent size (clamped by min/max)
        // But only if min is not Auto (Auto min needs content measurement first)
        var minWIsAuto = child.MinWidth?.Kind == UnitsKind.Auto;
        var minHIsAuto = child.MinHeight?.Kind == UnitsKind.Auto;
        if (!w.HasValue && (child.Width ?? Units.Auto).Kind is UnitsKind.Stretch && !minWIsAuto)
            w = DetermineFinalSize(pW, child.MinWidth, child.MaxWidth, pW);
        if (!h.HasValue && (child.Height ?? Units.Auto).Kind is UnitsKind.Stretch && !minHIsAuto)
            h = DetermineFinalSize(pH, child.MinHeight, child.MaxHeight, pH);

        if (w is not null && h is not null)
            return isRow ? new(w.Value, h.Value) : new(h.Value, w.Value);

        // 2. Try Native Content Sizing
        var childIsRow = childLayout is LayoutType.Row;
        var (childMainHint, childCrossHint) = childIsRow ? (w, h) : (h, w);

        var isStretchW = (child.Width ?? Units.Auto).Kind is UnitsKind.Stretch;
        var isStretchH = (child.Height ?? Units.Auto).Kind is UnitsKind.Stretch;
        
        // For stretch hints, apply max constraints even if min is Auto
        var hintW = w;
        if (!hintW.HasValue && isStretchW)
        {
            var maxW = child.MaxWidth?.ToPx(pW, float.MaxValue) ?? float.MaxValue;
            hintW = Math.Min(pW, maxW);
        }
        
        var hintH = h;
        if (!hintH.HasValue && isStretchH)
        {
            var maxH = child.MaxHeight?.ToPx(pH, float.MaxValue) ?? float.MaxValue;
            hintH = Math.Min(pH, maxH);
        }
        
        var (contentMainHint, contentCrossHint) = childIsRow ? (hintW, hintH) : (hintH, hintW);

        var content = child.ContentSizing(ref ctx.SubLayout, childLayout, contentMainHint, contentCrossHint);
        if (content.HasValue)
        {
            var cm = content.Value.Main;
            var cc = content.Value.Cross;
            return isRow // Map child orientation back to parent orientation
                ? childIsRow ? new AxisValues(cm, cc) : new(cc, cm)
                : childIsRow ? new AxisValues(cc, cm) : new(cm, cc);
        }

        // 3. Recursive Children Measurement
        var grandchildren = child.Children(ctx.Tree)
            .Where(gc => gc.Visible && (gc.PositionType ?? PositionType.Relative) is PositionType.Relative)
            .ToList();

        if (grandchildren.Count == 0)
            return isRow ? new(w ?? 0, h ?? 0) : new(h ?? 0, w ?? 0);

        float totalMain = 0;
        float maxCross = 0;
        
        var (gcRefW, gcRefH) = (w ?? (isStretchW ? pW : 0), h ?? (isStretchH ? pH : 0));

        foreach (var gc in grandchildren)
        {
            var gcSize = ComputeChildContentSize(gc, ctx, childLayout,
                childIsRow ? gcRefW : gcRefH,
                childIsRow ? gcRefH : gcRefW);

            totalMain += gcSize.Main;
            maxCross = Math.Max(maxCross, gcSize.Cross);
        }

        // Add Gaps
        var gap = (childIsRow ? child.HorizontalGap ?? default : child.VerticalGap ?? default)
            .ToPx(childIsRow ? gcRefW : gcRefH, 0);
        totalMain += gap * Math.Max(0, grandchildren.Count - 1);

        // Add Padding/Border
        var border = ResolveEdges(child, gcRefW, gcRefH, true);
        var padding = ResolveEdges(child, gcRefW, gcRefH, false);

        var mainSpace = childIsRow ? border.Horizontal + padding.Horizontal : border.Vertical + padding.Vertical;
        var crossSpace = childIsRow ? border.Vertical + padding.Vertical : border.Horizontal + padding.Horizontal;

        var finalMain = childMainHint ?? totalMain + mainSpace;
        var finalCross = childCrossHint ?? maxCross + crossSpace;

        // Map back to parent orientation
        var (finalW, finalH) = childIsRow ? (finalMain, finalCross) : (finalCross, finalMain);

        finalW = DetermineFinalSize(finalW, child.MinWidth, child.MaxWidth, pW);
        finalH = DetermineFinalSize(finalH, child.MinHeight, child.MaxHeight, pH);

        return isRow ? new(finalW, finalH) : new(finalH, finalW);
    }

    private static float? ResolveSize(Units? unit, Units? min, Units? max, float? parentBase)
    {
        var val = ResolveUnit(unit ?? Units.Auto, parentBase);
        if (!val.HasValue) return null;
        return DetermineFinalSize(val.Value, min, max, parentBase ?? 0);
    }

    private static float? ResolveUnit(Units u, float? parentBase) => u.Kind switch
    {
        UnitsKind.Pixels => u.Value,
        UnitsKind.Percentage when parentBase.HasValue => parentBase.Value * u.Value / 100f,
        // Stretch is resolved later in ResolveDerivedCrossSizes/DistributeStretchSpace where we have inner dimensions
        _ => null
    };

    private static float? ResolveAbsoluteSize(
        Units? size, Units? min, Units? max,
        float parentSize, float start, float end,
        bool startIsAuto, bool endIsAuto)
    {
        var u = size ?? Units.Auto;
        float? val = u.Kind switch
        {
            UnitsKind.Pixels => u.Value,
            UnitsKind.Percentage => parentSize * u.Value / 100f,
            UnitsKind.Stretch => parentSize - start - end,
            _ => null
        };

        if (!val.HasValue && !startIsAuto && !endIsAuto) val = parentSize - start - end;

        return val.HasValue
            ? DetermineFinalSize(val.Value, min, max, parentSize)
            : null;
    }

    // Same as ResolveAbsoluteSize but applies content-based minimum when min is Auto
    private static float? ResolveAbsoluteSizeWithContentMin(
        Units? size, Units? min, Units? max,
        float parentSize, float start, float end,
        bool startIsAuto, bool endIsAuto,
        float? contentSize)
    {
        var u = size ?? Units.Auto;
        float? val = u.Kind switch
        {
            UnitsKind.Pixels => u.Value,
            UnitsKind.Percentage => parentSize * u.Value / 100f,
            UnitsKind.Stretch => parentSize - start - end,
            _ => null
        };

        if (!val.HasValue && !startIsAuto && !endIsAuto) val = parentSize - start - end;
        
        if (!val.HasValue) return null;

        // Apply content-based minimum when min is Auto
        var effectiveMin = min;
        if (min?.Kind == UnitsKind.Auto && contentSize.HasValue)
            effectiveMin = Units.Pixels(contentSize.Value);
        
        return DetermineFinalSize(val.Value, effectiveMin, max, parentSize);
    }

    private static void DistributeStretchSpace(
        List<StretchItem> items,
        float availableSpace,
        Action<int, float> setSize)
    {
        if (items.Count == 0) return;

        // If no positive space available, assign minimum sizes to all stretch items
        if (availableSpace <= 0)
        {
            foreach (var item in items) setSize(item.Index, item.Min);

            return;
        }

        var maxPasses = items.Count + 1;
        while (maxPasses-- > 0)
        {
            var activeItems = items.Where(i => !i.Frozen).ToList();
            var totalFactor = activeItems.Sum(i => i.Factor);

            if (totalFactor <= 0) break;

            var perFactor = availableSpace / totalFactor;
            var violationItem = (Index: -1, MaxViolation: 0f);

            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (item.Frozen) continue;

                var raw = item.Factor * perFactor;
                var clamped = Math.Clamp(raw, item.Min, item.Max);
                var violation = clamped - raw;

                if (Math.Abs(violation) > Math.Abs(violationItem.MaxViolation))
                    violationItem = (i, violation);

                items[i] = item with { ComputedSize = clamped };
            }

            if (violationItem.Index >= 0)
            {
                var idx = violationItem.Index;
                var vItem = items[idx];
                items[idx] = vItem with { Frozen = true };
                availableSpace -= vItem.ComputedSize;
            }
            else
            {
                foreach (var item in items)
                {
                    if (!item.Frozen) Apply(item, item.Factor * perFactor);
                    else Apply(item, item.ComputedSize);
                }

                return;
            }
        }

        foreach (var item in items) Apply(item, item.ComputedSize);

        return;

        void Apply(StretchItem item, float v)
        {
            // Both Size and Gap items are applied via setSize callback
            // (Size uses positive indices, Gap uses negative indices)
            setSize(item.Index, v);
        }
    }

    // -------------------------------------------------------------------------
    // Grid Layout
    // -------------------------------------------------------------------------

    private static void LayoutGridNode<TTree, TSubLayout, TCacheKey, TCache>(
        INode<TTree, TSubLayout, TCacheKey> node,
        LayoutContext<TTree, TSubLayout, TCacheKey, TCache> ctx,
        float? parentWidth,
        float? parentHeight,
        float? overrideWidth,
        float? overrideHeight)
        where TCache : ICache<TCacheKey>
        where TCacheKey : notnull
    {
        var key = node.Key;

        // Resolve grid outer size
        var explicitW = overrideWidth ?? ResolveSize(node.Width, node.MinWidth, node.MaxWidth, parentWidth);
        var explicitH = overrideHeight ?? ResolveSize(node.Height, node.MinHeight, node.MaxHeight, parentHeight);

        var baseW = explicitW ?? parentWidth ?? 0;
        var baseH = explicitH ?? parentHeight ?? 0;

        var padding = ResolveEdges(node, baseW, baseH, isBorder: false);
        var border = ResolveEdges(node, baseW, baseH, isBorder: true);

        var totalEdgeH = padding.Horizontal + border.Horizontal;
        var totalEdgeV = padding.Vertical + border.Vertical;

        var gridW = explicitW ?? baseW;
        var gridH = explicitH ?? baseH;

        // Set grid bounds
        var curBounds = ctx.Cache.Bounds(key);
        ctx.Cache.SetBounds(key, curBounds?.PosX ?? 0, curBounds?.PosY ?? 0, gridW, gridH);

        // Inner dimensions for children
        var innerW = Math.Max(0, gridW - totalEdgeH);
        var innerH = Math.Max(0, gridH - totalEdgeV);

        // Get grid definitions
        var gridCols = node.GridColumns ?? [];
        var gridRows = node.GridRows ?? [];

        if (gridCols.Count == 0 || gridRows.Count == 0)
            return; // No grid to layout

        // Get gaps
        var colGap = (node.HorizontalGap ?? default).ToPx(innerW, 0);
        var rowGap = (node.VerticalGap ?? default).ToPx(innerH, 0);

        // Compute column sizes with interleaved gutters
        // Array structure: [gap0, col0, gap1, col1, gap2, col2, gap3]
        // For n columns: 2*n + 1 elements (but first and last gap are 0)
        var computedCols = ComputeGridTrackSizes(gridCols, innerW, colGap);
        var computedRows = ComputeGridTrackSizes(gridRows, innerH, rowGap);

        // Convert to cumulative positions
        ConvertToCumulativePositions(computedCols);
        ConvertToCumulativePositions(computedRows);

        // Layout children
        var allChildren = node.Children(ctx.Tree);
        var offsetX = padding.Left + border.Left;
        var offsetY = padding.Top + border.Top;

        foreach (var child in allChildren)
        {
            if (!child.Visible)
            {
                SetInvisibleRecursive(child, ctx);
                continue;
            }

            // Get child's grid placement (0-indexed in our API)
            var colStart = Math.Max(0, child.ColumnStart ?? 0);
            var rowStart = Math.Max(0, child.RowStart ?? 0);
            var colSpan = Math.Max(1, child.ColumnSpan ?? 1);
            var rowSpan = Math.Max(1, child.RowSpan ?? 1);

            // Clamp to valid range
            colStart = Math.Min(colStart, gridCols.Count - 1);
            rowStart = Math.Min(rowStart, gridRows.Count - 1);
            var colEnd = Math.Min(colStart + colSpan, gridCols.Count);
            var rowEnd = Math.Min(rowStart + rowSpan, gridRows.Count);

            // Get position and size from computed tracks
            // Array structure after cumulative: [pos_before_gutter, pos_at_track1_start, pos_at_track2_start, ...]
            // Position uses col_start * 2 + 1 (track start position)
            // Width uses col_end * 2 (track end position, which is next gutter start)
            var cellX = computedCols[colStart * 2 + 1];
            var cellW = computedCols[colEnd * 2] - cellX;

            var cellY = computedRows[rowStart * 2 + 1];
            var cellH = computedRows[rowEnd * 2] - cellY;

            // Layout child with cell as available space (but don't set overrides, let child compute its own size)
            LayoutNodeRecursive(child, ctx, cellW, cellH);

            // Set child position and size to fill the cell
            ctx.Cache.SetBounds(child.Key, offsetX + cellX, offsetY + cellY, cellW, cellH);
        }
    }

    private static float[] ComputeGridTrackSizes(IReadOnlyList<Units> tracks, float availableSpace, float gap)
    {
        // Structure: [gutter, track, gutter, track, ..., gutter]
        // 2 * n + 1 elements for n tracks
        var count = tracks.Count * 2 + 1;
        var computed = new float[count];

        // Collect stretch items and compute fixed sizes
        var stretchItems = new List<(int Index, float Factor)>();
        float totalFixed = 0;
        float totalStretch = 0;

        for (var i = 0; i < tracks.Count; i++)
        {
            var track = tracks[i];
            var arrayIdx = i * 2 + 1; // Track positions in array

            // Set gutters
            if (i > 0)
            {
                computed[i * 2] = gap; // Gutter before this track
                totalFixed += gap;
            }

            switch (track.Kind)
            {
                case UnitsKind.Pixels:
                    computed[arrayIdx] = track.Value;
                    totalFixed += track.Value;
                    break;
                case UnitsKind.Percentage:
                    var pxValue = track.Value / 100f * availableSpace;
                    computed[arrayIdx] = pxValue;
                    totalFixed += pxValue;
                    break;
                case UnitsKind.Stretch:
                    stretchItems.Add((arrayIdx, track.Value));
                    totalStretch += track.Value;
                    break;
                case UnitsKind.Auto:
                    // Auto in grid context defaults to stretch(1)
                    stretchItems.Add((arrayIdx, 1f));
                    totalStretch += 1f;
                    break;
            }
        }

        // Distribute remaining space to stretch items
        var remainingSpace = Math.Max(0, availableSpace - totalFixed);

        if (stretchItems.Count > 0 && totalStretch > 0)
        {
            var perFactor = remainingSpace / totalStretch;
            foreach (var (idx, factor) in stretchItems)
            {
                computed[idx] = factor * perFactor;
            }
        }

        return computed;
    }

    private static void ConvertToCumulativePositions(float[] sizes)
    {
        float cumulative = 0;
        for (var i = 0; i < sizes.Length; i++)
        {
            var size = sizes[i];
            sizes[i] = cumulative;
            cumulative += size;
        }
    }

    private static BoxEdges ResolveEdges<TTree, TSubLayout, TCacheKey>(
        INode<TTree, TSubLayout, TCacheKey> node, float w, float h, bool isBorder)
    {
        return isBorder
            ? new BoxEdges(
                Px(node.BorderLeft, w), Px(node.BorderTop, h),
                Px(node.BorderRight, w), Px(node.BorderBottom, h))
            : new BoxEdges(
                Px(node.PaddingLeft, w), Px(node.PaddingTop, h),
                Px(node.PaddingRight, w), Px(node.PaddingBottom, h));

        static float Px(Units? u, float dim) => (u ?? default).ToPx(dim, 0);
    }

    private static void SetInvisibleRecursive<TTree, TSubLayout, TCacheKey, TCache>(
        INode<TTree, TSubLayout, TCacheKey> node,
        LayoutContext<TTree, TSubLayout, TCacheKey, TCache> ctx)
        where TCache : ICache<TCacheKey>
        where TCacheKey : notnull
    {
        ctx.Cache.SetBounds(node.Key, 0, 0, 0, 0);
        foreach (var child in node.Children(ctx.Tree)) SetInvisibleRecursive(child, ctx);
    }
}