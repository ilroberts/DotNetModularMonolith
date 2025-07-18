using System.Text.Json;
using Microsoft.AspNetCore.JsonPatch;

namespace ECommerce.BusinessEvents.Services;

public static class JsonPatchUtility
{
    public static JsonPatchDocument GeneratePatch(string oldJson, string newJson)
    {
        var oldDoc = JsonSerializer.Deserialize<JsonElement>(oldJson);
        var newDoc = JsonSerializer.Deserialize<JsonElement>(newJson);
        var patch = new JsonPatchDocument();
        GeneratePatchRecursive(string.Empty, oldDoc, newDoc, patch);
        return patch;
    }

    private static void GeneratePatchRecursive(string path, JsonElement oldElem, JsonElement newElem, JsonPatchDocument patch)
    {
        if (oldElem.ValueKind != newElem.ValueKind)
        {
            patch.Replace(path, newElem);
            return;
        }
        switch (oldElem.ValueKind)
        {
            case JsonValueKind.Object:
                var oldProps = oldElem.EnumerateObject().ToDictionary(p => p.Name, p => p);
                var newProps = newElem.EnumerateObject().ToDictionary(p => p.Name, p => p);
                foreach (var prop in oldProps.Keys)
                {
                    if (!newProps.ContainsKey(prop))
                        patch.Remove(JoinPath(path, prop));
                    else
                        GeneratePatchRecursive(JoinPath(path, prop), oldProps[prop].Value, newProps[prop].Value, patch);
                }
                foreach (var prop in newProps.Keys)
                {
                    if (!oldProps.ContainsKey(prop))
                        patch.Add(JoinPath(path, prop), newProps[prop].Value);
                }
                break;
            case JsonValueKind.Array:
                // For simplicity, replace the array if changed
                if (!oldElem.ToString().Equals(newElem.ToString()))
                    patch.Replace(path, newElem);
                break;
            default:
                if (!oldElem.ToString().Equals(newElem.ToString()))
                    patch.Replace(path, newElem);
                break;
        }
    }

    private static string JoinPath(string basePath, string prop)
    {
        return string.IsNullOrEmpty(basePath) ? $"/{prop}" : $"{basePath}/{prop}";
    }
}

