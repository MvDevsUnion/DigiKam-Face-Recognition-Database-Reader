namespace facerecog_Db_reader.Models;

public class Tag
{
    public int Id { get; set; }
    public int Pid { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Icon { get; set; }
    public string? IconKde { get; set; }
}
