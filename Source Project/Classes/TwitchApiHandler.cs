// Decompiled with JetBrains decompiler
// Type: TwitchClipDownloader.Classes.TwitchApiHandler
// Assembly: Flight Sim Bests Downloader, Version=3.3.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FADFCB25-C995-475D-8D74-04FBE792920D
// Assembly location: C:\Users\samue\Desktop\FSBDownloader\Flight Sim Bests Downloader.exe

using System;
using System.ComponentModel;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using TwitchClipDownloader.Properties;

namespace TwitchClipDownloader.Classes
{
  internal class TwitchApiHandler
  {
    private MainForm form;

    public TwitchApiHandler(MainForm activeForm)
    {
      this.form = activeForm;
    }

    public async Task<string> sendNewRequest(string url)
    {
      HttpClient httpClient = new HttpClient();
      HttpRequestMessage httpRequestMessage = new HttpRequestMessage()
      {
        RequestUri = new Uri(url),
        Method = HttpMethod.Get
      };
      httpRequestMessage.Headers.Add("client-id", Settings.Default.twitchClientId);
      httpRequestMessage.Headers.Add("accept", "application/vnd.twitchtv.v5+json");
      HttpRequestMessage request = httpRequestMessage;
      return await (await httpClient.SendAsync(request)).Content.ReadAsStringAsync();
    }

    public void downloadClip(Clip item)
    {
      string uriString = item.thumbnails.medium.Replace("-preview-480x272.jpg", ".mp4");
      using (WebClient webClient = new WebClient())
      {
        webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(this.Wc_DownloadFileCompleted);
        string str = Uri.EscapeUriString((item.pre != "" ? item.pre + "_" : "") + item.slug);
        webClient.DownloadFileAsync(new Uri(uriString), Settings.Default.savePath.Replace("\\", "\\\\") + str + ".mp4");
        this.form.updateLog("---- " + uriString + " ----", true);
      }
    }

    private void Wc_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
    {
      if (e.Error != null)
        this.form.updateLog(e.Error.InnerException.Message, true);
      --this.form.downloadCounter;
      this.form.updateProgressBar("add");
      if (this.form.downloadCounter == 0)
      {
        this.form.updateLog("---- All downloads finished ----", true);
        this.form.enableButtons();
        this.form.updateProgressBar("reset");
      }
      else
        this.form.updateLog("Downloads left: " + this.form.downloadCounter.ToString(), true);
    }
  }
}
