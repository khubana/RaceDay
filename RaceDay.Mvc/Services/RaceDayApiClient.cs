using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace RaceDay.Mvc.Services;

public class RaceDayApiClient(HttpClient http, IHttpContextAccessor accessor)
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private HttpRequestMessage CreateRequest(HttpMethod method, string path)
    {
        var request = new HttpRequestMessage(method, path);
        var apiCookie = accessor.HttpContext?.Session.GetString("ApiCookie");
        if (!string.IsNullOrWhiteSpace(apiCookie))
            request.Headers.TryAddWithoutValidation("Cookie", apiCookie);
        return request;
    }

    private async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken = default)
    {
        var response = await http.SendAsync(request, cancellationToken);

        if (response.Headers.TryGetValues("Set-Cookie", out var values))
        {
            var sessionCookie = values.FirstOrDefault(value =>
                value.StartsWith(".RaceDay.Session=", StringComparison.OrdinalIgnoreCase));

            if (sessionCookie is not null)
                accessor.HttpContext?.Session.SetString("ApiCookie", sessionCookie.Split(';')[0]);
        }

        return response;
    }

    public async Task<JsonDocument?> Get(string path)
    {
        using var request = CreateRequest(HttpMethod.Get, path);
        using var response = await SendAsync(request);
        var raw = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode || string.IsNullOrWhiteSpace(raw))
            return null;

        try { return JsonDocument.Parse(raw); }
        catch (JsonException) { return null; }
    }

    public async Task<(bool Ok, string Message, JsonDocument? Data)> SendJson(
        HttpMethod method, string path, object? body = null)
    {
        using var request = CreateRequest(method, path);
        if (body is not null)
        {
            request.Content = new StringContent(
                JsonSerializer.Serialize(body, Json), Encoding.UTF8, "application/json");
        }

        using var response = await SendAsync(request);
        var raw = await response.Content.ReadAsStringAsync();
        JsonDocument? data = null;
        try
        {
            if (!string.IsNullOrWhiteSpace(raw))
                data = JsonDocument.Parse(raw);
        }
        catch (JsonException) { }

        var message = response.IsSuccessStatusCode
            ? "Request completed successfully."
            : ExtractMessage(data, response.StatusCode);

        return (response.IsSuccessStatusCode, message, data);
    }

    public async Task<(bool Ok, string Message)> Upload(
        string path, Dictionary<string, string> fields, IFormFile file)
    {
        using var form = new MultipartFormDataContent();
        foreach (var field in fields)
            form.Add(new StringContent(field.Value), field.Key);

        await using var stream = file.OpenReadStream();
        using var part = new StreamContent(stream);
        part.Headers.ContentType = new MediaTypeHeaderValue(
            string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType);
        form.Add(part, "file", Path.GetFileName(file.FileName));

        using var request = CreateRequest(HttpMethod.Post, path);
        request.Content = form;
        using var response = await SendAsync(request);
        var raw = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
            return (true, "Image uploaded successfully.");

        try
        {
            using var error = JsonDocument.Parse(raw);
            return (false, ExtractMessage(error, response.StatusCode));
        }
        catch (JsonException)
        {
            return (false, $"Image upload failed (HTTP {(int)response.StatusCode}).");
        }
    }

    private static string ExtractMessage(JsonDocument? data, HttpStatusCode statusCode)
    {
        if (data is not null)
        {
            var root = data.RootElement;
            foreach (var key in new[] { "message", "detail", "title" })
            {
                if (root.TryGetProperty(key, out var value) &&
                    value.ValueKind == JsonValueKind.String &&
                    !string.IsNullOrWhiteSpace(value.GetString()))
                    return value.GetString()!;
            }
        }

        return $"The API returned HTTP {(int)statusCode} ({statusCode}).";
    }
}
