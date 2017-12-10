using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace BBScoreboard
{
    public class WebHelper
    {
        public const string ICO_CHECK = "/Content/Assets/Images/Common/ico_check.gif";
        public const string ICO_X = "/Content/Assets/Images/Common/ico_x.gif";
        public const string ICO_PX = "/Content/Assets/Images/Common/px.gif";
        public const string ICO_NEW_POST = "~/Content/Assets/Images/Common/ico_postnew.gif";
        public const string ICO_SCRIPT = "~/Content/Assets/Images/Common/ico_script.gif";
        public const string TEMP_DATA_PATH = "~/Content/Admin/Data/";
        public const string SCRIPT_TAG_TEMPLATE = "<script type=\"text/javascript\" src=\"{0}\"></script>";
        public const string SCRIPT_URL_TEMPLATE = "window.location.href='{0}';";
        public const string AMPERSAND = "&";
        public const string AMPERSAND_SUBST = "|";

        public static bool IsWeb
        {
            get { return HttpContext.Current != null; }
        }

        public static string GetResponseString(string httpAddress)
        {
            if (!string.IsNullOrEmpty(httpAddress))
            {
                try
                {
                    // Create a request for the URL. 		
                    WebRequest request = WebRequest.Create(httpAddress);
                    // If required by the server, set the credentials.
                    request.Credentials = CredentialCache.DefaultCredentials;

                    // Get the response.
                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    {
                        // Open the stream using a StreamReader for easy access.
                        using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                        {
                            // Read the content.
                            return reader.ReadToEnd();
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog(ex);
                }
            }

            return null;
        }

        public static string UrlEncode(string url)
        {
            if (!string.IsNullOrEmpty(url))
                return HttpUtility.UrlEncode(url).Replace(AMPERSAND, AMPERSAND_SUBST);

            return string.Empty;
        }

        public static string UrlDecode(string url)
        {
            if (!string.IsNullOrEmpty(url))
                return HttpUtility.UrlDecode(url).Replace(AMPERSAND_SUBST, AMPERSAND);

            return string.Empty;
        }

        public static string BuildScriptTagWithSource(string src)
        {
            return string.Format(SCRIPT_TAG_TEMPLATE, src);
        }

        public static string SetStateImage(object obj)
        {
            bool b = false;

            try { b = (bool)obj; }
            catch { }

            return (b) ? ICO_CHECK : ICO_X;
        }

        public static string SetStateImageInt(object obj)
        {
            int b = 0;

            try { b = (int)obj; }
            catch { }

            return (b == 1) ? ICO_CHECK : ICO_X;
        }

        public static string SetStateImageIntActiveNull(object obj)
        {
            int b = 0;

            try { b = (int)obj; }
            catch { }

            return (b == 0) ? ICO_X : ICO_PX;
        }

        public static string SetStateImageNull(object obj)
        {
            bool b = false;

            try { b = (bool)obj; }
            catch { }

            return (b) ? ICO_CHECK : ICO_PX;
        }

        public static string SetMsgImage(int b)
        {
            if (b == 0)
                return ICO_NEW_POST;
            else
                return ICO_SCRIPT;
        }

        public static bool FindTreeNode(TreeNodeCollection nodes, string value)
        {
            foreach (TreeNode node in nodes)
            {
                if (node.Value == value)
                {
                    node.Expand();
                    node.Select();
                    return true;
                }
                else
                {
                    if (FindTreeNode(node.ChildNodes, value))
                    {
                        node.Expand();
                        return true;
                    }
                }
            }

            return false;
        }

        public static void Redirect(string url)
        {
            Redirect(url, HttpContext.Current);
        }

        public static void Redirect(string url, HttpContext context)
        {
            context.Response.Redirect(url, false);
            context.ApplicationInstance.CompleteRequest();
        }

        public static string MapPath(string path, bool extensive = false)
        {
            if (!string.IsNullOrWhiteSpace(path))
            {
                var context = HttpContext.Current;
                if (context == null || path.Contains(":") || path.StartsWith(@"\\"))
                    return FileHelper.EvalPath(path);

                if (extensive && path.Contains("~"))
                {
                    var newPath = path.Replace("~", context.Server.MapPath("~"));
                    return FileHelper.EvalPath(newPath);
                }

                return context.Server.MapPath(path);
            }

            return path;
        }

        //public static string EvalOrMapPath(string path)
        //{
        //    string newPath = path;

        //    if (newPath.Contains("~"))
        //        newPath = newPath.Replace("~", MapPath("~"));
        //    else if (!newPath.Contains(":") && !newPath.StartsWith(@"\\"))
        //        newPath = MapPath(newPath);

        //    return FileHelper.EvalPath(newPath);
        //}

        public static string CombineAddress(string baseUrl, params string[] relativeUrls)
        {
            string newUrl = baseUrl;
            foreach (string relativeUrl in relativeUrls)
                newUrl = string.Format("{0}/{1}", newUrl.TrimEnd(new char[] { '/', '\\' }), relativeUrl.TrimStart(new char[] { '/', '~' }));

            return newUrl;
        }

        public static string BuildAddress(string baseUrl, string remainingUrl)
        {
            if (remainingUrl.Contains(":"))
                return remainingUrl;

            return CombineAddress(baseUrl, remainingUrl);
        }

        public static HtmlLink CreateCssLink(string cssUrl)
        {
            var link = new HtmlLink();
            link.Attributes.Add("type", "text/css");
            link.Attributes.Add("rel", "stylesheet");
            link.Href = link.ResolveUrl(cssUrl);
            return link;
        }

        public static string CreateCssLinkText(string url)
        {
            return string.Format(@"<link rel=""stylesheet"" type=""text/css"" href=""{0}"" />", url);
        }

        public static void RemoveDropDownListItemByValue(DropDownList cbo, object value)
        {
            if (value != null && cbo != null)
            {
                var item = cbo.Items.FindByValue(value.ToString());
                if (item != null)
                    cbo.Items.Remove(item);
            }
        }

        public static HtmlGenericControl CreateJavaScriptLink(string scriptUrl)
        {
            var script = new HtmlGenericControl("script");
            script.Attributes.Add("type", "text/javascript");
            script.Attributes.Add("src", script.ResolveUrl(scriptUrl));

            return script;
        }

        public static string CreateJavaScriptLinkText(string url)
        {
            return string.Format(@"<script src=""{0}"" type=""text/javascript""></script>", url);
        }

        public static string CreateButtonLink(HtmlInputButton button, string url)
        {
            return button.Attributes["onclick"] = string.Format(SCRIPT_URL_TEMPLATE, url);
        }

        public static void AspNetAjaxComboBoxSelectText(ListControl comboBox, string text, string value = "")
        {
            var listItem = comboBox.Items.FindByText(text);
            if (listItem == null)
            {
                if (string.IsNullOrEmpty(value))
                    listItem = new ListItem(text);
                else
                    listItem = new ListItem(text, value);

                comboBox.Items.Insert(0, listItem);
            }

            listItem.Selected = true;
        }

        public static void DownloadAsXml(DataSet ds)
        {
            DownloadAsXml(ds, "DefaultDataSet", "DefaultTable");
        }

        public static void DownloadAsXml(DataSet ds, string dataSetName, string tableName)
        {
            string fileName = string.Format("{0}_{1}.xls", dataSetName.Replace(" ", "_"), tableName.Replace(" ", "_"));
            string filePath = MapPath(TEMP_DATA_PATH + fileName);

            string folder = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            ds.DataSetName = dataSetName;
            ds.Tables[0].TableName = tableName;
            ds.WriteXml(filePath);

            DownloadFile(filePath, needMapping: false);
        }

        public static void DownloadAsCsv(DataSet ds, string fileName = "Data")
        {
            bool IsOutputStreamed = false;
            var response = HttpContext.Current.Response;

            try
            {
                StringBuilder dataToExport = new StringBuilder();

                Func<object, string> FormatStringAsCsv = (value) =>
                {
                    if (value == null)
                        return "\"\"";

                    string valueString = value.ToString();

                    return string.Format("\"{0}\"", valueString.Contains("\"") ? valueString.Replace("\"", "\"\"") : valueString);
                };

                foreach (DataTable dtExport in ds.Tables)
                {
                    StringBuilder headerToExport = new StringBuilder();
                    foreach (DataColumn dCol in dtExport.Columns)
                        headerToExport.Append(FormatStringAsCsv(dCol.ColumnName) + ',');

                    headerToExport.Remove(headerToExport.Length - 1, 1);
                    headerToExport.Append(Environment.NewLine);
                    dataToExport.Append(headerToExport.ToString());

                    StringBuilder bodyToExport = new StringBuilder();
                    foreach (DataRow dRow in dtExport.Rows)
                    {
                        foreach (object obj in dRow.ItemArray)
                            bodyToExport.Append(FormatStringAsCsv(obj) + ',');

                        bodyToExport.Remove(bodyToExport.Length - 1, 1);
                        bodyToExport.Append(Environment.NewLine);
                    }

                    dataToExport.Append(bodyToExport.ToString());
                    dataToExport.Append(Environment.NewLine);
                    dataToExport.Append(Environment.NewLine);

                    var dataToExportString = dataToExport.ToString();
                    if (!string.IsNullOrEmpty(dataToExportString))
                    {
                        response.Clear();
                        response.ContentType = "text/vnd.ms-excel";
                        response.AddHeader("content-disposition",
                            string.Format("attachment;filename={0}_{1}.csv",
                                string.IsNullOrEmpty(dtExport.TableName) || dtExport.TableName.Equals("Default", StringComparison.InvariantCultureIgnoreCase) ? fileName : dtExport.TableName,
                                DateTime.Now.ToString("yyyyMMdd")));
                        response.Write(dataToExportString);
                        IsOutputStreamed = true;
                    }
                }
            }

            catch { }

            finally
            {
                if (IsOutputStreamed)
                    response.End();
            }
        }

        //public static void DownloadFile(string filePath, string downloadFileName = "", bool force = true)
        //{
        //    DownloadFile(filePath, downloadFileName, true, force);
        //}

        public static void DownloadFile(string filePath, string downloadFileName = "", bool needMapping = true, bool force = true)
        {
            var absFilePath = needMapping ? MapPath(filePath) : filePath;
            var fileName = string.IsNullOrEmpty(downloadFileName) ? Path.GetFileName(absFilePath) : downloadFileName;

            var response = HttpContext.Current.Response;

            response.Clear();

            if (force)
                response.AppendHeader("content-disposition", string.Format("attachment; filename=\"{0}\"", fileName));
            else
                response.AppendHeader("content-disposition", string.Format("filename=\"{0}\"", fileName));

            response.ContentType = MIMEHelper.GetMIMEType(fileName);
            response.TransmitFile(absFilePath);
            response.End();
        }

        public static void DownloadFolder(string folderPath, string baseFilename = "", string password = "")
        {
            DownloadFolder(folderPath, true, baseFilename, password);
        }

        public static void DownloadFolder(string folderPath, bool needMapping, string baseFilename = "", string password = "")
        {
            var absPath = needMapping ? MapPath(folderPath) : folderPath;
            Compression.Download(absPath, baseFilename, password);
        }

        public static bool SetCboValue(DropDownList cbo, int value)
        {
            return SetCboValue(cbo, value.ToString());
        }

        public static bool SetCboValue(DropDownList cbo, string value)
        {
            if (cbo.Items.FindByValue(value) != null)
            {
                cbo.SelectedValue = value;
                return true;
            }

            return false;
        }
    }
}
