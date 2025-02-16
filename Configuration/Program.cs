namespace Configuration;

public class Program
{
    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();

                //webBuilder.ConfigureKestrel(serverOptions =>
                //{
                //    serverOptions.ListenLocalhost(8080);
                //    serverOptions.ListenLocalhost(8081, listenOptions =>
                //    {
                //        listenOptions.UseHttps("certs/localhost.pfx", "");
                //    });
                //});
            });

    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }
}