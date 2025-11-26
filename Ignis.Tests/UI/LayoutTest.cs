using Ignis.Engine.UI;

namespace Ignis.Tests.UI;

// --- ECS Implementation for Tests ---
public class TestNode
{
    public Alignment Alignment = Alignment.TopLeft;
    public Units BorderBottom = Units.Pixels(0);

    public Units BorderLeft = Units.Pixels(0);
    public Units BorderRight = Units.Pixels(0);
    public Units BorderTop = Units.Pixels(0);
    public Units Bottom = Units.Auto;
    public List<int> Children = [];
    public Units ColGap = Units.Pixels(0);
    public int ColumnSpan = 1;
    public int ColumnStart;

    public Func<float?, float?, (float, float)>? ContentMeasurer;
    public List<Units> GridColumns = [];

    public List<Units> GridRows = [];
    public Units Height = Units.Auto;
    public int Id;
    public LayoutType LayoutType = LayoutType.Column;

    public Units Left = Units.Auto;
    public Units MaxHeight = Units.Auto;
    public Units MaxWidth = Units.Auto;
    public Units MinHeight = Units.Auto;
    public Units MinWidth = Units.Auto;
    public Units PadBottom = Units.Pixels(0);

    public Units PadLeft = Units.Pixels(0);
    public Units PadRight = Units.Pixels(0);
    public Units PadTop = Units.Pixels(0);
    public int? Parent;
    public PositionType PositionType = PositionType.Relative;
    public Units Right = Units.Auto;

    public Units RowGap = Units.Pixels(0);
    public int RowSpan = 1;
    public int RowStart;
    public Units Top = Units.Auto;
    public bool Visible = true;

    public Units Width = Units.Auto;
}

public class World : ILayoutNode, ILayoutCache
{
    private int _nextId;
    public Dictionary<int, Rect> Bounds = new();
    public Dictionary<int, TestNode> Nodes = new();

    // ILayoutCache Implementation
    public void SetBounds(object node, float x, float y, float w, float h)
    {
        Bounds[(int)node] = new Rect { PosX = x, PosY = y, Width = w, Height = h };
    }

    float ILayoutCache.GetWidth(object node)
    {
        return Bounds.TryGetValue((int)node, out var r) ? r.Width : 0;
    }

    float ILayoutCache.GetHeight(object node)
    {
        return Bounds.TryGetValue((int)node, out var r) ? r.Height : 0;
    }

    float ILayoutCache.GetPosX(object node)
    {
        return Bounds.TryGetValue((int)node, out var r) ? r.PosX : 0;
    }

    float ILayoutCache.GetPosY(object node)
    {
        return Bounds.TryGetValue((int)node, out var r) ? r.PosY : 0;
    }

    // ILayoutNode Implementation
    public IEnumerable<object> GetChildren(object node)
    {
        return Nodes[(int)node].Children.Cast<object>();
    }

    public bool IsVisible(object node)
    {
        return Nodes[(int)node].Visible;
    }

    public LayoutType GetLayoutType(object node)
    {
        return Nodes[(int)node].LayoutType;
    }

    public PositionType GetPositionType(object node)
    {
        return Nodes[(int)node].PositionType;
    }

    public Alignment GetAlignment(object node)
    {
        return Nodes[(int)node].Alignment;
    }

    public Units GetWidth(object node)
    {
        return Nodes[(int)node].Width;
    }

    public Units GetHeight(object node)
    {
        return Nodes[(int)node].Height;
    }

    public Units GetMinWidth(object node)
    {
        return Nodes[(int)node].MinWidth;
    }

    public Units GetMinHeight(object node)
    {
        return Nodes[(int)node].MinHeight;
    }

    public Units GetMaxWidth(object node)
    {
        return Nodes[(int)node].MaxWidth;
    }

    public Units GetMaxHeight(object node)
    {
        return Nodes[(int)node].MaxHeight;
    }

    public Units GetLeft(object node)
    {
        return Nodes[(int)node].Left;
    }

    public Units GetRight(object node)
    {
        return Nodes[(int)node].Right;
    }

