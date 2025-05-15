using System;
using System.Threading.Tasks;
using Azure.Data.AppConfiguration;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Azure;
using Microsoft.Graph;
using Microsoft.Identity.Client;

namespace PS.MIP.SampleApp
{
    class Program
    {
        private const string _clientId = "";
        private const string _tenantId = "";

        public static async Task Main(string[] args)
        {
            #region USER_AUTHENTICATION_WITH_MS_ENTRA_ID

            var app = PublicClientApplicationBuilder
                .Create(_clientId)
                .WithAuthority(AzureCloudInstance.AzurePublic, _tenantId)
                .WithRedirectUri("http://localhost")
                .Build();
            string[] authenticationScopes = { "user.read" };
            AuthenticationResult result = await app.AcquireTokenInteractive(authenticationScopes).ExecuteAsync();

            Console.WriteLine($"Token:\t{result.AccessToken}");

            #endregion USER_AUTHENTICATION_WITH_MS_ENTRA_ID



            #region MICROSOFT_GRAPH_INTEGRATION

            var options = new InteractiveBrowserCredentialOptions
            {
                TenantId = _tenantId,
                ClientId = _clientId,
                AuthorityHost = AzureAuthorityHosts.AzurePublicCloud,
                RedirectUri = new Uri("http://localhost"),
            };

            var interactiveCredential = new InteractiveBrowserCredential(options);
            string[] graphScopes = { "user.read" };
            var graphClient = new GraphServiceClient(interactiveCredential, graphScopes);

            var user = await graphClient.Me.GetAsync();

            Console.WriteLine($"You display name in Microsoft Entra ID is: {user.DisplayName}");

            Console.ReadKey();

            #endregion MICROSOFT_GRAPH_INTEGRATION


            #region AZURE_KEY_VAULT_INTEGRATION

            var secretClient = new SecretClient(new Uri("key-vault-url"),
                                          new ManagedIdentityCredential());

            await secretClient.SetSecretAsync("my-secret", "pluralsight-app-secret");

            var secret = await secretClient.GetSecretAsync("my-secret");

            Console.WriteLine($"Secret value: {secret.Value.Value}");

            #endregion


            #region AZURE_APP_CONFIGURATION_INTEGRATION

            var configurationClient = new ConfigurationClient(new Uri("app-config-url"),
                                                              new ManagedIdentityCredential());

            var setting = new ConfigurationSetting("MyApp:ProductsApi:Url", "https://sample-products-api.com");
            configurationClient.SetConfigurationSetting(setting);

            ConfigurationSetting existingConfig = configurationClient.GetConfigurationSetting("MyApp:ProductsApi:Url");

            setting.Label = "UAT";
            configurationClient.SetConfigurationSetting(setting);

            existingConfig = configurationClient.GetConfigurationSetting("MyApp:ProductsApi:Url");

            Console.WriteLine($"Configuration key: {existingConfig.Key}");
            Console.WriteLine($"Configuration value: {existingConfig.Value}");
            Console.WriteLine($"Configuration label: {existingConfig.Label}");

            #endregion AZURE_APP_CONFIGURATION_INTEGRATION




            Console.ReadKey();
        }
    }
}