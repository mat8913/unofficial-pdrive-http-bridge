using HandlebarsDotNet;

namespace unofficial_pdrive_http_bridge.HttpModels;

public sealed class NodeChildren : IHttpModel
{
    private static readonly HandlebarsTemplate<object, object> _htmlTemplate = HB.Compile("""
        <!DOCTYPE html>
        <html>
            <head>
                <title>Index of {{Path}}</title>
                <style>
                    td, th {
                        border: 1px solid;
                    }
                </style>
            </head>
            <h1>Index of {{Path}}</h1>
            <table>
                <tr>
                    <th>Type</th>
                    <th>Name</th>
                    <th>Last Modified</th>
                    <th>Size</th>
                </tr>
            {{#each Children}}
                <tr>
                    <td>{{this.Type}}</td>
                    <td><a href="{{this.Url}}">{{this.Name}}</a></td>
                    <td>{{this.LastModified}}</td>
                    <td>{{this.Size}}</td>
                </tr>
            {{/each}}
            </table>
        </html>
        """);

    public string? Path { get; set; }
    public NodeMetadata[]? Children { get; set; }

    public string ToHtml() => _htmlTemplate(this);
}
