using Grpc.Core;

public class LeaderboardService : Leaderboard.LeaderboardBase
{
    private readonly AuthContext _authContext;

    public LeaderboardService(AuthContext authContext)
    {
        _authContext = authContext;
    }

    public override Task<LeaderboardReply> GetScores(LeaderboardRequest request, ServerCallContext context)
    {
        var items = _authContext.Commanders.OrderByDescending(c => c.Score).Select(c => new LeaderboardItem { CommanderName = c.Username, Score = c.Score }).ToArray();

        var reply = new LeaderboardReply();
        reply.Items.AddRange(items);
        return Task.FromResult(reply);
    }
}
