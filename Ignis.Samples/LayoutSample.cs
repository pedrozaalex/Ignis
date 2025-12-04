using System.Numerics;
using CrucibleUI;
using CrucibleUI.Types;
using Friflo.Engine.ECS;
using Ignis.Gfx;
using Ignis.Samples.Layout;

namespace Ignis.Samples;

/// <summary>
/// Sample demonstrating CrucibleUI layout engine with rendered shapes.
/// </summary>
public class LayoutSample : GraphicsSample
{
    public override string Name => "Layout";
    
    private EntityStore _store = null!;
    private LayoutCache _cache = null!;
    private Entity _root;
    private SubLayoutContext _subLayout;
    private bool _needsLayout = true;
    
    protected override void Load()
    {
        _store = new EntityStore();
        _cache = new LayoutCache();
        _subLayout = new SubLayoutContext();
        
        Console.WriteLine($"[LayoutSample] Load called. Width={Width}, Height={Height}");
        BuildLayoutTree();
    }
    
    private void BuildLayoutTree()
    {
        // Root container - fills the screen with a column layout
        _root = _store.CreateEntity();
        _root.AddComponent(new LayoutProperties
        {
            LayoutType = LayoutType.Column,
            Width = Units.Pixels(Width),
            Height = Units.Pixels(Height),
            PaddingLeft = Units.Pixels(20),
            PaddingRight = Units.Pixels(20),
            PaddingTop = Units.Pixels(20),
            PaddingBottom = Units.Pixels(20),
            VerticalGap = Units.Pixels(10)
        });
        _root.AddComponent(new ShapeColor(0.15f, 0.15f, 0.2f));
        
        // Header row
        var header = CreateChild(_root, new LayoutProperties
        {
            LayoutType = LayoutType.Row,
            Width = Units.Stretch(1),
            Height = Units.Pixels(60),
            HorizontalGap = Units.Pixels(10)
        }, ShapeColor.Gray);
        
        // Header items
        CreateChild(header, new LayoutProperties
        {
            Width = Units.Pixels(60),
            Height = Units.Stretch(1)
        }, ShapeColor.Red);
        
        CreateChild(header, new LayoutProperties
        {
            Width = Units.Stretch(1),
            Height = Units.Stretch(1)
        }, ShapeColor.Blue);
        
        CreateChild(header, new LayoutProperties
        {
            Width = Units.Pixels(100),
            Height = Units.Stretch(1)
        }, ShapeColor.Green);
        
        // Main content area - horizontal split
        var main = CreateChild(_root, new LayoutProperties
        {
            LayoutType = LayoutType.Row,
            Width = Units.Stretch(1),
            Height = Units.Stretch(1),
            HorizontalGap = Units.Pixels(10)
        }, new ShapeColor(0.2f, 0.2f, 0.25f));
        
        // Sidebar
        var sidebar = CreateChild(main, new LayoutProperties
        {
            LayoutType = LayoutType.Column,
            Width = Units.Pixels(200),
            Height = Units.Stretch(1),
            PaddingLeft = Units.Pixels(10),
            PaddingRight = Units.Pixels(10),
            PaddingTop = Units.Pixels(10),
            PaddingBottom = Units.Pixels(10),
            VerticalGap = Units.Pixels(8)
        }, new ShapeColor(0.25f, 0.25f, 0.3f));
        
        // Sidebar items
        for (int i = 0; i < 5; i++)
        {
            var color = i switch
            {
                0 => ShapeColor.Red,
                1 => ShapeColor.Green,
                2 => ShapeColor.Blue,
                3 => ShapeColor.Yellow,
                _ => ShapeColor.Cyan
            };
            
            CreateChild(sidebar, new LayoutProperties
            {
                Width = Units.Stretch(1),
                Height = Units.Pixels(40)
            }, color);
        }
        
        // Content area
        var content = CreateChild(main, new LayoutProperties
        {
            LayoutType = LayoutType.Column,
            Width = Units.Stretch(1),
            Height = Units.Stretch(1),
            PaddingLeft = Units.Pixels(15),
            PaddingRight = Units.Pixels(15),
            PaddingTop = Units.Pixels(15),
            PaddingBottom = Units.Pixels(15),
            VerticalGap = Units.Pixels(10)
        }, new ShapeColor(0.18f, 0.18f, 0.22f));
        
        // Content rows with cards
        for (int i = 0; i < 3; i++)
        {
            var row = CreateChild(content, new LayoutProperties
            {
                LayoutType = LayoutType.Row,
                Width = Units.Stretch(1),
                Height = Units.Stretch(1),
                HorizontalGap = Units.Pixels(10)
            }, new ShapeColor(0.22f, 0.22f, 0.28f));
            
            for (int j = 0; j < 3; j++)
            {
                var cardColor = new ShapeColor(
                    0.3f + (i * 0.1f),
                    0.4f + (j * 0.1f),
                    0.5f + ((i + j) * 0.05f)
                );
                
                CreateChild(row, new LayoutProperties
                {
                    Width = Units.Stretch(1),
                    Height = Units.Stretch(1)
                }, cardColor);
            }
        }
        
        // Footer
        CreateChild(_root, new LayoutProperties
        {
            LayoutType = LayoutType.Row,
            Width = Units.Stretch(1),
            Height = Units.Pixels(40),
            HorizontalGap = Units.Pixels(10)
        }, ShapeColor.Gray);
        
        _needsLayout = true;
    }
    
