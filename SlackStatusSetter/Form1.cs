using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SlackStatusSetter
{
    public partial class SlackStatusForm : Form
    {
        private static readonly HttpClient client = new HttpClient();
        private readonly string token;
        List<SlackProfile> statusList = new List<SlackProfile>();

        public SlackStatusForm()
        {
            InitializeComponent();

            LoadStatusList();

            token = ConfigurationManager.AppSettings["token"].ToString();
        }

        private void LoadStatusList()
        {
            statusList = SqliteDataAccess.LoadStatuses();

            WireUpStatusList();
        }

        private void WireUpStatusList()
        {
            lbStatus.DataSource = null;
            lbStatus.DataSource = statusList;
        }

        private void btnAddStatus_Click(object sender, EventArgs e)
        {
            
            SlackProfile profile = new SlackProfile
            {
                status_text = txtStatus.Text.Trim(),
                status_emoji = txtEmoji.Text.Trim(),
                status_expiration = 0
            };

            SqliteDataAccess.SaveStatus(profile);

            LoadStatusList();

            txtStatus.Text = "";
            txtEmoji.Text = "";
        }


        private void SetStatus(string Status, string Emoji)
        {
            SlackMessage msg = new SlackMessage();
            SlackProfile profile = new SlackProfile { status_text = Status, status_emoji = Emoji, status_expiration = 0 };
            msg.profile = profile;
            _ = SendMessageAsync(token, msg);
        }

        public static async Task SendMessageAsync(string token, SlackMessage msg)
        {
            // serialize method parameters to JSON
            var content = JsonConvert.SerializeObject(msg);
            var httpContent = new StringContent(
                content,
                Encoding.UTF8,
                "application/json"
            );

            // set token in authorization header
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // send message to API
            var response = await client.PostAsync("https://slack.com/api/users.profile.set", httpContent);

            // fetch response from API
            var responseJson = await response.Content.ReadAsStringAsync();

            // convert JSON response to object
            SlackProfileSetResponse messageResponse =
                JsonConvert.DeserializeObject<SlackProfileSetResponse>(responseJson);

            // throw exception if sending failed
            if (messageResponse.ok == false)
            {
                throw new Exception(
                    "failed to send message. error: " + messageResponse.error
                );
            }
        }

        private void lbStatus_DoubleClick(object sender, EventArgs e)
        {
            SetStatus(((SlackProfile)((ListBox)sender).SelectedItem).status_text, ((SlackProfile)((ListBox)sender).SelectedItem).status_emoji.Trim());
        }

        private void lblStatus_Format(object sender, ListControlConvertEventArgs e)
        {
            e.Value = ((SlackProfile)e.ListItem).status_emoji + "   " + ((SlackProfile)e.ListItem).status_text;
        }

        private void btnDeleteStatus_Click(object sender, EventArgs e)
        {
            SlackProfile profile = new SlackProfile
            {
                status_text = txtStatus.Text.Trim(),
                status_emoji = txtEmoji.Text.Trim(),
                status_expiration = 0
            };

            SqliteDataAccess.DeleteStatus(profile);

            LoadStatusList();

            txtStatus.Text = "";
            txtEmoji.Text = "";
        }
    }

    internal class StatusModel
    {
    }

    public class SlackProfileSetResponse
    {
        public bool ok { get; set; }
        public string error { get; set; }
        public string warning { get; set; }

    }

    public class SlackMessage
    {
        public SlackProfile profile {get; set;}
    }
    public class SlackProfile
    {
        public string status_text { get; set; }
        public string status_emoji { get; set; }
        public int status_expiration { get; set; }
    }
}
