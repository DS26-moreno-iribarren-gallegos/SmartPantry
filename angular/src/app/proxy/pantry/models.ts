import type { EntityDto } from '@abp/ng.core';
import type { PantryItemStatus } from './pantry-item-status.enum';

export interface CreatePantryItemDto {
  productBarcode: string;
  expirationDate?: string | null;
  quantity?: number | null;
  unit?: string | null;
}

export interface PantryItemDto extends EntityDto<string> {
  productBarcode?: string;
  productName?: string | null;
  status?: PantryItemStatus;
  expirationDate?: string | null;
  quantity?: number | null;
  unit?: string | null;
}

export interface UpdatePantryItemDto {
  quantity?: number | null;
  unit?: string | null;
  expirationDate?: string | null;
}
