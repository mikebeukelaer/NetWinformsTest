using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NetWinformsTest
{
    public class ZRSConfigurationBuilder_List
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string _basePath = string.Empty;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string _jsonFile = string.Empty;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string _userSecretsFolder = string.Empty;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool _addEnvironmentVariables = false;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string _environmentVariablePrefix = string.Empty;

        struct JsonFileInfo
        {
            public string fileName;
            public bool optional;
        }
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private List<JsonFileInfo> _jsonFiles = new List<JsonFileInfo>();

        public ZRSConfigurationBuilder_List SetBasePath(string basePath)
        {
            _basePath = basePath;
            return this;
        }
        public ZRSConfigurationBuilder_List AddJsonFile(string jsonFile)
        {
            var jsonFileInfo = new JsonFileInfo()
            {
                fileName = jsonFile,
                optional = false,
            };

            _jsonFiles.Add(jsonFileInfo);
            return this;
        }
        public ZRSConfigurationBuilder_List AddJsonFile(string jsonFile, bool optional)
        {
            var jsonFileInfo = new JsonFileInfo()
            {
                fileName = jsonFile,
                optional = optional
            };

            _jsonFiles.Add(jsonFileInfo);

            return this;
        }
        public ZRSConfigurationBuilder_List AddUserSecrets(string userSecretsFolder)
        {
            _userSecretsFolder = userSecretsFolder;
            return this;
        }

        public ZRSConfigurationBuilder_List AddEnvironmentVariables()
        {
            _addEnvironmentVariables = true;
            return this;
        }
        public ZRSConfigurationBuilder_List AddEnvironmentVariables(string prefix)
        {
            _environmentVariablePrefix = prefix;
            _addEnvironmentVariables = true;
            return this;
        }


        public Keystore Build()
        {
            var tmp = new Keystore();

            foreach (var item in _jsonFiles)
            {
                if (File.Exists(_basePath + item.fileName))
                {
                    AddJsonFileToDictionary
                    (
                        _basePath + item.fileName,
                        tmp,
                        item.optional,
                        $"JsonConfigurationProvider for '{item.fileName}'"
                    );
                }

            }


            if (_addEnvironmentVariables)
            { AddEnvironmentVariablesToDictionary(tmp); }
            if (_userSecretsFolder != string.Empty)
            { AddUserSecretsToDictionary(tmp); }

            return tmp;
        }

        public void BuildDictionaryFromJson(string jsonFile, Keystore dict, bool optional, string provider)
        {
            if (optional && !File.Exists(jsonFile))
                return;

            var jsonString = File.ReadAllText(jsonFile);

            using (JsonDocument document = JsonDocument.Parse(jsonString))
            {
                List<string> prefix = new List<string>();
                JsonElement root = document.RootElement;

                FlattenJsonToDictionary(root, prefix, dict, 0, provider);

            };

        }

        private void AddJsonFileToDictionary(string jsonFile, Keystore dict, bool optional, string provider)
        {
            BuildDictionaryFromJson(jsonFile, dict, optional, provider);
        }

        private void AddEnvironmentVariablesToDictionary(IKeyStoreManager dict)
        {
            var provider = "JsonConfigurationProvider for environment variables";
            foreach (DictionaryEntry e in System.Environment.GetEnvironmentVariables())
            {
                Console.WriteLine(e.Key + ":" + e.Value);
                if (e.Key.ToString().ToLower().StartsWith(_environmentVariablePrefix.ToLower()))
                {
                  //  dict[(string)e.Key] = e.Value as string;
                    dict.Add
                    (
                        new ConfigurationItem { Key = e.Key as string,  Value = e.Value as string, Provider = provider}
                    );
                }
            }
        }


        public void AddUserSecretsToDictionary(Keystore dict)
        {
            var appFolder =
                System.Environment.GetEnvironmentVariable("APPDATA") + $@"\Microsoft\UserSecrets\{_userSecretsFolder}\secrets.json";
            var provider = "JsonConfigurationProvider for 'secrets.json'";
            AddJsonFileToDictionary(appFolder, dict, true,  provider);

        }


        private void FlattenJsonToDictionary(JsonElement element, List<string> key, IKeyStoreManager keyValuePairs, int arrIndex, string provider)
        {
            var localkey = string.Empty;
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    foreach (JsonProperty property in element.EnumerateObject())
                    {
                        key.Add(property.Name);

                        FlattenJsonToDictionary(property.Value, key, keyValuePairs, arrIndex,provider);

                        key.RemoveAt(key.Count - 1);

                    }
                    break;

                case JsonValueKind.Array:
                    arrIndex = 0;
                    foreach (JsonElement item in element.EnumerateArray())
                    {
                        key.Add(arrIndex.ToString());
                        arrIndex++;
                        FlattenJsonToDictionary(item, key, keyValuePairs, arrIndex,provider);
                        key.RemoveAt(key.Count - 1);
                    }
                    break;

                case JsonValueKind.String:
                    localkey = String.Join(":", key);
                    keyValuePairs.Add
                    (
                        new ConfigurationItem { Key = localkey, Value = element.GetString(), Provider = provider }
                    );

                    break;

                case JsonValueKind.Number:

                    localkey = String.Join(":", key);
                   // keyValuePairs[localkey] = element.GetInt32().ToString();
                    keyValuePairs.Add
                  (
                      new ConfigurationItem { Key = localkey, Value = element.GetInt32().ToString(), Provider = provider }
                  );
                    break;

                case JsonValueKind.True:
                case JsonValueKind.False:
                    localkey = String.Join(":", key);
                    //keyValuePairs[localkey] = element.GetBoolean().ToString();
                    keyValuePairs.Add
                  (
                      new ConfigurationItem { Key = localkey, Value = element.GetBoolean().ToString(), Provider = provider }
                  );
                    break;

                case JsonValueKind.Null:
                    //log("Null");
                    break;

                default:
                    //  log("Unknown type");
                    break;
            }

        }




    }
}
