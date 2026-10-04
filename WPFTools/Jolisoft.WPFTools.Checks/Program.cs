using Jolisoft.WPFTools.Core;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;

var service = new LocalOrganizationService(AssessmentExamples.Fixtures());
void Check(bool condition, string message) { if (!condition) throw new Exception(message); Console.WriteLine($"PASS: {message}"); }
var current = service.RetrieveMultiple(AssessmentExamples.Query(true, true));
Check(current.Entities.Select(row => row.Id.ToString()).SequenceEqual(new[] { "00000000-0000-0000-0000-000000000001", "00000000-0000-0000-0000-000000000002", "00000000-0000-0000-0000-000000000005" }), "active/current excludes expired and inactive, includes null expiry and boundary date");
Check(service.RetrieveMultiple(AssessmentExamples.Query(false, false)).Entities.Count == 5, "removing filters changes results");
Check(service.RetrieveMultiple(AssessmentExamples.Query(true, false)).Entities.Count == 4, "active filter excludes inactive record");
Check(current.Entities.All(row => !row.Attributes.ContainsKey("acme_assessmentid")), "column projection omits unrequested attributes");
var wrapped = (RetrieveMultipleResponse)service.Execute(new RetrieveMultipleRequest { Query = AssessmentExamples.Query(true, true) });
Check(wrapped.EntityCollection.Entities.Count == 3, "SDK Execute request dispatch returns query results");
var invalid = AssessmentExamples.Query(true, false);
invalid.Criteria.AddCondition("acme_name", ConditionOperator.Like, "ACME%");
try { service.RetrieveMultiple(invalid); throw new Exception("Unsupported operator was silently accepted."); }
catch (NotSupportedException) { Console.WriteLine("PASS: unsupported operator rejected explicitly"); }
try { service.RetrieveMultiple(new FetchExpression("<fetch />")); throw new Exception("FetchXML was silently accepted."); }
catch (NotSupportedException) { Console.WriteLine("PASS: unsupported FetchXML rejected explicitly"); }

var joinService = new LocalOrganizationService(AssessmentExamples.JoinFixtures());
var joinedQuery = AssessmentExamples.JoinedQuery(true, true);
var joined = joinService.RetrieveMultiple(joinedQuery);
Check(joined.Entities.Select(row => row.Id).SequenceEqual(current.Entities.Select(row => row.Id)), "join excludes inactive, unrelated, and missing organisations while retaining qualifying assessments");
Check(joined.Entities.All(row => row.GetAttributeValue<Microsoft.Xrm.Sdk.AliasedValue>("organisation.acme_name") is { EntityLogicalName: "acme_organisation", AttributeLogicalName: "acme_name", Value: "ACME primary organisation" }), "joined names retain SDK AliasedValue metadata");
var allowInactive = AssessmentExamples.JoinedQuery(true, true);
allowInactive.LinkEntities[0].LinkCriteria.Conditions.RemoveAt(0);
Check(joinService.RetrieveMultiple(allowInactive).Entities.Any(row => row.Id.ToString().EndsWith("000006")), "removing linked active filter admits the inactive organisation assessment");
var otherOrganisation = joinService.RetrieveMultiple(AssessmentExamples.JoinedQuery(true, true, "ACME-LOCAL-OTHER"));
Check(otherOrganisation.Entities.Count == 1 && otherOrganisation.Entities[0].Id.ToString().EndsWith("000007"), "changing linked registration selects a different organisation's assessment");
var allOrganisations = AssessmentExamples.JoinedQuery(true, true);
allOrganisations.LinkEntities[0].LinkCriteria.Conditions.Clear();
var allJoined = joinService.RetrieveMultiple(allOrganisations);
Check(allJoined.Entities.Count == 5 && allJoined.Entities.All(row => !row.Id.ToString().EndsWith("000008")), "inner join excludes missing lookup targets even without linked filters");
var renamed = AssessmentExamples.JoinedQuery(true, true);
renamed.LinkEntities[0].EntityAlias = "owner";
renamed.LinkEntities[0].Columns = new ColumnSet("acme_name");
Check(joinService.RetrieveMultiple(renamed).Entities.All(row => row.Attributes.ContainsKey("owner.acme_name") && !row.Attributes.ContainsKey("organisation.acme_name") && !row.Attributes.ContainsKey("owner.acme_registrationnumber")), "linked alias and requested columns determine projected attributes");
var unsupportedJoin = AssessmentExamples.JoinedQuery(true, true);
unsupportedJoin.LinkEntities[0].JoinOperator = JoinOperator.LeftOuter;
unsupportedJoin.Criteria.AddCondition("acme_name", ConditionOperator.Equal, "No matching row");
try { joinService.RetrieveMultiple(unsupportedJoin); throw new Exception("Unsupported join was silently accepted."); }
catch (NotSupportedException) { Console.WriteLine("PASS: unsupported outer join rejected even when base filters match no rows"); }

