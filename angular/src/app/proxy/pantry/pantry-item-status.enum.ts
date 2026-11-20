import { mapEnumToOptions } from '@abp/ng.core';

export enum PantryItemStatus {
  Available = 0,
  Consumed = 1,
  Discarded = 2,
}

export const pantryItemStatusOptions = mapEnumToOptions(PantryItemStatus);
