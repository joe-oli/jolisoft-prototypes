using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;

namespace Jolisoft.WPFTools.Core;

// Deliberately bounded, read-only evaluator. No Dataverse connection is opened.
public sealed class LocalOrganizationService : IOrganizationService
{
    private static readonly Dictionary<string, HashSet<string>> Fields = new()
    {
        ["acme_assessment"] = ["acme_assessmentid", "acme_name", "statecode", "acme_expireson", "acme_organisation"],
        ["acme_organisation"] = ["acme_organisationid", "acme_name", "statecode", "acme_registrationnumber"]
    };
    private readonly List<Entity> records;
    private readonly Action<QueryExpression>? queryObserved;
    private readonly bool typedAssessments;
    public LocalOrganizationService(IEnumerable<Entity> records, Action<QueryExpression>? queryObserved = null, bool typedAssessments = false)
    {
        this.records = records.ToList();
        this.queryObserved = queryObserved;
        this.typedAssessments = typedAssessments;
    }

    public EntityCollection RetrieveMultiple(QueryBase query)
    {
        if (query is not QueryExpression expression) throw Unsupported("Only QueryExpression is supported in this checkpoint.");
        queryObserved?.Invoke(expression);
        if (expression.EntityName != "acme_assessment") throw Unsupported("Unknown fixture entity.");
        if (expression.Orders.Count != 0 || expression.Distinct || expression.TopCount != null || expression.NoLock)
            throw Unsupported("Ordering, distinct, TopCount, and NoLock are not supported yet.");
        var paging = expression.PageInfo;
        if (paging.ReturnTotalRecordCount || paging.Count < 0 || paging.Count > 5000 || paging.PageNumber < 0)
            throw Unsupported("Unsupported paging options; page size must be between 0 and 5000.");
        var pageNumber = Math.Max(1, paging.PageNumber);
        if (paging.Count == 0 && (pageNumber > 1 || paging.PagingCookie != null))
            throw Unsupported("Continuation requires a page size.");
        if (paging.Count > 0 && paging.PagingCookie != (pageNumber == 1 ? null : $"local:{pageNumber - 1}:{paging.Count}"))
            throw Unsupported("Invalid local paging cookie.");
        foreach (var column in expression.ColumnSet.Columns) ValidateField(expression.EntityName, column);
        if (expression.LinkEntities.Count > 1) throw Unsupported("Only one fixture organisation join is supported.");
        var link = expression.LinkEntities.SingleOrDefault();
        if (link != null) ValidateLink(link);
        ValidateFilter(expression.EntityName, expression.Criteria, link);
        var result = new EntityCollection { EntityName = expression.EntityName, MoreRecords = false };
        foreach (var record in records.Where(row => row.LogicalName == expression.EntityName))
        {
            IEnumerable<Entity?> joined = link == null ? new Entity?[] { null } : records.Where(row =>
                row.LogicalName == link.LinkToEntityName &&
                record.GetAttributeValue<EntityReference>(link.LinkFromAttributeName) is { } lookup &&
                lookup.LogicalName == row.LogicalName && lookup.Id == row.Id && Matches(row, link.LinkCriteria)).Select(row => (Entity?)row);
            foreach (var related in joined)
            {
                if (!Matches(record, expression.Criteria, related, link)) continue;
                var projected = new Entity(record.LogicalName, record.Id);
                foreach (var attribute in record.Attributes)
                    if (expression.ColumnSet.AllColumns || expression.ColumnSet.Columns.Contains(attribute.Key))
                        projected[attribute.Key] = attribute.Value;
                if (related != null && link != null)
                    foreach (var attribute in related.Attributes)
                        if (link.Columns.AllColumns || link.Columns.Columns.Contains(attribute.Key))
                            projected[$"{link.EntityAlias}.{attribute.Key}"] = new AliasedValue(related.LogicalName, attribute.Key, attribute.Value);
                result.Entities.Add(typedAssessments ? projected.ToEntity<FixtureAssessment>() : projected);
            }
        }
        // Fixture order never determines page boundaries. These are local tokens,
        // not Dataverse server cookies; keep the query and fixtures fixed between pages.
        var ordered = result.Entities.OrderBy(row => row.Id).ToList();
        if (paging.Count > 0)
        {
            var offset = (long)(pageNumber - 1) * paging.Count;
            result.MoreRecords = ordered.Count > offset + paging.Count;
            result.PagingCookie = result.MoreRecords ? $"local:{pageNumber}:{paging.Count}" : null;
            ordered = offset >= ordered.Count ? [] : ordered.Skip((int)offset).Take(paging.Count).ToList();
        }
        result.Entities.Clear();
        result.Entities.AddRange(ordered);
        return result;
    }

