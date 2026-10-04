using System.Windows;
using Jolisoft.WPFTools.Core;

namespace Jolisoft.WPFTools;

public partial class RetryWindow : Window
{
    private readonly CancellationTokenSource lifetime = new();
    public RetryWindow()
    {
        InitializeComponent();
        Closed += (_, _) => lifetime.Cancel();
    }

    private async void RunRetryScenario(object sender, RoutedEventArgs e)
    {
        RunScenario.IsEnabled = false;
        Scenario.IsEnabled = false;
        Trace.Clear();
        Outcome.Text = "Running…";
        try
        {
            var result = await RetryExamples.RunAsync((RetryScenario)Scenario.SelectedIndex, message =>
            {
                Trace.AppendText(message + Environment.NewLine);
                Trace.ScrollToEnd();
            }, cancellationToken: lifetime.Token);
            Outcome.Text = result.Message;
        }
        catch (OperationCanceledException) { Outcome.Text = "Cancelled."; }
        catch (Exception error) { Outcome.Text = error.Message; }
        finally { RunScenario.IsEnabled = true; Scenario.IsEnabled = true; }
    }
}
