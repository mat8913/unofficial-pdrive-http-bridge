using HandlebarsDotNet;

namespace unofficial_pdrive_http_bridge.HttpModels;

public sealed class LoginResult : IHttpModel
{
    private static readonly HandlebarsTemplate<object, object> _htmlTemplate = HB.Compile("""
    <!DOCTYPE html>
    <html>
        <head>
            <title>Login Result</title>
        </head>
        <body>
            <h1>Login Result</h1>
            <p>{{Message}}</p>
            <pre>{{Error}}</pre>
            <p><a href="/">Click here to continue</a></p>
        </body>
    </html>
    """);

    public string? Message { get; set; }
    public string? Error { get; set; }

    public string ToHtml() => _htmlTemplate(this);
}