    private static void ValidateLink(LinkEntity link)
    {
        if (link.JoinOperator != JoinOperator.Inner || link.LinkEntities.Count != 0 || link.Orders.Count != 0)
            throw Unsupported("Only a single inner join without nested links or linked ordering is supported.");
        if (link.LinkFromEntityName != "acme_assessment" || link.LinkToEntityName != "acme_organisation" ||
            link.LinkFromAttributeName != "acme_organisation" || link.LinkToAttributeName != "acme_organisationid")
            throw Unsupported("Only the assessment-to-organisation lookup join is supported.");
        if (string.IsNullOrWhiteSpace(link.EntityAlias) || link.EntityAlias.Contains('.'))
            throw Unsupported("The fixture join requires an explicit alias without dots.");
        foreach (var column in link.Columns.Columns) ValidateField(link.LinkToEntityName, column);
        ValidateFilter(link.LinkToEntityName, link.LinkCriteria);
    }
    private static void ValidateField(string entity, string field)
    {
        if (!Fields.TryGetValue(entity, out var fields) || !fields.Contains(field)) throw Unsupported($"Unknown fixture attribute: {entity}.{field}");
    }
    private static void ValidateFilter(string entity, FilterExpression filter, LinkEntity? link = null)
    {
        foreach (var condition in filter.Conditions)
        {
            var target = string.IsNullOrEmpty(condition.EntityName) || condition.EntityName == entity ? entity
                : link != null && condition.EntityName == link.EntityAlias ? link.LinkToEntityName
                : throw Unsupported($"Unknown condition entity or alias: {condition.EntityName}");
            ValidateField(target, condition.AttributeName);
            if (condition.CompareColumns) throw Unsupported("Column-comparison conditions are not supported yet.");
            var arity = condition.Operator switch
            {
                ConditionOperator.Null or ConditionOperator.NotNull => 0,
                ConditionOperator.Equal or ConditionOperator.GreaterEqual or ConditionOperator.LessEqual => 1,
                _ => throw Unsupported($"Unsupported condition: {condition.Operator}")
            };
            if (condition.Values.Count != arity) throw Unsupported("Invalid condition value count.");
            if (condition.Operator is ConditionOperator.GreaterEqual or ConditionOperator.LessEqual &&
                (condition.AttributeName != "acme_expireson" || condition.Values[0] is not DateTime))
                throw Unsupported("Range comparisons support only fixture UTC dates.");
        }
        foreach (var child in filter.Filters) ValidateFilter(entity, child, link);
    }
    private static bool Matches(Entity row, FilterExpression filter, Entity? related = null, LinkEntity? link = null)
    {
        var values = filter.Conditions.Select(condition =>
                link != null && condition.EntityName == link.EntityAlias
                    ? related != null && Matches(related, condition) : Matches(row, condition))
            .Concat(filter.Filters.Select(child => Matches(row, child, related, link))).ToList();
        return values.Count == 0 || (filter.FilterOperator == LogicalOperator.And ? values.All(value => value) : values.Any(value => value));
    }
    private static bool Matches(Entity row, ConditionExpression condition)
    {
        row.Attributes.TryGetValue(condition.AttributeName, out var attribute);
        var value = attribute is OptionSetValue option ? option.Value : attribute;
        return condition.Operator switch
        {
            ConditionOperator.Null => value == null,
            ConditionOperator.NotNull => value != null,
            ConditionOperator.Equal => value != null && Equals(value, condition.Values[0]),
            ConditionOperator.GreaterEqual => value is DateTime date && date >= (DateTime)condition.Values[0],
            ConditionOperator.LessEqual => value is DateTime date && date <= (DateTime)condition.Values[0],
            _ => throw Unsupported("Unsupported condition.")
        };
    }
    private static NotSupportedException Unsupported(string message) => new(message);
    public OrganizationResponse Execute(OrganizationRequest request) => request switch
    {
        RetrieveMultipleRequest retrieval => new RetrieveMultipleResponse { Results = { ["EntityCollection"] = RetrieveMultiple(retrieval.Query) } },
        RetrieveAttributeRequest metadata => ChoiceExamples.RetrieveFixture(metadata),
        _ => throw Unsupported($"Unsupported SDK request: {request.RequestName}")
    };
    public Guid Create(Entity entity) => throw Unsupported("Read-only fixture service.");
    public void Update(Entity entity) => throw Unsupported("Read-only fixture service.");
    public void Delete(string entityName, Guid id) => throw Unsupported("Read-only fixture service.");
    public Entity Retrieve(string entityName, Guid id, ColumnSet columnSet) => throw Unsupported("Single-record retrieval is not supported yet.");
    public void Associate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities) => throw Unsupported("Relationships are not supported yet.");
    public void Disassociate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities) => throw Unsupported("Relationships are not supported yet.");
}
