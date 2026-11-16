using System;
using Volo.Abp.Domain.Entities;

namespace SmartPantry.Pantry
{
    public class PantryItem : AggregateRoot<Guid>
    {
        // Identificador del propietario (tomado de la persistencia, no de sesion HTTP)
        public Guid UserId { get; set; } 
        
        // Referencia al producto guardado (ajusta a Guid o string según tu diseño previo)
        public string ProductBarcode { get; set; } 
        
        public PantryItemStatus Status { get; set; }
        
        // Fecha de vencimiento opcional requerida por RF-15
        public DateTime? ExpirationDate { get; set; } 

        protected PantryItem() { } // Requerido por EF Core

        public PantryItem(Guid id, Guid userId, string productBarcode, PantryItemStatus status, DateTime? expirationDate = null)
            : base(id)
        {
            UserId = userId;
            ProductBarcode = productBarcode;
            Status = status;
            ExpirationDate = expirationDate;
        }
    }
}