using System.Numerics;
using CrucibleUI;
using CrucibleUI.Types;
using Friflo.Engine.ECS;
using Ignis.Gfx;
using Ignis.Gfx.Backends.OpenGL;
using Samples.Common;

namespace Samples.Layout;

/// <summary>
/// Sample demonstrating CrucibleUI layout engine with rendered shapes and text.
/// </summary>
public class LayoutSample : GraphicsSample
{
    public override string Name => "Layout";
    
    private EntityStore _store = null!;
    private LayoutCache _cache = null!;
    private Entity _root;
    private SubLayoutContext _subLayout;
    private bool _needsLayout = true;
    private FontHandle _font;
    
    protected override void Load()
    {
        _store = new EntityStore();
        _cache = new LayoutCache();
        _subLayout = new SubLayoutContext();
        
        _font = LoadFont();
        BuildLayoutTree();
    }
    
    private FontHandle LoadFont()
    {
        string[] fontPaths = 
        {
            @"C:\Windows\Fonts\segoeui.ttf",
            @"C:\Windows\Fonts\arial.ttf",
            @"C:\Windows\Fonts\tahoma.ttf",
            "/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf",
            "/System/Library/Fonts/Helvetica.ttc"
        };
        
        foreach (var path in fontPaths)
        {
            if (File.Exists(path))
            {
                var handle = RenderingServer.CreateFontFromFile(path);
                if (handle.Id != 0) return handle;
            }
        }
        
        return FontHandle.Invalid;
    }
    
