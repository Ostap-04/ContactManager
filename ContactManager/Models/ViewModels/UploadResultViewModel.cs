namespace ContactManager.Models.ViewModels;

public sealed class UploadResultViewModel
{
    public int Imported { get; init; }
    public int Failed { get; init; }
    public List<string> Errors { get; init; } = [];
}