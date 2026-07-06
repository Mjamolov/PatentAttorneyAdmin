namespace PatentAttorneyAdmin.Models;

public class ServiceCategory
{
    public int Id { get; set; }
    public string IconClass { get; set; } = "bi-file-earmark-text";
    public string TitleRu { get; set; } = string.Empty;
    public string TitleTj { get; set; } = string.Empty;
    public string DescriptionRu { get; set; } = string.Empty;
    public string DescriptionTj { get; set; } = string.Empty;
    public string? ServiceCode { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
