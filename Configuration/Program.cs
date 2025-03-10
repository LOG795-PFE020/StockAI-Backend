namespace Configuration;

public class Program
{
    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>()
                    .ConfigureKestrel(serverOptions =>
                    {
                        serverOptions.ListenAnyIP(8081,
                            listenOptions =>
                            {
                                listenOptions.UseHttps("./certs/localhost.pfx", "secret");
                                listenOptions.UseConnectionLogging();
                            });
                    });

            });

    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }
}