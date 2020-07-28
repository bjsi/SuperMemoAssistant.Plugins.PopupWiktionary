using Anotar.Serilog;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Builders;
using SuperMemoAssistant.Plugins.PopupWindow.Interop;
using SuperMemoAssistant.Sys.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;

namespace SuperMemoAssistant.Plugins.PopupWiktionary.Service
{

  //
  // ARTICLE RETRIEVAL METHODS

  public partial class ContentService
  {

    [LogToErrorOnException]
    public RemoteTask<BrowserContent> FetchArticleHtml(string url)
    {

      if (!UrlUtils.IsMobileWiktionaryUrl(url))
        return null;

      return GetArticleHtmlAsync(url);

    }

    private async Task<BrowserContent> GetArticleHtmlAsync(string url)
    {

      string html = null;
      var refs = new References();

      try
      {

        Uri uri = new Uri(url);
        var articleTitle = uri.Segments.Last();
        string[] splitUri = uri.Host.Split('.');
        string language = splitUri[0];
        string response = await GetAsync(url);

        if (!string.IsNullOrEmpty(response))
          html = HtmlEx.FilterMobileWiktionaryArticle(response, language);

        refs.Title = articleTitle;
        refs.Link = UrlUtils.ConvMobWiktionaryToDesktop(url);
        refs.Source = "Wiktionary";

        return new BrowserContent(html, refs, true, ContentType.Article);

      }
      catch (RemotingException) { }

      return null;

    }

  }
}
