#region License & Metadata

// The MIT License (MIT)
// 
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the 
// Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
// 
// 
// Created On:   7/16/2020 9:52:21 PM
// Modified By:  james

#endregion




namespace SuperMemoAssistant.Plugins.PopupWiktionary
{
  using System.Collections.Generic;
  using System.Diagnostics.CodeAnalysis;
  using System.Runtime.Remoting;
  using System.Windows.Input;
  using Anotar.Serilog;
  using SuperMemoAssistant.Plugins.PopupWindow.Interop;
  using SuperMemoAssistant.Services;
  using SuperMemoAssistant.Services.IO.HotKeys;
  using SuperMemoAssistant.Services.IO.Keyboard;
  using SuperMemoAssistant.Services.Sentry;
  using SuperMemoAssistant.Services.UI.Configuration;
  using SuperMemoAssistant.Sys.IO.Devices;
  using SuperMemoAssistant.Sys.Remoting;

  // ReSharper disable once UnusedMember.Global
  // ReSharper disable once ClassNeverInstantiated.Global
  [SuppressMessage("Microsoft.Naming", "CA1724:TypeNamesShouldNotMatchNamespaces")]
  public class PopupWiktionaryPlugin : SentrySMAPluginBase<PopupWiktionaryPlugin>
  {
    #region Constructors

    /// <inheritdoc />
    public PopupWiktionaryPlugin() : base("Enter your Sentry.io api key (strongly recommended)") { }

    #endregion




    #region Properties Impl - Public

    /// <inheritdoc />
    public override string Name => "PopupWiktionary";

    /// <inheritdoc />
    public override bool HasSettings => false;
    private PopupWiktionaryProvider _popupWikiProvider { get; } = new PopupWiktionaryProvider();
    private IPopupWindowSvc popupWindowSvc { get; set; }
    public PopupWiktionaryCfg Config;

    #endregion


    private void LoadConfig()
    {
      Config = Svc.Configuration.Load<PopupWiktionaryCfg>() ?? new PopupWiktionaryCfg();
    }



    #region Methods Impl

    /// <inheritdoc />
    protected override void PluginInit()
    {
      LoadConfig();

      this.RegisterPopupWindowProvider("Wiktionary", new List<string> { UrlUtils.DesktopWiktionaryRegex, UrlUtils.MobileWiktionaryRegex }, _popupWikiProvider);

      popupWindowSvc = GetService<IPopupWindowSvc>();

      Svc.HotKeyManager
         .RegisterGlobal(
           "SearchWiktionary",
           "Search Wiktionary for the selected term",
           HotKeyScopes.SM,
           new HotKey(Key.W, KeyModifiers.CtrlAltShift),
           WiktionarySearch
      );

    }

    /// <inheritdoc />
    public override void ShowSettings()
    {
      ConfigurationWindow.ShowAndActivate(HotKeyManager.Instance, Config);
    }


    [LogToErrorOnException]
    public async void WiktionarySearch()
    {
      try
      {

        string searchTerm = Popups.GetSearchQuery("Search Wiktionary");
        if (searchTerm.IsNullOrEmpty())
          return;

        if (popupWindowSvc.IsNull())
        {
          LogTo.Warning("Failed to open new Wiktionary window in PopupBrowser because popupService is null.");
          return;
        }

        if (await popupWindowSvc.Open(searchTerm, ContentType.Search))
          LogTo.Debug("Successfully opened new Wiktionary window in PopupBrowser");
        else
          LogTo.Error("Failed to open new Wiktionary window in PopupBrowser");

      }
      catch (RemotingException) { }

    }

    #endregion


    #region Methods

    #endregion
  }
}
