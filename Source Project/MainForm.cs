// Decompiled with JetBrains decompiler
// Type: TwitchClipDownloader.MainForm
// Assembly: Flight Sim Bests Downloader, Version=3.3.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FADFCB25-C995-475D-8D74-04FBE792920D
// Assembly location: C:\Users\samue\Desktop\FSBDownloader\Flight Sim Bests Downloader.exe

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using TwitchClipDownloader.Classes;
using TwitchClipDownloader.Properties;
using VC;

namespace TwitchClipDownloader
{
  public class MainForm : Form
  {
    private SortedDictionary<string, Clip> queue = new SortedDictionary<string, Clip>();
    private VersionControl mvc = new VersionControl();
    public int downloadCounter;
    private TwitchApiHandler mHandler;
    public const int WM_NCLBUTTONDOWN = 161;
    public const int HT_CAPTION = 2;
    private IContainer components;
    private TextBox txtSave;
    private Label label5;
    private Button btSave;
    private Label label7;
    private TextBox txtSlug;
    private GroupBox groupBox3;
    private Button btDownloadByLink;
    private Label label8;
    private TextBox txtLink;
    private Label lbVersion;
    private CheckBox chkAddToQueue;
    private Label label12;
    private RichTextBox rtLog;
    private Button btStartDownloadQueue;
    private ListBox lbQueue;
    private GroupBox groupBox2;
    private Button btLfSavePath;
    private System.Windows.Forms.Timer versionTimer;
    private FolderBrowserDialog fdb_SavePath;
    private Button btClose;
    private Label label9;
    private Panel panel1;
    private ToolStripProgressBar pbDownload;
    private ToolStripStatusLabel toolTipText;
    private ToolStripStatusLabel lbNewVersion;
    private ToolStripStatusLabel txtVersion;
    private StatusStrip statusStrip1;
    private Label label6;
    private ComboBox cbGame;
    private TextBox txtLanguage;
    private Label label1;
    private Label label10;
    private TextBox txtChannel;
    private Button bLookupTop;
    private Button btLookUpTopOnly;
    private GroupBox groupBox4;
    private CheckBox chk_useChannel;
    private CheckBox ckTrend;
    private CheckBox chk_useGame;
    private GroupBox groupBox5;
    private Label label16;
    private Label label2;
    private Label label15;
    private ComboBox cbPeriod;
    private DateTimePicker dtTo;
    private ComboBox cbLimit;
    private Label label14;
    private DateTimePicker dtFrom;
    private Label label3;
    private Label label11;
    private DomainUpDown sTime;
    private GroupBox groupBox1;
    private CheckBox chkUbs;
    private Label label13;
    private TextBox txtPre;
    private Label label4;

    public MainForm()
    {
      this.mHandler = new TwitchApiHandler(this);
      this.InitializeComponent();
      ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
      this.cbGame.SelectedIndex = 0;
      this.cbPeriod.SelectedIndex = 0;
      this.cbLimit.SelectedItem = (object) "10";
      this.txtSave.Text = Settings.Default.savePath;
      this.lbVersion.Text = Assembly.GetExecutingAssembly().GetName().Version.ToString(2);
      this.txtVersion.Text = "Version: " + Assembly.GetExecutingAssembly().GetName().Version.ToString(2);
      this.chkAddToQueue.Checked = Settings.Default.addToQueue;
      this.chkUbs.Checked = Settings.Default.brd;
      this.chk_useChannel.Checked = Settings.Default.cusec;
      this.chk_useGame.Checked = Settings.Default.cuseg;
      this.checkVersionAsync();
    }

    private void saveLocals()
    {
      Settings.Default.savePath = this.txtSave.Text;
      Settings.Default.addToQueue = this.chkAddToQueue.Checked;
      Settings.Default.brd = this.chkUbs.Checked;
      Settings.Default.cusec = this.chk_useChannel.Checked;
      Settings.Default.cuseg = this.chk_useGame.Checked;
      Settings.Default.Save();
    }

    private async void checkVersionAsync()
    {
      if (await this.mvc.checkServerJsonAsync())
        this.lbNewVersion.Text = "New Version Available";
      else
        this.lbNewVersion.Text = "";
    }

    private async void bLookupTop_Click(object sender, EventArgs e)
    {
      await this.crawler(false, "");
    }

    private void btSave_Click(object sender, EventArgs e)
    {
      if (Directory.Exists(this.txtSave.Text))
      {
        this.saveLocals();
        this.updateLog("Saved Download Location: " + this.txtSave.Text, true);
      }
      else
        this.updateLog("Invalid Download Location: " + this.txtSave.Text, true);
    }

    private async void buildDownloadStringBySlug(string slug)
    {
      if (slug == "" || slug.Contains("/"))
      {
        int num1 = (int) MessageBox.Show("Slug not valid.");
      }
      Clip clip = (Clip) JsonConvert.DeserializeObject(await this.mHandler.sendNewRequest("https://api.twitch.tv/kraken/clips/" + slug), typeof (Clip));
      if (clip.slug == null)
      {
        this.updateLog("Nothing found.", true);
        this.txtLink.Text = "";
        this.txtSlug.Text = "";
      }
      else if (this.queue.ContainsKey(clip.slug))
      {
        int num2 = (int) MessageBox.Show("Already in queue.");
        this.txtLink.Text = "";
        this.txtSlug.Text = "";
        if (!this.chkUbs.Checked)
          return;
        this.txtPre.Text = "";
      }
      else if (this.chkAddToQueue.Checked)
      {
        clip.pre = this.chkUbs.Checked || this.txtPre.Text != "" ? (this.chkUbs.Checked ? clip.broadcaster.display_name : this.txtPre.Text) : "";
        this.lbQueue.Items.Add((object) (Uri.EscapeUriString((clip.pre != "" ? clip.pre + "_" : "") + clip.slug) + ".mp4"));
        this.queue.Add(clip.slug, clip);
        this.txtLink.Text = "";
        this.txtSlug.Text = "";
        if (!this.chkUbs.Checked)
          return;
        this.txtPre.Text = "";
      }
      else
      {
        this.bLookupTop.Enabled = false;
        ++this.downloadCounter;
        if (this.pbDownload.Maximum == 0)
          this.pbDownload.Maximum = 1;
        else
          ++this.pbDownload.Maximum;
        this.mHandler.downloadClip(clip);
        this.txtLink.Text = "";
        this.txtSlug.Text = "";
      }
    }

