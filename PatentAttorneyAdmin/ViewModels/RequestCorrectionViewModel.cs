namespace PatentAttorneyAdmin.ViewModels;

public class RequestCorrectionViewModel
{
    public int Id { get; set; }
    public string StaffComment { get; set; } = string.Empty;
    public List<int> DocumentsToCorrect { get; set; } = new();
    public Dictionary<int, string>? DocumentComments { get; set; }
}
