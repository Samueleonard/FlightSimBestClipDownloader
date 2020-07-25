// Decompiled with JetBrains decompiler
// Type: TwitchClipDownloader.Properties.Settings
// Assembly: Flight Sim Bests Downloader, Version=3.3.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FADFCB25-C995-475D-8D74-04FBE792920D
// Assembly location: C:\Users\samue\Desktop\FSBDownloader\Flight Sim Bests Downloader.exe

using System.CodeDom.Compiler;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace TwitchClipDownloader.Properties
{
  [CompilerGenerated]
  [GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "15.9.0.0")]
  internal sealed class Settings : ApplicationSettingsBase
  {
    private static Settings defaultInstance = (Settings) SettingsBase.Synchronized((SettingsBase) new Settings());

    public static Settings Default
    {
      get
      {
        return Settings.defaultInstance;
      }
    }

    [UserScopedSetting]
    [DebuggerNonUserCode]
    [DefaultSettingValue("C:\\")]
    public string savePath
    {
      get
      {
        return (string) this[nameof (savePath)];
      }
      set
      {
        this[nameof (savePath)] = (object) value;
      }
    }

    [ApplicationScopedSetting]
    [DebuggerNonUserCode]
    [DefaultSettingValue("h91ejo3shtecvixy0xhcd6mulvitd8")]
    public string twitchClientId
    {
      get
      {
        return (string) this[nameof (twitchClientId)];
      }
    }

    [UserScopedSetting]
    [DebuggerNonUserCode]
    [DefaultSettingValue("True")]
    public bool addToQueue
    {
      get
      {
        return (bool) this[nameof (addToQueue)];
      }
      set
      {
        this[nameof (addToQueue)] = (object) value;
      }
    }

    [UserScopedSetting]
    [DebuggerNonUserCode]
    [DefaultSettingValue("True")]
    public bool brd
    {
      get
      {
        return (bool) this[nameof (brd)];
      }
      set
      {
        this[nameof (brd)] = (object) value;
      }
    }

    [UserScopedSetting]
    [DebuggerNonUserCode]
    [DefaultSettingValue("True")]
    public bool cusec
    {
      get
      {
        return (bool) this[nameof (cusec)];
      }
      set
      {
        this[nameof (cusec)] = (object) value;
      }
    }

    [UserScopedSetting]
    [DebuggerNonUserCode]
    [DefaultSettingValue("True")]
    public bool cuseg
    {
      get
      {
        return (bool) this[nameof (cuseg)];
      }
      set
      {
        this[nameof (cuseg)] = (object) value;
      }
    }
  }
}
