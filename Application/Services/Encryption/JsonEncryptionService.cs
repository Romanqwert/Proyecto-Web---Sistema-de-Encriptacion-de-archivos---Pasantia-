using System.Security.Cryptography;
using System.Text.Json;
using System.Text;

namespace EncriptacionApi.Application.Services.Encryption
{
    public class JsonEncryptionService
    {
        private readonly EncryptHelper encryptHelper = new EncryptHelper();
        public string EncryptJson(string json, string key)
        {
            var data = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
            var encrypted = EncryptValuesRecursively(data, key);
            return JsonSerializer.Serialize(encrypted, new JsonSerializerOptions { WriteIndented = true });
        }
        public string DecryptJson(string json, string key)
        {
            var data = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
            var decrypted = DecryptValuesRecursively(data, key);
            return JsonSerializer.Serialize(decrypted, new JsonSerializerOptions { WriteIndented = true });
        }
        
        public string EncryptJsonWithTargets(string json, string key, List<string> encryptTargets)
        {
            var data = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
            var encrypted = EncryptJsonRecursivelyWithTargets(data, key, encryptTargets);
            return JsonSerializer.Serialize(encrypted, new JsonSerializerOptions { WriteIndented = true });
        }

        private object EncryptValuesRecursively(object value, string key)
        {
            if (value is JsonElement el)
            {
                // Console.WriteLine($"json element: {value.ToString()}");
                return el.ValueKind switch
                {
                    JsonValueKind.String => encryptHelper.EncryptString(el.GetString() ?? "", key),
                    JsonValueKind.Object => EncryptValuesRecursively(JsonToDictionary(el), key),
                    JsonValueKind.Array => el.EnumerateArray().Select(v => EncryptValuesRecursively(v, key)).ToList(),
                    _ => value
                };
            }
            if (value is Dictionary<string, object> dict)
            {
                // Console.WriteLine($"dictionary {value.ToString()}");
                return dict.ToDictionary(kvp => kvp.Key, kvp => EncryptValuesRecursively(kvp.Value, key));
            }
            if (value is string st)
            {
                return encryptHelper.EncryptString(st, key);
            }
            return value;
        }

        private Dictionary<string, object> JsonToDictionary(JsonElement element)
        {
            var dict = new Dictionary<string, object>();

            foreach (var property in element.EnumerateObject())
            {
                switch (property.Value.ValueKind)
                {
                    case JsonValueKind.Object:
                        dict[property.Name] = JsonToDictionary(property.Value);
                        break;

                    case JsonValueKind.Array:
                        dict[property.Name] = property.Value.EnumerateArray()
                            .Select(item => item.ValueKind == JsonValueKind.Object
                                ? JsonToDictionary(item)
                                : GetPrimitiveValue(item))
                            .ToList();
                        break;

                    default:
                        dict[property.Name] = GetPrimitiveValue(property.Value);
                        break;
                }
            }

            return dict;
        }

        private object GetPrimitiveValue(JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.String => element.GetString() ?? string.Empty,
                JsonValueKind.Number => element.TryGetInt64(out var l) ? l :
                                        element.TryGetDouble(out var d) ? d : element.GetRawText(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Null => null!,
                _ => element.GetRawText()
            };
        }

        private object DecryptValuesRecursively(object value, string key)
        {
            if (value is JsonElement el)
            {
                return el.ValueKind switch
                {
                    JsonValueKind.String => encryptHelper.TryDecryptString(el.GetString() ?? "", key),
                    JsonValueKind.Object => DecryptValuesRecursively(JsonToDictionary(el), key),
                    JsonValueKind.Array => el.EnumerateArray().Select(v => DecryptValuesRecursively(v, key)).ToList(),
                    _ => value
                };
            }
            if (value is Dictionary<string, object> dict)
            {
                return dict.ToDictionary(kvp => kvp.Key, kvp => DecryptValuesRecursively(kvp.Value, key));
            }
            if (value is string st)
            {
                return encryptHelper.TryDecryptString(st, key);
            }
            return value;
        }
        
        private object EncryptJsonRecursivelyWithTargets(object value, string key, List<string> encryptTargets, string currentPath = "")
        {
            if (value is JsonElement el)
            {
                return el.ValueKind switch
                {
                    JsonValueKind.String => encryptHelper.ShouldEncryptKey(currentPath, encryptTargets)
                                            ? encryptHelper.EncryptString(el.GetString() ?? "", key)
                                            : el.GetString() ?? "",
                    JsonValueKind.Object => EncryptJsonRecursivelyWithTargets(JsonToDictionary(el), key, encryptTargets, currentPath),
                    JsonValueKind.Array => el.EnumerateArray().Select(v => EncryptJsonRecursivelyWithTargets(v, key, encryptTargets, currentPath)).ToList(),
                    _ => value
                };
            }
            if (value is Dictionary<string, object> dict)
            {
                return dict.ToDictionary(
                    kvp => kvp.Key,
                    kvp => {
                        string newPath = string.IsNullOrEmpty(currentPath) ? kvp.Key : $"{currentPath}.{kvp.Key}";
                        return EncryptJsonRecursivelyWithTargets(kvp.Value, key, encryptTargets, newPath);
                    }
                );
            }
            if (value is string st)
            {
                return encryptHelper.ShouldEncryptKey(currentPath, encryptTargets) ? encryptHelper.EncryptString(st, key) : st;
            }
            return value;
        }
    }
}