var pagedQuery = AssessmentExamples.JoinedQuery(true, true);
var originalPaging = pagedQuery.PageInfo;
var paged = QueryPaging.RetrieveAll(joinService, pagedQuery);
Check(paged.PagesRead == 2 && paged.Records.Select(row => row.Id).SequenceEqual(joined.Entities.Select(row => row.Id)), "retrieve-all joins two pages without missing or duplicate rows");
Check(ReferenceEquals(originalPaging, pagedQuery.PageInfo), "retrieve-all restores caller paging options");
var exactPages = QueryPaging.RetrieveAll(joinService, AssessmentExamples.Query(false, false));
Check(exactPages.PagesRead == 4 && exactPages.Records.Count == 8, "exact page boundary stops without an extra request");
var reversedService = new LocalOrganizationService(AssessmentExamples.JoinFixtures().Reverse());
Check(QueryPaging.RetrieveAll(reversedService, AssessmentExamples.Query(false, false)).Records.Select(row => row.Id).SequenceEqual(exactPages.Records.Select(row => row.Id)), "page ordering is independent of fixture insertion order");
var pageQuery = AssessmentExamples.JoinedQuery(true, true);
pageQuery.PageInfo = new PagingInfo { Count = 2, PageNumber = 1 };
var firstPage = joinService.RetrieveMultiple(pageQuery);
Check(firstPage.Entities.Count == 2 && firstPage.MoreRecords && firstPage.PagingCookie != null, "first page advertises continuation");
pageQuery.PageInfo.PageNumber = 2;
pageQuery.PageInfo.PagingCookie = firstPage.PagingCookie;
var finalPage = joinService.RetrieveMultiple(pageQuery);
Check(finalPage.Entities.Count == 1 && !finalPage.MoreRecords && finalPage.PagingCookie == null, "final partial page ends continuation");
pageQuery.PageInfo.PagingCookie = "invalid";
try { joinService.RetrieveMultiple(pageQuery); throw new Exception("Invalid cookie accepted."); }
catch (NotSupportedException) { Console.WriteLine("PASS: invalid local cookie rejected"); }
var emptyQuery = AssessmentExamples.Query(false, false);
emptyQuery.Criteria.AddCondition("acme_name", ConditionOperator.Equal, "No matching row");
var empty = QueryPaging.RetrieveAll(joinService, emptyQuery);
Check(empty.Records.Count == 0 && empty.PagesRead == 1, "empty query completes in one request");
var failingQuery = AssessmentExamples.Query(false, false);
failingQuery.Criteria.AddCondition("acme_name", ConditionOperator.Like, "%");
var savedPaging = failingQuery.PageInfo;
try { QueryPaging.RetrieveAll(joinService, failingQuery); throw new Exception("Invalid query accepted."); }
catch (NotSupportedException) { Check(ReferenceEquals(savedPaging, failingQuery.PageInfo), "retrieve-all restores caller options after failure"); }