    public Units GetTop(object node)
    {
        return Nodes[(int)node].Top;
    }

    public Units GetBottom(object node)
    {
        return Nodes[(int)node].Bottom;
    }

    public Units GetPaddingLeft(object node)
    {
        return Nodes[(int)node].PadLeft;
    }

    public Units GetPaddingRight(object node)
    {
        return Nodes[(int)node].PadRight;
    }

    public Units GetPaddingTop(object node)
    {
        return Nodes[(int)node].PadTop;
    }

    public Units GetPaddingBottom(object node)
    {
        return Nodes[(int)node].PadBottom;
    }

    public Units GetBorderLeft(object node)
    {
        return Nodes[(int)node].BorderLeft;
    }

    public Units GetBorderRight(object node)
    {
        return Nodes[(int)node].BorderRight;
    }

    public Units GetBorderTop(object node)
    {
        return Nodes[(int)node].BorderTop;
    }

    public Units GetBorderBottom(object node)
    {
        return Nodes[(int)node].BorderBottom;
    }

    public Units GetChildLeft(object node)
    {
        return Units.Auto;
    }

    public Units GetChildTop(object node)
    {
        return Units.Auto;
    }

    public Units GetRowGap(object node)
    {
        return Nodes[(int)node].RowGap;
    }

    public Units GetColumnGap(object node)
    {
        return Nodes[(int)node].ColGap;
    }

    public List<Units> GetGridRows(object node)
    {
        return Nodes[(int)node].GridRows;
    }

    public List<Units> GetGridColumns(object node)
    {
        return Nodes[(int)node].GridColumns;
    }

    public int GetRowStart(object node)
    {
        return Nodes[(int)node].RowStart;
    }

    public int GetRowSpan(object node)
    {
        return Nodes[(int)node].RowSpan;
    }

    public int GetColumnStart(object node)
    {
        return Nodes[(int)node].ColumnStart;
    }

    public int GetColumnSpan(object node)
    {
        return Nodes[(int)node].ColumnSpan;
    }

    public (float width, float height)? MeasureContent(object node, float? w, float? h)
    {
        var n = Nodes[(int)node];
        return n.ContentMeasurer?.Invoke(w, h);
    }

    public int Add(int? parent)
    {
        var id = _nextId++;
        var node = new TestNode { Id = id, Parent = parent };
        Nodes[id] = node;
        if (parent.HasValue) Nodes[parent.Value].Children.Add(id);
        return id;
    }

    public Rect? GetRect(int node)
    {
        return Bounds.ContainsKey(node) ? Bounds[node] : null;
    }
}

public class LayoutTests
{
    [Fact] // absolute.rs: absolute_pixels_width_pixels_height
    public void AbsolutePixels()
    {
        var w = new World();
        var root = w.Add(null);
        w.Nodes[root].Width = Units.Pixels(600);
        w.Nodes[root].Height = Units.Pixels(600);

        var node = w.Add(root);
        w.Nodes[node].Width = Units.Pixels(100);
        w.Nodes[node].Height = Units.Pixels(100);
        w.Nodes[node].PositionType = PositionType.Absolute;

        LayoutEngine.Layout(root, w, w);

        var r = w.GetRect(node)!.Value;
        Assert.Equal(0, r.PosX);
        Assert.Equal(0, r.PosY);
        Assert.Equal(100, r.Width);
        Assert.Equal(100, r.Height);
    }

    [Fact] // auto.rs: auto_min_width
    public void AutoMinWidth()
    {
        var w = new World();
        var root = w.Add(null);
        w.Nodes[root].Width = Units.Pixels(600);
        w.Nodes[root].Height = Units.Pixels(600);
        w.Nodes[root].LayoutType = LayoutType.Row;

        var node = w.Add(root);
        w.Nodes[node].Width = Units.Auto;
        w.Nodes[node].Height = Units.Auto;

        var c1 = w.Add(node);
        w.Nodes[c1].Width = Units.Stretch(1);
        w.Nodes[c1].Height = Units.Pixels(50);
        w.Nodes[c1].ContentMeasurer = (wd, ht) => (50f, ht ?? 0);

        var c2 = w.Add(node);
        w.Nodes[c2].Width = Units.Stretch(1);
        w.Nodes[c2].Height = Units.Pixels(50);
        w.Nodes[c2].ContentMeasurer = (wd, ht) => (80f, ht ?? 0);

        LayoutEngine.Layout(root, w, w);

        var r = w.GetRect(node)!.Value;
        // Expect max of children widths (80) and sum of heights (100)
        Assert.Equal(80, r.Width);
        Assert.Equal(100, r.Height);
    }

