using Microsoft.Data.Sqlite;
using facerecog_Db_reader.Models;

namespace facerecog_Db_reader;

public class FaceRecognitionDbReader
{
    private readonly string _recognitionDbPath;
    private readonly string _digikamDbPath;
    private readonly string _thumbnailsDbPath;

    public FaceRecognitionDbReader(string baseDir)
    {
        _recognitionDbPath = Path.Combine(baseDir, "recognition.db");
        _digikamDbPath = Path.Combine(baseDir, "digikam4.db");
        _thumbnailsDbPath = Path.Combine(baseDir, "thumbnails-digikam.db");
    }

    // Get all identities with their names from recognition.db
    public List<Identity> GetAllIdentities()
    {
        var identities = new List<Identity>();

        using var connection = new SqliteConnection($"Data Source={_recognitionDbPath};Mode=ReadOnly");
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT i.id, i.type, ia.value as name
            FROM Identities i
            LEFT JOIN IdentityAttributes ia ON i.id = ia.id AND ia.attribute = 'name'
            ORDER BY ia.value;
        ";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            identities.Add(new Identity
            {
                Id = reader.GetInt32(0),
                Type = reader.GetInt32(1),
                Name = reader.IsDBNull(2) ? null : reader.GetString(2)
            });
        }

        return identities;
    }

    // Get all tags (including person tags) from digikam4.db
    public List<Tag> GetAllTags()
    {
        var tags = new List<Tag>();

        using var connection = new SqliteConnection($"Data Source={_digikamDbPath};Mode=ReadOnly");
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT id, pid, name, icon, iconkde
            FROM Tags
            ORDER BY name;
        ";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            tags.Add(new Tag
            {
                Id = reader.GetInt32(0),
                Pid = reader.IsDBNull(1) ? 0 : reader.GetInt32(1),
                Name = reader.GetString(2),
                Icon = reader.IsDBNull(3) ? 0 : reader.GetInt32(3),
                IconKde = reader.IsDBNull(4) ? null : reader.GetString(4)
            });
        }

        return tags;
    }

    // Get images by person name (searches through tags)
    public List<Image> GetImagesByPersonName(string personName)
    {
        var images = new List<Image>();

        using var connection = new SqliteConnection($"Data Source={_digikamDbPath};Mode=ReadOnly");
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT DISTINCT
                i.id,
                i.album,
                i.name,
                i.status,
                i.category,
                ii.creationDate,
                i.fileSize,
                i.uniqueHash,
                ar.specificPath,
                a.relativePath
            FROM Images i
            INNER JOIN ImageTags it ON i.id = it.imageid
            INNER JOIN Tags t ON it.tagid = t.id
            INNER JOIN Albums a ON i.album = a.id
            INNER JOIN AlbumRoots ar ON a.albumRoot = ar.id
            LEFT JOIN ImageInformation ii ON i.id = ii.imageid
            WHERE t.name LIKE @personName
            ORDER BY ii.creationDate DESC;
        ";

        command.Parameters.AddWithValue("@personName", $"%{personName}%");

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            string specificPath = reader.IsDBNull(8) ? "" : reader.GetString(8);
            string relativePath = reader.IsDBNull(9) ? "" : reader.GetString(9);
            string imageName = reader.GetString(2);

            // Build full path
            string fullPath = Path.Combine(specificPath, relativePath, imageName);

            images.Add(new Image
            {
                Id = reader.GetInt32(0),
                Album = reader.GetInt32(1),
                Name = imageName,
                Status = reader.GetInt32(3),
                Category = reader.GetInt32(4),
                ModificationDate = reader.IsDBNull(5) ? null : reader.GetDateTime(5),
                FileSize = reader.GetInt64(6),
                UniqueHash = reader.IsDBNull(7) ? null : reader.GetString(7),
                FullPath = fullPath
            });
        }

        return images;
    }

    // Search for people by name pattern
    public List<string> SearchPeople(string searchPattern)
    {
        var peopleNames = new HashSet<string>();

        // Search in recognition.db identities
        using (var connection = new SqliteConnection($"Data Source={_recognitionDbPath};Mode=ReadOnly"))
        {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT DISTINCT ia.value
                FROM IdentityAttributes ia
                WHERE ia.attribute = 'name' AND ia.value LIKE @pattern
                ORDER BY ia.value;
            ";
            command.Parameters.AddWithValue("@pattern", $"%{searchPattern}%");

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                if (!reader.IsDBNull(0))
                    peopleNames.Add(reader.GetString(0));
            }
        }

        // Search in digikam4.db tags
        using (var connection = new SqliteConnection($"Data Source={_digikamDbPath};Mode=ReadOnly"))
        {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT DISTINCT name
                FROM Tags
                WHERE name LIKE @pattern
                ORDER BY name;
            ";
            command.Parameters.AddWithValue("@pattern", $"%{searchPattern}%");

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                peopleNames.Add(reader.GetString(0));
            }
        }

        return peopleNames.OrderBy(n => n).ToList();
    }

    // Get statistics about the database
    public void PrintStatistics()
    {
        Console.WriteLine("\n=== Database Statistics ===");

        // Recognition DB stats
        using (var connection = new SqliteConnection($"Data Source={_recognitionDbPath};Mode=ReadOnly"))
        {
            connection.Open();

            var identitiesCount = ExecuteScalar<long>(connection, "SELECT COUNT(*) FROM Identities");
            var faceCount = ExecuteScalar<long>(connection, "SELECT COUNT(*) FROM FaceMatrices");
            var namedIdentities = ExecuteScalar<long>(connection,
                "SELECT COUNT(DISTINCT id) FROM IdentityAttributes WHERE attribute = 'name'");

            Console.WriteLine($"\nRecognition Database:");
            Console.WriteLine($"  Total Identities: {identitiesCount}");
            Console.WriteLine($"  Named Identities: {namedIdentities}");
            Console.WriteLine($"  Face Matrices: {faceCount}");
        }

        // DigiKam DB stats
        using (var connection = new SqliteConnection($"Data Source={_digikamDbPath};Mode=ReadOnly"))
        {
            connection.Open();

            var imageCount = ExecuteScalar<long>(connection, "SELECT COUNT(*) FROM Images");
            var albumCount = ExecuteScalar<long>(connection, "SELECT COUNT(*) FROM Albums");
            var tagCount = ExecuteScalar<long>(connection, "SELECT COUNT(*) FROM Tags");
            var taggedImages = ExecuteScalar<long>(connection, "SELECT COUNT(DISTINCT imageid) FROM ImageTags");

            Console.WriteLine($"\nDigiKam Database:");
            Console.WriteLine($"  Total Images: {imageCount}");
            Console.WriteLine($"  Albums: {albumCount}");
            Console.WriteLine($"  Tags: {tagCount}");
            Console.WriteLine($"  Tagged Images: {taggedImages}");
        }
    }

    private T ExecuteScalar<T>(SqliteConnection connection, string query)
    {
        var command = connection.CreateCommand();
        command.CommandText = query;
        var result = command.ExecuteScalar();
        return result != null ? (T)result : default!;
    }
}
