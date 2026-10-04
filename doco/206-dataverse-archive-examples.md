# Dataverse techniques beyond WPFTools

Revised: 4 October 2026. This document preserves the selected techniques as self-contained examples. It does not depend on the historical source archive: deleting `temp/` does not remove any code or explanation needed below.

## 1. What is preserved and how to use it

WPFTools already demonstrates selected filters, an organisation inner join, aliased output, automatic local paging, state labels, simulated retries and typed SDK LINQ. The additional techniques below remain documentation examples because they involve broader metadata, server writes, live identity or model-driven browser APIs. No WPFTools changes are required.

The C# listing in section 2 is one complete reference class, including imports, method parameters and helper logic. It uses the Microsoft.PowerPlatform.Dataverse.Client SDK already referenced by the repository. Copy it into a .NET 10 project referencing that SDK to use the helpers. It is a library of examples, not a standalone executable. Methods accepting IOrganizationService need an implementation supporting the corresponding request; WPFTools' deliberately bounded fake rejects most of them.

Fictional schema assumptions: `acme_assessment` has `acme_name`, `statecode`, `modifiedon` and an `acme_folder` lookup; `acme_folder` has an `acme_site` lookup; `acme_site` has `acme_url`; `acme_account` has an `acme_organisation` lookup. The custom picklist is `acme_category`. These extra entities/columns/actions are not deployed by this repository. Browser snippets belong inside a model-driven app or an embedded resource with access to its Xrm object. Example state/status values must come from the target schema.

These are adapted reference examples, not copied historical business code or claims of successful live integration. Historical credentials, business identities and record IDs are not required.

## 2. Complete SDK reference class

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;

public static class AcmeDataverseExamples
{
    // Caller owns/disposes the returned client. Supply connection details externally.
    public static ServiceClient Connect(string connectionString)
    {
        var client = new ServiceClient(connectionString);
        if (client.IsReady) return client;
        var error = client.LastError;
        client.Dispose();
        throw new InvalidOperationException(error);
    }

    public static WhoAmIResponse CallerIdentity(IOrganizationService service) =>
        (WhoAmIResponse)service.Execute(new WhoAmIRequest());

    public static EntityMetadata[] CustomEntityCatalog(IOrganizationService service)
    {
        var response = (RetrieveAllEntitiesResponse)service.Execute(new RetrieveAllEntitiesRequest
        {
            EntityFilters = EntityFilters.Entity, RetrieveAsIfPublished = true
        });
        return response.EntityMetadata.Where(entity => entity.IsCustomEntity == true).ToArray();
    }

    public static AttributeMetadata[] AttributeCatalog(IOrganizationService service, string entityName)
    {
        var response = (RetrieveEntityResponse)service.Execute(new RetrieveEntityRequest
        {
            LogicalName = entityName, EntityFilters = EntityFilters.Attributes,
            RetrieveAsIfPublished = false
        });
        return response.EntityMetadata.Attributes;
    }

    public static string ReadLabel(Label? label, int languageCode = 1033) =>
        label?.LocalizedLabels.FirstOrDefault(item => item.LanguageCode == languageCode)?.Label
        ?? label?.UserLocalizedLabel?.Label
        ?? label?.LocalizedLabels.FirstOrDefault()?.Label
        ?? "(unlabelled)";

    public static IReadOnlyList<(int Value, string Label)> PicklistChoices(
        IOrganizationService service, string entityName, string attributeName,
        int languageCode = 1033, bool includeUnpublished = false)
    {
        var response = (RetrieveAttributeResponse)service.Execute(new RetrieveAttributeRequest
        {
            EntityLogicalName = entityName, LogicalName = attributeName,
            RetrieveAsIfPublished = includeUnpublished
        });
        if (response.AttributeMetadata is not PicklistAttributeMetadata picklist)
            throw new InvalidOperationException("The requested field is not a picklist.");
        return picklist.OptionSet.Options.Where(option => option.Value.HasValue)
            .Select(option => (option.Value!.Value, ReadLabel(option.Label, languageCode))).ToList();
    }

