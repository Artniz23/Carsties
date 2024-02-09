using AuctionService;
using BiddingService.Models;
using Grpc.Net.Client;

namespace BiddingService.Services;

public class GrpcAuctionClient(ILogger<GrpcAuctionClient> logger, IConfiguration config)
{
    public Auction GetAuction(string id)
    {
        logger.LogInformation("Calling GRPC Service");

        GrpcChannel channel = GrpcChannel.ForAddress(config["GrpcAuction"]);

        GrpcAuction.GrpcAuctionClient client = new GrpcAuction.GrpcAuctionClient(channel);

        var request = new GetAuctionRequest
        {
            Id = id
        };

        try
        {
            GrpcAuctionResponse reply = client.GetAuction(request);

            Auction auction = new Auction
            {
                ID = reply.Auction.Id,
                AuctionEnd = DateTime.Parse(reply.Auction.AuctionEnd),
                Seller = reply.Auction.Seller,
                ReservePrice = reply.Auction.ReservePrice
            };

            return auction;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Could not call GRPC Server");
            return null;
        }
    }
}