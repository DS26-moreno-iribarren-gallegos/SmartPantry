using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SmartPantry.Pantry; // Namespace de tu Domain Service
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Threading;
using Volo.Abp.Uow;

namespace SmartPantry.Workers
{
    public class ExpirationWarningWorker : AsyncPeriodicBackgroundWorkerBase
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public ExpirationWarningWorker(
            AbpAsyncTimer timer,
            IServiceScopeFactory serviceScopeFactory) : base(timer, serviceScopeFactory)
        {
            // Configuramos la ejecucion para cada 1 minuto (60000 ms) para facilitar las pruebas locales.
            // En produccion, esto suele configurarse a 24 horas (1000 * 60 * 60 * 24).
            Timer.Period = 60000; 
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
        {
            Logger.LogInformation("Iniciando Worker de revisión de vencimientos...");

            try
            {
                // 1. Resolver dependencias creando un nuevo alcance (scope) en cada ejecución aislando el proceso
                using var scope = _serviceScopeFactory.CreateAsyncScope();
                
                var expirationManager = scope.ServiceProvider.GetRequiredService<PantryExpirationManager>();
                var uowManager = scope.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();

                // 2. Delimitar la Unidad de Trabajo explícitamente para las escrituras en BD
                using var uow = uowManager.Begin();

                // 3. Ejecutar la regla de negocio usando UTC
                await expirationManager.ProcessExpirationsAsync(DateTime.UtcNow);

                // 4. Confirmar la transaccion
                await uow.CompleteAsync();
                
                Logger.LogInformation("Worker de vencimientos finalizado con éxito.");
            }
            catch (Exception ex)
            {
                // 5. Registrar el error sin lanzar la excepcion hacia afuera, para que el temporizador no se rompa
                Logger.LogError(ex, "Ocurrió un error procesando los vencimientos. Se reintentará en el próximo ciclo.");
            }
        }
    }
}