    public static Entity? FollowLookup(IOrganizationService service, string entityName,
        Guid id, string lookupName, params string[] relatedColumns)
    {
        var root = service.Retrieve(entityName, id, new ColumnSet(lookupName));
        var reference = root.GetAttributeValue<EntityReference>(lookupName);
        return reference == null ? null : service.Retrieve(reference.LogicalName, reference.Id,
            new ColumnSet(relatedColumns));
    }

    public static Entity? NewestNamedAssessment(IOrganizationService service, string pattern)
    {
        var query = new QueryExpression("acme_assessment")
        {
            ColumnSet = new ColumnSet("acme_name", "modifiedon"), TopCount = 1
        };
        query.Criteria.AddCondition("acme_name", ConditionOperator.Like, pattern);
        query.AddOrder("modifiedon", OrderType.Descending);
        query.AddOrder("acme_assessmentid", OrderType.Ascending); // Tie breaker.
        return service.RetrieveMultiple(query).Entities.FirstOrDefault();
    }

    public static EntityCollection AssessmentsWithSite(IOrganizationService service)
    {
        var query = new QueryExpression("acme_assessment") { ColumnSet = new ColumnSet("acme_name") };
        var folder = query.AddLink("acme_folder", "acme_folder", "acme_folderid", JoinOperator.Inner);
        folder.EntityAlias = "folder";
        var site = folder.AddLink("acme_site", "acme_site", "acme_siteid", JoinOperator.Inner);
        site.EntityAlias = "site";
        site.Columns = new ColumnSet("acme_url");
        return service.RetrieveMultiple(query);
    }

    private static object? Comparable(object? value) => value switch
    {
        OptionSetValue choice => choice.Value,
        Money money => money.Value,
        EntityReference reference => (reference.LogicalName, reference.Id),
        _ => value
    };

    // Only attributes supplied in desired are compared. Explicit null clears a value;
    // an omitted attribute leaves it alone. Retrieve all desired fields in existing.
    public static Entity? ChangedAttributes(Entity desired, Entity existing)
    {
        if (desired.LogicalName != existing.LogicalName)
            throw new ArgumentException("Entities must have the same logical name.");
        var update = new Entity(existing.LogicalName, existing.Id);
        foreach (var attribute in desired.Attributes)
        {
            existing.Attributes.TryGetValue(attribute.Key, out var oldValue);
            if (!Equals(Comparable(attribute.Value), Comparable(oldValue)))
                update[attribute.Key] = attribute.Value;
        }
        return update.Attributes.Count == 0 ? null : update;
    }

    // A caller-provided null means it has already established that no target exists.
    // This is application reconciliation, not platform MergeRequest or UpsertRequest.
    public static OrganizationRequest? PlanWrite(Entity desired, Entity? existing) =>
        existing == null ? new CreateRequest { Target = desired }
        : ChangedAttributes(desired, existing) is { } changed
            ? new UpdateRequest { Target = changed } : null;

    public static IReadOnlyList<OrganizationRequest> PlanReconciliation(
        IEnumerable<Entity> desiredRows, IEnumerable<Entity> existingRows,
        Func<Entity, string> businessKey, bool removeUnmatched = false)
    {
        // Supply complete, explicitly scoped target rows. Duplicate keys fail instead
        // of silently picking/deleting duplicates as the historical helper could do.
        var desired = desiredRows.ToDictionary(businessKey, StringComparer.Ordinal);
        var existing = existingRows.ToDictionary(businessKey, StringComparer.Ordinal);
        var requests = new List<OrganizationRequest>();
        foreach (var pair in desired)
        {
            existing.TryGetValue(pair.Key, out var target);
            var write = PlanWrite(pair.Value, target);
            if (write != null) requests.Add(write);
        }
        if (removeUnmatched)
            foreach (var pair in existing.Where(pair => !desired.ContainsKey(pair.Key)))
                requests.Add(new DeleteRequest { Target = pair.Value.ToEntityReference() });
        return requests;
    }

