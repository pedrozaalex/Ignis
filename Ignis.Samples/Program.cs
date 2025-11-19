using Ignis.Samples;

Console.WriteLine("Ignis Engine Samples");
Console.WriteLine("====================");
Console.WriteLine("1. Phase 1: Transform System (Visual)");
Console.WriteLine("2. Phase 2: Asset Manager (Console)");
Console.WriteLine();
Console.Write("Select sample (1-2): ");

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

    default:
        Console.WriteLine("Invalid choice. Running default (Phase 1)...");
        using (var game = new HelloGame())
        {
            game.Run();
        }

        break;
}