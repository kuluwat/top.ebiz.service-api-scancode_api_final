namespace top.ebiz.helper
{
    public static class AppEnvironment
    {
        public static string CurrentEnvironment { get; private set; } = "PROD";

        public static void SetEnvironment(IWebHostEnvironment env)
        {
            CurrentEnvironment = env.EnvironmentName?.ToUpperInvariant() ?? "PROD";
        }

        public static bool IsLocal() => CurrentEnvironment == "LOCAL";
        public static bool IsDevelopment() => CurrentEnvironment == "DEV";
        public static bool IsQAS() => CurrentEnvironment == "QAS";
        public static bool IsProduction() => CurrentEnvironment == "PROD";

        private static string GetEnvSuffix()
        {
            return CurrentEnvironment switch
            {
                "DEV" => "_DEV",
                "QAS" => "_QAS",
                "PROD" => "_PROD",
                _ => throw new InvalidOperationException("Environment not recognized")
            };
        }

        private static string GetConfigurationValue(string keyPrefix)
        {
            var key = keyPrefix + GetEnvSuffix();
            var value = Environment.GetEnvironmentVariable(key);
            if (string.IsNullOrEmpty(value))
            {
                throw new InvalidOperationException($"Environment variable '{key}' is not set.");
            }
            return value;
        }

        public static string GeteServerWebString() => GetConfigurationValue("ServerWeb");
        public static string GeteConnectionString() => GetConfigurationValue("eBizConnection");
        public static string GeteTripConnectionString() => GetConfigurationValue("eBizConnection");
        public static string GeteTravelerProfileConnectionString() => GetConfigurationValue("eBizConnection");
        public static string GeteCarServiceConnectionString() => GetConfigurationValue("CarServiceConnection");
        public static string GeteLinkLogin() => GetConfigurationValue("LinkLogin");
        public static string GeteLinkLoginTravelerhistory() => GetConfigurationValue("LinkLoginTravelerhistory");
        public static string GeteServerFolder() => GetConfigurationValue("ServerFolder");
        public static string GeteServerPathAPI() => GetConfigurationValue("ServerPathAPI");

        // Additional keys requested
        public static string GetGraphUserScopes() => Environment.GetEnvironmentVariable("GraphUserScopes") ?? "https://graph.microsoft.com/.default";
        public static string GetTenantRoleRead() => Environment.GetEnvironmentVariable("tenantRoleRead") ?? "";
        public static string GetClientIDRoleRead() => Environment.GetEnvironmentVariable("clientIDRoleRead") ?? "";
        public static string GetSecretRoleRead() => Environment.GetEnvironmentVariable("secretRoleRead") ?? "";
    }
}
