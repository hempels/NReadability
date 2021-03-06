﻿/*
 * NReadability
 * http://code.google.com/p/nreadability/
 * 
 * Copyright 2010 Marek Stój
 * http://immortal.pl/
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Sgml;
using System.IO;

namespace NReadability
{
  /// <summary>
  /// A class for constructing a DOM from HTML markup.
  /// </summary>
  public class SgmlDomBuilder
  {
    #region Public methods

    /// <summary>
    /// Constructs a DOM (System.Xml.Linq.XDocument) from HTML markup.
    /// </summary>
    /// <param name="htmlContent">HTML markup from which the DOM is to be constructed.</param>
    /// <returns>System.Linq.Xml.XDocument instance which is a DOM of the provided HTML markup.</returns>
    public XDocument BuildDocument(string htmlContent)
    {
      if (htmlContent == null)
      {
        throw new ArgumentNullException("htmlContent");
      }

      if (htmlContent.Trim().Length == 0)
      {
        return new XDocument();
      }

      // Remove all conditional comments (SgmlDomBuilder doesn't understand them correctly)
      htmlContent = Regex.Replace(htmlContent, @"<!--\s*\[if .*?\]\s*>.*?<!\s*\[endif\]\s*-->", string.Empty, RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Compiled);
      htmlContent = Regex.Replace(htmlContent, @"<!--\s*\[if .*?\]\s*>\s*(-->)?", string.Empty, RegexOptions.IgnoreCase | RegexOptions.Compiled);
      htmlContent = Regex.Replace(htmlContent, @"(<!--\s*)?<!\s*\[endif\]\s*-->", string.Empty, RegexOptions.IgnoreCase | RegexOptions.Compiled);
      // remove malformed conditionals
      htmlContent = Regex.Replace(htmlContent, @"<!--\s*\[if .*?>\s*-->", string.Empty, RegexOptions.IgnoreCase | RegexOptions.Compiled);

      // "trim end" htmlContent to ...</html>$ (codinghorror.com puts some scripts after the </html> - sic!)
      const string htmlEnd = "</html";
      int indexOfHtmlEnd = htmlContent.LastIndexOf(htmlEnd);
      if (indexOfHtmlEnd != -1)
      {
        int indexOfHtmlEndBracket = htmlContent.IndexOf('>', indexOfHtmlEnd);

        if (indexOfHtmlEndBracket != -1)
        {
          htmlContent = htmlContent.Substring(0, indexOfHtmlEndBracket + 1);
        }
      }

      // "trim start" htmlContent to ...^<html (some sites put scripts before the <html>..)
      const string htmlStart = "<html";
      int indexOfHtmlStart = htmlContent.IndexOf(htmlStart);
      if (indexOfHtmlStart != -1)
      {
        htmlContent = htmlContent.Substring(indexOfHtmlStart);
      }

      XDocument document;

      try
      {
        document = LoadDocument(htmlContent);
      }
      catch (InvalidOperationException exc)
      {
        // sometimes SgmlReader doesn't handle <script> tags well and XDocument.Load() throws,
        // so we can retry with the html content with <script> tags stripped off

        if (!exc.Message.Contains("EndOfFile"))
        {
          throw;
        }

        htmlContent = HtmlUtils.RemoveScriptTags(htmlContent);
        try
        {
          document = LoadDocument(htmlContent);
        }
        catch (InvalidOperationException exc2)
        {
          // if removing the script tags wasn't enough, we can retry with the
          // <style> tags also stripped off
          if (!exc2.Message.Contains("EndOfFile"))
          {
            throw;
          }

          htmlContent = HtmlUtils.RemoveStyleTags(htmlContent);
          document = LoadDocument(htmlContent);
        }
      }

      // remove any *extra* <body> or <html> tags
      Action<XDocument, string> fnRemoveExtra = (doc, tagName) =>
      {
        var bodyElements = doc.GetElementsByTagName(tagName);
        if (bodyElements != null && bodyElements.Count() > 1)
        {
          // Skip the first (top-most) match, then reverse the list so they're processed bottom-up
          foreach (var bodyElem in bodyElements.Skip(1).Reverse())
          {
            // Push any child nodes up a level, then delete the body element
            var children = bodyElem.Descendants();
            bodyElem.AddAfterSelf(children.ToArray());
            bodyElem.Remove();
          }
        }
      };
      fnRemoveExtra(document, "html");
      fnRemoveExtra(document, "body");

      return document;
    }

    private static XDocument LoadDocument(string htmlContent)
    {
      using (var sgmlReader = new SgmlReader())
      {
        sgmlReader.CaseFolding = CaseFolding.ToLower;
        sgmlReader.DocType = "HTML";
        sgmlReader.WhitespaceHandling = WhitespaceHandling.None;

        string xmlContent = StripInvalidXmlCharacters(htmlContent);
        using (var sr = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(xmlContent))))
        {
          sgmlReader.InputStream = sr;

          var document = XDocument.Load(sgmlReader);

          return document;
        }
      }
    }

    private static string StripInvalidXmlCharacters(string textIn)
    {
        var builder = new StringBuilder();
        if (string.IsNullOrEmpty(textIn))
        {
            return string.Empty;
        }
        for (int i = 0; i < textIn.Length; i++)
        {
            char ch = textIn[i];
            if (((((ch == '\t') || (ch == '\n')) || (ch == '\r')) || ((ch >= ' ') && (ch <= 0xd7ff))) || ((ch >= 0xe000) && (ch <= 0xfffd)))
            {
                builder.Append(ch);
            }
        }
        return builder.ToString();
    }

    #endregion
  }
}