    public void updateLog(string logContent, bool forContext = true)
    {
      this.rtLog.Text = logContent + "\r\n" + this.rtLog.Text;
      if (!forContext)
        return;
      this.toolTipText.Text = logContent;
    }

    public void updateProgressBar(string action)
    {
      switch (action)
      {
        case "add":
          ++this.pbDownload.Value;
          break;
        case "reset":
          this.pbDownload.Value = 0;
          this.pbDownload.Maximum = 0;
          break;
      }
    }

    public void enableButtons()
    {
      this.bLookupTop.Enabled = true;
    }

    private void btDownloadByLink_Click(object sender, EventArgs e)
    {
      if (this.txtLink.Text != "")
      {
        Match match = Regex.Match(this.txtLink.Text, "[^/]+$");
        if (match.Success)
        {
          this.buildDownloadStringBySlug(match.Value);
        }
        else
        {
          int num = (int) MessageBox.Show("Link not valid.");
        }
      }
      else if (this.txtLink.Text == "" && this.txtSlug.Text != "")
      {
        this.buildDownloadStringBySlug(this.txtSlug.Text);
      }
      else
      {
        int num1 = (int) MessageBox.Show("Link or Slug missing.");
      }
    }

    private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
    {
      this.saveLocals();
    }

    private void chkUseBroadcasterAsPre_CheckedChanged(object sender, EventArgs e)
    {
    }

    private void chkAddToQueue_CheckedChanged(object sender, EventArgs e)
    {
      this.btDownloadByLink.Text = this.chkAddToQueue.Checked ? "Add To Queue" : "Download All";
    }

    private void btStartDownloadQueue_Click_1(object sender, EventArgs e)
    {
      foreach (KeyValuePair<string, Clip> keyValuePair in this.queue)
      {
        ++this.downloadCounter;
        if (this.pbDownload.Maximum == 0)
          this.pbDownload.Maximum = 1;
        else
          ++this.pbDownload.Maximum;
        this.mHandler.downloadClip(keyValuePair.Value);
        this.lbQueue.Items.Remove((object) (Uri.EscapeUriString((keyValuePair.Value.pre != "" ? keyValuePair.Value.pre + "_" : "") + keyValuePair.Value.slug) + ".mp4"));
      }
      this.queue.Clear();
    }

    private void btClose_Click(object sender, EventArgs e)
    {
      this.Close();
    }

    [DllImport("user32.dll")]
    public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

    [DllImport("user32.dll")]
    public static extern bool ReleaseCapture();

    private void MainForm_MouseDown(object sender, MouseEventArgs e)
    {
      if (e.Button != MouseButtons.Left)
        return;
      MainForm.ReleaseCapture();
      MainForm.SendMessage(this.Handle, 161, 2, 0);
    }

    private void panel1_MouseDown(object sender, MouseEventArgs e)
    {
      if (e.Button != MouseButtons.Left)
        return;
      MainForm.ReleaseCapture();
      MainForm.SendMessage(this.Handle, 161, 2, 0);
    }

    private void btLfSavePath_Click(object sender, EventArgs e)
    {
      this.fdb_SavePath.SelectedPath = this.txtSave.Text;
      if (this.fdb_SavePath.ShowDialog() != DialogResult.OK)
        return;
      this.txtSave.Text = this.fdb_SavePath.SelectedPath + "\\";
      this.saveLocals();
    }

    private async void btLookUpTopOnly_Click(object sender, EventArgs e)
    {
      await this.crawler(true, "");
    }

    private async Task crawler(bool topOnly, string cursor = "")
    {
      string str = "https://api.twitch.tv/kraken/clips/top?limit=100";
      bool betweenDates = this.dtFrom.Checked && this.dtTo.Checked;
      if (((!(this.cbGame.Text != "") || !this.chk_useGame.Checked ? (!(this.txtChannel.Text != "") ? 0 : (this.chk_useChannel.Checked ? 1 : 0)) : 1) | (topOnly ? 1 : 0)) != 0)
      {
        if (this.cbGame.Text != "" && this.chk_useGame.Checked && !topOnly)
          str = str + "&game=" + this.cbGame.Text;
        string stringToEscape = !(this.cbPeriod.Text != "") || betweenDates ? str + "&period=month" : str + "&period=" + this.cbPeriod.Text;
        if (this.txtChannel.Text != "" && this.chk_useChannel.Checked && !topOnly)
          stringToEscape = stringToEscape + "&channel=" + this.txtChannel.Text;
        if (this.ckTrend.Checked)
          stringToEscape += "&trending=true";
        if (this.txtLanguage.Text != "")
          stringToEscape = stringToEscape + "&language=" + this.txtLanguage.Text;
        if (this.cbLimit.Text != "" && !betweenDates)
          stringToEscape = stringToEscape + "&limit=" + this.cbLimit.Text;
        if (cursor != "")
          stringToEscape = stringToEscape + "&cursor=" + cursor;
        string url = Uri.EscapeUriString(stringToEscape);
        this.updateLog("REQUEST TO: " + url, false);
        ClipsObject clipsObject = (ClipsObject) JsonConvert.DeserializeObject(await this.mHandler.sendNewRequest(url), typeof (ClipsObject));
        if (clipsObject.clips == null)
        {
          this.updateLog("Nothing found.", true);
        }
        else
        {
          this.bLookupTop.Enabled = false;
          bool flag1 = false;
          foreach (Clip clip in clipsObject.clips)
          {
            bool flag2 = false;
            if (this.sTime.Text != "")
            {
              if (DateTime.Now < clip.created_at.AddHours((double) int.Parse(this.sTime.Text)))
                flag2 = true;
            }
            else
              flag2 = true;
            if (betweenDates && (clip.created_at > this.dtTo.Value || clip.created_at < this.dtFrom.Value))
              flag2 = false;
            if (clip.created_at < this.dtFrom.Value)
              flag1 = true;
            if (flag2)
            {
              ++this.downloadCounter;
              this.mHandler.downloadClip(clip);
            }
          }
          if (betweenDates && (!flag1 || clipsObject.clips.Count > 0) && (clipsObject._cursor != "" && clipsObject._cursor != cursor))
            await this.crawler(topOnly, clipsObject._cursor);
          else if (this.downloadCounter == 0)
          {
            this.bLookupTop.Enabled = true;
            this.updateLog("---- Could not find any clips ----", true);
          }
          else
          {
            this.updateLog("---- Starting " + this.downloadCounter.ToString() + " Downloads ----", true);
            this.pbDownload.Maximum = this.downloadCounter;
          }
        }
      }
      else if (!this.chk_useGame.Checked && !this.chk_useChannel.Checked)
        this.updateLog("At least one of the checkboxes of usage must be checked.", true);
      else
        this.updateLog("There must be at least a game or channel name given.", true);
    }

