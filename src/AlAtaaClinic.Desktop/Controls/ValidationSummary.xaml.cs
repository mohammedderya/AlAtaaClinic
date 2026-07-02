using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace AlAtaaClinic.Desktop.Controls;

public partial class ValidationSummary : UserControl
{
    public static readonly DependencyProperty MessagesProperty = DependencyProperty.Register(
        nameof(Messages),
        typeof(IEnumerable),
        typeof(ValidationSummary));

    public ValidationSummary()
    {
        InitializeComponent();
    }

    public IEnumerable? Messages
    {
        get => (IEnumerable?)GetValue(MessagesProperty);
        set => SetValue(MessagesProperty, value);
    }
}
