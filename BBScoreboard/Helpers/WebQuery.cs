using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.WebPages;

namespace BBScoreboard
{
    public class WebQuery : NameValueCollection
    {
        #region Constructors

        public WebQuery() { }

        public WebQuery(NameValueCollection queryString)
        {
            base.Add(queryString);
        }

        public WebQuery(WebQuery toCopy)
        {
            this.BasePath = toCopy.BasePath;
            this.Add(toCopy);
        }

        public WebQuery(bool useHttpContext)
        {
            if (useHttpContext)
                Init(HttpContext.Current.Request);
        }

        public void Init(HttpRequest request)
        {
            BasePath = request.CurrentExecutionFilePath;
            Add(request.QueryString);
        }

        public void Init(HttpRequestBase request)
        {
            BasePath = request.CurrentExecutionFilePath;
            Add(request.QueryString);
        }


        public WebQuery(HttpRequest request)
        {
            Init(request);
        }

        public WebQuery(HttpRequestBase request)
        {
            Init(request);
        }

        public WebQuery(WebPageRenderingBase page)
        {
            BasePath = page.Request.CurrentExecutionFilePath;
            Add(page.Request.QueryString);
        }

        public WebQuery(Page p)
            : this(p.Request) { }

        public WebQuery(UserControl c)
            : this(c.Request) { }

        public WebQuery(Control c)
            : this(c.Page.Request) { }

        public WebQuery(HttpApplication a)
            : this(a.Request) { }

        public WebQuery(HttpContext c)
            : this(c.Request) { }

        public WebQuery(string queryStringOrPath)
        {
            string queryString = null;

            if (queryStringOrPath.Contains("?"))
            {
                string[] path = queryStringOrPath.Split('?');
                BasePath = path[0];
                queryString = path[1];
            }
            else if (queryStringOrPath.Contains("="))
            {
                // all query string
                queryString = queryStringOrPath;
            }
            else if (!string.IsNullOrEmpty(queryStringOrPath))
            {
                BasePath = queryStringOrPath;
            }

            if (!string.IsNullOrEmpty(queryString))
                AddParams(queryString);
        }

        #endregion

        #region Properties

        private string _basePath;
        public string BasePath
        {
            get { return _basePath; }
            set { _basePath = value; }
        }


        public bool HasValue(string key)
        {
            string value = this[key];
            return !string.IsNullOrEmpty(value);
        }

        public string EncodedBasePath
        {
            get { return HttpUtility.UrlEncode(_basePath); }
        }

        public string BaseFileName
        {
            get { return Path.GetFileName(BasePath); }
        }

        public static HttpContext Context
        {
            get { return HttpContext.Current; }
        }

        #endregion

        #region Instance Methods

        private void BaseRedirect(string path)
        {
            WebHelper.Redirect(path, Context);
        }

        public void AddParams(string queryString)
        {
            base.Add(HttpUtility.ParseQueryString(queryString));
        }

        public string Get(string key, string nullEmptyDefaultValue)
        {
            return GetValue(key, nullEmptyDefaultValue);
        }

        public override string ToString()
        {
            string queryString = string.Empty;

            foreach (string key in base.AllKeys)
            {
                queryString += key + "=" + base[key] + "&";
            }

            return queryString.TrimEnd(new char[] { '&', '=', ' ', '?' });
        }

        public string BuildQuery()
        {
            return BuildQuery(BasePath);
        }

        public string BuildQuery(string basePath)
        {
            string queryString = this.ToString();

            if (!string.IsNullOrEmpty(queryString))
                queryString = "?" + queryString;

            return basePath + queryString;
        }

        public void Redirect()
        {
            WebHelper.Redirect(BuildQuery(), Context);
        }

        public void Redirect(string basePath)
        {
            WebHelper.Redirect(BuildQuery(basePath), Context);
        }

        public int GetInt32(string name)
        {
            return DataHelper.GetInt32(this[name]);
        }

        public int GetInt32(string name, int defaultValue)
        {
            return DataHelper.GetInt32(this[name], defaultValue);
        }

        public int GetId(string name)
        {
            return DataHelper.GetId(this[name]);
        }

        public int GetId(string name, int defaultValue)
        {
            return DataHelper.GetId(this[name], defaultValue);
        }

        public bool GetBool(string name, bool defaultValue)
        {
            return DataHelper.GetBool(this[name], defaultValue);
        }

        public WebQuery Set(string name, object value)
        {
            base[name] = value == null ? "" : value.ToString();

            return this;
        }

        public string GetValue(string name, string nullEmptyDefaultValue)
        {
            string value = Get(name);
            return string.IsNullOrEmpty(value) ? nullEmptyDefaultValue : value;
        }

        public virtual WebQuery Clone()
        {
            return new WebQuery(this);
        }

        public void SetEncode(string key, string value)
        {
            Set(key, value.Replace("&", "|"));
        }

        public string GetDecode(string key)
        {
            string value = this[key];
            if (!string.IsNullOrEmpty(value))
                return HttpUtility.UrlDecode(value).Replace("|", "&");

            return string.Empty;
        }

        #endregion

        #region Static Methods

        public static void StaticBaseRedirect(string basePath)
        {
            WebHelper.Redirect(basePath, Context);
        }

        public static string BuildQuery(Control instance)
        {
            WebQuery query = new WebQuery(instance);
            return query.BuildQuery();
        }

        public static string BuildQuery(string url, string name, object value)
        {
            WebQuery query = new WebQuery(url);
            query.Set(name, value);

            return query.BuildQuery();
        }

        public static void StaticRedirect()
        {
            WebQuery query = new WebQuery(HttpContext.Current);
            query.Redirect();
        }

        public static void StaticRedirect(string addressOrBasePath, bool retainParameters)
        {
            if (retainParameters)
            {
                WebQuery query = new WebQuery(Context);
                query.Redirect(addressOrBasePath);
            }
            else
            {
                Context.Response.Redirect(addressOrBasePath);
            }
        }

        public static void StaticRedirect(string basePath)
        {
            StaticRedirect(basePath, false);
        }

        public static string StaticGet(string key, HttpRequest request)
        {
            return request[key];
        }

        #endregion
    }
}
