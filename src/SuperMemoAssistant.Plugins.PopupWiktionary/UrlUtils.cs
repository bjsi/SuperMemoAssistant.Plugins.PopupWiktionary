using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperMemoAssistant.Plugins.PopupWiktionary
{
  public static class UrlUtils
  {
    public const string MobileWiktionaryRegex = @"";
    public const string DesktopWiktionaryRegex = @"";

    // Example url: https://en.m.wiktionary.org/wiki/Hello_World
    public static bool IsMobileWiktionaryUrl(string url)
    {

      if (string.IsNullOrEmpty(url))
        return false;

      if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
        return false;

      Uri uri = new Uri(url);
      string[] split = uri.Host.Split('.');
      if (split.Length != 4)
        return false;

      if (split[1] == "m" && split[2] == "wiktionary")
        return true;

      return false;
    }

    // Example url: https://en.m.wikipedia.org/wiki/Hello_World
    public static bool IsMobileWikipediaUrl(string url)
    {

      if (url.IsNullOrEmpty())
        return false;

      // Should fail for relative links like /wiki/Hello_World
      if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
        return false;

      Uri uri = new Uri(url);
      // Host == en.m.wikipedia.org
      string[] splitUri = uri.Host.Split('.');

      // Should fail for destop links (length 3)
      if (splitUri.Length != 4)
        return false;

      // Should fail for desktop links
      if (splitUri[1] != "m")
        return false;

      // Should fail for wiktionary links
      if (splitUri[2] != "wikipedia")
        return false;

      return true;

    }

    public static string ConvMobWiktionaryToDesktop(string url)
    {
      if (!IsMobileWiktionaryUrl(url))
      {
        var desktop = url.Split('.').ToList();
        desktop.RemoveAt(1);
        url = string.Join(".", desktop);
      }
      return url;
    }
    public static string ConvDesktopWiktionaryToMob(string url)
    {
      if (IsDesktopWiktionaryUrl(url))
      {
        var mobile = url.Split('.').ToList();
        mobile.Insert(1, "m");
        url = string.Join(".", mobile);
      }
      return url;
    }

    // Example url: https://en.wiktionary.org/wiki/Hello_World
    public static bool IsDesktopWiktionaryUrl(string url)
    {
      if (url.IsNullOrEmpty())
        return false;

      // Should fail for relative urls like /wiki/Hello_World
      if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
        return false;

      Uri uri = new Uri(url);
      // Host == en.wiktionary.org
      string[] splitUri = uri.Host.Split('.');

      // Should fail for mobile wiktionary (length 4)
      if (splitUri.Length != 3)
        return false;

      // Should fail for wikipedia links
      if (splitUri[1] != "wiktionary")
        return false;

      return true;

    }

    public static string ConvRelToAbsLink(string baseUrl, string relUrl)
    {
      if (!string.IsNullOrEmpty(baseUrl) && !string.IsNullOrEmpty(relUrl))
      {
        // UriKind.Relative will be false for rel urls containing #
        if (Uri.IsWellFormedUriString(baseUrl, UriKind.Absolute))
        {
          if (baseUrl.EndsWith("/"))
          {
            baseUrl = baseUrl.TrimEnd('/');
          }

          if (relUrl.StartsWith("/") && !relUrl.StartsWith("//"))
          {
            if (relUrl.StartsWith("/wiki") || relUrl.StartsWith("/w/"))
            {
              return $"{baseUrl}{relUrl}";
            }
            return $"{baseUrl}/wiki{relUrl}";
          }
          else if (relUrl.StartsWith("./"))
          {
            if (relUrl.StartsWith("./wiki") || relUrl.StartsWith("./w/"))
            {
              return $"{baseUrl}{relUrl.Substring(1)}";
            }
            return $"{baseUrl}/wiki{relUrl.Substring(1)}";
          }
          else if (relUrl.StartsWith("#"))
          {
            return $"{baseUrl}/wiki/{relUrl}";
          }
          else if (relUrl.StartsWith("//"))
          {
            return $"https:{relUrl}";
          }
        }
      }
      return relUrl;
    }
  }
}
