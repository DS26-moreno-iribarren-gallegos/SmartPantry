using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace SmartPantry.Pantry
{
    public class PantryExpirationManager : DomainService
    {
        private readonly IRepository<PantryItem, Guid> _itemRepository;
        private readonly IRepository<ExpirationWarning, Guid> _warningRepository;
        
        // Umbral documentado: 3 días antes del vencimiento
        private const int ThresholdDays = 3;

        public PantryExpirationManager(
            IRepository<PantryItem, Guid> itemRepository,
            IRepository<ExpirationWarning, Guid> warningRepository)
        {
            _itemRepository = itemRepository;
            _warningRepository = warningRepository;
        }

        // Recibe la fecha por parámetro para aislar las pruebas. 
        // Usaremos UTC para evitar problemas de zonas horarias (exigido en TP08).
        public async Task ProcessExpirationsAsync(DateTime referenceDateUtc)
        {
            // Traemos todos los items. En un sistema en produccion se usaria paginacion.
            var items = await _itemRepository.GetListAsync();

            foreach (var item in items)
            {
                // Buscamos si ya existe una advertencia previa para mantener la idempotencia
                var existingWarning = await _warningRepository.FirstOrDefaultAsync(w => w.PantryItemId == item.Id);

                // Regla de negocio: Necesita advertencia si esta Disponible, TIENE fecha,
                // y la fecha es menor o igual a hoy + 3 días (incluye vencidos y dia limite)
                bool needsWarning = item.Status == PantryItemStatus.Available 
                                    && item.ExpirationDate.HasValue 
                                    && item.ExpirationDate.Value.Date <= referenceDateUtc.Date.AddDays(ThresholdDays);

                if (needsWarning)
                {
                    if (existingWarning == null)
                    {
                        // No existe: creamos una nueva
                        var newWarning = new ExpirationWarning(GuidGenerator.Create(), item.Id, item.ExpirationDate.Value, true);
                        await _warningRepository.InsertAsync(newWarning, autoSave: true);
                    }
                    else
                    {
                        // Ya existe: la actualizamos y aseguramos que este activa
                        existingWarning.IsActive = true;
                        existingWarning.TargetExpirationDate = item.ExpirationDate.Value;
                        await _warningRepository.UpdateAsync(existingWarning, autoSave: true);
                    }
                }
                else
                {
                    // Si no necesita advertencia (ej. se consumio, se le quito la fecha o se pateo el vencimiento a futuro)
                    if (existingWarning != null && existingWarning.IsActive)
                    {
                        existingWarning.IsActive = false;
                        await _warningRepository.UpdateAsync(existingWarning, autoSave: true);
                    }
                }
            }
        }
    }
}