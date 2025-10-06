using System;
using System.Net.Sockets;
using System.Reflection;
using WatsonWebserver.Core;

namespace unofficial_pdrive_http_bridge;

internal static class Utils
{
    public static NetworkStream GetResponseStream(HttpResponseBase response)
    {
        var responseType = response.GetType();
        var fieldInfo = responseType.GetField("_Stream", BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("No _Stream field");
        var stream = (NetworkStream?)fieldInfo.GetValue(response)
            ?? throw new InvalidOperationException("_Stream field was null");
        return stream;
    }
}
