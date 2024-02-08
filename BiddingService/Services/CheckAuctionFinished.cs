using BiddingService.Enums;
using BiddingService.Models;
using Contracts;
using MassTransit;
using MongoDB.Entities;

namespace BiddingService.Services;

public class CheckAuctionFinished(ILogger<CheckAuctionFinished> logger, IServiceProvider services)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Starting check for finished auctions");

        stoppingToken.Register(() => logger.LogInformation("===> Auction check is stopping"));

        while (!stoppingToken.IsCancellationRequested)
        {
            await CheckAuctions(stoppingToken);

            await Task.Delay(5000, stoppingToken);
        }
    }

    private async Task CheckAuctions(CancellationToken stoppingToken)
    {
        List<Auction> finishedAuctions = await DB.Find<Auction>()
            .Match(x => x.AuctionEnd <= DateTime.UtcNow)
            .Match(x => !x.Finished)
            .ExecuteAsync(stoppingToken);
        
        if (finishedAuctions.Count == 0) return;
        
        logger.LogInformation("==> Found {count} auctions that have completed", finishedAuctions.Count);

        using IServiceScope scope = services.CreateScope();
        IPublishEndpoint endpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        foreach (Auction finishedAuction in finishedAuctions)
        {
            finishedAuction.Finished = true;

            await finishedAuction.SaveAsync(null, stoppingToken);

            Bid winningBid = await DB.Find<Bid>()
                .Match(a => a.AuctionId == finishedAuction.ID)
                .Match(b => b.BidStatus == BidStatus.Accepted)
                .Sort(x => x.Descending(s => s.Amount))
                .ExecuteFirstAsync(stoppingToken);

            await endpoint.Publish(new AuctionFinished
            {
                ItemSold = winningBid != null,
                AuctionId = finishedAuction.ID,
                Winner = winningBid?.Bidder,
                Amount = winningBid?.Amount,
                Seller = finishedAuction.Seller
            }, stoppingToken);
        }
    }
}