    private Entity CreateChild(Entity parent, LayoutProperties props, ShapeColor color)
    {
        var entity = _store.CreateEntity();
        entity.AddComponent(props);
        entity.AddComponent(color);
        parent.AddChild(entity);
        return entity;
    }
    
    protected override void OnUpdate(float deltaTime)
    {
        if (_needsLayout)
        {
            PerformLayout();
            _needsLayout = false;
        }
    }
    
    public override void OnResize(int width, int height)
    {
        base.OnResize(width, height);
        
        if (_root.IsNull) return;
        
        ref var props = ref _root.GetComponent<LayoutProperties>();
        props.Width = Units.Pixels(width);
        props.Height = Units.Pixels(height);
        _needsLayout = true;
    }
    
    private void PerformLayout()
    {
        Console.WriteLine($"[LayoutSample] PerformLayout. Width={Width}, Height={Height}");
        var node = new LayoutNode(_root);
        LayoutEngine.Compute<LayoutNode, EntityStore, SubLayoutContext, Entity, LayoutCache>(
            node, _cache, _store, ref _subLayout);
        
        // Debug: print all bounds
        PrintEntityBounds(_root, 0);
    }
    
    private void PrintEntityBounds(Entity entity, int depth)
    {
        if (entity.IsNull) return;
        
        var indent = new string(' ', depth * 2);
        if (entity.TryGetComponent<LayoutBounds>(out var bounds))
        {
            var name = entity.TryGetComponent<LayoutProperties>(out var props) 
                ? $"{props.LayoutType}" 
                : "?";
            Console.WriteLine($"{indent}[{name}] pos=({bounds.PosX:F0},{bounds.PosY:F0}) size=({bounds.Width:F0}x{bounds.Height:F0})");
        }
        
        foreach (var child in entity.ChildEntities)
        {
            PrintEntityBounds(child, depth + 1);
        }
    }
    
    public override void Render(float alpha)
    {
        var pass = new RenderPass
        {
            Target = RenderTargetHandle.Screen,
            ClearColor = new Color4(0.1f, 0.1f, 0.15f),
            ClearDepth = true,
            Viewport = new Gfx.Rect(0, 0, Width, Height)
        };
        
        RenderingServer.BeginPass(pass);
        
        var commands = RenderingServer.CreateCommandList();
        var projection = Matrix4x4.CreateOrthographicOffCenter(0, Width, Height, 0, -1, 1);
        commands.SetPipeline(RenderingServer.DefaultShader2D);
        commands.SetProjectionMatrix(projection);
        commands.SetViewMatrix(Matrix4x4.Identity);
        
        var quadCount = 0;
        RenderEntity(_root, commands, ref quadCount, 0, 0);
        
        if (quadCount > 0 && _frameCount++ % 60 == 0)
            Console.WriteLine($"[LayoutSample] Drawing {quadCount} quads");
        
        RenderingServer.Submit(commands);
        RenderingServer.EndPass();
    }
    
    private int _frameCount = 0;
    
    private void RenderEntity(Entity entity, IRenderCommandList commands, ref int quadCount, float parentX, float parentY)
    {
        if (entity.IsNull) return;
        if (!entity.TryGetComponent<LayoutBounds>(out var bounds)) return;
        
        // Calculate absolute position
        float absX = parentX + bounds.PosX;
        float absY = parentY + bounds.PosY;
        
        if (entity.TryGetComponent<ShapeColor>(out var color) && bounds.Width > 0 && bounds.Height > 0)
        {
            commands.DrawQuad(
                new Vector2(absX, absY),
                new Vector2(bounds.Width, bounds.Height),
                new Color4(color.R, color.G, color.B, color.A)
            );
            quadCount++;
        }
        
        // Render children with accumulated offset
        foreach (var child in entity.ChildEntities)
        {
            RenderEntity(child, commands, ref quadCount, absX, absY);
        }
    }
    
    protected override void Unload()
    {
    }
}

