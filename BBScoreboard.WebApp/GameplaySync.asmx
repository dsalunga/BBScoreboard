<%@ WebService Language="C#" Class="GameplaySync" %>

using System;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Script.Services;
using System.Web.Services;
using System.Web.Services.Protocols;

using BBScoreboard;
using Newtonsoft.Json;

[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
[System.Web.Script.Services.ScriptService]
public class GameplaySync : System.Web.Services.WebService
{

    [WebMethod]
    public string HelloWorld()
    {
        return "Hello World";
    }

    [WebMethod]
    public int UpdateAction(int id, int mm, int ss)
    {
        if (id > 0)
        {
            var db = new BBScoreboardEntities();
            var action = db.UCGameplayActions.Find(id);
            if (action != null)
            {
                var now = DateTime.UtcNow;
                var gameplay = GameplayModel.Create(action.GameId, db);
                var game = gameplay.Game;

                var teamModel = gameplay.TeamModels.Find(i => i.TeamId == action.TeamId);
                var tstat = teamModel.Stat;
                tstat.LastUpdate = now;
                
                if (mm == -1)
                {
                    // Toggle Action
                    var apply = action.Status == 0;

                    if (action.Status == 1) // from Active to UNDO, remove
                    {
                        action.Status = 0;
                        GameHelper.UpdateScore(tstat, action.Quarter, action.Arg * -1); //arg);
                    }
                    else // REDO action, apply
                    {
                        action.Status = 1;
                        GameHelper.UpdateScore(tstat, action.Quarter, action.Arg); //arg);
                    }

                    if (action.PlayerId > 0)
                    {
                        var playerModel = teamModel.PlayerModels.Find(i => i.PlayerId == action.PlayerId);
                        var pstat = playerModel.Stat;

                        PlayerHelper.UpdateStat(pstat, (GameActions)action.ActionCode, apply);
                        pstat.LastUpdate = now;
                    }
                }
                else
                {
                    // Update Action Time
                    var gt = action.GameTime;
                    action.GameTime = new DateTime(gt.Year, gt.Month, gt.Day, gt.Hour, mm, ss, gt.Millisecond);
                }

                action.LastUpdate = now;
                game.LastActivityDate = now;
                gameplay.Db.SaveChanges();
            }
        }

        return 0;
    }

    [WebMethod]
    public string SendAction(int gameId, int teamId, int playerId, int action, int arg, int recPlayerId)
    {
        if (gameId > 0 && teamId > 0)
        {
            var gameplay = GameplayModel.Create(gameId);
            var game = gameplay.Game;
            var teamModel = gameplay.TeamModels.Find(i => i.TeamId == teamId);
            var teamModel2 = gameplay.TeamModels.Find(i => i.TeamId != teamId);
            var tstat = teamModel.Stat;
            var tstat2 = teamModel2.Stat;

            var now = DateTime.UtcNow;

            tstat.LastUpdate = DateTime.UtcNow;
            if (arg > 0)
            {
                GameHelper.UpdateScore(tstat, game.CurrentQuarter, arg);
            }

            var gameAction = new UCGameplayAction();
            gameAction.GameId = gameId;
            gameAction.Quarter = game.CurrentQuarter;
            gameAction.TeamId = teamId;
            gameAction.PlayerId = playerId;
            gameAction.ActionCode = action;
            gameAction.Arg = arg;
            gameAction.RecPlayerId = recPlayerId;
            gameAction.ActionDate = now;
            gameAction.LastUpdate = now;
            gameAction.GameTime = GameHelper.ComputeTimeRemaining(game);
            gameAction.TeamScore1 = GameHelper.GetScore(tstat);
            gameAction.TeamScore2 = GameHelper.GetScore(tstat2);
            gameAction.Status = 1;
            gameplay.Db.UCGameplayActions.Add(gameAction);

            if (playerId > 0)
            {
                var playerModel = teamModel.PlayerModels.Find(i => i.PlayerId == playerId);
                var pstat = playerModel.Stat;

                PlayerHelper.UpdateStat(pstat, (GameActions)action);
                pstat.LastUpdate = now;
            }

            game.LastActivityDate = now;
            gameplay.Db.SaveChanges();

            //return GetDelta(gameplay, lastUpdate);
        }

        return "{\"return\":0}";
    }

    //[WebMethod]
    //public string GetDelta(int gameId, DateTime lastUpdate)
    //{
    //    var gameplay = GameplayModel.Create(gameId);
    //    return GetDelta(gameplay, lastUpdate);
    //}

    [WebMethod]
    public int UpdateTimer(int gameId, int start, DateTime timeLeft, int tlMs, DateTime timerLastModified, int tlmMs)
    {
        var gameplay = GameplayModel.Create(gameId);
        var game = gameplay.Game;

        var now = DateTime.UtcNow;
        game.TimeLeft = timeLeft.AddMilliseconds(tlMs).ToUniversalTime(); //GameHelper.ComputeTimeRemaining(game);
        game.TimeLastModified = timerLastModified.AddMilliseconds(tlmMs).ToUniversalTime();
        game.IsTimeOn = start == 1;
        game.LastActivityDate = now;
        game.LastUpdate = now;

        gameplay.Db.SaveChanges();

        return 0;
    }

    [WebMethod]
    public int UpdateGame(int gameId, int quarter, bool updateScores, UCGameTeamStat ts0, UCGameTeamStat ts1, bool updateTime, DateTime timeLeft, int tlMs, DateTime timerLastModified, int tlmMs)
    {
        return GameHelper.UpdateGame(gameId, quarter, updateScores, ts0, ts1, updateTime, timeLeft.AddMilliseconds(tlMs).ToUniversalTime(), timerLastModified.AddMilliseconds(tlmMs).ToUniversalTime());
    }

    [WebMethod]
    public int IsGameStarted(int gameId)
    {
        var db = new BBScoreboardEntities();
        var game = db.UCGames.Find(gameId);
        if (game != null)
        {
            return game.IsStarted ? 1 : 0;
        }

        return 0;
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string GetDelta(int gameId, DateTime lastUpdate)
    {
        var lastUpdateUtc = lastUpdate.ToUniversalTime();
        var gameplay = GameplayModel.Create(gameId);
        return GetDelta(gameplay, lastUpdateUtc);
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string GetDelta2(int gameId, DateTime lastUpdate, bool firstSync)
    {
        var lastUpdateUtc = lastUpdate.ToUniversalTime();
        var gameplay = GameplayModel.Create(gameId);
        return GetDelta(gameplay, lastUpdateUtc, firstSync);
    }

    public string GetDelta(GameplayModel model, DateTime lastUpdateUtc, bool firstSync = false)
    {
        var game = model.Game;
        if (game.LastUpdateForRefresh > lastUpdateUtc)
        {
            return "{\"return\": 2}";
        }

        if (game.LastActivityDate > lastUpdateUtc || firstSync)
        {
            // JavaScriptSerializer serializer = new JavaScriptSerializer();
            var delta = new StringBuilder();
            delta.Append("{");

            if (game.LastUpdate > lastUpdateUtc || firstSync)
            {
                delta.AppendFormat("\"game\": {0},", JsonConvert.SerializeObject(game));
            }

            for (int i = 0; i < 2; i++)
            {
                var teamModel = model.TeamModels[i];
                var stat = teamModel.Stat;
                if (stat.LastUpdate > lastUpdateUtc || firstSync)
                {
                    delta.Append("\"team" + teamModel.TeamId + "\": {");
                    delta.AppendFormat("\"ts\": {0}, \"ps\": [", JsonConvert.SerializeObject(stat));

                    // Player stats
                    var pstats = teamModel.PlayerStats;
                    var sb = new StringBuilder();
                    foreach (var ps in pstats)
                    {
                        if (ps.LastUpdate > lastUpdateUtc || firstSync)
                        {
                            sb.AppendFormat("{0},", JsonConvert.SerializeObject(ps));
                        }
                    }

                    delta.Append(sb.ToString().TrimEnd(','));
                    delta.Append("]");

                    // team actions
                    if (!firstSync)
                    {
                        var actions = teamModel.GetActions(game.CurrentQuarter >= 4 ? 4 : game.CurrentQuarter, lastUpdateUtc).OrderByDescending(y => y.ActionDate);
                        delta.Append(",\"actions\": [");
                        sb.Clear();

                        foreach (var action in actions)
                        {
                            sb.Append("{\"id\":" + action.Id);

                            if (action.ActionDate < action.LastUpdate)
                            {
                                sb.Append(",\"status\":" + action.Status);
                                sb.Append(",\"time\":\"" + action.GameTime.ToString("mm:ss") + "\"");
                            }
                            else
                            {
                                sb.Append(",\"status\": -1");
                                sb.Append(",\"action\":" + JsonConvert.SerializeObject(action));
                                sb.Append(",\"text\": \"" + GameHelper.ActionToString(action, teamModel) + "\"");
                            }

                            sb.Append("},");
                        }

                        delta.Append(sb.ToString().TrimEnd(','));
                        delta.Append("]");
                    }

                    delta.Append("},");
                }
            }

            delta.AppendFormat("\"lastUpdate\": {0},", JsonConvert.SerializeObject(game.LastActivityDate.AddSeconds(1)));
            delta.Append("\"return\": 1}");

            return delta.ToString();
        }

        return "{\"return\": 0}";
    }
}