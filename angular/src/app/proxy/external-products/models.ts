import type { ExternalProductStatus } from './external-product-status.enum';

export interface ExternalProductDto {
  status?: ExternalProductStatus;
  barcode?: string | null;
  name?: string | null;
  brand?: string | null;
  imageUrl?: string | null;
}

export interface GetExternalProductInputDto {
  barcode: string;
}
