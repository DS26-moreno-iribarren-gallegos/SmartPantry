using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace SmartPantry.Pantry
{
    public class PantryItem : AggregateRoot<Guid>
    {
        public Guid UserId { get; set; }

        public string ProductBarcode { get; set; }

        public PantryItemStatus Status { get; set; }

        public DateTime? ExpirationDate { get; set; }

        public decimal? Quantity { get; set; }

        public string? Unit { get; set; }

        protected PantryItem() { } // Requerido por EF Core

        public PantryItem(
            Guid id,
            Guid userId,
            string productBarcode,
            PantryItemStatus status,
            DateTime? expirationDate = null,
            decimal? quantity = null,
            string? unit = null)
            : base(id)
        {
            UserId = userId;
            ProductBarcode = productBarcode;
            Status = status;
            ExpirationDate = expirationDate;
            SetQuantityAndUnit(quantity, unit);
        }

        public void UpdateDetails(decimal? quantity, string? unit, DateTime? expirationDate)
        {
            SetQuantityAndUnit(quantity, unit);
            ExpirationDate = expirationDate;
        }

        public void MarkAsConsumed()
        {
            Status = PantryItemStatus.Consumed;
        }

        private void SetQuantityAndUnit(decimal? quantity, string? unit)
        {
            if (quantity.HasValue && quantity.Value < 0)
            {
                throw new BusinessException("SmartPantry:NegativeQuantity")
                    .WithData("Quantity", quantity.Value);
            }

            Quantity = quantity;
            Unit = string.IsNullOrWhiteSpace(unit) ? null : unit.Trim();
        }
    }
}