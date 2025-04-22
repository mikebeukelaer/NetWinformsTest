using Microsoft.Extensions.Configuration;
using System.Runtime.InteropServices;


namespace NetWinformsTest
{
    public partial class Form1 : Form
    {
        IConfigurationRoot _config;

        public Form1()
        {
            InitializeComponent();
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("AppSettings.json")
                .AddUserSecrets("myclientid")
                .AddEnvironmentVariables();
            
            
             
            
            _config = builder.Build();
            var os = _config["POSH_THEMES_PATH"];
            
        }

        private void button1_Click(object sender, EventArgs e)
        {

            var builder = new ZRSConfigurationBuilder()
                .SetBasePath (AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("AppSettings.json")
                .AddUserSecrets("myclientid")
                .AddEnvironmentVariables()
                .Build();





            label1.Text = _config?["Playtime:somevalue"];
            label2.Text = _config?["Movies:ServiceApiKey"];
            var x = _config?["Movies:ServiceAasdf"];

            var mydictionary = new ConfigurationRoot<string,string>();
            
            mydictionary["mike"] = "user";
            var j = mydictionary["mike"];
            var l = mydictionary["mi"];



        }

        

    }
}
