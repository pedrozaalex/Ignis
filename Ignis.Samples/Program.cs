using Ignis.Samples;

Console.WriteLine("Ignis Engine Samples");
Console.WriteLine("====================");
Console.WriteLine();
Console.WriteLine("ECS & Graphics Samples:");
Console.WriteLine("  1. Phase 1: Transform System (Visual)");
Console.WriteLine("  2. Phase 2: Asset Manager (Console)");
Console.WriteLine("  3. Phase 3: Spinning Cube with Orbiting Camera (Visual)");
Console.WriteLine();
Console.WriteLine("UI Widget Samples:");
Console.WriteLine("  4. Basic Widgets - TextField, NumberField, Checkbox, Slider");
Console.WriteLine("  5. Hierarchy & Lists - TreeView, Console, Dynamic Updates");
Console.WriteLine("  6. Transform Inspector - Declarative UI with Signal.Lens()");
Console.WriteLine();
Console.Write("Select sample (1-6): ");

var choice = Console.ReadLine();

switch (choice)
{
    case "1":
        Console.WriteLine("\nStarting Phase 1 Sample...\n");
        using (var game = new HelloGame())
        {
            game.Run();
        }

        break;

    case "2":
        Console.WriteLine("\nStarting Phase 2 Sample...\n");
        AssetSample.Run();
        break;

    case "3":
        Console.WriteLine("\nStarting Phase 3 Sample...\n");
        using (var game = new SpinningCubeSample())
        {
            game.Run();
        }

        break;

    case "4":
        Console.WriteLine("\nStarting Basic Widgets Sample...\n");
        using (var game = new BasicWidgetsSample())
        {
            game.Run();
        }

        break;

    case "5":
        Console.WriteLine("\nStarting Hierarchy Widget Sample...\n");
        using (var game = new HierarchyWidgetSample())
        {
            game.Run();
        }

        break;

    case "6":
        Console.WriteLine("\nStarting Transform Inspector Sample...\n");
        using (var game = new TransformInspectorSample())
        {
            game.Run();
        }

        break;

    default:
        Console.WriteLine("Invalid choice. Running Basic Widgets Sample...");
        using (var game = new BasicWidgetsSample())
        {
            game.Run();
        }

        break;
}