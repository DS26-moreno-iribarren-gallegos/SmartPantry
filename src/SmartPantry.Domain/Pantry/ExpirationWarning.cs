using System;
using Volo.Abp.Domain.Entities;

namespace SmartPantry.Pantry
{
    public class ExpirationWarning : Entity<Guid>
    {
        // Clave logica estable: Un item solo puede tener una advertencia activa a la vez
        public Guid PantryItemId { get; set; }
        
        public bool IsActive { get; set; }
        
        // Fecha para saber cuándo se emitio o para quefecha de vencimiento aplica
        public DateTime TargetExpirationDate { get; set; }

        protected ExpirationWarning() { }

        public ExpirationWarning(Guid id, Guid pantryItemId, DateTime targetExpirationDate, bool isActive = true) 
            : base(id)
        {
            PantryItemId = pantryItemId;
            TargetExpirationDate = targetExpirationDate;
            IsActive = isActive;
        }
    }
}