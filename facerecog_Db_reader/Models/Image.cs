namespace facerecog_Db_reader.Models;

public class Image
{
    public int Id { get; set; }
    public int Album { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Status { get; set; }
    public int Category { get; set; }
    public DateTime? ModificationDate { get; set; }
    public long FileSize { get; set; }
    public string? UniqueHash { get; set; }
    public string? FullPath { get; set; }
}
