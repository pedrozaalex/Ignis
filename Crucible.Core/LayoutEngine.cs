using Crucible.Core.Extensions;
using Crucible.Core.Interfaces;
using Crucible.Core.Types;

namespace Crucible.Core;

public static class LayoutEngine
{
    // -------------------------------------------------------------------------
    // Internal State Types
    // -------------------------------------------------------------------------

    private enum StretchType
    {
        Size
    }

    private record struct StretchItem(
        int Index,
        StretchType Type,
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

        // 2. Fast Path: Grid (Pass-through sizing)
        if (layoutType is LayoutType.Grid)
        {
            var gridW = overrideWidth ?? parentWidth ?? 0;
            var gridH = overrideHeight ?? parentHeight ?? 0;
            var cur = ctx.Cache.Bounds(key);
            ctx.Cache.SetBounds(key, cur?.PosX ?? 0, cur?.PosY ?? 0, gridW, gridH);
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

            var minMain = child.MinMain(layoutType).ToPx(knownMain ?? 0, 0);
            var maxMain = child.MaxMain(layoutType).ToPx(knownMain ?? 0, float.MaxValue);
            var minCross = child.MinCross(layoutType).ToPx(knownCross ?? 0, 0);
            var maxCross = child.MaxCross(layoutType).ToPx(knownCross ?? 0, float.MaxValue);

            // Calculate Initial Sizes (if not auto)
            float? mainSize = ResolveUnit(cMain, knownMain);
            if (mainSize.HasValue) mainSize = Math.Clamp(mainSize.Value, minMain, maxMain);

            float? crossSize = ResolveUnit(cCross, knownCross);
            if (crossSize.HasValue) crossSize = Math.Clamp(crossSize.Value, minCross, maxCross);

            // Recursive Measurement if needed
            var needMain = cMain.Kind is not UnitsKind.Stretch && !mainSize.HasValue;
            var needCross = cCross.Kind is UnitsKind.Auto && !crossSize.HasValue;

            // If Main is Stretch, we can't reliably resolve Auto Cross derived from it yet 
            // because the Stretch distribution hasn't happened.
            if (cMain.Kind is UnitsKind.Stretch)
            {
                needMain = false;
                needCross = false;
            }

            if (needMain || needCross)
            {
                var contentSize = ComputeChildContentSize(child, ctx, layoutType, knownMain ?? 0, knownCross ?? 0);
                if (needMain) mainSize = Math.Clamp(contentSize.Main, minMain, maxMain);
                if (needCross) crossSize = Math.Clamp(contentSize.Cross, minCross, maxCross);
            }

            // Register Stretch or Accumulate Size
            if (cMain.Kind is UnitsKind.Stretch)
                stretchItems.Add(new StretchItem(i, StretchType.Size, Math.Max(cMain.Value, 1f), minMain, maxMain));
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

        // Solve Stretch
        var availableForStretch = finalMain - edgeMain - totalDefinedMain - totalFixedGap;
        DistributeStretchSpace(stretchItems, availableForStretch,
            (idx, v) => childMainSizes[idx] = v);

        // Resolve derived cross sizes
        ResolveDerivedCrossSizes(flexChildren, ctx, layoutType, childMainSizes, childCrossSizes, finalMain, finalCross,
            edgeCross);

        // Calculate Stretch Gap
        var finalGap = fixedGap;
        if (isStretchGap && flexChildren.Count > 1)
        {
            var usedSpace = flexChildren.Select((_, i) => childMainSizes[i] ?? 0).Sum();
            finalGap = Math.Max(0, (finalMain - edgeMain - usedSpace) / (flexChildren.Count - 1));
        }

        // -------------------------------------------------------------
        // PHASE 3: Positioning (Set Bounds)
        // -------------------------------------------------------------

        var (startMain, startCross) = ToAxis(border.Left + padding.Left, border.Top + padding.Top);
        var currentMainPos = startMain;

        // Flex Children
        for (var i = 0; i < flexChildren.Count; i++)
        {
            var child = flexChildren[i];
            var sizeM = childMainSizes[i] ?? 0;
            var sizeC = childCrossSizes[i] ?? 0;

            var alignOff = GetAlignmentOffset(node.Alignment ?? Alignment.TopLeft, finalCross - edgeCross, sizeC);
            var posCross = startCross + alignOff;

            var (x, y) = FromAxis(currentMainPos, posCross);
            var (w, h) = FromAxis(sizeM, sizeC);

            ctx.Cache.SetBounds(child.Key, x, y, w, h);
            LayoutNodeRecursive(child, ctx, null, null, w, h);

            currentMainPos += sizeM;
            if (i < flexChildren.Count - 1) currentMainPos += finalGap;
        }

        // Absolute Children
        var (nodeW, nodeH) = FromAxis(finalMain, finalCross);
        foreach (var abs in absoluteChildren)
        {
            LayoutAbsoluteChild(abs, ctx, nodeW, nodeH);
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

            // Case 1: Stretch Cross
            if (!knownC.HasValue && child.Cross(type).Kind is UnitsKind.Stretch)
            {
                var min = child.MinCross(type).ToPx(containerCross, 0);
                var max = child.MaxCross(type).ToPx(containerCross, float.MaxValue);
                crossSizes[i] = Math.Clamp(availableCross, min, max);
                knownC = crossSizes[i]; // Update local for use in Case 2
            }

            // Case 2: Content Sizing needed
            if (!knownC.HasValue || !knownM.HasValue)
            {
                var content = child.ContentSizing(ref ctx.SubLayout, type, knownM, knownC);

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

    private static void LayoutAbsoluteChild<TTree, TSubLayout, TCacheKey, TCache>(
        INode<TTree, TSubLayout, TCacheKey> child,
        LayoutContext<TTree, TSubLayout, TCacheKey, TCache> ctx,
        float parentW, float parentH)
        where TCache : ICache<TCacheKey>
        where TCacheKey : notnull
    {
        // 1. Resolve Edges
        var leftU = child.Left ?? Units.Auto;
        var rightU = child.Right ?? Units.Auto;
        var topU = child.Top ?? Units.Auto;
        var bottomU = child.Bottom ?? Units.Auto;

        var left = leftU.ToPx(parentW, 0);
        var right = rightU.ToPx(parentW, 0);
        var top = topU.ToPx(parentH, 0);
        var bottom = bottomU.ToPx(parentH, 0);

        // 2. Resolve Size
        var w = ResolveAbsoluteSize(child.Width, child.MinWidth, child.MaxWidth, parentW, left, right, leftU.IsAuto,
            rightU.IsAuto);
        var h = ResolveAbsoluteSize(child.Height, child.MinHeight, child.MaxHeight, parentH, top, bottom, topU.IsAuto,
            bottomU.IsAuto);

        // 3. Fallback to Content Sizing if still unknown
        if (w == null || h == null)
        {
            var lType = child.LayoutType ?? LayoutType.Column;
            var content = ComputeChildContentSize(child, ctx, lType, parentW, parentH);

            var (measW, measH) = lType is LayoutType.Row
                ? (content.Main, content.Cross)
                : (content.Cross, content.Main);

            w ??= measW;
            h ??= measH;
        }

        // 4. Clamp & Position
        var finalW = DetermineFinalSize(w.Value, child.MinWidth, child.MaxWidth, parentW);
        var finalH = DetermineFinalSize(h.Value, child.MinHeight, child.MaxHeight, parentH);

        var x = (leftU.IsAuto, rightU.IsAuto) switch
        {
            (false, _) => left,
            (true, false) => parentW - right - finalW,
            _ => 0f
        };

        var y = (topU.IsAuto, bottomU.IsAuto) switch
        {
            (false, _) => top,
            (true, false) => parentH - bottom - finalH,
            _ => 0f
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
        if (!w.HasValue && (child.Width ?? Units.Auto).Kind is UnitsKind.Stretch)
            w = DetermineFinalSize(pW, child.MinWidth, child.MaxWidth, pW);
        if (!h.HasValue && (child.Height ?? Units.Auto).Kind is UnitsKind.Stretch)
            h = DetermineFinalSize(pH, child.MinHeight, child.MaxHeight, pH);

        if (w is not null && h is not null)
            return isRow ? new(w.Value, h.Value) : new(h.Value, w.Value);

        // 2. Try Native Content Sizing
        var childIsRow = childLayout is LayoutType.Row;
        var (childMainHint, childCrossHint) = childIsRow ? (w, h) : (h, w);

        var content = child.ContentSizing(ref ctx.SubLayout, childLayout, childMainHint, childCrossHint);
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
        var (gcRefW, gcRefH) = (w ?? pW, h ?? pH);

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
            if (item.Type == StretchType.Size)
                setSize(item.Index, v);
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