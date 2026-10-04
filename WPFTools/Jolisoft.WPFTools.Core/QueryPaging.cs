using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Jolisoft.WPFTools.Core;

public static class QueryPaging
{
    public static (List<Entity> Records, int PagesRead) RetrieveAll(
        IOrganizationService service, QueryExpression query, int pageSize = 2)
    {
        if (pageSize < 1 || pageSize > 5000) throw new ArgumentOutOfRangeException(nameof(pageSize));
        var original = query.PageInfo;
        query.PageInfo = new PagingInfo { Count = pageSize, PageNumber = 1 };
        try
        {
            var records = new List<Entity>();
            var pages = 0;
            while (true)
            {
                var result = service.RetrieveMultiple(query);
                pages++;
                records.AddRange(result.Entities);
                if (!result.MoreRecords) return (records, pages);
                if (result.Entities.Count == 0 || string.IsNullOrEmpty(result.PagingCookie) ||
                    result.PagingCookie == query.PageInfo.PagingCookie)
                    throw new InvalidOperationException("Service returned an invalid continuation page.");
                query.PageInfo.PageNumber++;
                query.PageInfo.PagingCookie = result.PagingCookie;
            }
        }
        finally { query.PageInfo = original; }
    }
}
