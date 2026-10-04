using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;

[assembly: ProxyTypesAssembly]

namespace Jolisoft.WPFTools.Core;

// Handwritten fixture model, not generated from live Dataverse metadata.
[EntityLogicalName("acme_assessment")]
public sealed class FixtureAssessment : Entity
{
    public FixtureAssessment() : base("acme_assessment") { }
    [AttributeLogicalName("acme_name")]
    public string Name => GetAttributeValue<string>("acme_name");
    [AttributeLogicalName("statecode")]
    public OptionSetValue State => GetAttributeValue<OptionSetValue>("statecode");
    [AttributeLogicalName("acme_expireson")]
    public DateTime? ExpiresOn => GetAttributeValue<DateTime?>("acme_expireson");
    [AttributeLogicalName("acme_organisation")]
    public EntityReference Organisation => GetAttributeValue<EntityReference>("acme_organisation");
}

[EntityLogicalName("acme_organisation")]
public sealed class FixtureOrganisation : Entity
{
    public FixtureOrganisation() : base("acme_organisation") { }
    [AttributeLogicalName("acme_organisationid")]
    public Guid OrganisationId => Id;
    [AttributeLogicalName("acme_name")]
    public string Name => GetAttributeValue<string>("acme_name");
    [AttributeLogicalName("statecode")]
    public OptionSetValue State => GetAttributeValue<OptionSetValue>("statecode");
    [AttributeLogicalName("acme_registrationnumber")]
    public string Registration => GetAttributeValue<string>("acme_registrationnumber");
}

public sealed record LinqRow(Guid Id, string Name, int State, DateTime? ExpiresOn, string? Organisation = null);
public sealed record LinqResult(IReadOnlyList<LinqRow> Rows, IReadOnlyList<QueryExpression> Requests);

public static class LinqExamples
{
    public static LinqResult Run(bool activeOnly, bool currentOnly, bool joinOrganisation = false, string registration = "ACME-LOCAL-001")
    {
        var requests = new List<QueryExpression>();
        var service = new LocalOrganizationService(AssessmentExamples.JoinFixtures(), requests.Add, typedAssessments: true);
        using var context = new OrganizationServiceContext(service);
        var query = context.CreateQuery<FixtureAssessment>();
        if (activeOnly) query = query.Where(row => row.State.Value == 0);
        if (currentOnly) query = query.Where(row => row.ExpiresOn == null || row.ExpiresOn >= AssessmentExamples.ReferenceDate);
        List<LinqRow> rows;
        if (joinOrganisation)
        {
            // The SDK provider requires Join before Where; do not materialize to bypass it.
            var joined = context.CreateQuery<FixtureAssessment>()
                .Join(context.CreateQuery<FixtureOrganisation>(), assessment => assessment.Organisation.Id,
                    organisation => organisation.OrganisationId,
                    (assessment, organisation) => new { Assessment = assessment, Organisation = organisation })
                .Where(row => row.Organisation.State.Value == 0)
                .Where(row => row.Organisation.Registration == registration);
            if (activeOnly) joined = joined.Where(row => row.Assessment.State.Value == 0);
            if (currentOnly) joined = joined.Where(row => row.Assessment.ExpiresOn == null || row.Assessment.ExpiresOn >= AssessmentExamples.ReferenceDate);
            rows = joined.ToList().Select(row => new LinqRow(row.Assessment.Id, row.Assessment.Name,
                row.Assessment.State.Value, row.Assessment.ExpiresOn, row.Organisation.Name)).ToList();
        }
        else rows = query.ToList().Select(row => new LinqRow(row.Id, row.Name, row.State.Value, row.ExpiresOn)).ToList();
        return new LinqResult(rows, requests);
    }
}
