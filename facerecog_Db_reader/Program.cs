namespace facerecog_Db_reader;

class Program
{
    static void Main(string[] args)
    {
        string userProfile = Environment.GetEnvironmentVariable("USERPROFILE");
        string baseDir = Path.Combine(userProfile, "Pictures", "DH");
        var dbReader = new FaceRecognitionDbReader(baseDir);

        Console.WriteLine("===========================================");
        Console.WriteLine("Face Recognition Database Reader");
        Console.WriteLine("===========================================");

        // Show statistics
        dbReader.PrintStatistics();

        // Main menu loop
        while (true)
        {
            Console.WriteLine("\n\n=== Main Menu ===");
            Console.WriteLine("1. List all people");
            Console.WriteLine("2. Search for a person");
            Console.WriteLine("3. Find images by person name");
            Console.WriteLine("4. Exit");
            Console.Write("\nEnter your choice (1-4): ");

            string? choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    ListAllPeople(dbReader);
                    break;
                case "2":
                    SearchPeople(dbReader);
                    break;
                case "3":
                    FindImagesByPerson(dbReader);
                    break;
                case "4":
                    Console.WriteLine("\nGoodbye!");
                    return;
                default:
                    Console.WriteLine("\nInvalid choice. Please try again.");
                    break;
            }
        }
    }

    static void ListAllPeople(FaceRecognitionDbReader dbReader)
    {
        Console.WriteLine("\n=== All People in Database ===");

        var identities = dbReader.GetAllIdentities();
        var tags = dbReader.GetAllTags();

        // Combine and deduplicate names
        var allNames = new HashSet<string>();

        foreach (var identity in identities)
        {
            if (!string.IsNullOrWhiteSpace(identity.Name))
                allNames.Add(identity.Name);
        }

        foreach (var tag in tags)
        {
            if (!string.IsNullOrWhiteSpace(tag.Name))
                allNames.Add(tag.Name);
        }

        var sortedNames = allNames.OrderBy(n => n).ToList();

        Console.WriteLine($"\nFound {sortedNames.Count} unique names:\n");

        int count = 0;
        foreach (var name in sortedNames)
        {
            count++;
            Console.WriteLine($"{count,4}. {name}");
        }
    }

    static void SearchPeople(FaceRecognitionDbReader dbReader)
    {
        Console.Write("\nEnter search term: ");
        string? searchTerm = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            Console.WriteLine("Search term cannot be empty.");
            return;
        }

        var results = dbReader.SearchPeople(searchTerm);

        if (results.Count == 0)
        {
            Console.WriteLine($"\nNo people found matching '{searchTerm}'");
        }
        else
        {
            Console.WriteLine($"\nFound {results.Count} people matching '{searchTerm}':\n");

            int count = 0;
            foreach (var name in results)
            {
                count++;
                Console.WriteLine($"{count,4}. {name}");
            }
        }
    }

    static void FindImagesByPerson(FaceRecognitionDbReader dbReader)
    {
        Console.Write("\nEnter person name (or partial name): ");
        string? personName = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(personName))
        {
            Console.WriteLine("Person name cannot be empty.");
            return;
        }

        var images = dbReader.GetImagesByPersonName(personName);

        if (images.Count == 0)
        {
            Console.WriteLine($"\nNo images found for '{personName}'");
            Console.WriteLine("Try searching for the person first (option 2) to see available names.");
        }
        else
        {
            Console.WriteLine($"\n=== Found {images.Count} images with '{personName}' ===\n");

            for (int i = 0; i < images.Count; i++)
            {
                var image = images[i];
                Console.WriteLine($"{i + 1,4}. {image.Name}");
                Console.WriteLine($"      Path: {image.FullPath}");
                Console.WriteLine($"      Size: {FormatFileSize(image.FileSize)}");
                Console.WriteLine($"      Date Taken: {image.ModificationDate}");
                Console.WriteLine();

                // Pause every 20 images
                if ((i + 1) % 20 == 0 && i < images.Count - 1)
                {
                    Console.Write("Press Enter to continue or 'q' to stop: ");
                    var key = Console.ReadLine();
                    if (key?.ToLower() == "q")
                        break;
                }
            }

            // Option to export to file
            Console.Write("\nExport image paths to file? (y/n): ");
            if (Console.ReadLine()?.ToLower() == "y")
            {
                string fileName = $"images_{personName.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                File.WriteAllLines(fileName, images.Select(img => img.FullPath ?? img.Name));
                Console.WriteLine($"Exported to: {Path.GetFullPath(fileName)}");
            }
        }
    }

    static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}