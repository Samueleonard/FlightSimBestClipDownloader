// Decompiled with JetBrains decompiler
// Type: VC.VersionControl
// Assembly: Flight Sim Bests Downloader, Version=3.3.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FADFCB25-C995-475D-8D74-04FBE792920D
// Assembly location: C:\Users\samue\Desktop\FSBDownloader\Flight Sim Bests Downloader.exe

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace VC
{
  internal class VersionControl
  {
    private string hashedVersion = VersionControl.GetMd5Hash(Assembly.GetExecutingAssembly().GetName().Version.ToString(2));
    private string hashedApplication = VersionControl.GetMd5Hash("TwitchClipDownloader");
    private string homepage = "https://machigatta.com/projects.json";

    public async Task<bool> checkServerJsonAsync()
    {
      if (!this.checkInternetConnection())
        return false;
      HttpClient httpClient = new HttpClient();
      HttpRequestMessage request = new HttpRequestMessage()
      {
        RequestUri = new Uri(this.homepage),
        Method = HttpMethod.Get
      };
      foreach (KeyValuePair<string, JToken> keyValuePair in (JObject) ((ServerJson) JsonConvert.DeserializeObject(await (await httpClient.SendAsync(request)).Content.ReadAsStringAsync(), typeof (ServerJson))).check)
      {
        if (keyValuePair.Key == this.hashedApplication && keyValuePair.Value.Value<string>() != this.hashedVersion)
          return true;
      }
      return false;
    }

    private static string GetMd5Hash(string input)
    {
      using (MD5 md5 = MD5.Create())
      {
        byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
        StringBuilder stringBuilder = new StringBuilder();
        for (int index = 0; index < hash.Length; ++index)
          stringBuilder.Append(hash[index].ToString("x2"));
        return stringBuilder.ToString();
      }
    }

    private bool checkInternetConnection()
    {
      try
      {
        using (WebClient webClient = new WebClient())
        {
          using (webClient.OpenRead(this.homepage))
            return true;
        }
      }
      catch
      {
        return false;
      }
    }
  }
}
