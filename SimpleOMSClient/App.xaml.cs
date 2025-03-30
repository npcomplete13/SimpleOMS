using System;
using System.Configuration;
using System.Data;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using SimpleOMS.ViewModel;
using SimpleOMSClient.Clients;

namespace SimpleOMS
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IServiceProvider _serviceProvider;
        protected override void OnStartup(StartupEventArgs e)
        {

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            _serviceProvider = serviceCollection.BuildServiceProvider();

            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // a bit ugly, but...
            var broadcaseServiceUrl = ConfigurationManager.AppSettings["BroadcastServiceUrl"];
            var crudServiceUrl = ConfigurationManager.AppSettings["CrudServiceUrl"];

            if(string.IsNullOrEmpty(broadcaseServiceUrl) || string.IsNullOrEmpty(crudServiceUrl))
            {
                throw new Exception("Missing configuration for BroadcastServiceUrl or CrudServiceUrl");
            }

            // Register Services
            services.AddSingleton<CrudServiceClient.CrudServiceClient>(
                    provider =>
                        {
                            return new CrudServiceClient.CrudServiceClient(crudServiceUrl, new System.Net.Http.HttpClient());
                        });

            services.AddSingleton<BroadcastServiceClient>(
                provider =>
                {
                    return new BroadcastServiceClient(broadcaseServiceUrl);
                });

            // Register ViewModels
            services.AddSingleton<OMSViewModel>(
                provider =>
                {
                    return new OMSViewModel(provider.GetRequiredService<CrudServiceClient.CrudServiceClient>(),
                        provider.GetRequiredService<BroadcastServiceClient>());
                }
                );

            // Register Views
            services.AddSingleton<MainWindow>(
                provider =>
                {
                    return new MainWindow(provider.GetRequiredService<OMSViewModel>());
                }
                );
        }

        private void OnExit(object sender, ExitEventArgs e)
        {
            // Dispose of services if needed
            if (_serviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }



}
