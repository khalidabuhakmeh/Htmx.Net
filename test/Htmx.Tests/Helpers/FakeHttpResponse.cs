using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Htmx.Tests.Helpers;

#nullable disable

public class FakeHttpResponse : HttpResponse
{
    public FakeHttpResponse()
    {
        Headers = new FakeHeaderDictionary();
    }

    public override void OnStarting(Func<object, Task> callback, object state)
    {
    }

    public override void OnCompleted(Func<object, Task> callback, object state)
    {
    }

    public override void Redirect(string location, bool permanent)
    {
    }
    // ReSharper disable once UnassignedGetOnlyAutoProperty
    public override HttpContext HttpContext { get; }
    public override int StatusCode { get; set; }
    public override IHeaderDictionary Headers { get; }
    public override Stream Body { get; set; }
    public override long? ContentLength { get; set; }
    public override string ContentType { get; set; }
    // ReSharper disable once UnassignedGetOnlyAutoProperty
    public override IResponseCookies Cookies { get; }
    // ReSharper disable once UnassignedGetOnlyAutoProperty
    public override bool HasStarted { get; }
}