    [Fact] // content_size.rs: content_size_height
    public void ContentSizeHeight()
    {
        var w = new World();
        var root = w.Add(null);
        w.Nodes[root].Width = Units.Pixels(600);
        w.Nodes[root].Height = Units.Pixels(600);
        w.Nodes[root].LayoutType = LayoutType.Row;

        var node = w.Add(root);
        w.Nodes[node].Width = Units.Pixels(400);
        w.Nodes[node].Height = Units.Auto;
        w.Nodes[node].ContentMeasurer = (wd, ht) => (wd ?? 0, 100);

        LayoutEngine.Layout(root, w, w);

        var r = w.GetRect(node)!.Value;
        Assert.Equal(400, r.Width);
        Assert.Equal(100, r.Height);
    }

    [Fact] // gap.rs: pixels_horizontal_gap (Row layout)
    public void HorizontalGap()
    {
        var w = new World();
        var root = w.Add(null);
        w.Nodes[root].Width = Units.Pixels(600);
        w.Nodes[root].Height = Units.Pixels(600);
        w.Nodes[root].LayoutType = LayoutType.Row;
        w.Nodes[root].ColGap = Units.Pixels(20); // In Row, ColGap applies between items

        var n1 = w.Add(root);
        w.Nodes[n1].Width = Units.Pixels(100);
        w.Nodes[n1].Height = Units.Pixels(150);
        var n2 = w.Add(root);
        w.Nodes[n2].Width = Units.Pixels(100);
        w.Nodes[n2].Height = Units.Pixels(150);

        LayoutEngine.Layout(root, w, w);

        Assert.Equal(0, w.GetRect(n1)!.Value.PosX);
        Assert.Equal(120, w.GetRect(n2)!.Value.PosX); // 100 width + 20 gap
    }

    [Fact] // padding.rs: pixels_padding_left
    public void PaddingLeft()
    {
        var w = new World();
        var root = w.Add(null);
        w.Nodes[root].Width = Units.Pixels(600);
        w.Nodes[root].Height = Units.Pixels(600);
        w.Nodes[root].PadLeft = Units.Pixels(20);

        var node = w.Add(root);
        w.Nodes[node].Width = Units.Pixels(100);
        w.Nodes[node].Height = Units.Pixels(150);

        LayoutEngine.Layout(root, w, w);

        Assert.Equal(20, w.GetRect(node)!.Value.PosX);
    }

    [Fact] // size_constraints.rs: min_width_pixels
    public void MinWidthPixels()
    {
        var w = new World();
        var root = w.Add(null);
        w.Nodes[root].Width = Units.Pixels(600);
        w.Nodes[root].Height = Units.Pixels(600);

        var node = w.Add(root);
        w.Nodes[node].Width = Units.Pixels(100);
        w.Nodes[node].Height = Units.Pixels(100);
        w.Nodes[node].MinWidth = Units.Pixels(200);

        LayoutEngine.Layout(root, w, w);

        Assert.Equal(200, w.GetRect(node)!.Value.Width);
    }

    [Fact] // border.rs: border_pixels_stretch_child
    public void Border_Reduces_Available_Space()
    {
        var w = new World();
        var root = w.Add(null);
        w.Nodes[root].Width = Units.Pixels(100);
        w.Nodes[root].Height = Units.Pixels(100);
        w.Nodes[root].BorderLeft = Units.Pixels(10);
        w.Nodes[root].BorderRight = Units.Pixels(10);
        w.Nodes[root].BorderTop = Units.Pixels(10);
        w.Nodes[root].BorderBottom = Units.Pixels(10);

        var child = w.Add(root);
        w.Nodes[child].Width = Units.Stretch(1);
        w.Nodes[child].Height = Units.Stretch(1);

        LayoutEngine.Layout(root, w, w);

        var r = w.GetRect(child)!.Value;
        Assert.Equal(10, r.PosX);
        Assert.Equal(10, r.PosY);
        Assert.Equal(80, r.Width);
        Assert.Equal(80, r.Height);
    }