    public static void ExecuteBatches(IOrganizationService service,
        IEnumerable<OrganizationRequest> requests, int batchSize = 100)
    {
        if (batchSize < 1 || batchSize > 100) throw new ArgumentOutOfRangeException(nameof(batchSize));
        foreach (var chunk in requests.Chunk(batchSize))
        {
            var batch = new ExecuteMultipleRequest
            {
                Settings = new ExecuteMultipleSettings { ContinueOnError = false, ReturnResponses = true },
                Requests = new OrganizationRequestCollection()
            };
            foreach (var request in chunk) batch.Requests.Add(request);
            var response = (ExecuteMultipleResponse)service.Execute(batch);
            var failure = response.Responses.FirstOrDefault(item => item.Fault != null);
            if (failure != null)
                throw new InvalidOperationException($"Batch request {failure.RequestIndex} failed: {failure.Fault.Message}");
        }
    }

    // Historical SDK request technique; caller supplies valid target state/status.
    public static void ChangeState(IOrganizationService service, EntityReference record,
        int state, int status) => service.Execute(new SetStateRequest
        {
            EntityMoniker = record, State = new OptionSetValue(state), Status = new OptionSetValue(status)
        });

    public static OrganizationResponse ProcessAssessments(IOrganizationService service, IEnumerable<Guid> ids)
    {
        var request = new OrganizationRequest("acme_ProcessAssessments");
        request.Parameters["RecordIds"] = string.Join(",", ids);
        return service.Execute(request);
    }
}
```

## 3. Metadata discovery and localization

CustomEntityCatalog returns custom entity definitions, not entity rows. ReadLabel can display their DisplayName and Description. AttributeCatalog returns a field list; check `attributes.Any(a => a.LogicalName == "acme_category")` to establish schema existence even when every record has a null value. Checking a retrieved row's Attributes dictionary only establishes whether a value was returned.

PicklistChoices supports a custom picklist and a chosen language, with fallback labels. Passing includeUnpublished changes the metadata request, not the row query. This preserves the useful distinction between published definitions and development-time definitions. WPFTools supports only a fixed published state field in English.

Expected example: a fixture picklist with values 10/20 and labels Standard/Priority would produce two numeric/label pairs. A missing requested-language label falls back to the current user's label, then the first available label, then `(unlabelled)`. A state field is not a PicklistAttributeMetadata; the helper rejects that mismatch.

## 4. Connection identity and lookup traversal

Connect accepts configuration supplied by the caller and checks readiness; dispose its result with `using`. CallerIdentity returns the SDK WhoAmI response, including caller identity fields. This is a live-service diagnostic, not something the desktop fake currently emulates. No secret or connection string is embedded here.

FollowLookup preserves both LogicalName and ID when following a reference. For example, call it with an account ID, `acme_organisation` and `acme_name`. A null lookup returns null; a missing referenced row produces the service's normal error. Polymorphic parent references can target different entity names, so using the reference's name matters.

## 5. Ordering, TopCount and nested joins

NewestNamedAssessment selects at most one row matching a Like pattern such as `ACME%`, orders newest first and uses the primary key to break equal timestamps. It returns null for no match. These are explicit query behaviors, beyond WPFTools' fixed ID ordering. Other archived examples used NotEqual to exclude draft status; add that condition only with a schema-defined value.

AssessmentsWithSite demonstrates assessment -> folder -> site. Both links are inner joins, so missing related records exclude a root row. Read the selected related URL with `row.GetAttributeValue<AliasedValue>("site.acme_url")?.Value`. This preserves the document-location hierarchy technique independently of any SharePoint implementation or historical schema.

## 6. Differential writes and batched reconciliation

ChangedAttributes compares supplied scalar fields, numeric choices, Money values and lookup name/ID pairs. An unchanged desired value emits no update; an explicit null can clear a previously populated field. Multiselect collections and other complex values need their own comparison policy. Pass only writable fields and retrieve those fields before comparing; this helper does not discover write permissions or calculate schema rules.

PlanReconciliation matches desired and existing rows by a caller-defined key, for example `row.GetAttributeValue<string>("acme_externalkey")`. New keys produce CreateRequest, changed keys UpdateRequest, unchanged rows no request. Deletion is opt-in and applies only to targets in the caller's supplied scope. Duplicate keys throw. This is an adapted preservation of the original matching/delta/batching technique; it deliberately avoids silent duplicate deletion. It is not platform merge/upsert behavior.

Concrete case: existing keys A/B have names Old/Keep; desired keys A/C have names New/Create. With deletion disabled, the plan is update A and create C; B remains. With removeUnmatched enabled, it additionally deletes B. Equal source/target values produce an empty plan.

ExecuteBatches chunks that plan, requests responses and stops when a request fault is reported. Its limit of 100 is a local example policy, not a claim about the server's maximum. Successful writes before a fault remain applied: ExecuteMultiple is not an atomic transaction. The helper does not automatically retry writes, and a connection failure can leave an uncertain outcome requiring reconciliation.

## 7. State/status changes and custom messages

ChangeState preserves the historical SetStateRequest technique: changing state and status is different from displaying or filtering their numeric values. Both values must belong to the target entity's definitions. This example preserves the request shape rather than prescribing an API migration strategy.

ProcessAssessments preserves custom-message invocation with named inputs. It requires a matching server action called `acme_ProcessAssessments` accepting RecordIds; this repository does not create that action. An action response's expected output keys form part of its server contract.

Generated message proxies can wrap those parameter dictionaries with typed properties. For example, a standalone shape is:

```csharp
public sealed class AcmeProcessRequest : Microsoft.Xrm.Sdk.OrganizationRequest
{
    public AcmeProcessRequest() { RequestName = "acme_ProcessAssessments"; }
    public string RecordIds
    {
        get => Parameters.TryGetValue("RecordIds", out var value) ? (string)value : "";
        set => Parameters["RecordIds"] = value;
    }
}
```

The historical generated searchquery/searchautocomplete proxies additionally exposed search parameters such as top/skip or filter/entities/options/fuzzy. Their presence established request shapes, not proof of execution. Preserve that distinction when adapting a generated contract. WPFTools' models are handwritten fixtures, not evidence of live generation.

## 8. FetchXML with an outer join

Run this function in a model-driven browser context, passing its Xrm object. The fictional schema needs an assessment organisation lookup and organisation name.

```javascript
async function assessmentNamesWithOptionalOrganisation(xrm) {
  const fetch = "<fetch><entity name='acme_assessment'>" +
    "<attribute name='acme_name'/>" +
    "<link-entity name='acme_organisation' from='acme_organisationid' " +
    "to='acme_organisation' link-type='outer' alias='org'>" +
    "<attribute name='acme_name'/></link-entity></entity></fetch>";
  const result = await xrm.WebApi.retrieveMultipleRecords(
    "acme_assessment", "?fetchXml=" + encodeURIComponent(fetch));
  return {
    rows: result.entities.map(row => ({
      name: row.acme_name,
      organisation: row["org.acme_name"] ?? "No related organisation"
    })),
    nextLink: result.nextLink
  };
}
```

An outer join retains an assessment when its organisation is absent. This example returns one response page and exposes nextLink; it is not a retrieve-all implementation. FetchXML is a different query representation from QueryExpression, and WPFTools intentionally rejects it. No interpolation of record IDs or names into XML is needed by this example.

## 9. Browser record writes and action execution

These functions accept an explicit Xrm object, so the caller chooses `Xrm` in a form or `parent.Xrm` in an appropriately hosted resource. Errors reject their promises rather than disappearing inside a callback.

```javascript
async function renameAssessment(xrm, id, newName) {
  const before = await xrm.WebApi.retrieveRecord("acme_assessment", id, "?$select=acme_name");
  await xrm.WebApi.updateRecord("acme_assessment", id, { acme_name: newName });
  return { previousName: before.acme_name, submittedName: newName };
}

