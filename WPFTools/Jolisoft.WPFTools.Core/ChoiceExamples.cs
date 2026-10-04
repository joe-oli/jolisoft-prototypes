using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;

namespace Jolisoft.WPFTools.Core;

public sealed record ChoiceLabel(int Value, string Label);

public static class ChoiceExamples
{
    public static IReadOnlyList<ChoiceLabel> RetrieveStateChoices(IOrganizationService service)
    {
        var response = (RetrieveAttributeResponse)service.Execute(new RetrieveAttributeRequest
        {
            EntityLogicalName = "acme_assessment", LogicalName = "statecode"
        });
        var metadata = (StateAttributeMetadata)response.AttributeMetadata;
        return metadata.OptionSet.Options.Select(option => new ChoiceLabel(
            option.Value!.Value, option.Label.UserLocalizedLabel.Label)).ToList();
    }

    internal static RetrieveAttributeResponse RetrieveFixture(RetrieveAttributeRequest request)
    {
        if (request.MetadataId != Guid.Empty || request.RetrieveAsIfPublished ||
            request.EntityLogicalName != "acme_assessment" || request.LogicalName != "statecode")
            throw new NotSupportedException("Only published acme_assessment.statecode fixture metadata by logical name is supported.");

        // Return fresh SDK objects so callers cannot mutate later fixture responses.
        var metadata = new StateAttributeMetadata
        {
            LogicalName = "statecode", DisplayName = FixtureLabel("Assessment state"),
            OptionSet = new OptionSetMetadata
            {
                IsGlobal = false, OptionSetType = OptionSetType.State,
                Options =
                {
                    new OptionMetadata(FixtureLabel("Active"), 0),
                    new OptionMetadata(FixtureLabel("Inactive"), 1)
                }
            }
        };
        return new RetrieveAttributeResponse { Results = { ["AttributeMetadata"] = metadata } };
    }

    private static Label FixtureLabel(string text) => new(text, 1033)
    {
        UserLocalizedLabel = new LocalizedLabel(text, 1033)
    };
}
