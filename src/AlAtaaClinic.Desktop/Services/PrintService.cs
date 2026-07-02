using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace AlAtaaClinic.Desktop.Services;

public sealed class PrintService : IPrintService
{
    public void PrintTextReport(string title, IReadOnlyList<string> lines)
    {
        var dialog = new PrintDialog();
        if (dialog.ShowDialog() != true)
        {
            return;
        }

        var document = BuildDocument(title, lines);
        dialog.PrintDocument(((IDocumentPaginatorSource)document).DocumentPaginator, title);
    }

    private static FlowDocument BuildDocument(string title, IReadOnlyList<string> lines)
    {
        var document = new FlowDocument
        {
            FontFamily = new FontFamily("Segoe UI"),
            FontSize = 12,
            PagePadding = new Thickness(48),
            ColumnWidth = double.PositiveInfinity
        };

        document.Blocks.Add(new Paragraph(new Run(title))
        {
            FontSize = 22,
            FontWeight = FontWeights.SemiBold,
            TextAlignment = TextAlignment.Right
        });

        foreach (var line in lines)
        {
            document.Blocks.Add(new Paragraph(new Run(line)) { TextAlignment = TextAlignment.Right });
        }

        return document;
    }
}
