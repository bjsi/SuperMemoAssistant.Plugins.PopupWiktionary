using Anotar.Serilog;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PluginManager.Interop.Sys;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Builders;
using SuperMemoAssistant.Plugins.PopupWindow.Interop;
using SuperMemoAssistant.Sys.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SuperMemoAssistant.Plugins.PopupWiktionary
{
  //
  // MAIN ARTICLE RETRIEVAL METHODS

  public partial class PopupWiktionaryProvider : PerpetualMarshalByRefObject, IContentProvider
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

        return new BrowserContent(html, refs, true);

      }
      catch (RemotingException) { }

      return null;

    }

  }

  // SEARCH METHODS

  public partial class PopupWiktionaryProvider
  {

    /// <summary>
    /// Base URL for search. {0} is a placeholder for the language.
    /// </summary>
    public const string searchUrl = @"https://{0}.wiktionary.org/w/api.php?action=opensearch";

    [LogToErrorOnException]
    public RemoteTask<BrowserContent> Search(string searchTerm, string[] languages, int maxResults)
    {

      if (searchTerm.IsNullOrEmpty())
        return null;

      if (languages.IsNull() || languages.Length == 0)
        return null;

      if (maxResults < 1)
        return null;

      return GetSearchResultsAsync(searchTerm, languages, maxResults);

    }

    public async Task<BrowserContent> GetSearchResultsAsync(string searchTerm, string[] languages, int maxResults)
    {
      string searchResults = null;

      try
      {

        foreach (string language in languages)
        {

          string baseurl = string.Format(searchUrl, language);

          // TODO: Experiment with the different search types eg. fuzzy
          // possibly allow the user to change in the config.
          string options = $"&search={HttpUtility.UrlEncode(searchTerm)}" +
                           $"&limit={maxResults}" +
                           $"&namespace=0" +
                           $"&format=json";

          string fullurl = baseurl + options;

          try
          {
            string response = await GetAsync(fullurl);
            if (response.IsNullOrEmpty())
              continue;
            dynamic search = JsonConvert.DeserializeObject(response);
            JArray searchResultTitles = search[1];
            JArray searchResultUrls = search[3];

            // Returns a block of results for a language
            // TODO: Use a mustache template?
            if (searchResultTitles.Count > 0 && searchResultUrls.Count > 0)
            {
              searchResults += $"<h3>{language} search results</h3>";
              searchResults += "<ul>";

              List<string> htmlLinkNodes = new List<string>();

              // TODO: Can these be written more clearly?
              htmlLinkNodes = searchResultTitles
                                         .Zip(searchResultUrls,
                                              (title, link) =>
                                              $"<a href=\"{UrlUtils.ConvDesktopWiktionaryToMob(link.ToString())}\">{title}</a>")
                                         .ToList();

              // Zip and iterate over the searchTitles and searchUrls arrays
              // to pair them and add to the searchResults.
              if (htmlLinkNodes != null && htmlLinkNodes.Count > 0)
              {
                foreach (var linkNode in htmlLinkNodes)
                {
                  searchResults += $"<li>{linkNode}</li>";
                }
                searchResults += "</ul>";
              }
            }
          }
          catch (UriFormatException e) 
          {
            LogTo.Error($"UriFormatException on {fullurl}");
          }
        }

        return new BrowserContent(searchResults, null, false);

      }
      catch (RemotingException) { }

      return null;
    }

  }

  // HTTP REQUEST METHODS
  public partial class PopupWiktionaryProvider
  {

    private readonly HttpClient _httpClient;

    public PopupWiktionaryProvider()
    {
      _httpClient = new HttpClient();
      _httpClient.DefaultRequestHeaders.Accept.Clear();
      _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public void Dispose()
    {
      _httpClient?.Dispose();
    }


    /// <summary>
    /// Send an HTTP Get request to the URL.
    /// </summary>
    /// <param name="url">
    /// The URL to send the request to.
    /// </param>
    /// <returns>
    /// A string representing the content of the response.
    /// </returns>
    public async Task<string> GetAsync(string url)
    {
      HttpResponseMessage responseMsg = null;

      try
      {
        responseMsg = await _httpClient.GetAsync(url);

        if (responseMsg.IsSuccessStatusCode)
        {
          return await responseMsg.Content.ReadAsStringAsync();
        }
        else
        {
          return null;
        }
      }
      catch (HttpRequestException)
      {
        if (responseMsg != null && responseMsg.StatusCode == System.Net.HttpStatusCode.NotFound)
          return null;
        else
          throw;
      }
      catch (OperationCanceledException)
      {
        return null;
      }
      finally
      {
        responseMsg?.Dispose();
      }
    }
  }
}
