import { mapEnumToOptions } from '@abp/ng.core';

export enum ExternalProductStatus {
  Found = 0,
  NotFound = 1,
  RateLimitExceeded = 2,
  ServiceUnavailable = 3,
}

export const externalProductStatusOptions = mapEnumToOptions(ExternalProductStatus);
