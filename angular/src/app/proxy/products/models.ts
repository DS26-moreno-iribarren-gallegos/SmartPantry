import type { AuditedEntityDto } from '@abp/ng.core';

export interface CreateUpdateProductDto {
  name: string;
  barcode: string;
  maker: string;
}

export interface ProductDto extends AuditedEntityDto<string> {
  name?: string;
  maker?: string;
  barcode?: string;
}
