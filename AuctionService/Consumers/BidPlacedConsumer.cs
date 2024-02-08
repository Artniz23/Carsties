using AuctionService.Data;
using AuctionService.Entities;
using Contracts;
using MassTransit;

namespace AuctionService.Consumers;

public class BidPlacedConsumer(AuctionDbContext dbContext) : IConsumer<BidPlaced>
{
    public async Task Consume(ConsumeContext<BidPlaced> context)
    {
        Console.WriteLine("--> Consuming bid placed");

        Auction auction = await dbContext.Auctions.FindAsync(Guid.Parse(context.Message.AuctionId));

        if (auction == null) return;

        if (
            auction.CurrentHighBid == null || 
            context.Message.BidStatus.Contains("Accepted") &&
            context.Message.Amount > auction.CurrentHighBid
        )
        {
            auction.CurrentHighBid = context.Message.Amount;

            await dbContext.SaveChangesAsync();
        }
    }
}