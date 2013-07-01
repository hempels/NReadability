using System;

namespace NReadability
{
  public static class HtmlUtils
  {
    public static string RemoveScriptTags(string htmlContent)
    {
      if (htmlContent == null)
      {
        throw new ArgumentNullException("htmlContent");
      }

      if (htmlContent.Length == 0)
      {
        return "";
      }

      int indexOfScriptTagStart = htmlContent.IndexOf("<script", StringComparison.OrdinalIgnoreCase);

      if (indexOfScriptTagStart == -1)
      {
        return htmlContent;
      }

      int indexOfScriptTagEnd = htmlContent.IndexOf("</script>", indexOfScriptTagStart, StringComparison.OrdinalIgnoreCase);

      if (indexOfScriptTagEnd == -1)
      {
        return htmlContent.Substring(0, indexOfScriptTagStart);
      }

      string strippedHtmlContent =
        htmlContent.Substring(0, indexOfScriptTagStart) +
        htmlContent.Substring(indexOfScriptTagEnd + "</script>".Length);

      return RemoveScriptTags(strippedHtmlContent);
    }

    public static string RemoveStyleTags(string htmlContent)
    {
      if (htmlContent == null)
      {
        throw new ArgumentNullException("htmlContent");
      }

      if (htmlContent.Length == 0)
      {
        return "";
      }

      int indexOfStyleTagStart = htmlContent.IndexOf("<style", StringComparison.OrdinalIgnoreCase);

      if (indexOfStyleTagStart == -1)
      {
        return htmlContent;
      }

      int indexOfStyleTagEnd = htmlContent.IndexOf("</style>", indexOfStyleTagStart, StringComparison.OrdinalIgnoreCase);

      if (indexOfStyleTagEnd == -1)
      {
        return htmlContent.Substring(0, indexOfStyleTagStart);
      }

      string strippedHtmlContent =
        htmlContent.Substring(0, indexOfStyleTagStart) +
        htmlContent.Substring(indexOfStyleTagEnd + "</style>".Length);

      return RemoveStyleTags(strippedHtmlContent);
    }

    internal static string RemoveExcessTags(string htmlContent, string tagName)
    {
      int firstIndexOfStartTag = htmlContent.IndexOf("<" + tagName);
      int lastIndexOfStartTag = htmlContent.LastIndexOf("<" + tagName);
      while (lastIndexOfStartTag > firstIndexOfStartTag)
      {
        int tagEnd = htmlContent.IndexOf(">", lastIndexOfStartTag);
        htmlContent = htmlContent.Remove(lastIndexOfStartTag, (tagEnd - lastIndexOfStartTag));

        firstIndexOfStartTag = htmlContent.IndexOf("<" + tagName);
        lastIndexOfStartTag = htmlContent.LastIndexOf("<" + tagName);
      }

      int lastIndexOfEndTag = htmlContent.LastIndexOf("</" + tagName + ">");
      int firstIndexOfEndTag = htmlContent.IndexOf("</" + tagName + ">");
      while (firstIndexOfEndTag < lastIndexOfEndTag)
      {
        htmlContent = htmlContent.Remove(firstIndexOfEndTag, ("</" + tagName + ">").Length);

        lastIndexOfEndTag = htmlContent.LastIndexOf("</" + tagName + ">");
        firstIndexOfEndTag = htmlContent.IndexOf("</" + tagName + ">");
      }

      return htmlContent;
    }

  }
}
