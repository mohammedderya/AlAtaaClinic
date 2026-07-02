namespace AlAtaaClinic.Desktop.Services;

public interface IPrintService
{
    void PrintTextReport(string title, IReadOnlyList<string> lines);
}
