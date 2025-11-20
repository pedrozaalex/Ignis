using Ignis.Engine.UI;
using Xunit;

namespace Ignis.Tests.UI
{
    // --- ECS Implementation for Tests ---
    public class TestNode
    {
        public int Id;
        public int? Parent;
        public List<int> Children = [];
        public bool Visible = true;
        public LayoutType LayoutType = LayoutType.Column;
        public PositionType PositionType = PositionType.Relative;
        public Alignment Alignment = Alignment.TopLeft;
        
        public Units Width = Units.Auto;
        public Units Height = Units.Auto;
        public Units MinWidth = Units.Auto;
        public Units MinHeight = Units.Auto;
        public Units MaxWidth = Units.Auto;
        public Units MaxHeight = Units.Auto;
        
        public Units Left = Units.Auto;
        public Units Right = Units.Auto;
        public Units Top = Units.Auto;
        public Units Bottom = Units.Auto;
        
        public Units PadLeft = Units.Pixels(0);
        public Units PadRight = Units.Pixels(0);
        public Units PadTop = Units.Pixels(0);
        public Units PadBottom = Units.Pixels(0);
        
        public Units BorderLeft = Units.Pixels(0);
        public Units BorderRight = Units.Pixels(0);
        public Units BorderTop = Units.Pixels(0);
        public Units BorderBottom = Units.Pixels(0);
        
        public Units RowGap = Units.Pixels(0);
        public Units ColGap = Units.Pixels(0);
        
        public List<Units> GridRows = [];
        public List<Units> GridColumns = [];
        public int RowStart = 0;
        public int RowSpan = 1;
        public int ColumnStart = 0;
        public int ColumnSpan = 1;
        
        public Func<float?, float?, (float, float)>? ContentMeasurer;
    }

    public class World : ILayoutNode, ILayoutCache
    {
        private int _nextId = 0;
        public Dictionary<int, TestNode> Nodes = new();
        public Dictionary<int, Rect> Bounds = new();

        public int Add(int? parent)
        {
            var id = _nextId++;
            var node = new TestNode { Id = id, Parent = parent };
            Nodes[id] = node;
            if (parent.HasValue) Nodes[parent.Value].Children.Add(id);
            return id;
        }

        // ILayoutNode Implementation
        public IEnumerable<object> GetChildren(object node) => Nodes[(int)node].Children.Cast<object>();
        public bool IsVisible(object node) => Nodes[(int)node].Visible;
        public LayoutType GetLayoutType(object node) => Nodes[(int)node].LayoutType;
        public PositionType GetPositionType(object node) => Nodes[(int)node].PositionType;
        public Alignment GetAlignment(object node) => Nodes[(int)node].Alignment;
        public Units GetWidth(object node) => Nodes[(int)node].Width;
        public Units GetHeight(object node) => Nodes[(int)node].Height;
        public Units GetMinWidth(object node) => Nodes[(int)node].MinWidth;
        public Units GetMinHeight(object node) => Nodes[(int)node].MinHeight;
        public Units GetMaxWidth(object node) => Nodes[(int)node].MaxWidth;
        public Units GetMaxHeight(object node) => Nodes[(int)node].MaxHeight;
        public Units GetLeft(object node) => Nodes[(int)node].Left;
        public Units GetRight(object node) => Nodes[(int)node].Right;
        public Units GetTop(object node) => Nodes[(int)node].Top;
        public Units GetBottom(object node) => Nodes[(int)node].Bottom;
        public Units GetPaddingLeft(object node) => Nodes[(int)node].PadLeft;
        public Units GetPaddingRight(object node) => Nodes[(int)node].PadRight;
        public Units GetPaddingTop(object node) => Nodes[(int)node].PadTop;
        public Units GetPaddingBottom(object node) => Nodes[(int)node].PadBottom;
        public Units GetBorderLeft(object node) => Nodes[(int)node].BorderLeft;
        public Units GetBorderRight(object node) => Nodes[(int)node].BorderRight;
        public Units GetBorderTop(object node) => Nodes[(int)node].BorderTop;
        public Units GetBorderBottom(object node) => Nodes[(int)node].BorderBottom;
        public Units GetChildLeft(object node) => Units.Auto;
        public Units GetChildTop(object node) => Units.Auto;
        public Units GetRowGap(object node) => Nodes[(int)node].RowGap;
        public Units GetColumnGap(object node) => Nodes[(int)node].ColGap;
        public List<Units> GetGridRows(object node) => Nodes[(int)node].GridRows; 
        public List<Units> GetGridColumns(object node) => Nodes[(int)node].GridColumns;
        public int GetRowStart(object node) => Nodes[(int)node].RowStart;
        public int GetRowSpan(object node) => Nodes[(int)node].RowSpan;
        public int GetColumnStart(object node) => Nodes[(int)node].ColumnStart;
        public int GetColumnSpan(object node) => Nodes[(int)node].ColumnSpan;

        public (float width, float height)? MeasureContent(object node, float? w, float? h)
        {
            var n = Nodes[(int)node];
            return n.ContentMeasurer?.Invoke(w, h);
        }

        // ILayoutCache Implementation
        public void SetBounds(object node, float x, float y, float w, float h) => Bounds[(int)node] = new Rect { PosX = x, PosY = y, Width = w, Height = h };
        float ILayoutCache.GetWidth(object node) => Bounds.TryGetValue((int)node, out var r) ? r.Width : 0;
        float ILayoutCache.GetHeight(object node) => Bounds.TryGetValue((int)node, out var r) ? r.Height : 0;
        float ILayoutCache.GetPosX(object node) => Bounds.TryGetValue((int)node, out var r) ? r.PosX : 0;
        float ILayoutCache.GetPosY(object node) => Bounds.TryGetValue((int)node, out var r) ? r.PosY : 0;
        
        public Rect? GetRect(int node) => Bounds.ContainsKey(node) ? Bounds[node] : null;
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
            Assert.Equal(0, r.PosX); Assert.Equal(0, r.PosY); Assert.Equal(100, r.Width); Assert.Equal(100, r.Height);
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
            w.Nodes[n1].Width = Units.Pixels(100); w.Nodes[n1].Height = Units.Pixels(150);
            var n2 = w.Add(root);
            w.Nodes[n2].Width = Units.Pixels(100); w.Nodes[n2].Height = Units.Pixels(150);

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
    }
}

