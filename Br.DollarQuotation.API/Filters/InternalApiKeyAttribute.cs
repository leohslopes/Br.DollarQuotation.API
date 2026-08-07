using Microsoft.AspNetCore.Mvc;

namespace Br.DollarQuotation.API.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class InternalApiKeyAttribute : TypeFilterAttribute
{
    public InternalApiKeyAttribute() : base(typeof(InternalApiKeyFilter))
    {

    }
}