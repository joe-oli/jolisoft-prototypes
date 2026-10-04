using System.Windows;
using Jolisoft.WPFTools.Core;
using Microsoft.Xrm.Sdk.Query;

namespace Jolisoft.WPFTools;

public partial class LinqWindow : Window
{
    public LinqWindow() { InitializeComponent(); RunLinq(this, new RoutedEventArgs()); }
    private void RunLinq(object sender, RoutedEventArgs e)
    {
        Results.ItemsSource = null;
        RequestTrace.Clear();
        try
        {
            var join = JoinOrganisation.IsChecked == true;
            QueryCode.Text = join
                ? "assessments.Join(organisations, a => a.Organisation.Id, o => o.OrganisationId, ...)\n  .Where(o.State.Value == 0 && o.Registration == ACME-LOCAL-001)\n  + optional assessment active/expiry filters"
                : "context.CreateQuery<FixtureAssessment>()\n  + optional assessment active/expiry Where clauses";
            var result = LinqExamples.Run(ActiveOnly.IsChecked == true, CurrentOnly.IsChecked == true, join);
            Results.ItemsSource = result.Rows.Select(row => new
            {
                row.Id, row.Name, StateValue = row.State,
                Organisation = row.Organisation ?? "Not joined",
                Expiry = row.ExpiresOn?.ToString("yyyy-MM-dd") ?? "No expiry"
            }).ToList();
            RequestTrace.Text = string.Join(Environment.NewLine, result.Requests.Select(query =>
                $"Observed SDK QueryExpression: {query.EntityName}; all columns = {query.ColumnSet.AllColumns}; links = {query.LinkEntities.Count}" +
                Environment.NewLine + Describe(query.Criteria) + Environment.NewLine +
                string.Join(Environment.NewLine, query.LinkEntities.Select(link => $"{link.JoinOperator} {link.LinkToEntityName} as {link.EntityAlias}: {Describe(link.LinkCriteria)}"))));
            Status.Text = $"{result.Rows.Count} rows; {result.Requests.Count} observed SDK request(s). Typed results are materialized before display.";
        }
        catch (Exception error) { Status.Text = error.Message; }
    }
    private static string Describe(FilterExpression filter) => $"{filter.FilterOperator}(" +
        string.Join(", ", filter.Conditions.Select(condition => $"{condition.EntityName}.{condition.AttributeName} {condition.Operator} {string.Join("/", condition.Values)}")
            .Concat(filter.Filters.Select(Describe))) + ")";
}
