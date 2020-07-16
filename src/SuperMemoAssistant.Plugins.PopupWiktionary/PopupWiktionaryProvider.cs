using Anotar.Serilog;
using PluginManager.Interop.Sys;
using SuperMemoAssistant.Plugins.PopupWindow.Interop;
using SuperMemoAssistant.Sys.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;

namespace SuperMemoAssistant.Plugins.PopupWiktionary
{
  //
  // MAIN ARTICLE RETRIEVAL METHODS

  public partial class PopupWiktionaryProvider : PerpetualMarshalByRefObject, IContentProvider
  {

    [LogToErrorOnException]
    public RemoteTask<BrowserContent> FetchHtml(string url)
    {

      try
      {

      }
      catch (RemotingException) { }

    }

  }

  public partial class PopupWiktionaryProvidera
  {

    [LogToErrorOnException]
    public RemoteTask<BrowserContent> Search(string url)
    {

      try
      {

      }
      catch (RemotingException) { }

    }


  }
}