    [Fact] // visibility.rs: invisible_node_ignored_in_stack
    public void Invisible_Node_Ignored_In_Stack()
    {
        var w = new World();
        var root = w.Add(null);
        w.Nodes[root].Width = Units.Auto;
        w.Nodes[root].Height = Units.Auto;
        w.Nodes[root].LayoutType = LayoutType.Row;

        var c1 = w.Add(root);
        w.Nodes[c1].Width = Units.Pixels(50);
        w.Nodes[c1].Height = Units.Pixels(50);

        var c2 = w.Add(root);
        w.Nodes[c2].Width = Units.Pixels(50);
        w.Nodes[c2].Height = Units.Pixels(50);
        w.Nodes[c2].Visible = false;

        var c3 = w.Add(root);
        w.Nodes[c3].Width = Units.Pixels(50);
        w.Nodes[c3].Height = Units.Pixels(50);

        LayoutEngine.Layout(root, w, w);

        var rootRect = w.GetRect(root)!.Value;
        var c3Rect = w.GetRect(c3)!.Value;

        Assert.Equal(100, rootRect.Width); // Only c1 + c3
        Assert.Equal(50, c3Rect.PosX); // Immediately after c1
    }

    [Fact] // top.rs / right.rs: relative_offset_does_not_affect_siblings
    public void Relative_Offset_Does_Not_Affect_Siblings()
    {
        var w = new World();
        var root = w.Add(null);
        w.Nodes[root].Width = Units.Auto;
        w.Nodes[root].Height = Units.Auto;
        w.Nodes[root].LayoutType = LayoutType.Column;

        var c1 = w.Add(root);
        w.Nodes[c1].Width = Units.Pixels(100);
        w.Nodes[c1].Height = Units.Pixels(100);
        w.Nodes[c1].Left = Units.Pixels(20);

        var c2 = w.Add(root);
        w.Nodes[c2].Width = Units.Pixels(100);
        w.Nodes[c2].Height = Units.Pixels(100);

        LayoutEngine.Layout(root, w, w);

        var rootRect = w.GetRect(root)!.Value;
        var c1Rect = w.GetRect(c1)!.Value;
        var c2Rect = w.GetRect(c2)!.Value;

        Assert.Equal(20, c1Rect.PosX); // Shifted by Left
        Assert.Equal(0, c1Rect.PosY);
        Assert.Equal(0, c2Rect.PosX); // Not affected by c1's offset
        Assert.Equal(100, c2Rect.PosY); // Positioned after c1's original position
        Assert.Equal(100, rootRect.Width); // Root size not affected by offset
    }

    [Fact] // alignment.rs: alignment_center_child
    public void Alignment_Center_Child()
    {
        var w = new World();
        var root = w.Add(null);
        w.Nodes[root].Width = Units.Pixels(200);
        w.Nodes[root].Height = Units.Pixels(200);
        w.Nodes[root].Alignment = Alignment.Center;

        var child = w.Add(root);
        w.Nodes[child].Width = Units.Pixels(50);
        w.Nodes[child].Height = Units.Pixels(50);

        LayoutEngine.Layout(root, w, w);

        var r = w.GetRect(child)!.Value;
        Assert.Equal(75, r.PosX); // (200 - 50) / 2
        Assert.Equal(75, r.PosY); // (200 - 50) / 2
    }

