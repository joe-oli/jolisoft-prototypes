using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Jolisoft.WPFTools.Core;

public static class AssessmentExamples
{
    public static readonly DateTime ReferenceDate = new(2026, 10, 3, 0, 0, 0, DateTimeKind.Utc);
    public static IEnumerable<Entity> Fixtures()
    {
        yield return Row(1, "ACME current assessment", 0, ReferenceDate.AddDays(30));
        yield return Row(2, "ACME no expiry", 0, null);
        yield return Row(3, "ACME expired assessment", 0, ReferenceDate.AddDays(-1));
        yield return Row(4, "ACME inactive assessment", 1, ReferenceDate.AddDays(30));
        yield return Row(5, "ACME boundary date", 0, ReferenceDate);
    }
    private static Entity Row(int number, string name, int state, DateTime? expiry)
    {
        var id = Guid.Parse($"00000000-0000-0000-0000-{number:000000000000}");
        var row = new Entity("acme_assessment", id) { ["acme_assessmentid"] = id, ["acme_name"] = name, ["statecode"] = new OptionSetValue(state) };
        if (expiry.HasValue) row["acme_expireson"] = expiry.Value;
        return row;
    }
    public static QueryExpression Query(bool activeOnly, bool currentOnly)
    {
        var query = new QueryExpression("acme_assessment") { ColumnSet = new ColumnSet("acme_name", "statecode", "acme_expireson") };
        if (activeOnly) query.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);
        if (currentOnly)
        {
            var expiry = new FilterExpression(LogicalOperator.Or);
            expiry.AddCondition("acme_expireson", ConditionOperator.Null);
            expiry.AddCondition("acme_expireson", ConditionOperator.GreaterEqual, ReferenceDate);
            query.Criteria.AddFilter(expiry);
        }
        return query;
    }

    public static IEnumerable<Entity> JoinFixtures()
    {
        foreach (var assessment in Fixtures())
        {
            assessment["acme_organisation"] = new EntityReference("acme_organisation", OrganisationId(101));
            yield return assessment;
        }
        foreach (var (number, name, organisation) in new[] {
            (6, "ACME inactive organisation assessment", 102),
            (7, "ACME unrelated organisation assessment", 103),
            (8, "ACME missing organisation assessment", 104) })
        {
            var assessment = Row(number, name, 0, ReferenceDate.AddDays(30));
            assessment["acme_organisation"] = new EntityReference("acme_organisation", OrganisationId(organisation));
            yield return assessment;
        }
        yield return Organisation(101, "ACME primary organisation", 0, "ACME-LOCAL-001");
        yield return Organisation(102, "ACME inactive organisation", 1, "ACME-LOCAL-001");
        yield return Organisation(103, "ACME other organisation", 0, "ACME-LOCAL-OTHER");
    }
    private static Guid OrganisationId(int number) => Guid.Parse($"00000000-0000-0000-0000-{number:000000000000}");
    private static Entity Organisation(int number, string name, int state, string registration)
    {
        var id = OrganisationId(number);
        return new Entity("acme_organisation", id) {
            ["acme_organisationid"] = id, ["acme_name"] = name,
            ["statecode"] = new OptionSetValue(state), ["acme_registrationnumber"] = registration };
    }
    public static QueryExpression JoinedQuery(bool activeOnly, bool currentOnly, string registration = "ACME-LOCAL-001")
    {
        var query = Query(activeOnly, currentOnly);
        var organisation = query.AddLink("acme_organisation", "acme_organisation", "acme_organisationid", JoinOperator.Inner);
        organisation.EntityAlias = "organisation";
        organisation.Columns = new ColumnSet("acme_name", "acme_registrationnumber");
        organisation.LinkCriteria.AddCondition("statecode", ConditionOperator.Equal, 0);
        organisation.LinkCriteria.AddCondition("acme_registrationnumber", ConditionOperator.Equal, registration);
        return query;
    }
}
