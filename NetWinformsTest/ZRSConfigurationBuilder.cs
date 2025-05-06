using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Collections;
using static System.Windows.Forms.Design.AxImporter;
using System.Security.Policy;

namespace NetWinformsTest
{
    public class ZRSConfigurationBuilder
    {

        private string _basePath = string.Empty;
        private string _jsonFile = string.Empty;
        private string _userSecretsFolder = string.Empty;
        private bool _addEnvironmentVariables = false;
        private string _environmentVariablePrefix = string.Empty;

        struct JsonFileInfo
        {
            public string fileName;
            public bool optional;
        }

        private List<JsonFileInfo> _jsonFiles = new List<JsonFileInfo>();

        public ZRSConfigurationBuilder SetBasePath(string basePath)
        {
            _basePath = basePath;
            return this;
        }
        public ZRSConfigurationBuilder AddJsonFile(string jsonFile)
        {
            var jsonFileInfo = new JsonFileInfo()
            {
                fileName = jsonFile,
                optional = false,
            };

            _jsonFiles.Add(jsonFileInfo);
            return this;
        }
        public ZRSConfigurationBuilder AddJsonFile(string jsonFile, bool optional)
        {
            var jsonFileInfo = new JsonFileInfo()
            {
                fileName = jsonFile,
                optional = optional
            };

            _jsonFiles.Add(jsonFileInfo);

            return this;
        }
        public ZRSConfigurationBuilder AddUserSecrets(string userSecretsFolder) 
        {
            _userSecretsFolder = userSecretsFolder;
            return this;
        }

        public ZRSConfigurationBuilder AddEnvironmentVariables()
        {
            _addEnvironmentVariables = true;
            return this;
        }
        public ZRSConfigurationBuilder AddEnvironmentVariables(string prefix)
        {
            _environmentVariablePrefix = prefix;
            _addEnvironmentVariables = true;
            return this;
        }

        public ConfigurationRoot<string,string> Build()
        {
            ConfigurationRoot<string, string> tmp = new ConfigurationRoot<string, string>();

            foreach (var item in _jsonFiles)
            {
                if (File.Exists(_basePath + item.fileName))
                {
                    AddJsonFileToDictionary
                    (
                        _basePath + item.fileName, 
                        tmp,
                        item.optional
                    );
                }

            }
            

            if(_addEnvironmentVariables) {AddEnvironmentVariablesToDictionary(tmp); }
            if(_userSecretsFolder != string.Empty) {AddUserSecretsToDictionary(tmp); }

            return tmp;
        }

        public void BuildDictionaryFromJson(string jsonFile, ConfigurationRoot<string, string> dict, bool optional)
        {
            if (optional && !File.Exists(jsonFile)) return;

            var jsonString = File.ReadAllText(jsonFile);
         
            using (JsonDocument document = JsonDocument.Parse(jsonString))
            {
                List<string> prefix = new List<string>();
                JsonElement root = document.RootElement;
               
                FlattenJsonToDictionary(root, prefix, dict , 0);

            };

        }
        public void AddJsonFileToDictionary(string jsonFile, ConfigurationRoot<string, string> dict,bool optional)
        {
            BuildDictionaryFromJson(jsonFile, dict,optional);
        }

        public void AddEnvironmentVariablesToDictionary(ConfigurationRoot<string, string> dict)
        {

            foreach (DictionaryEntry e in System.Environment.GetEnvironmentVariables())
            {
                Console.WriteLine(e.Key + ":" + e.Value);
                if (e.Key.ToString().ToLower().StartsWith(_environmentVariablePrefix.ToLower()))
                {
                    dict[(string)e.Key] = e.Value as string;
                }
            }
        }


        public void AddUserSecretsToDictionary(ConfigurationRoot<string, string> dict)
        {
            var appFolder = 
                System.Environment.GetEnvironmentVariable("APPDATA") + $@"\Microsoft\UserSecrets\{_userSecretsFolder}\secrets.json";
                
            AddJsonFileToDictionary(appFolder,dict,true);

        }


        private void FlattenJsonToDictionary(JsonElement element, List<string> key, Dictionary<string, string> keyValuePairs, int arrIndex)
        {
            var localkey = string.Empty;
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    foreach (JsonProperty property in element.EnumerateObject())
                    {
                        key.Add(property.Name);

                        FlattenJsonToDictionary(property.Value, key, keyValuePairs, arrIndex);

                        key.RemoveAt(key.Count - 1);

                    }
                    break;

                case JsonValueKind.Array:
                    arrIndex = 0;
                    foreach (JsonElement item in element.EnumerateArray())
                    {
                        key.Add(arrIndex.ToString());
                        arrIndex++;
                        FlattenJsonToDictionary(item, key, keyValuePairs, arrIndex);
                        key.RemoveAt(key.Count - 1);
                    }
                    break;

                case JsonValueKind.String:
                    localkey = String.Join(":", key);
                    keyValuePairs[localkey] = element.GetString();

                    break;

                case JsonValueKind.Number:

                    localkey = String.Join(":", key);
                    keyValuePairs[localkey] = element.GetInt32().ToString();

                    break;

                case JsonValueKind.True:
                case JsonValueKind.False:
                    localkey = String.Join(":", key);
                    keyValuePairs[localkey] = element.GetBoolean().ToString();

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