async function processSelectedAssessments(xrm, selectedIds) {
  if (selectedIds.length === 0) throw new Error("Select at least one assessment.");
  const request = {
    RecordIds: selectedIds.join(","),
    getMetadata: () => ({
      boundParameter: null,
      parameterTypes: { RecordIds: { typeName: "Edm.String", structuralProperty: 1 } },
      operationType: 0, operationName: "acme_ProcessAssessments"
    })
  };
  const response = await xrm.WebApi.online.execute(request);
  if (!response.ok) throw new Error(`Action failed: ${response.status}`);
  return response.status === 204 ? null : response.json();
}
```

renameAssessment shows record lookup and a partial update. processSelectedAssessments shows an unbound action metadata contract, selected-grid input and response handling. Its fictional server action must exist; naming a request does not install it. The response example assumes JSON when a body is returned.

## 10. Model-driven form behavior and navigation

The following event-handler example uses the CRM form context. It changes tab visibility, disables a control and displays a dialog. Names must match the target form.

```javascript
async function showAssessmentReadOnlyNotice(executionContext, xrm) {
  const form = executionContext.getFormContext();
  form.ui.tabs.get("acme_documents")?.setVisible(true);
  form.getControl("acme_decision")?.setDisabled(true);
  await xrm.Navigation.openAlertDialog({ text: "This assessment is read-only." });
}

