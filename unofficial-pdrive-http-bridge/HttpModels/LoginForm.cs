using HandlebarsDotNet;

namespace unofficial_pdrive_http_bridge.HttpModels;

public sealed class LoginForm : IHttpModel
{
    private static readonly HandlebarsTemplate<object, object> _htmlTemplate = HB.Compile("""
    <!DOCTYPE html>
    <html>
        <head>
            <title>Login</title>
        </head>
        <body>
            <h1>Login</h1>
            <form action="/login" method="POST">
                <label for="username">Username:</label>
                <input type="text" id="username" name="username" placeholder="Username" required>
                <br>

                <label for="password">Password:</label>
                <input type="password" id="password" name="password" placeholder="Password" required>
                <br>

                <label for="otp">OTP:</label>
                <input type="text" id="otp" name="otp" placeholder="OTP">
                <br>

                <input type="submit" value="Login">
            </form>
        </body>
    </html>
    """);

    public string ToHtml() => _htmlTemplate(this);
}
