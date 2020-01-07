using JsonDiffPatchDotNet;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace SqlFormatter
{
    internal class SQLFormatter
    {
        private const string _defaultFormatOptionsFileName = "SqlFormatOptions.json";
        private const string _userFormatOptionsFileName = "SqlFormatOptions.user.json";
        private string urlEndpoint;
        private RestClient restClient;
        private SqlFormatOption[] defaultFormatOptions;
        internal SqlFormatOption[] UserFormatOptions { get; private set; }
        internal event Action<string, string, ToolTipIcon> Notification;
        private JObject pendingOptionsToSet = new JObject();

        internal SQLFormatter()
        {
            CreateOptionsFile();

            defaultFormatOptions = JsonConvert.DeserializeObject<SqlFormatOption[]>(File.ReadAllText(_defaultFormatOptionsFileName));
            UserFormatOptions = JsonConvert.DeserializeObject<SqlFormatOption[]>(File.ReadAllText(_userFormatOptionsFileName));

            urlEndpoint = ConfigurationManager.AppSettings[nameof(urlEndpoint)];
            restClient = new RestClient(urlEndpoint) { CookieContainer = new System.Net.CookieContainer() };
            SetCookies();
            FindUserOptionChanges();
        }

        internal void SaveUserChanges()
        {
            File.WriteAllText(_userFormatOptionsFileName, JsonConvert.SerializeObject(UserFormatOptions, Formatting.None, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore }));
        }

        internal void FindUserOptionChanges()
        {
            JToken optionDifferences = new JsonDiffPatch().Diff(JArray.FromObject(defaultFormatOptions), JArray.FromObject(UserFormatOptions));

            List<string> changes = new List<string>();
            GetChangedOptionPositions(optionDifferences?.Skip(1).First(), changes);

            foreach (var change in changes)
            {
                Queue<int> parts = new Queue<int>(change.Split('_').Select(c => int.Parse(c)));
                SqlFormatOption currentFormatOption = UserFormatOptions[parts.Dequeue()];

                while (parts.Count > 0)
                {
                    int level = parts.Dequeue();
                    currentFormatOption = currentFormatOption.Childs[level];
                }

                QueuePendingChanges(currentFormatOption);
            }
        }

        internal void AddPendingOptionChange(JObject value)
        {
            pendingOptionsToSet.Merge(value);
        }

        internal void QueuePendingChanges(SqlFormatOption checkedFormatOption)
        {
            JObject formatValueObject = new JObject();
            if (checkedFormatOption.IsCheckable)
            {
                formatValueObject.Add(checkedFormatOption.Id, checkedFormatOption.IsChecked);
                AddPendingOptionChange(formatValueObject);
            }
            else
            {
                JObject radioFormatValueObject = new JObject();
                radioFormatValueObject.Add(checkedFormatOption.FormatOptionType, checkedFormatOption.FormatOptionValue);

                formatValueObject.Add(checkedFormatOption.FormatOptionId, radioFormatValueObject);
                AddPendingOptionChange(formatValueObject);
            }
        }

        private void GetChangedOptionPositions(JToken diff, List<string> nodes, string prefix = "")
        {
            while (diff != null)
            {
                if (diff.Type == JTokenType.Object)
                {
                    GetChangedOptionPositions(diff.First, nodes, prefix);
                    return;
                }

                string propertyName = (diff as JProperty).Name;

                if (propertyName.Equals("_t"))
                {
                    GetChangedOptionPositions(diff.Next, nodes, prefix);
                    return;
                }
                else if (propertyName.Equals("c"))
                {
                    nodes.Add(prefix.Trim('_'));
                    return;
                }
                else
                {
                    if (!propertyName.Equals("ch"))
                    {
                        do
                        {
                            propertyName = (diff as JProperty).Name;
                            GetChangedOptionPositions(diff.First, nodes, prefix + "_" + propertyName);
                            diff = diff.Next;
                        } while (diff != null);
                        return;
                    }
                    else
                    {
                        GetChangedOptionPositions(diff.First, nodes, prefix);
                        return;
                    }
                }
            }
        }

        private void SetCookies()
        {
            RestRequest restRequest = new RestRequest(Method.GET);
            restClient.Execute(restRequest);
        }

        internal void Format()
        {
            string sqlText = Clipboard.GetText();

            if (string.IsNullOrWhiteSpace(sqlText))
            {
                OnNotification("Error", "Text not set.", ToolTipIcon.Error);
                return;
            }

            if (!SQLParser.IsValid(sqlText))
            {
                OnNotification("Error", "SQL query contains error.", ToolTipIcon.Error);
                return;
            }

            RestRequest request = new RestRequest(Method.POST);
            request.AddHeader("content-type", "application/x-www-form-urlencoded");

            Dictionary<string, string> formDataParams = new Dictionary<string, string>();
            formDataParams.Add("text", sqlText);
            formDataParams.Add("options", pendingOptionsToSet.ToString(Formatting.None));
            formDataParams.Add("caretPosition[x]", "1");
            formDataParams.Add("caretPosition[y]", "1");
            formDataParams.Add("saveHistory", "true");
            request.AddParameter("application/x-www-form-urlencoded", string.Join("&", formDataParams.Select(p => $"{p.Key}={p.Value}")), ParameterType.RequestBody);

            IRestResponse<SQLFormatterResponse> response = restClient.Execute<SQLFormatterResponse>(request);

            Clipboard.SetText(response.Data.Text);

            if (response.Data.ErrorInfo != null)
                OnNotification("Error", response.Data.ErrorInfo.ErrorMessage, ToolTipIcon.Error);
            else
                OnNotification("Copied", response.Data.Text, ToolTipIcon.Info);

            if (pendingOptionsToSet.Count > 0)
            {
                pendingOptionsToSet.RemoveAll();
                SaveUserChanges();
            }
        }

        private void OnNotification(string title, string message, ToolTipIcon toolTipIcon)
        {
            Notification?.Invoke(title, message, toolTipIcon);
        }

        private void CreateOptionsFile()
        {
            if (!File.Exists(_defaultFormatOptionsFileName))
                File.WriteAllBytes(_defaultFormatOptionsFileName, Properties.Resources.SqlFormatOptions);

            if (!File.Exists(_userFormatOptionsFileName))
                File.WriteAllBytes(_userFormatOptionsFileName, Properties.Resources.SqlFormatOptions);
        }
    }
}
