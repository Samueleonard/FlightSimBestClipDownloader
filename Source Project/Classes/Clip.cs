// Decompiled with JetBrains decompiler
// Type: TwitchClipDownloader.Classes.Clip
// Assembly: Flight Sim Bests Downloader, Version=3.3.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FADFCB25-C995-475D-8D74-04FBE792920D
// Assembly location: C:\Users\samue\Desktop\FSBDownloader\Flight Sim Bests Downloader.exe

using System;

namespace TwitchClipDownloader.Classes
{
  internal class Clip
  {
    public string slug { get; set; }

    public Broadcaster broadcaster { get; set; }

    public string title { get; set; }

    public Thumbnails thumbnails { get; set; }

    public DateTime created_at { get; set; }

    public string pre { get; set; }
  }
}
