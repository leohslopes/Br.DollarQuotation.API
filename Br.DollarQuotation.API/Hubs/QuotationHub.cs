using Br.DollarQuotation.API.Hubs.Clients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Br.DollarQuotation.API.Hubs;

[Authorize]
public sealed class QuotationHub : Hub<IQuotationHubClient>
{
    private readonly ILogger<QuotationHub> _logger;

    public QuotationHub(ILogger<QuotationHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Cliente conectado ao Hub de cotações. ConnectionId: {ConnectionId}", Context.ConnectionId);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (exception is null)
        {
            _logger.LogInformation("Cliente desconectado do Hub de cotações. ConnectionId: {ConnectionId}", Context.ConnectionId);
        }
        else
        {
            _logger.LogWarning(exception, "Cliente desconectado com erro. ConnectionId: {ConnectionId}",Context.ConnectionId);
        }

        await base.OnDisconnectedAsync(exception);
    }
}