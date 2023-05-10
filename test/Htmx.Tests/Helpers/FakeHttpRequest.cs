using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Htmx.Tests.Helpers;

public class FakeHttpRequest : HttpRequest
{
    public FakeHttpRequest()
    {
        Headers = new FakeHeaderDictionary();
    }

    public override Task<IFormCollection> ReadFormAsync(CancellationToken cancellationToken = new())
    {
        throw new NotImplementedException();
    }

    // ReSharper disable once UnassignedGetOnlyAutoProperty
    public override HttpContext HttpContext { get; }
    public override string Method { get; set; }
    public override string Scheme { get; set; }
    public override bool IsHttps { get; set; }
    public override HostString Host { get; set; }
    public override PathString PathBase { get; set; }
    public override PathString Path { get; set; }
    public override QueryString QueryString { get; set; }
    public override IQueryCollection Query { get; set; }
    public override string Protocol { get; set; }
    public override IHeaderDictionary Headers { get; }
    public override IRequestCookieCollection Cookies { get; set; }
    public override long? ContentLength { get; set; }
    public override string ContentType { get; set; }
    public override Stream Body { get; set; }
    // ReSharper disable once UnassignedGetOnlyAutoProperty
    public override bool HasFormContentType { get; }
    public override IFormCollection Form { get; set; }
}