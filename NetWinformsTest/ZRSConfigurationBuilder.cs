using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Collections;
using static System.Windows.Forms.Design.AxImporter;

namespace NetWinformsTest
{
    public class ZRSConfigurationBuilder
    {

        private string _basePath = string.Empty;
        private string _jsonFile = string.Empty;
        private string _userSecretsFolder = string.Empty;
        private bool _addEnvironmentVariables = false;

        public ZRSConfigurationBuilder SetBasePath(string basePath)
        {
            _basePath = basePath;
            return this;
        }
        public ZRSConfigurationBuilder AddJsonFile(string jsonFile)
        {
            _jsonFile = jsonFile;
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

        public ConfigurationRoot<string,string> Build()
        {
            ConfigurationRoot<string, string> tmp = new ConfigurationRoot<string, string>();

            if (File.Exists(_basePath + _jsonFile))
            {
                tmp = BuildDictionaryFromJson(_basePath + _jsonFile);
            }

            if(_addEnvironmentVariables) {AddEnvironmentVariablesToDictionary(tmp); }
            if(_userSecretsFolder != string.Empty) {AddUserSecretsToDictionary(tmp); }

            return tmp;
        }

        public ConfigurationRoot<string, string> BuildDictionaryFromJson(string jsonFile)
        {
           
            var json = File.ReadAllText(jsonFile);

            var result = JsonSerializer.Deserialize<ConfigurationRoot<string, string>>(json);
            
            return result;

        }

        public void AddEnvironmentVariablesToDictionary(ConfigurationRoot<string, string> dict)
        {

            foreach (DictionaryEntry e in System.Environment.GetEnvironmentVariables())
            {
                Console.WriteLine(e.Key + ":" + e.Value);
                dict[(string)e.Key] = e.Value as string;
            }
        }

        public void AddUserSecretsToDictionary(ConfigurationRoot<string, string> dict)
        {
            var appFolder = System.Environment.GetEnvironmentVariable("APPDATA") + $@"\Microsoft\UserSecrets\{_userSecretsFolder}\secrets.json";
            if (File.Exists(appFolder))
            {
                var jsonFile = appFolder;


                var json = File.ReadAllText(jsonFile);
                var options = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
                Dictionary<string, string> tmp =
                    JsonSerializer.Deserialize<Dictionary<string, string>>(json, options);

                foreach(var kvp in tmp)
                {
                    dict[kvp.Key] = kvp.Value;
                }

            }

        }



    }
}