async function openAssessmentDialog(xrm, assessmentId) {
  await xrm.Navigation.navigateTo(
    { pageType: "entityrecord", entityName: "acme_assessment", entityId: assessmentId },
    { target: 2, width: { value: 80, unit: "%" } }
  );
}
```

These are browser/model-driven interactions, not WPF control behavior. Some host/context concepts have a local counterpart in DynamicQuestions, but its fake host does not execute the real CRM APIs. A disabled control alone does not establish server-side access control.

## 11. Model-generation settings preserved as an example

The relevant configuration technique can be understood without any historical settings file:

```json
{
  "entityNamesFilter": ["acme_assessment", "acme_organisation"],
  "entityTypesFolder": "Entities",
  "generateGlobalOptionSets": false,
  "generateSdkMessages": true,
  "messageNamesFilter": ["acme_ProcessAssessments"],
  "messagesTypesFolder": "Messages",
  "optionSetsTypesFolder": "OptionSets",
  "namespace": "Jolisoft.Dataverse.Model",
  "serviceContextName": "JolisoftContext",
  "language": "CS"
}
```

The preserved choices are a narrow entity list, selected SDK messages, separated output folders and a named namespace/context. A wildcard entity filter was another historical variant; explicit names keep an example small. This is a model-builder settings shape, not a tested generation command or a generated output. Actual generation requires suitable environment metadata and a compatible model-builder tool. The typed WPFTools fixtures remain clearly identified as handwritten.

## 12. Validation and preservation boundary

Validation on 4 October 2026: extracted the C# reference class and request wrapper directly from this document and built them against the repository's installed SDK; zero warnings and zero errors. Extracted all browser functions and passed `node --check`. Parsed the JSON settings example successfully. Temporary verification outputs are under ignored artifacts/dataverse-reference-check; they are not needed to use the document. Compilation establishes syntax and API availability, not successful server behavior. Browser functions require the stated CRM context/schema to run. Fictional extra entities and the action implementation are not deployed by this reference document.

The original scan covered all seven archive families and found 315 keyword candidates across five of them, followed by selected code inspection. It excluded dependency/build noise and did not exhaustively inspect ZIPs, images, PDFs or every generated file. Some metadata experiments were commented or unreachable. An old IOrganizationService fake consisted of throwing stubs; its Associate/Disassociate signatures were not working examples. Working implementations of plugins, impersonation, optimistic concurrency and atomic transaction requests were not confirmed by that pass.

This document preserves the selected additional techniques, including explanations and replacement examples, without requiring access to the archive. It does not establish that every possible technique in that archive has been salvaged. No historical paths are needed to use it, and no further WPFTools UI or emulator features were added.