    private void MainForm_FormClosing_1(object sender, FormClosingEventArgs e)
    {
      this.saveLocals();
    }

    private void versionTimer_TickAsync(object sender, EventArgs e)
    {
      this.checkVersionAsync();
    }

    private void groupBox3_Enter(object sender, EventArgs e)
    {
    }

    private void lbQueue_SelectedIndexChanged(object sender, EventArgs e)
    {
    }

    private void label4_Click(object sender, EventArgs e)
    {
    }

    private void txtLink_TextChanged(object sender, EventArgs e)
    {
    }

    private void txtSlug_TextChanged(object sender, EventArgs e)
    {
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && this.components != null)
        this.components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      this.components = (IContainer) new Container();
      this.txtSave = new TextBox();
      this.label5 = new Label();
      this.btSave = new Button();
      this.lbQueue = new ListBox();
      this.btStartDownloadQueue = new Button();
      this.groupBox3 = new GroupBox();
      this.label4 = new Label();
      this.label7 = new Label();
      this.txtSlug = new TextBox();
      this.btDownloadByLink = new Button();
      this.label8 = new Label();
      this.txtLink = new TextBox();
      this.chkAddToQueue = new CheckBox();
      this.label12 = new Label();
      this.rtLog = new RichTextBox();
      this.lbVersion = new Label();
      this.groupBox2 = new GroupBox();
      this.btLfSavePath = new Button();
      this.versionTimer = new System.Windows.Forms.Timer(this.components);
      this.fdb_SavePath = new FolderBrowserDialog();
      this.btClose = new Button();
      this.label9 = new Label();
      this.panel1 = new Panel();
      this.pbDownload = new ToolStripProgressBar();
      this.toolTipText = new ToolStripStatusLabel();
      this.lbNewVersion = new ToolStripStatusLabel();
      this.txtVersion = new ToolStripStatusLabel();
      this.statusStrip1 = new StatusStrip();
      this.label6 = new Label();
      this.cbGame = new ComboBox();
      this.txtLanguage = new TextBox();
      this.label1 = new Label();
      this.label10 = new Label();
      this.txtChannel = new TextBox();
      this.bLookupTop = new Button();
      this.btLookUpTopOnly = new Button();
      this.groupBox4 = new GroupBox();
      this.chk_useChannel = new CheckBox();
      this.ckTrend = new CheckBox();
      this.chk_useGame = new CheckBox();
      this.groupBox5 = new GroupBox();
      this.label16 = new Label();
      this.label2 = new Label();
      this.label15 = new Label();
      this.cbPeriod = new ComboBox();
      this.dtTo = new DateTimePicker();
      this.cbLimit = new ComboBox();
      this.label14 = new Label();
      this.dtFrom = new DateTimePicker();
      this.label3 = new Label();
      this.label11 = new Label();
      this.sTime = new DomainUpDown();
      this.groupBox1 = new GroupBox();
      this.chkUbs = new CheckBox();
      this.label13 = new Label();
      this.txtPre = new TextBox();
      this.groupBox3.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.panel1.SuspendLayout();
      this.statusStrip1.SuspendLayout();
      this.groupBox4.SuspendLayout();
      this.groupBox5.SuspendLayout();
      this.groupBox1.SuspendLayout();
      this.SuspendLayout();
      this.txtSave.Cursor = Cursors.Default;
      this.txtSave.Location = new Point(13, 67);
      this.txtSave.Margin = new Padding(4, 5, 4, 5);
      this.txtSave.Name = "txtSave";
      this.txtSave.Size = new Size(447, 26);
      this.txtSave.TabIndex = 10;
      this.label5.AutoSize = true;
      this.label5.Location = new Point(9, 38);
      this.label5.Margin = new Padding(4, 0, 4, 0);
      this.label5.Name = "label5";
      this.label5.Size = new Size(86, 20);
      this.label5.TabIndex = 11;
      this.label5.Text = "Save Path:";
      this.btSave.FlatStyle = FlatStyle.Flat;
      this.btSave.Location = new Point(13, 117);
      this.btSave.Margin = new Padding(4, 5, 4, 5);
      this.btSave.Name = "btSave";
      this.btSave.Size = new Size(520, 35);
      this.btSave.TabIndex = 12;
      this.btSave.Text = "Save Download Location";
      this.btSave.UseVisualStyleBackColor = true;
      this.btSave.Click += new EventHandler(this.btSave_Click);
      this.lbQueue.FormattingEnabled = true;
      this.lbQueue.ItemHeight = 20;
      this.lbQueue.Location = new Point(15, 195);
      this.lbQueue.Margin = new Padding(4, 5, 4, 5);
      this.lbQueue.Name = "lbQueue";
      this.lbQueue.Size = new Size(415, 124);
      this.lbQueue.TabIndex = 6;
      this.lbQueue.SelectedIndexChanged += new EventHandler(this.lbQueue_SelectedIndexChanged);
      this.btStartDownloadQueue.FlatStyle = FlatStyle.Flat;
      this.btStartDownloadQueue.Font = new Font("Microsoft Sans Serif", 16f);
      this.btStartDownloadQueue.Location = new Point(438, 195);
      this.btStartDownloadQueue.Margin = new Padding(4, 5, 4, 5);
      this.btStartDownloadQueue.Name = "btStartDownloadQueue";
      this.btStartDownloadQueue.Size = new Size(278, 124);
      this.btStartDownloadQueue.TabIndex = 5;
      this.btStartDownloadQueue.Text = "Download All";
      this.btStartDownloadQueue.UseVisualStyleBackColor = true;
      this.btStartDownloadQueue.Click += new EventHandler(this.btStartDownloadQueue_Click_1);
      this.groupBox3.BackColor = Color.Transparent;
      this.groupBox3.Controls.Add((Control) this.label4);
      this.groupBox3.Controls.Add((Control) this.lbQueue);
      this.groupBox3.Controls.Add((Control) this.label7);
      this.groupBox3.Controls.Add((Control) this.btStartDownloadQueue);
      this.groupBox3.Controls.Add((Control) this.txtSlug);
      this.groupBox3.Controls.Add((Control) this.btDownloadByLink);
      this.groupBox3.Controls.Add((Control) this.label8);
      this.groupBox3.Controls.Add((Control) this.txtLink);
      this.groupBox3.Cursor = Cursors.Default;
      this.groupBox3.FlatStyle = FlatStyle.Flat;
      this.groupBox3.ForeColor = Color.Black;
      this.groupBox3.Location = new Point(18, 55);
      this.groupBox3.Margin = new Padding(4, 5, 4, 5);
      this.groupBox3.Name = "groupBox3";
      this.groupBox3.Padding = new Padding(4, 5, 4, 5);
      this.groupBox3.Size = new Size(740, 560);
      this.groupBox3.TabIndex = 1;
      this.groupBox3.TabStop = false;
      this.groupBox3.Enter += new EventHandler(this.groupBox3_Enter);
      this.label4.AutoSize = true;
      this.label4.Font = new Font("Microsoft Sans Serif", 12f);
      this.label4.Location = new Point(9, 151);
      this.label4.Margin = new Padding(4, 0, 4, 0);
      this.label4.Name = "label4";
      this.label4.Size = new Size(92, 29);
      this.label4.TabIndex = 7;
      this.label4.Text = "Queue:";
      this.label4.Click += new EventHandler(this.label4_Click);
      this.label7.AutoSize = true;
      this.label7.Font = new Font("Microsoft Sans Serif", 12f);
      this.label7.Location = new Point(10, 93);
      this.label7.Margin = new Padding(4, 0, 4, 0);
      this.label7.Name = "label7";
      this.label7.Size = new Size(68, 29);
      this.label7.TabIndex = 1;
      this.label7.Text = "Slug:";
      this.txtSlug.Location = new Point(81, 96);
      this.txtSlug.Margin = new Padding(4, 5, 4, 5);
      this.txtSlug.Name = "txtSlug";
      this.txtSlug.Size = new Size(349, 26);
      this.txtSlug.TabIndex = 0;
      this.txtSlug.TextChanged += new EventHandler(this.txtSlug_TextChanged);
      this.btDownloadByLink.FlatStyle = FlatStyle.Flat;
      this.btDownloadByLink.Font = new Font("Microsoft Sans Serif", 16f);
      this.btDownloadByLink.Location = new Point(438, 31);
      this.btDownloadByLink.Margin = new Padding(4, 5, 4, 5);
      this.btDownloadByLink.Name = "btDownloadByLink";
      this.btDownloadByLink.Size = new Size(278, 91);
      this.btDownloadByLink.TabIndex = 2;
      this.btDownloadByLink.Text = "Add To Queue";
      this.btDownloadByLink.UseVisualStyleBackColor = true;
      this.btDownloadByLink.Click += new EventHandler(this.btDownloadByLink_Click);
      this.label8.AutoSize = true;
      this.label8.Cursor = Cursors.Default;
      this.label8.Font = new Font("Microsoft Sans Serif", 12f);
      this.label8.Location = new Point(10, 31);
      this.label8.Margin = new Padding(4, 0, 4, 0);
      this.label8.Name = "label8";
      this.label8.Size = new Size(63, 29);
      this.label8.TabIndex = 1;
      this.label8.Text = "Link:";
      this.txtLink.Location = new Point(77, 31);
      this.txtLink.Margin = new Padding(4, 5, 4, 5);
      this.txtLink.Name = "txtLink";
      this.txtLink.Size = new Size(353, 26);
      this.txtLink.TabIndex = 0;
      this.txtLink.TextChanged += new EventHandler(this.txtLink_TextChanged);
      this.chkAddToQueue.AutoSize = true;
      this.chkAddToQueue.Location = new Point(947, 900);
      this.chkAddToQueue.Margin = new Padding(4, 5, 4, 5);
      this.chkAddToQueue.Name = "chkAddToQueue";
      this.chkAddToQueue.Size = new Size(116, 24);
      this.chkAddToQueue.TabIndex = 2;
      this.chkAddToQueue.Text = "Add to Que";
      this.chkAddToQueue.UseVisualStyleBackColor = true;
      this.chkAddToQueue.Visible = false;
      this.chkAddToQueue.CheckedChanged += new EventHandler(this.chkAddToQueue_CheckedChanged);
      this.label12.AutoSize = true;
      this.label12.ForeColor = Color.FromArgb(38, 38, 38);
      this.label12.Location = new Point(14, 620);
      this.label12.Margin = new Padding(4, 0, 4, 0);
      this.label12.Name = "label12";
      this.label12.Size = new Size(40, 20);
      this.label12.TabIndex = 14;
      this.label12.Text = "Log:";
      this.rtLog.Font = new Font("Microsoft Sans Serif", 8f);
      this.rtLog.ForeColor = Color.FromArgb(38, 38, 38);
      this.rtLog.Location = new Point(22, 645);
      this.rtLog.Margin = new Padding(4, 5, 4, 5);
      this.rtLog.Name = "rtLog";
      this.rtLog.ReadOnly = true;
      this.rtLog.Size = new Size(1297, 239);
      this.rtLog.TabIndex = 13;
      this.rtLog.Text = "";
      this.lbVersion.AutoSize = true;
      this.lbVersion.ForeColor = Color.Black;
      this.lbVersion.Location = new Point(1298, 901);
      this.lbVersion.Margin = new Padding(4, 0, 4, 0);
      this.lbVersion.Name = "lbVersion";
      this.lbVersion.Size = new Size(24, 20);
      this.lbVersion.TabIndex = 4;
      this.lbVersion.Text = "---";
      this.lbVersion.Visible = false;
      this.groupBox2.BackColor = Color.Transparent;
      this.groupBox2.Controls.Add((Control) this.btLfSavePath);
      this.groupBox2.Controls.Add((Control) this.btSave);
      this.groupBox2.Controls.Add((Control) this.txtSave);
      this.groupBox2.Controls.Add((Control) this.label5);
      this.groupBox2.ForeColor = Color.Black;
      this.groupBox2.Location = new Point(766, 56);
      this.groupBox2.Margin = new Padding(4, 5, 4, 5);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Padding = new Padding(4, 5, 4, 5);
      this.groupBox2.Size = new Size(550, 179);
      this.groupBox2.TabIndex = 16;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Settings";
      this.btLfSavePath.FlatStyle = FlatStyle.Flat;
      this.btLfSavePath.Location = new Point(477, 63);
      this.btLfSavePath.Margin = new Padding(4, 5, 4, 5);
      this.btLfSavePath.Name = "btLfSavePath";
      this.btLfSavePath.Size = new Size(56, 35);
      this.btLfSavePath.TabIndex = 13;
      this.btLfSavePath.Text = "...";
      this.btLfSavePath.UseVisualStyleBackColor = true;
      this.btLfSavePath.Click += new EventHandler(this.btLfSavePath_Click);
      this.versionTimer.Enabled = true;
      this.versionTimer.Interval = 30000;
      this.versionTimer.Tick += new EventHandler(this.versionTimer_TickAsync);
      this.btClose.FlatStyle = FlatStyle.Flat;
      this.btClose.Location = new Point(1293, 6);
      this.btClose.Margin = new Padding(4, 5, 4, 5);
      this.btClose.Name = "btClose";
      this.btClose.Size = new Size(33, 35);
      this.btClose.TabIndex = 17;
      this.btClose.Text = "X";
      this.btClose.UseVisualStyleBackColor = true;
      this.btClose.Click += new EventHandler(this.btClose_Click);
      this.label9.AutoSize = true;
      this.label9.BackColor = Color.Transparent;
      this.label9.Font = new Font("Leelawadee UI", 11.25f, FontStyle.Regular, GraphicsUnit.Point, (byte) 0);
      this.label9.ForeColor = SystemColors.ControlDarkDark;
      this.label9.Location = new Point(9, 8);
      this.label9.Margin = new Padding(4, 0, 4, 0);
      this.label9.Name = "label9";
      this.label9.Size = new Size(305, 31);
      this.label9.TabIndex = 18;
      this.label9.Text = "Flight Sim Bests Downloader V1.1.0";
      this.panel1.BackColor = Color.LightGray;
      this.panel1.Controls.Add((Control) this.label9);
      this.panel1.Controls.Add((Control) this.btClose);
      this.panel1.Cursor = Cursors.Default;
      this.panel1.Location = new Point(0, -2);
      this.panel1.Margin = new Padding(4, 5, 4, 5);
      this.panel1.Name = "panel1";
      this.panel1.Size = new Size(1335, 48);
      this.panel1.TabIndex = 18;
      this.panel1.MouseDown += new MouseEventHandler(this.panel1_MouseDown);
      this.pbDownload.Name = "pbDownload";
      this.pbDownload.Size = new Size(300, 25);
      this.toolTipText.Name = "toolTipText";
      this.toolTipText.Size = new Size(0, 26);
      this.lbNewVersion.ForeColor = Color.DarkRed;
      this.lbNewVersion.ImageScaling = ToolStripItemImageScaling.None;
      this.lbNewVersion.Name = "lbNewVersion";
      this.lbNewVersion.Size = new Size(505, 26);
      this.lbNewVersion.Spring = true;
      this.lbNewVersion.Text = "VERSION";
      this.lbNewVersion.TextAlign = ContentAlignment.MiddleRight;
      this.txtVersion.ForeColor = SystemColors.ControlDark;
      this.txtVersion.Name = "txtVersion";
      this.txtVersion.Size = new Size(505, 26);
      this.txtVersion.Spring = true;
      this.txtVersion.Text = "VERSION";
      this.txtVersion.TextAlign = ContentAlignment.MiddleRight;
      this.statusStrip1.ImageScalingSize = new Size(24, 24);
      this.statusStrip1.Items.AddRange(new ToolStripItem[4]
      {
        (ToolStripItem) this.pbDownload,
        (ToolStripItem) this.toolTipText,
        (ToolStripItem) this.lbNewVersion,
        (ToolStripItem) this.txtVersion
      });
      this.statusStrip1.Location = new Point(0, 894);
      this.statusStrip1.Name = "statusStrip1";
      this.statusStrip1.Padding = new Padding(2, 0, 21, 0);
      this.statusStrip1.Size = new Size(1335, 31);
      this.statusStrip1.SizingGrip = false;
      this.statusStrip1.Stretch = false;
      this.statusStrip1.TabIndex = 16;
      this.statusStrip1.Text = "mainStatusStrip";
      this.statusStrip1.Visible = false;
      this.label6.AutoSize = true;
      this.label6.Location = new Point(9, 108);
      this.label6.Margin = new Padding(4, 0, 4, 0);
      this.label6.Name = "label6";
      this.label6.Size = new Size(85, 20);
      this.label6.TabIndex = 13;
      this.label6.Text = "Language:";
      this.cbGame.FormattingEnabled = true;
      this.cbGame.Items.AddRange(new object[2]
      {
        (object) "PLAYERUNKNOWN'S BATTLEGROUNDS",
        (object) "Fortnite"
      });
      this.cbGame.Location = new Point(144, 22);
      this.cbGame.Margin = new Padding(4, 5, 4, 5);
      this.cbGame.Name = "cbGame";
      this.cbGame.Size = new Size(290, 28);
      this.cbGame.TabIndex = 0;
      this.txtLanguage.Location = new Point(144, 103);
      this.txtLanguage.Margin = new Padding(4, 5, 4, 5);
      this.txtLanguage.Name = "txtLanguage";
      this.txtLanguage.Size = new Size(290, 26);
      this.txtLanguage.TabIndex = 14;
      this.txtLanguage.Text = "en,de";
      this.label1.AutoSize = true;
      this.label1.Location = new Point(9, 31);
      this.label1.Margin = new Padding(4, 0, 4, 0);
      this.label1.Name = "label1";
      this.label1.Size = new Size(57, 20);
      this.label1.TabIndex = 1;
      this.label1.Text = "Game:";
      this.label10.AutoSize = true;
      this.label10.Location = new Point(9, 68);
      this.label10.Margin = new Padding(4, 0, 4, 0);
      this.label10.Name = "label10";
      this.label10.Size = new Size(112, 20);
      this.label10.TabIndex = 17;
      this.label10.Text = "Channelname:";
      this.txtChannel.Location = new Point(144, 63);
      this.txtChannel.Margin = new Padding(4, 5, 4, 5);
      this.txtChannel.Name = "txtChannel";
      this.txtChannel.Size = new Size(290, 26);
      this.txtChannel.TabIndex = 18;
      this.bLookupTop.FlatStyle = FlatStyle.Flat;
      this.bLookupTop.Location = new Point(340, 515);
      this.bLookupTop.Margin = new Padding(4, 5, 4, 5);
      this.bLookupTop.Name = "bLookupTop";
      this.bLookupTop.Size = new Size(112, 35);
      this.bLookupTop.TabIndex = 2;
      this.bLookupTop.Text = "Crawl";
      this.bLookupTop.UseVisualStyleBackColor = true;
      this.bLookupTop.Click += new EventHandler(this.bLookupTop_Click);
      this.btLookUpTopOnly.FlatStyle = FlatStyle.Flat;
      this.btLookUpTopOnly.Location = new Point(9, 515);
      this.btLookUpTopOnly.Margin = new Padding(4, 5, 4, 5);
      this.btLookUpTopOnly.Name = "btLookUpTopOnly";
      this.btLookUpTopOnly.Size = new Size(171, 35);
      this.btLookUpTopOnly.TabIndex = 24;
      this.btLookUpTopOnly.Text = "Crawl top only";
      this.btLookUpTopOnly.UseVisualStyleBackColor = true;
      this.btLookUpTopOnly.Click += new EventHandler(this.btLookUpTopOnly_Click);
      this.groupBox4.Controls.Add((Control) this.chk_useChannel);
      this.groupBox4.Controls.Add((Control) this.ckTrend);
      this.groupBox4.Controls.Add((Control) this.chk_useGame);
      this.groupBox4.Location = new Point(9, 438);
      this.groupBox4.Margin = new Padding(4, 5, 4, 5);
      this.groupBox4.Name = "groupBox4";
      this.groupBox4.Padding = new Padding(4, 5, 4, 5);
      this.groupBox4.Size = new Size(444, 68);
      this.groupBox4.TabIndex = 29;
      this.groupBox4.TabStop = false;
      this.groupBox4.Text = "Filter";
      this.chk_useChannel.AutoSize = true;
      this.chk_useChannel.Location = new Point(250, 29);
      this.chk_useChannel.Margin = new Padding(4, 5, 4, 5);
      this.chk_useChannel.Name = "chk_useChannel";
      this.chk_useChannel.Size = new Size(167, 24);
      this.chk_useChannel.TabIndex = 23;
      this.chk_useChannel.Text = "Use Channelname";
      this.chk_useChannel.UseVisualStyleBackColor = true;
      this.ckTrend.AutoSize = true;
      this.ckTrend.Checked = true;
      this.ckTrend.CheckState = CheckState.Checked;
      this.ckTrend.Location = new Point(16, 29);
      this.ckTrend.Margin = new Padding(4, 5, 4, 5);
      this.ckTrend.Name = "ckTrend";
      this.ckTrend.Size = new Size(97, 24);
      this.ckTrend.TabIndex = 3;
      this.ckTrend.Text = "Trending";
      this.ckTrend.UseVisualStyleBackColor = true;
      this.chk_useGame.AutoSize = true;
      this.chk_useGame.Location = new Point(128, 29);
      this.chk_useGame.Margin = new Padding(4, 5, 4, 5);
      this.chk_useGame.Name = "chk_useGame";
      this.chk_useGame.Size = new Size(112, 24);
      this.chk_useGame.TabIndex = 22;
      this.chk_useGame.Text = "Use Game";
      this.chk_useGame.UseVisualStyleBackColor = true;
      this.groupBox5.Controls.Add((Control) this.label16);
      this.groupBox5.Controls.Add((Control) this.label2);
      this.groupBox5.Controls.Add((Control) this.label15);
      this.groupBox5.Controls.Add((Control) this.cbPeriod);
      this.groupBox5.Controls.Add((Control) this.dtTo);
      this.groupBox5.Controls.Add((Control) this.cbLimit);
      this.groupBox5.Controls.Add((Control) this.label14);
      this.groupBox5.Controls.Add((Control) this.dtFrom);
      this.groupBox5.Controls.Add((Control) this.label3);
      this.groupBox5.Controls.Add((Control) this.label11);
      this.groupBox5.Controls.Add((Control) this.sTime);
      this.groupBox5.Location = new Point(9, 143);
      this.groupBox5.Margin = new Padding(4, 5, 4, 5);
      this.groupBox5.Name = "groupBox5";
      this.groupBox5.Padding = new Padding(4, 5, 4, 5);
      this.groupBox5.Size = new Size(444, 286);
      this.groupBox5.TabIndex = 7;
      this.groupBox5.TabStop = false;
      this.groupBox5.Text = "Time-Filter";
      this.label16.AutoSize = true;
      this.label16.BackColor = Color.Transparent;
      this.label16.ForeColor = Color.Silver;
      this.label16.Location = new Point(56, 152);
      this.label16.Margin = new Padding(4, 0, 4, 0);
      this.label16.Name = "label16";
      this.label16.Size = new Size(371, 20);
      this.label16.TabIndex = 29;
      this.label16.Text = "--------------------------------- OR ---------------------------------";
      this.label2.AutoSize = true;
      this.label2.Location = new Point(9, 25);
      this.label2.Margin = new Padding(4, 0, 4, 0);
      this.label2.Name = "label2";
      this.label2.Size = new Size(58, 20);
      this.label2.TabIndex = 5;
      this.label2.Text = "Period:";
      this.label15.AutoSize = true;
      this.label15.Location = new Point(10, 249);
      this.label15.Margin = new Padding(4, 0, 4, 0);
      this.label15.Name = "label15";
      this.label15.Size = new Size(31, 20);
      this.label15.TabIndex = 28;
      this.label15.Text = "To:";
      this.cbPeriod.FormattingEnabled = true;
      this.cbPeriod.Items.AddRange(new object[4]
      {
        (object) "day",
        (object) "week",
        (object) "month",
        (object) "all"
      });
      this.cbPeriod.Location = new Point(177, 20);
      this.cbPeriod.Margin = new Padding(4, 5, 4, 5);
      this.cbPeriod.Name = "cbPeriod";
      this.cbPeriod.Size = new Size(256, 28);
      this.cbPeriod.TabIndex = 4;
      this.dtTo.Checked = false;
      this.dtTo.Cursor = Cursors.Hand;
      this.dtTo.CustomFormat = "DD.MM.YYYY";
      this.dtTo.Format = DateTimePickerFormat.Short;
      this.dtTo.ImeMode = ImeMode.Off;
      this.dtTo.Location = new Point(236, 240);
      this.dtTo.Margin = new Padding(4, 5, 4, 5);
      this.dtTo.Name = "dtTo";
      this.dtTo.ShowCheckBox = true;
      this.dtTo.Size = new Size(196, 26);
      this.dtTo.TabIndex = 27;
      this.cbLimit.FormattingEnabled = true;
      this.cbLimit.Items.AddRange(new object[13]
      {
        (object) "1",
        (object) "2",
        (object) "3",
        (object) "4",
        (object) "5",
        (object) "6",
        (object) "7",
        (object) "8",
        (object) "9",
        (object) "10",
        (object) "25",
        (object) "50",
        (object) "100"
      });
      this.cbLimit.Location = new Point(177, 102);
      this.cbLimit.Margin = new Padding(4, 5, 4, 5);
      this.cbLimit.Name = "cbLimit";
      this.cbLimit.Size = new Size(256, 28);
      this.cbLimit.TabIndex = 15;
      this.label14.AutoSize = true;
      this.label14.Location = new Point(9, 211);
      this.label14.Margin = new Padding(4, 0, 4, 0);
      this.label14.Name = "label14";
      this.label14.Size = new Size(50, 20);
      this.label14.TabIndex = 26;
      this.label14.Text = "From:";
      this.dtFrom.Checked = false;
      this.dtFrom.Cursor = Cursors.Hand;
      this.dtFrom.CustomFormat = "DD.MM.YYYY";
      this.dtFrom.Format = DateTimePickerFormat.Short;
      this.dtFrom.ImeMode = ImeMode.Off;
      this.dtFrom.Location = new Point(236, 202);
      this.dtFrom.Margin = new Padding(4, 5, 4, 5);
      this.dtFrom.Name = "dtFrom";
      this.dtFrom.ShowCheckBox = true;
      this.dtFrom.Size = new Size(196, 26);
      this.dtFrom.TabIndex = 25;
      this.label3.AutoSize = true;
      this.label3.Location = new Point(12, 106);
      this.label3.Margin = new Padding(4, 0, 4, 0);
      this.label3.Name = "label3";
      this.label3.Size = new Size(46, 20);
      this.label3.TabIndex = 16;
      this.label3.Text = "Limit:";
      this.label11.AutoSize = true;
      this.label11.Location = new Point(9, 65);
      this.label11.Margin = new Padding(4, 0, 4, 0);
      this.label11.Name = "label11";
      this.label11.Size = new Size(110, 20);
      this.label11.TabIndex = 20;
      this.label11.Text = "Time in Hours:";
      this.sTime.Items.Add((object) "48");
      this.sTime.Items.Add((object) "47");
      this.sTime.Items.Add((object) "46");
      this.sTime.Items.Add((object) "45");
      this.sTime.Items.Add((object) "44");
      this.sTime.Items.Add((object) "43");
      this.sTime.Items.Add((object) "42");
      this.sTime.Items.Add((object) "41");
      this.sTime.Items.Add((object) "40");
      this.sTime.Items.Add((object) "39");
      this.sTime.Items.Add((object) "38");
      this.sTime.Items.Add((object) "37");
      this.sTime.Items.Add((object) "36");
      this.sTime.Items.Add((object) "35");
      this.sTime.Items.Add((object) "34");
      this.sTime.Items.Add((object) "33");
      this.sTime.Items.Add((object) "32");
      this.sTime.Items.Add((object) "31");
      this.sTime.Items.Add((object) "30");
      this.sTime.Items.Add((object) "29");
      this.sTime.Items.Add((object) "28");
      this.sTime.Items.Add((object) "27");
      this.sTime.Items.Add((object) "26");
      this.sTime.Items.Add((object) "25");
      this.sTime.Items.Add((object) "24");
      this.sTime.Items.Add((object) "23");
      this.sTime.Items.Add((object) "22");
      this.sTime.Items.Add((object) "21");
      this.sTime.Items.Add((object) "20");
      this.sTime.Items.Add((object) "19");
      this.sTime.Items.Add((object) "18");
      this.sTime.Items.Add((object) "17");
      this.sTime.Items.Add((object) "16");
      this.sTime.Items.Add((object) "15");
      this.sTime.Items.Add((object) "14");
      this.sTime.Items.Add((object) "13");
      this.sTime.Items.Add((object) "12");
      this.sTime.Items.Add((object) "11");
      this.sTime.Items.Add((object) "10");
      this.sTime.Items.Add((object) "9");
      this.sTime.Items.Add((object) "8");
      this.sTime.Items.Add((object) "7");
      this.sTime.Items.Add((object) "6");
      this.sTime.Items.Add((object) "5");
      this.sTime.Items.Add((object) "4");
      this.sTime.Items.Add((object) "3");
      this.sTime.Items.Add((object) "2");
      this.sTime.Items.Add((object) "1");
      this.sTime.Location = new Point(177, 62);
      this.sTime.Margin = new Padding(4, 5, 4, 5);
      this.sTime.Name = "sTime";
      this.sTime.Size = new Size(258, 26);
      this.sTime.TabIndex = 21;
      this.groupBox1.Controls.Add((Control) this.groupBox5);
      this.groupBox1.Controls.Add((Control) this.groupBox4);
      this.groupBox1.Controls.Add((Control) this.btLookUpTopOnly);
      this.groupBox1.Controls.Add((Control) this.bLookupTop);
      this.groupBox1.Controls.Add((Control) this.txtChannel);
      this.groupBox1.Controls.Add((Control) this.label10);
      this.groupBox1.Controls.Add((Control) this.label1);
      this.groupBox1.Controls.Add((Control) this.txtLanguage);
      this.groupBox1.Controls.Add((Control) this.cbGame);
      this.groupBox1.Controls.Add((Control) this.label6);
      this.groupBox1.ForeColor = Color.FromArgb(38, 38, 38);
      this.groupBox1.Location = new Point(44, 901);
      this.groupBox1.Margin = new Padding(4, 5, 4, 5);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Padding = new Padding(4, 5, 4, 5);
      this.groupBox1.Size = new Size(10, 10);
      this.groupBox1.TabIndex = 15;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Crawler";
      this.groupBox1.Visible = false;
      this.chkUbs.AutoSize = true;
      this.chkUbs.Location = new Point(1071, 901);
      this.chkUbs.Margin = new Padding(4, 5, 4, 5);
      this.chkUbs.Name = "chkUbs";
      this.chkUbs.Size = new Size(219, 24);
      this.chkUbs.TabIndex = 5;
      this.chkUbs.Text = "Use Broadcaster as Prefix";
      this.chkUbs.UseVisualStyleBackColor = true;
      this.chkUbs.Visible = false;
      this.chkUbs.CheckedChanged += new EventHandler(this.chkUseBroadcasterAsPre_CheckedChanged);
      this.label13.AutoSize = true;
      this.label13.Location = new Point(536, 898);
      this.label13.Margin = new Padding(4, 0, 4, 0);
      this.label13.Name = "label13";
      this.label13.Size = new Size(52, 20);
      this.label13.TabIndex = 4;
      this.label13.Text = "Prefix:";
      this.label13.Visible = false;
      this.txtPre.Location = new Point(590, 898);
      this.txtPre.Margin = new Padding(4, 5, 4, 5);
      this.txtPre.Name = "txtPre";
      this.txtPre.Size = new Size(349, 26);
      this.txtPre.TabIndex = 3;
      this.txtPre.Visible = false;
      this.AutoScaleDimensions = new SizeF(9f, 20f);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.AutoSizeMode = AutoSizeMode.GrowAndShrink;
      this.BackColor = Color.WhiteSmoke;
      this.ClientSize = new Size(1335, 925);
      this.Controls.Add((Control) this.panel1);
      this.Controls.Add((Control) this.chkUbs);
      this.Controls.Add((Control) this.lbVersion);
      this.Controls.Add((Control) this.label13);
      this.Controls.Add((Control) this.groupBox2);
      this.Controls.Add((Control) this.chkAddToQueue);
      this.Controls.Add((Control) this.txtPre);
      this.Controls.Add((Control) this.statusStrip1);
      this.Controls.Add((Control) this.label12);
      this.Controls.Add((Control) this.groupBox1);
      this.Controls.Add((Control) this.groupBox3);
      this.Controls.Add((Control) this.rtLog);
      this.FormBorderStyle = FormBorderStyle.None;
      this.Margin = new Padding(4, 5, 4, 5);
      this.Name = nameof (MainForm);
      this.StartPosition = FormStartPosition.CenterScreen;
      this.Text = "Flight Sim Bests Downloader V1.1.0";
      this.FormClosing += new FormClosingEventHandler(this.MainForm_FormClosing_1);
      this.MouseDown += new MouseEventHandler(this.MainForm_MouseDown);
      this.groupBox3.ResumeLayout(false);
      this.groupBox3.PerformLayout();
      this.groupBox2.ResumeLayout(false);
      this.groupBox2.PerformLayout();
      this.panel1.ResumeLayout(false);
      this.panel1.PerformLayout();
      this.statusStrip1.ResumeLayout(false);
      this.statusStrip1.PerformLayout();
      this.groupBox4.ResumeLayout(false);
      this.groupBox4.PerformLayout();
      this.groupBox5.ResumeLayout(false);
      this.groupBox5.PerformLayout();
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();
    }
  }
}
