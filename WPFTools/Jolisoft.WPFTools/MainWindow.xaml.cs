using System.Windows;
using Jolisoft.WPFTools.Core;
using Microsoft.Xrm.Sdk;

namespace Jolisoft.WPFTools;

public partial class MainWindow : Window
{
    private RetryWindow? retryWindow;
    private LinqWindow? linqWindow;
    private readonly IOrganizationService service = new LocalOrganizationService(AssessmentExamples.JoinFixtures());
    public MainWindow() { InitializeComponent(); RunQuery(this, new RoutedEventArgs()); }
    private void OpenLinq(object sender, RoutedEventArgs e)
    {
        if (linqWindow == null || !linqWindow.IsLoaded)
        {
            linqWindow = new LinqWindow { Owner = this };
            linqWindow.Show();
        }
        else linqWindow.Activate();
    }

    private void OpenRetryScenarios(object sender, RoutedEventArgs e)
    {
        if (retryWindow == null || !retryWindow.IsLoaded)
        {
            retryWindow = new RetryWindow { Owner = this };
            retryWindow.Show();
        }
        else retryWindow.Activate();
    }

    private void RunQuery(object sender, RoutedEventArgs e)
    {
        try
        {
            var query = JoinOrganisation.IsChecked == true
                ? AssessmentExamples.JoinedQuery(ActiveOnly.IsChecked == true, CurrentOnly.IsChecked == true)
                : AssessmentExamples.Query(ActiveOnly.IsChecked == true, CurrentOnly.IsChecked == true);
            QuerySummary.Text = $"QueryExpression: acme_assessment; active filter = {ActiveOnly.IsChecked}; expiry filter = {CurrentOnly.IsChecked}; organisation join = {JoinOrganisation.IsChecked}. The join requires an active organisation with registration ACME-LOCAL-001.";
            var result = QueryPaging.RetrieveAll(service, query);
            var choices = ChoiceExamples.RetrieveStateChoices(service);
            var stateLabels = choices.ToDictionary(choice => choice.Value, choice => choice.Label);
            ChoiceSummary.Text = "Assessment state choices: " +
                string.Join("; ", choices.Select(choice => $"{choice.Value} = {choice.Label}")) +
                ". These fixed local metadata labels are used in the State column.";
            Results.ItemsSource = result.Records.Select(row => new
            {
                Id = row.Id, Name = row.GetAttributeValue<string>("acme_name"),
                State = stateLabels.GetValueOrDefault(row.GetAttributeValue<OptionSetValue>("statecode").Value, "Unknown state"),
                Organisation = row.GetAttributeValue<AliasedValue>("organisation.acme_name")?.Value as string ?? "Not joined",
                Expiry = row.GetAttributeValue<DateTime?>("acme_expireson")?.ToString("yyyy-MM-dd") ?? "No expiry"
            }).ToList();
            Status.Text = $"{result.Records.Count} rows retrieved across {result.PagesRead} pages (2 rows per page). The local service orders by assessment ID and the retrieve-all loop follows continuation cookies.";
        }
        catch (Exception error) { Status.Text = error.Message; }
    }

}
