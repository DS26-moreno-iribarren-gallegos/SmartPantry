using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace SmartPantry.Products;

public class Product:BasicAggregateRoot<Guid>
{
    public string Name { get; private set; } = string.Empty;
    public string Maker { get; private set; }  = string.Empty;
    public string Barcode { get; private set; }  = string.Empty;

    // constructor de la clase 
    protected Product()
    {
      
    }
    
    // constructor publico, recibe los datos y los valida.
    public Product( Guid id, string name, string maker, string barcode) : base(id)
    {
        Name = Check.NotNullOrWhiteSpace(name?.Trim(), nameof(name), maxLength:ProductConsts.NameMaxLength);
        Maker = Check.NotNullOrWhiteSpace(maker?.Trim(), nameof(maker), maxLength: ProductConsts.MakerMaxLength);
        Barcode = Check.NotNullOrWhiteSpace(barcode?.Trim(), nameof(barcode), maxLength: ProductConsts.BarcodeMaxLength);
    }
}