    [Fact] // size_constraints.rs: stretch_with_max_constraint_redistribution
    public void Stretch_With_Max_Constraint_Redistribution()
    {
        var w = new World();
        var root = w.Add(null);
        w.Nodes[root].Width = Units.Pixels(200);
        w.Nodes[root].Height = Units.Pixels(100);
        w.Nodes[root].LayoutType = LayoutType.Row;

        var c1 = w.Add(root);
        w.Nodes[c1].Width = Units.Stretch(1);
        w.Nodes[c1].Height = Units.Pixels(50);
        w.Nodes[c1].MaxWidth = Units.Pixels(50);

        var c2 = w.Add(root);
        w.Nodes[c2].Width = Units.Stretch(1);
        w.Nodes[c2].Height = Units.Pixels(50);

        LayoutEngine.Layout(root, w, w);

        var c1Rect = w.GetRect(c1)!.Value;
        var c2Rect = w.GetRect(c2)!.Value;

        Assert.Equal(50, c1Rect.Width); // Constrained by MaxWidth
        Assert.Equal(150, c2Rect.Width); // Should take remaining space
    }

    [Fact] // auto.rs / size_constraints.rs: min_width_auto_overrides_stretch_shrink
    public void Min_Width_Auto_Overrides_Stretch_Shrink()
    {
        var w = new World();
        var root = w.Add(null);
        w.Nodes[root].Width = Units.Pixels(50);
        w.Nodes[root].Height = Units.Pixels(100);

        var child = w.Add(root);
        w.Nodes[child].Width = Units.Stretch(1);
        w.Nodes[child].Height = Units.Pixels(50);
        w.Nodes[child].MinWidth = Units.Auto;
        w.Nodes[child].ContentMeasurer = (wd, ht) => (100f, ht ?? 0);

        LayoutEngine.Layout(root, w, w);

        var r = w.GetRect(child)!.Value;
        Assert.Equal(100, r.Width); // Should use content width, not shrink to 50
    }

    [Fact] // layout.rs: basic_grid_positioning
    public void Basic_Grid_Positioning()
    {
        var w = new World();
        var root = w.Add(null);
        w.Nodes[root].Width = Units.Pixels(200);
        w.Nodes[root].Height = Units.Pixels(200);
        w.Nodes[root].LayoutType = LayoutType.Grid;
        w.Nodes[root].GridRows = [Units.Stretch(1), Units.Stretch(1)];
        w.Nodes[root].GridColumns = [Units.Stretch(1), Units.Stretch(1)];

        var child = w.Add(root);
        w.Nodes[child].RowStart = 1;
        w.Nodes[child].ColumnStart = 1;
        w.Nodes[child].Width = Units.Stretch(1);
        w.Nodes[child].Height = Units.Stretch(1);

        LayoutEngine.Layout(root, w, w);

        var r = w.GetRect(child)!.Value;
        Assert.Equal(100, r.PosX); // Second column starts at 100
        Assert.Equal(100, r.PosY); // Second row starts at 100
        Assert.Equal(100, r.Width); // Takes full cell width
        Assert.Equal(100, r.Height); // Takes full cell height
    }
    
    [Fact]
    public void Row_WithAutoHeight_ShouldExpandToChildContent()
    {
        // Simulate the World/Node structure
        var w = new World();
        
        // Root Container (e.g. MenuBar)
        // Layout: Row, Width: 100px, Height: Auto
        var root = w.Add(null);
        w.Nodes[root].LayoutType = LayoutType.Row;
        w.Nodes[root].Width = Units.Pixels(100);
        w.Nodes[root].Height = Units.Auto;

        // Child Container (e.g. MenuButton)
        // Simulates Elements.Row/Container behavior: Child Height becomes Stretch if Auto
        var child = w.Add(root);
        w.Nodes[child].LayoutType = LayoutType.Column;
        w.Nodes[child].Width = Units.Pixels(50);
        w.Nodes[child].Height = Units.Stretch(1); // Container.AddChild logic applies this
        
        // Child Content (e.g. Label)
        // Fixed size content
        var label = w.Add(child);
        w.Nodes[label].Width = Units.Pixels(50);
        w.Nodes[label].Height = Units.Pixels(20);
        w.Nodes[label].ContentMeasurer = (wd, ht) => (50, 20); // Content size

        // Perform Layout
        LayoutEngine.Layout(root, w, w);

        var rootRect = w.GetRect(root)!.Value;
        var childRect = w.GetRect(child)!.Value;

        // Assert
        // Root height should be determined by child content (20), not collapsed to 0
        Assert.Equal(20, rootRect.Height);
        
        // Child should fill that height
        Assert.Equal(20, childRect.Height);
    }

