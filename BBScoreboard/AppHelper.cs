using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Security;
using System.Web.UI;
using System.Web.WebPages;
using WebMatrix.WebData;

namespace BBScoreboard
{
    public class UserInfo
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public bool IsAdmin { get; set; }

        public static Dictionary<string, UserInfo> Users { get; set; }

        static UserInfo()
        {
            Users = new Dictionary<string, UserInfo>(StringComparer.InvariantCultureIgnoreCase);
        }

        public UserInfo() { }

        public UserInfo(string email, string password, bool isAdmin)
        {
            Email = email;
            Password = password;
            IsAdmin = isAdmin;
        }
    }

    public class AppHelper
    {
        //public const string LOGIN_KEY = "App.User";

        //public static void Login(WebPage page, string email, string password, bool redirect = false, string redirectUrl = "~/")
        //{
        //    if (page != null)
        //    {
        //        if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(password))
        //        {
        //            var user = UserInfo.Users.ContainsKey(email) ? UserInfo.Users[email] : null;
        //            if (user != null && password == user.Password)
        //            {
        //                page.Session[LOGIN_KEY] = user;
        //                if (redirect)
        //                {
        //                    page.Response.Redirect(redirectUrl);
        //                }

        //                return;
        //            }
        //        }

        //        page.Session.Remove(LOGIN_KEY);
        //    }
        //}

        public static bool CheckLogin(WebPage page, bool redirect = false, string deniedRedirectUrl = "~/Login")
        {
            if (WebSecurity.IsAuthenticated)
            {
                return true;
            }

            //if (page != null)
            //{
            //    if (page.Session[LOGIN_KEY] != null)
            //    {
            //        return true;
            //    };
            //}

            if (redirect)
            {
                page.Response.Redirect(deniedRedirectUrl, false);
            }

            return false;
        }

        public static bool CheckLogin(Page page, bool redirect = false, string deniedRedirectUrl = "~/Login")
        {
            if (WebSecurity.IsAuthenticated)
            {
                return true;
            }
            //if (page != null)
            //{
            //    if (page.Session[LOGIN_KEY] != null)
            //    {
            //        return true;
            //    };
            //}

            if (redirect)
            {
                page.Response.Redirect(deniedRedirectUrl, false);
            }

            return false;
        }

        public static void LogOff(WebPage page = null)
        {
            WebSecurity.Logout();
            //if (page != null)
            //{
            //    page.Session.Remove(LOGIN_KEY);
            //}
        }

        public static void Delete(string email)
        {
            Membership.DeleteUser(email, true);
        }

        public static UserProfile GetCurrentUser()
        {
            var userId = WebSecurity.CurrentUserId;
            UserProfile user = userId > 0 ? (new BBScoreboardEntities().UserProfiles.Find(userId)) : null;

            return user;
        }

        public const string C_SHOW_ALL_ACTIONS = "ShowAllActions";
        private static bool? _showAllActions = null;
        public static bool ShowAllActions
        {
            get
            {
                if (_showAllActions == null)
                {
                    var db = new BBScoreboardEntities();
                    var showAllActions = db.AppConfigs.FirstOrDefault(i => i.Key == C_SHOW_ALL_ACTIONS); //ConfigHelper.GetBool("ShowAllActions", false);
                    _showAllActions = showAllActions == null ? false : DataHelper.GetBool(showAllActions.Value);
                }

                return _showAllActions.Value;
            }

            set
            {
                var db = new BBScoreboardEntities();
                var showAllActions = db.AppConfigs.FirstOrDefault(i => i.Key == C_SHOW_ALL_ACTIONS); //ConfigHelper.GetBool("ShowAllActions", false);

                if (showAllActions == null)
                {
                    showAllActions = new AppConfig();
                    showAllActions.Key = C_SHOW_ALL_ACTIONS;
                    showAllActions.Value = value ? "1" : "0";
                    db.AppConfigs.Add(showAllActions);
                }
                else
                {
                    showAllActions.Value = value ? "1" : "0";
                }

                _showAllActions = value;
                db.SaveChanges();
            }
        }

        public const string C_ENABLE_TIMER = "EnableTimer";
        private static bool? _enableTimer = null;
        public static bool EnableTimer
        {
            get
            {
                if (_enableTimer == null)
                {
                    var db = new BBScoreboardEntities();
                    var enableTimer = db.AppConfigs.FirstOrDefault(i => i.Key == C_ENABLE_TIMER); //ConfigHelper.GetBool("EnableTimer", true);
                    _enableTimer = enableTimer == null ? false : DataHelper.GetBool(enableTimer.Value);
                }

                return _enableTimer.Value;
            }

            set
            {
                var db = new BBScoreboardEntities();
                var enableTimer = db.AppConfigs.FirstOrDefault(i => i.Key == C_ENABLE_TIMER); //ConfigHelper.GetBool("ShowAllActions", false);

                if (enableTimer == null)
                {
                    enableTimer = new AppConfig();
                    enableTimer.Key = C_ENABLE_TIMER;
                    enableTimer.Value = value ? "1" : "0";
                    db.AppConfigs.Add(enableTimer);
                }
                else
                {
                    enableTimer.Value = value ? "1" : "0";
                }

                _enableTimer = value;
                db.SaveChanges();
            }
        }
    }
}
