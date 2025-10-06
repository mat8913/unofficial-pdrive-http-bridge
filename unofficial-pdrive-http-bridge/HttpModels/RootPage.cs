using HandlebarsDotNet;

namespace unofficial_pdrive_http_bridge.HttpModels;

public sealed class RootPage : IHttpModel
{
    private static readonly HandlebarsTemplate<object, object> _htmlTemplate = HB.Compile("""
    <!DOCTYPE html>
    <html>
        <head>
            <title>unofficial-pdrive-http-bridge</title>
        </head>
        <body>
            <h1>unofficial-pdrive-http-bridge</h1>
            <ul>
                <li><a href="/login">Login</a></li>
                <li><a href="/files">Files</a></li>
            </ul>
        </body>
    </html>
    """);

    public string ToHtml() => _htmlTemplate(this);
}
