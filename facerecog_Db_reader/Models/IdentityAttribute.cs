namespace facerecog_Db_reader.Models;

public class IdentityAttribute
{
    public int Id { get; set; }
    public string Attribute { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
