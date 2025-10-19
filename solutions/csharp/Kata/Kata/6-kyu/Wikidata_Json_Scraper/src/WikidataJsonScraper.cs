using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

#nullable enable

namespace Kata._6_kyu.Wikidata_Json_Scraper.src;

public sealed class WikidataJsonScraper
{
    private static readonly HttpClientHandler _handler = CreateHandler();
    private static readonly HttpClient _client = CreateClient(_handler);
    
    private static readonly SemaphoreSlim _gate = new(1, 1);
    
    private static readonly Dictionary<string, Dictionary<string, string>> _cache = new();
    
    public static async Task<Dictionary<string, string>> WikidataScraper(string url)
    {
        if (_cache.TryGetValue(url, out var cached))
            return cached;
        
        var attempts = 5;
        Exception? last = null;
        var delay = TimeSpan.FromMilliseconds(250);
        for (var i = 0; i < attempts; i++)
        {
            await _gate.WaitAsync();
            try
            {
                var dict = await TryScrap(url);
                _cache[url] = dict;
                return dict;
            }
            catch (HttpRequestException e) when (e.StatusCode is HttpStatusCode.Forbidden or (HttpStatusCode)429)
            {
                last = e;
                var backoff = delay;
                if (e.Data["RetryAfter"] is TimeSpan ra && ra > TimeSpan.Zero)
                    backoff = ra;
                await Task.Delay(backoff);
                delay = TimeSpan.FromMilliseconds(Math.Min(delay.TotalMilliseconds * 2, 4000));
            }
            finally
            {
                _gate.Release();
            }
        }
        throw last ?? new HttpRequestException("Request failed after multiple attempts.");
    }

    private static async Task<Dictionary<string, string>> TryScrap(string url)
    {
        using var req = new HttpRequestMessage(HttpMethod.Get, url);
        req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        var resp = await _client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead);
        if (!resp.IsSuccessStatusCode)
        {
            var ex = new HttpRequestException(
                $"Response status code does not indicate success: {(int)resp.StatusCode} ({resp.StatusCode}) : ({resp.ReasonPhrase}).",
                null,
                resp.StatusCode);
            if (resp.Headers.RetryAfter?.Delta is not null)
                ex.Data["RetryAfter"] = resp.Headers.RetryAfter!.Delta!.Value;
            throw ex;
        }

        var json = await resp.Content.ReadAsStringAsync();
        return GetDictionary(json);
    }

    private static Dictionary<string, string> GetDictionary(string jsonContent)
    {
        var result = new Dictionary<string, string>();
        
        var doc = JObject.Parse(jsonContent);
        var entities = (JObject)doc["entities"]!;
        foreach (var keyValuePair in entities)
        {
            var id = keyValuePair.Key;
            var e = (JObject)keyValuePair.Value!;

            var label = e["labels"]?["en"]?["value"]?.ToString();
            var description = e["descriptions"]?["en"]?["value"]?.ToString();
            
            result.TryAdd("ID", id);
            result.TryAdd("LABEL", label ?? "No Label");
            result.TryAdd("DESCRIPTION", description ?? "No Description");
        }
        return result;
    }

    private static HttpClientHandler CreateHandler()
    {
        var h = new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        };
        return h;
    }

    private static HttpClient CreateClient(HttpMessageHandler handler)
    {
        var client = new HttpClient(handler, disposeHandler: false);
        client.DefaultRequestHeaders.UserAgent.ParseAdd(
            "CodewarsWikidataJsonScraper/1.0 (+https://github.com/proninp/codewars-keys; velvel@gmail.com)");
        client.DefaultRequestHeaders.Add("Api-User-Agent",
            "CodewarsWikidataJsonScraper/1.0 (velvel@gmail.com)");

        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return client;
    }
}