    [Fact]
    public void Row_WithPercentageChildrenAndGap_ShouldNotExceedContainerBounds()
    {
        // Simulates the Splitter component: Row with 2 percentage-based children and a gap between them
        // Bug: The percentages are based on full container width, but gap adds extra pixels,
        // causing total width to exceed container bounds
        
        var w = new World();
        
        var container = w.Add(null);
        w.Nodes[container].LayoutType = LayoutType.Row;
        w.Nodes[container].Width = Units.Pixels(1000);
        w.Nodes[container].Height = Units.Pixels(600);
        w.Nodes[container].ColGap = Units.Pixels(4); // Gap between children
        
        // First child: 20% width
        var firstPanel = w.Add(container);
        w.Nodes[firstPanel].Width = Units.Percentage(20);
        w.Nodes[firstPanel].Height = Units.Stretch(1);
        
        // Second child: 80% width
        var secondPanel = w.Add(container);
        w.Nodes[secondPanel].Width = Units.Percentage(80);
        w.Nodes[secondPanel].Height = Units.Stretch(1);
        
        LayoutEngine.Layout(container, w, w);
        
        var containerRect = w.GetRect(container)!.Value;
        var firstRect = w.GetRect(firstPanel)!.Value;
        var secondRect = w.GetRect(secondPanel)!.Value;
        
        var totalWidth = firstRect.Width + secondRect.Width + 4; // children + gap
        
        // This assertion will FAIL with current implementation, exposing the bug
        Assert.True(totalWidth <= containerRect.Width, 
            $"Total width ({totalWidth}) exceeds container width ({containerRect.Width}). " +
            $"First: {firstRect.Width}, Gap: 4, Second: {secondRect.Width}");
        
        // Verify children don't extend beyond container bounds
        Assert.True(firstRect.PosX + firstRect.Width <= containerRect.Width,
            "First panel extends beyond container right edge");
        Assert.True(secondRect.PosX + secondRect.Width <= containerRect.Width,
            "Second panel extends beyond container right edge");
    }

    [Fact]
    public void Column_WithPercentageChildrenAndGap_ShouldNotExceedContainerBounds()
    {
        // Same issue but in Column direction (vertical splitter)
        
        var w = new World();
        
        var container = w.Add(null);
        w.Nodes[container].LayoutType = LayoutType.Column;
        w.Nodes[container].Width = Units.Pixels(800);
        w.Nodes[container].Height = Units.Pixels(1000);
        w.Nodes[container].RowGap = Units.Pixels(4); // Gap between children
        
        // First child: 30% height
        var firstPanel = w.Add(container);
        w.Nodes[firstPanel].Width = Units.Stretch(1);
        w.Nodes[firstPanel].Height = Units.Percentage(30);
        
        // Second child: 70% height
        var secondPanel = w.Add(container);
        w.Nodes[secondPanel].Width = Units.Stretch(1);
        w.Nodes[secondPanel].Height = Units.Percentage(70);
        
        LayoutEngine.Layout(container, w, w);
        
        var containerRect = w.GetRect(container)!.Value;
        var firstRect = w.GetRect(firstPanel)!.Value;
        var secondRect = w.GetRect(secondPanel)!.Value;
        
        var totalHeight = firstRect.Height + secondRect.Height + 4; // children + gap
        
        // This assertion will FAIL with current implementation
        Assert.True(totalHeight <= containerRect.Height,
            $"Total height ({totalHeight}) exceeds container height ({containerRect.Height}). " +
            $"First: {firstRect.Height}, Gap: 4, Second: {secondRect.Height}");
        
        // Verify children don't extend beyond container bounds
        Assert.True(firstRect.PosY + firstRect.Height <= containerRect.Height,
            "First panel extends beyond container bottom edge");
        Assert.True(secondRect.PosY + secondRect.Height <= containerRect.Height,
            "Second panel extends beyond container bottom edge");
    }
}