    private void BuildLayoutTree()
    {
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
        _root.AddComponent(new ShapeColor(0.12f, 0.12f, 0.15f));
        
        // Header
        var header = CreateChild(_root, new LayoutProperties
        {
            LayoutType = LayoutType.Row,
            Width = Units.Stretch(1),
            Height = Units.Pixels(50),
            HorizontalGap = Units.Pixels(10),
            PaddingLeft = Units.Pixels(15),
            PaddingRight = Units.Pixels(15)
        }, new ShapeColor(0.2f, 0.2f, 0.25f));
        
        var logo = CreateChild(header, new LayoutProperties
        {
            Width = Units.Pixels(40),
            Height = Units.Pixels(40),
            Top = Units.Pixels(5)
        }, ShapeColor.Blue);
        logo.AddComponent(new TextLabel("◆", 24f));
        
        var title = CreateChild(header, new LayoutProperties
        {
            Width = Units.Stretch(1),
            Height = Units.Stretch(1)
        }, new ShapeColor(0, 0, 0, 0));
        title.AddComponent(new TextLabel("Layout Sample", 20f));
        
        CreateMenuButton(header, "File");
        CreateMenuButton(header, "Edit");
        CreateMenuButton(header, "View");
        
        // Main content area
        var main = CreateChild(_root, new LayoutProperties
        {
            LayoutType = LayoutType.Row,
            Width = Units.Stretch(1),
            Height = Units.Stretch(1),
            HorizontalGap = Units.Pixels(10)
        }, new ShapeColor(0, 0, 0, 0));
        
        // Sidebar
        var sidebar = CreateChild(main, new LayoutProperties
        {
            LayoutType = LayoutType.Column,
            Width = Units.Pixels(180),
            Height = Units.Stretch(1),
            PaddingLeft = Units.Pixels(8),
            PaddingRight = Units.Pixels(8),
            PaddingTop = Units.Pixels(8),
            PaddingBottom = Units.Pixels(8),
            VerticalGap = Units.Pixels(4)
        }, new ShapeColor(0.18f, 0.18f, 0.22f));
        
        CreateSidebarItem(sidebar, "Dashboard", ShapeColor.Blue);
        CreateSidebarItem(sidebar, "Projects", ShapeColor.Green);
        CreateSidebarItem(sidebar, "Tasks", ShapeColor.Yellow);
        CreateSidebarItem(sidebar, "Calendar", ShapeColor.Cyan);
        CreateSidebarItem(sidebar, "Settings", ShapeColor.Gray);
        
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
            VerticalGap = Units.Pixels(15)
        }, new ShapeColor(0.15f, 0.15f, 0.18f));
        
        var contentHeader = CreateChild(content, new LayoutProperties
        {
            Width = Units.Stretch(1),
            Height = Units.Pixels(40)
        }, new ShapeColor(0, 0, 0, 0));
        contentHeader.AddComponent(new TextLabel("Dashboard Overview", 24f));
        
        // Card grid
        var cardGrid = CreateChild(content, new LayoutProperties
        {
            LayoutType = LayoutType.Row,
            Width = Units.Stretch(1),
            Height = Units.Pixels(120),
            HorizontalGap = Units.Pixels(15)
        }, new ShapeColor(0, 0, 0, 0));
        
        CreateStatCard(cardGrid, "Total Users", "1,234", ShapeColor.Blue);
        CreateStatCard(cardGrid, "Revenue", "$45.2K", ShapeColor.Green);
        CreateStatCard(cardGrid, "Orders", "892", ShapeColor.Yellow);
        CreateStatCard(cardGrid, "Growth", "+12.5%", ShapeColor.Cyan);
        
        // Main content grid
        var mainGrid = CreateChild(content, new LayoutProperties
        {
            LayoutType = LayoutType.Row,
            Width = Units.Stretch(1),
            Height = Units.Stretch(1),
            HorizontalGap = Units.Pixels(15)
        }, new ShapeColor(0, 0, 0, 0));
        
        var chart = CreateChild(mainGrid, new LayoutProperties
        {
            LayoutType = LayoutType.Column,
            Width = Units.Stretch(2),
            Height = Units.Stretch(1),
            PaddingLeft = Units.Pixels(15),
            PaddingRight = Units.Pixels(15),
            PaddingTop = Units.Pixels(15),
            PaddingBottom = Units.Pixels(15)
        }, new ShapeColor(0.2f, 0.2f, 0.25f));
        
        var chartTitle = CreateChild(chart, new LayoutProperties
        {
            Width = Units.Stretch(1),
            Height = Units.Pixels(30)
        }, new ShapeColor(0, 0, 0, 0));
        chartTitle.AddComponent(new TextLabel("Analytics Chart", 16f));
        
        var chartArea = CreateChild(chart, new LayoutProperties
        {
            Width = Units.Stretch(1),
            Height = Units.Stretch(1)
        }, new ShapeColor(0.25f, 0.25f, 0.3f));
        chartArea.AddComponent(new TextLabel("Chart Placeholder", 14f, 0.5f, 0.5f, 0.5f));
        
        var activity = CreateChild(mainGrid, new LayoutProperties
        {
            LayoutType = LayoutType.Column,
            Width = Units.Stretch(1),
            Height = Units.Stretch(1),
            PaddingLeft = Units.Pixels(15),
            PaddingRight = Units.Pixels(15),
            PaddingTop = Units.Pixels(15),
            PaddingBottom = Units.Pixels(15),
            VerticalGap = Units.Pixels(8)
        }, new ShapeColor(0.2f, 0.2f, 0.25f));
        
        var actTitle = CreateChild(activity, new LayoutProperties
        {
            Width = Units.Stretch(1),
            Height = Units.Pixels(30)
        }, new ShapeColor(0, 0, 0, 0));
        actTitle.AddComponent(new TextLabel("Recent Activity", 16f));
        
        CreateActivityItem(activity, "New user registered");
        CreateActivityItem(activity, "Order #1234 completed");
        CreateActivityItem(activity, "Payment received");
        CreateActivityItem(activity, "Report generated");
        
        // Footer
        var footer = CreateChild(_root, new LayoutProperties
        {
            LayoutType = LayoutType.Row,
            Width = Units.Stretch(1),
            Height = Units.Pixels(30),
            PaddingLeft = Units.Pixels(15),
            PaddingRight = Units.Pixels(15)
        }, new ShapeColor(0.18f, 0.18f, 0.22f));
        
        var footerText = CreateChild(footer, new LayoutProperties
        {
            Width = Units.Stretch(1),
            Height = Units.Stretch(1)
        }, new ShapeColor(0, 0, 0, 0));
        footerText.AddComponent(new TextLabel("© 2024 Ignis Engine - Layout Demo", 12f, 0.5f, 0.5f, 0.5f));
        
        _needsLayout = true;
    }
    
    private void CreateMenuButton(Entity parent, string label)
    {
        var btn = CreateChild(parent, new LayoutProperties
        {
            Width = Units.Pixels(60),
            Height = Units.Pixels(30),
            Top = Units.Pixels(10),
        }, new ShapeColor(0.25f, 0.25f, 0.3f));
        btn.AddComponent(new TextLabel(label, 14f));
    }
    
    private void CreateSidebarItem(Entity parent, string label, ShapeColor accentColor)
    {
        var item = CreateChild(parent, new LayoutProperties
        {
            LayoutType = LayoutType.Row,
            Width = Units.Stretch(1),
            Height = Units.Pixels(36),
            PaddingLeft = Units.Pixels(10),
            HorizontalGap = Units.Pixels(10)
        }, new ShapeColor(0.22f, 0.22f, 0.28f));
        
        CreateChild(item, new LayoutProperties
        {
            Width = Units.Pixels(8),
            Height = Units.Pixels(8),
            Top = Units.Pixels(14)
        }, accentColor);
        
        var labelEntity = CreateChild(item, new LayoutProperties
        {
            Width = Units.Stretch(1),
            Height = Units.Stretch(1)
        }, new ShapeColor(0, 0, 0, 0));
        labelEntity.AddComponent(new TextLabel(label, 14f));
    }
    
    private void CreateStatCard(Entity parent, string title, string value, ShapeColor accentColor)
    {
        var card = CreateChild(parent, new LayoutProperties
        {
            LayoutType = LayoutType.Column,
            Width = Units.Stretch(1),
            Height = Units.Stretch(1),
            PaddingLeft = Units.Pixels(15),
            PaddingRight = Units.Pixels(15),
            PaddingTop = Units.Pixels(12),
            PaddingBottom = Units.Pixels(12)
        }, new ShapeColor(0.2f, 0.2f, 0.25f));
        
        CreateChild(card, new LayoutProperties
        {
            Width = Units.Stretch(1),
            Height = Units.Pixels(4)
        }, accentColor);
        
        var titleEntity = CreateChild(card, new LayoutProperties
        {
            Width = Units.Stretch(1),
            Height = Units.Pixels(20),
            Top = Units.Pixels(8)
        }, new ShapeColor(0, 0, 0, 0));
        titleEntity.AddComponent(new TextLabel(title, 12f, 0.6f, 0.6f, 0.6f));
        
        var valueEntity = CreateChild(card, new LayoutProperties
        {
            Width = Units.Stretch(1),
            Height = Units.Stretch(1)
        }, new ShapeColor(0, 0, 0, 0));
        valueEntity.AddComponent(new TextLabel(value, 28f));
    }
    
    private void CreateActivityItem(Entity parent, string text)
    {
        var item = CreateChild(parent, new LayoutProperties
        {
            Width = Units.Stretch(1),
            Height = Units.Pixels(28),
            PaddingLeft = Units.Pixels(8)
        }, new ShapeColor(0.25f, 0.25f, 0.3f));
        item.AddComponent(new TextLabel("• " + text, 12f, 0.8f, 0.8f, 0.8f));
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
        var node = new LayoutNode(_root);
        LayoutEngine.Compute<LayoutNode, EntityStore, SubLayoutContext, Entity, LayoutCache>(
            node, _cache, _store, ref _subLayout);
    }
    
    public override void Render(float alpha)
    {
        var pass = new RenderPass
        {
            Target = RenderTargetHandle.Screen,
            ClearColor = new Color4(0.08f, 0.08f, 0.1f),
            ClearDepth = true,
            Viewport = new Ignis.Gfx.Rect(0, 0, Width, Height)
        };
        
        RenderingServer.BeginPass(pass);
        
        var commands = RenderingServer.CreateCommandList();
        var projection = Matrix4x4.CreateOrthographicOffCenter(0, Width, Height, 0, -1, 1);
        commands.SetPipeline(RenderingServer.DefaultShader2D);
        commands.SetProjectionMatrix(projection);
        commands.SetViewMatrix(Matrix4x4.Identity);
        
        RenderQuads(_root, commands, 0, 0);
        
        RenderingServer.Submit(commands);
        RenderingServer.EndPass();
        
        RenderText(_root, projection, 0, 0);
    }
    
    private void RenderQuads(Entity entity, IRenderCommandList commands, float parentX, float parentY)
    {
        if (entity.IsNull) return;
        if (!entity.TryGetComponent<LayoutBounds>(out var bounds)) return;
        
        float absX = parentX + bounds.PosX;
        float absY = parentY + bounds.PosY;
        
        if (entity.TryGetComponent<ShapeColor>(out var color) && bounds.Width > 0 && bounds.Height > 0 && color.A > 0)
        {
            commands.DrawQuad(
                new Vector2(absX, absY),
                new Vector2(bounds.Width, bounds.Height),
                new Color4(color.R, color.G, color.B, color.A)
            );
        }
        
        foreach (var child in entity.ChildEntities)
        {
            RenderQuads(child, commands, absX, absY);
        }
    }
    
    private void RenderText(Entity entity, Matrix4x4 projection, float parentX, float parentY)
    {
        if (entity.IsNull) return;
        if (!entity.TryGetComponent<LayoutBounds>(out var bounds)) return;
        
        float absX = parentX + bounds.PosX;
        float absY = parentY + bounds.PosY;
        
        if (entity.TryGetComponent<TextLabel>(out var label) && !string.IsNullOrEmpty(label.Text))
        {
            if (RenderingServer is OpenGLRenderingServer { FontRenderer: not null } glServer)
            {
                var font = glServer.GetFont(_font, label.FontSize);
                if (font != null)
                {
                    var textColor = new FontStashSharp.FSColor(
                        (byte)(label.R * 255),
                        (byte)(label.G * 255),
                        (byte)(label.B * 255),
                        (byte)(label.A * 255)
                    );
                    
                    var textSize = font.MeasureString(label.Text);
                    float textY = absY + (bounds.Height - textSize.Y) / 2;
                    float textX = absX + 5;
                    
                    glServer.FontRenderer.Begin(projection);
                    font.DrawText(glServer.FontRenderer, label.Text, new Vector2(textX, textY), textColor);
                    glServer.FontRenderer.End();
                }
            }
        }
        
        foreach (var child in entity.ChildEntities)
        {
            RenderText(child, projection, absX, absY);
        }
    }
    
    protected override void Unload()
    {
        if (_font.Id != 0)
        {
            RenderingServer.DestroyFont(_font);
        }
    }
}

