using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;


namespace NetWinformsTest
{
    internal class OAuth
    {
        HttpClient _client = new HttpClient();
        IConfigurationRoot _config;
        Logger _logger;


        OAuth(IConfigurationRoot config)
        {
            _config = config;
        }


        private async void button1_Click(object sender, EventArgs e)
        {

            var localPort = _config["LocalPort"];

            // Creates a redirect URI using an available port on the loopback address.
            var redirectURI = $"http://localhost:{localPort}/";


            // Creates an HttpListener to listen for requests on that redirect URI.
            var http = new HttpListener();
            http.Prefixes.Add(redirectURI);

            http.Start();

            var client_id = _config["Configuration:ClientId"]; //    "Ov23lihBZ9OGpBRWhSko";
            var client_secret = _config["Configuration:ClientSecret"]; // "81a18aba29da99d6720ebcdd117c68f9fc3d84c4";

            string authorizationRequest = $"https://github.com/login/oauth/authorize?client_id={client_id}&scope=repo,user&prompt=select_account";
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = authorizationRequest,
                UseShellExecute = true
            };

            Logger.LogMessage($"Launching browser");
            // Opens request in the browser.
            var proc = System.Diagnostics.Process.Start(psi);

            // Waits for the OAuth authorization response.
            var context = await http.GetContextAsync();

            // Brings this app back to the foreground.
           // this.Activate(); <-- if we are using a winforms app

            // Sends an HTTP response to the browser.
            var response = context.Response;

            //

            string responseString = string.Format("<html><head><meta http-equiv='refresh' content='10;url=https://www.zebra.com'></head><body>Please return to the app.</body></html>");
            var buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            var responseOutput = response.OutputStream;
            Task responseTask = responseOutput.WriteAsync(buffer, 0, buffer.Length).ContinueWith((task) =>
            {
                responseOutput.Close();
                http.Stop();
                Console.WriteLine("HTTP server stopped.");
            });

            // Checks for errors.
            if (context.Request.QueryString.Get("error") != null)
            {

                return;
            }
            if (context.Request.QueryString.Get("code") == null)

            {

                return;
            }

            var code = context.Request.QueryString.Get("code");
            // var client_id = "Ov23lihBZ9OGpBRWhSko";
            // var client_secret = "81a18aba29da99d6720ebcdd117c68f9fc3d84c4";

            var accessTokenRequest = $"https://github.com/login/oauth/access_token";//?client_id=Ov23lihBZ9OGpBRWhSko&client_secret=81a18aba29da99d6720ebcdd117c68f9fc3d84c4&code={code}";

            var access_token = await performCodeExchange(accessTokenRequest, code, client_id, client_secret);

            proc?.CloseMainWindow();



        }


        async Task<string> performCodeExchange(string tokenRequestURI, string code, string client_id, string client_secret)
        {

            Dictionary<string, string> mydict = new Dictionary<string, string>()
            {
                ["client_id"] = client_id,
                ["client_secret"] = client_secret,
                ["code"] = code

            };

            // using HttpClient client = new HttpClient();

            _client.DefaultRequestHeaders.Add("Accept", "application/json");
            _client.DefaultRequestHeaders.Add("ContentType", "application/x-www-form-urlencoded");

            using var myresponse =
                await _client.PostAsync(tokenRequestURI, new FormUrlEncodedContent(mydict));

            var responseText =
                await myresponse.Content.ReadAsStringAsync();

            Dictionary<string, string>? tokenEndpointDecoded =
                JsonSerializer.Deserialize<Dictionary<string, string>>(responseText);

            string? access_token = tokenEndpointDecoded?["access_token"];
            Logger.LogMessage($"Oauth Token : {access_token}");

            Clipboard.SetText(access_token != null ? access_token : string.Empty);
            Logger.LogMessage("Oauth token copied to clipboard");

            return access_token;
        }

    }
}
