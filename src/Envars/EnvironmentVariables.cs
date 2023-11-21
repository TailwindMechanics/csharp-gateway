//path: src\Envars\EnvironmentVariables.cs

using dotenv.net;

namespace Neurocache.Envars
{
    public class EnvironmentVariables
    {
        public static IDictionary<string, string> Init(List<string> envars)
        {
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                return DotEnv.Fluent()
                    .WithExceptions()
                    .WithEnvFiles()
                    .WithTrimValues()
                    .WithOverwriteExistingVars()
                    .WithProbeForEnv(probeLevelsToSearch: 6)
                    .Read();
            }
            else
            {
                var envDictionary = new Dictionary<string, string>();
                envars.ForEach(envar =>
                {
                    envDictionary[envar] = Environment.GetEnvironmentVariable(envar)
                                            ?? string.Empty;
                });
                return envDictionary;
            }
        }
    }
}
