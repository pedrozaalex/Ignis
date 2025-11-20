using Ignis.Samples;

Console.WriteLine("Ignis Engine Samples");
Console.WriteLine("====================");
Console.WriteLine("1. Phase 1: Transform System (Visual)");
Console.WriteLine("2. Phase 2: Asset Manager (Console)");
Console.WriteLine("3. Phase 3: Spinning Cube with Orbiting Camera (Visual)");
Console.WriteLine();
Console.Write("Select sample (1-3): ");

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

    default:
        Console.WriteLine("Invalid choice. Running default (Phase 1)...");
        using (var game = new HelloGame())
        {
            game.Run();
        }

        break;
}