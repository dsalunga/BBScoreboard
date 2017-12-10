using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BBScoreboard
{
    public enum AccessType
    {
        ReadOnly = 0,
        Admin = 1,
        Scorer = 2
    }

    public enum GameActions
    {
        FreeThrowAttempt = -1,
        FieldGoalAttempt = -2,
        FieldGoal3Attempt = -3,

        FreeThrowMade = 1,
        FieldGoalMade = 2,
        FieldGoal3Made = 3,

        Assist = 4,
        Steal = 5,
        Block = 6,

        ReboundOffensive = 7,
        RebourdDefensive = 8,

        Turnover = 9,

        FoulPersonal = 10,
        FoulTechnical = 11
    }

    public class Manage
    {
        public static Dictionary<int, string> AccessTypes { get; set; }

        static Manage()
        {
            AccessTypes = new Dictionary<int, string>
            {
                {0, "Read-Only"},
                {1, "Administrator"},
                {2, "Statistician"}
            };
        }

        public static string GetAccessType(int access)
        {
            if (AccessTypes.ContainsKey(access))
                return AccessTypes[access];

            return string.Empty;
        }
    }

    public class GameAction
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public string Code { get; set; }

        public static GameAction GetAction(int id)
        {
            if (Actions.ContainsKey(id))
                return Actions[id];

            return new GameAction();
        }

        public static Dictionary<int, GameAction> Actions { get; set; }

        static GameAction()
        {
            Actions = new Dictionary<int, GameAction>{
                {-1, new GameAction {Id=-1, Text= "Free Throw Missed", Code= "FTm"}},
                {-2, new GameAction{Id= -2, Text= "2-Pointer Missed", Code= "FGm"}},
                {-3, new GameAction{Id= -3, Text= "3-Pointer Missed", Code= "FG3m"}},

                {1, new GameAction{Id= 1, Text= "Free Throw", Code= "FT"}},
                {2, new GameAction{Id= 2, Text= "2-Pointer Made", Code= "FG"}},
                {3, new GameAction{Id= 3, Text= "3-Pointer Made", Code= "FG3"}},

                {4, new GameAction{Id= 4, Text= "Assist", Code= "AST"}},
                {5, new GameAction{Id= 5, Text= "Steal", Code= "STL"}},
                {6, new GameAction{Id= 6, Text= "Block", Code= "BLK"}},

                {7, new GameAction{Id= 7, Text= "Offensive Rebound", Code= "RBO"}},
                {8, new GameAction{Id= 8, Text= "Defensive Rebound", Code= "RBD"}},

                {9, new GameAction{Id= 9, Text= "Turnover", Code= "TO"}},

                {10, new GameAction{Id= 10, Text= "Personal Foul", Code= "PF"}},
                {11, new GameAction{Id= 11, Text= "Technical Foul", Code= "TF"}}
            };
        }
    }

    public class UCConstants
    {
        public static readonly DateTime BaseDate = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    }
}