var choiceRequest = new RetrieveAttributeRequest { EntityLogicalName = "acme_assessment", LogicalName = "statecode" };
var choiceResponse = (RetrieveAttributeResponse)joinService.Execute(choiceRequest);
Check(choiceResponse.AttributeMetadata is Microsoft.Xrm.Sdk.Metadata.StateAttributeMetadata { LogicalName: "statecode" }, "metadata request returns SDK state attribute metadata");
var choices = ChoiceExamples.RetrieveStateChoices(joinService);
Check(choices.SequenceEqual(new[] { new ChoiceLabel(0, "Active"), new ChoiceLabel(1, "Inactive") }), "choice helper extracts fixture numeric values and English labels");
var stateMetadata = (Microsoft.Xrm.Sdk.Metadata.StateAttributeMetadata)choiceResponse.AttributeMetadata;
Check(stateMetadata.OptionSet.OptionSetType == Microsoft.Xrm.Sdk.Metadata.OptionSetType.State && stateMetadata.OptionSet.Options.All(option => option.Label.UserLocalizedLabel.LanguageCode == 1033), "choice metadata identifies state options and label language");
Check(exactPages.Records.All(row => choices.Any(choice => choice.Value == row.GetAttributeValue<Microsoft.Xrm.Sdk.OptionSetValue>("statecode").Value)), "every fixture state value has a metadata label");
stateMetadata.OptionSet.Options.Clear();
Check(ChoiceExamples.RetrieveStateChoices(joinService).Count == 2, "caller mutation cannot alter later metadata responses");
foreach (var unsupportedMetadata in new[]
{
    new RetrieveAttributeRequest { EntityLogicalName = "unknown", LogicalName = "statecode" },
    new RetrieveAttributeRequest { EntityLogicalName = "acme_assessment", LogicalName = "acme_name" },
    new RetrieveAttributeRequest { MetadataId = Guid.NewGuid() },
    new RetrieveAttributeRequest { EntityLogicalName = "acme_assessment", LogicalName = "statecode", RetrieveAsIfPublished = true }
})
{
    try { joinService.Execute(unsupportedMetadata); throw new Exception("Unsupported metadata accepted."); }
    catch (NotSupportedException) { Console.WriteLine("PASS: unsupported metadata target or retrieval mode rejected"); }
}

var retryTrace = new List<string>();
var waits = new List<double>();
Task RecordDelay(TimeSpan wait, CancellationToken token) { token.ThrowIfCancellationRequested(); waits.Add(wait.TotalMilliseconds); return Task.CompletedTask; }
var recovered = await RetryExamples.RunAsync(RetryScenario.RecoverAfterTwoFailures, retryTrace.Add, RecordDelay);
Check(recovered is { Succeeded: true, Attempts: 3, Rows: 3 }, "two transient failures recover on third attempt with expected query rows");
Check(waits.SequenceEqual(new double[] { 200, 400 }), "retry waits increase deterministically without sleeping in checks");
Check(retryTrace.Count(line => line.StartsWith("Attempt ")) == 3, "trace records every retry attempt");
waits.Clear();
var exhausted = await RetryExamples.RunAsync(RetryScenario.ExhaustRetries, _ => { }, RecordDelay);
Check(exhausted is { Succeeded: false, Attempts: 3, Rows: 0 } && waits.Count == 2, "retry exhaustion stops after three attempts and never waits after final failure");
waits.Clear();
var permanent = await RetryExamples.RunAsync(RetryScenario.UnsupportedQuery, _ => { }, RecordDelay);
Check(permanent is { Succeeded: false, Attempts: 1, Rows: 0 } && waits.Count == 0, "unsupported SDK query fails once without retry or delay");
Check((await RetryExamples.RunAsync(RetryScenario.RecoverAfterTwoFailures, _ => { }, RecordDelay)).Attempts == 3, "each scenario run resets its failure counter");
var cancellation = new CancellationTokenSource();
var calls = 0;
try
{
    await LocalRetryPolicy.ExecuteAsync<int>(() => { calls++; throw new SimulatedTransientException(); }, 3,
        (_, token) => { cancellation.Cancel(); token.ThrowIfCancellationRequested(); return Task.CompletedTask; },
        _ => { }, cancellation.Token);
    throw new Exception("Cancellation ignored.");
}
catch (OperationCanceledException) { Check(calls == 1, "cancellation during retry wait prevents another attempt"); }
var unexpectedCalls = 0;
try
{
    await LocalRetryPolicy.ExecuteAsync<int>(() => { unexpectedCalls++; throw new InvalidOperationException("Unexpected failure"); }, 3, RecordDelay, _ => { });
    throw new Exception("Unexpected error swallowed.");
}
catch (InvalidOperationException) { Check(unexpectedCalls == 1, "unclassified exceptions propagate without retry"); }

