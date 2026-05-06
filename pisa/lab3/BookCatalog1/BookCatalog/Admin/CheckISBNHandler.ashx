<%@ WebHandler Language="C#" Class="CheckISBNHandler" %>

using System;
using System.Web;
using System.Collections.Generic;
using System.Web.Script.Serialization;

public class CheckISBNHandler : IHttpHandler
{
    public void ProcessRequest(HttpContext context)
    {
        context.Response.ContentType = "application/json";

        string isbn = context.Request.Form["isbn"] ?? context.Request.QueryString["isbn"] ?? "";

        if (string.IsNullOrWhiteSpace(isbn) || isbn.Trim().Length < 5)
        {
            context.Response.Write("{\"exists\":false,\"count\":0}");
            return;
        }

        isbn = isbn.Trim();
        List<Book> duplicates = DataStore.FindDuplicatesByISBN(isbn);

        var result = new { exists = duplicates.Count > 0, count = duplicates.Count };
        var json = new JavaScriptSerializer().Serialize(result);
        context.Response.Write(json);
    }

    public bool IsReusable { get { return false; } }
}
