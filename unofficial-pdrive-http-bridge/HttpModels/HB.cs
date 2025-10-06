using HandlebarsDotNet;

namespace unofficial_pdrive_http_bridge.HttpModels;

internal static class HB
{
    private static readonly IHandlebars _handlebars;

    static HB()
    {
        var config = new HandlebarsConfiguration
        {
            TextEncoder = new HtmlEncoder()
        };
        _handlebars = Handlebars.CreateSharedEnvironment(config);
    }

    public static HandlebarsTemplate<object, object> Compile(string template) => _handlebars.Compile(template);
}