var sdkLinq = LinqExamples.Run(true, true);
Check(sdkLinq.Rows.Count == 6, "real SDK LINQ query returns six active/current assessments");
Check(sdkLinq.Requests.Count == 1 && sdkLinq.Requests[0].EntityName == "acme_assessment", "SDK LINQ reaches local service as QueryExpression");
Check(sdkLinq.Rows.Select(row => row.Id).SequenceEqual(joinService.RetrieveMultiple(AssessmentExamples.Query(true, true)).Entities.Select(row => row.Id)), "typed LINQ matches direct QueryExpression row identities including null and boundary expiry");
Check(LinqExamples.Run(false, false).Rows.Count == 8 && LinqExamples.Run(true, false).Rows.Count == 7 && LinqExamples.Run(false, true).Rows.Count == 7, "SDK LINQ checkbox combinations change results as expected");
Check(sdkLinq.Rows.All(row => !string.IsNullOrWhiteSpace(row.Name)) && sdkLinq.Rows.Any(row => row.ExpiresOn == null), "typed SDK materialization preserves names and nullable expiry");
IEnumerable<ConditionExpression> Conditions(FilterExpression filter) => filter.Conditions.Concat(filter.Filters.SelectMany(Conditions));
var translated = Conditions(sdkLinq.Requests[0].Criteria).ToList();
Check(translated.Any(condition => condition.AttributeName == "statecode" && condition.Operator == ConditionOperator.Equal) && translated.Any(condition => condition.AttributeName == "acme_expireson" && condition.Operator == ConditionOperator.Null) && translated.Any(condition => condition.AttributeName == "acme_expireson" && condition.Operator == ConditionOperator.GreaterEqual), "SDK provider translates typed predicates into equality, null and date conditions");
using (var sdkContext = new Microsoft.Xrm.Sdk.Client.OrganizationServiceContext(new LocalOrganizationService(AssessmentExamples.JoinFixtures(), typedAssessments: true)))
{
    try { sdkContext.CreateQuery<FixtureAssessment>().Where(row => row.Name.Contains("ACME")).ToList(); throw new Exception("Unsupported SDK LINQ operation accepted."); }
    catch (NotSupportedException) { Console.WriteLine("PASS: translated unsupported string operator rejected by bounded evaluator"); }
}

var joinedLinq = LinqExamples.Run(true, true, true);
Check(joinedLinq.Rows.Count == 3 && joinedLinq.Rows.All(row => row.Organisation == "ACME primary organisation"), "typed SDK LINQ organisation join projects related names");
Check(joinedLinq.Rows.Select(row => row.Id).SequenceEqual(joined.Entities.Select(row => row.Id)), "typed LINQ join matches direct SDK join and excludes inactive, unrelated and missing organisations");
var translatedLink = joinedLinq.Requests.Single().LinkEntities.Single();
Check(translatedLink.JoinOperator == JoinOperator.Inner && translatedLink.LinkFromAttributeName == "acme_organisation" && translatedLink.LinkToAttributeName == "acme_organisationid", "real SDK provider translates the typed lookup join");
Check(Conditions(joinedLinq.Requests.Single().Criteria).Any(condition => condition.EntityName == translatedLink.EntityAlias && condition.AttributeName == "acme_registrationnumber"), "SDK join emits aliased organisation predicates");
Check(LinqExamples.Run(false, false, true).Rows.Count == 5 && LinqExamples.Run(true, false, true).Rows.Count == 4 && LinqExamples.Run(false, true, true).Rows.Count == 4, "typed join assessment filters retain linked organisation restrictions");
var alternateJoin = LinqExamples.Run(true, true, true, "ACME-LOCAL-OTHER");
Check(alternateJoin.Rows.Count == 1 && alternateJoin.Rows[0].Id.ToString().EndsWith("000007"), "changing typed join registration selects the alternate organisation");
Check(LinqExamples.Run(true, true, true, "No match").Rows.Count == 0, "typed join handles empty related matches");
var badAlias = AssessmentExamples.JoinedQuery(true, true);
badAlias.Criteria.AddCondition(new ConditionExpression("unknown", "statecode", ConditionOperator.Equal, 0));
try { joinService.RetrieveMultiple(badAlias); throw new Exception("Unknown alias accepted."); }
catch (NotSupportedException) { Console.WriteLine("PASS: unknown condition alias rejected explicitly"); }
