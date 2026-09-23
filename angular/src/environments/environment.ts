import { Environment } from '@abp/ng.core';

const baseUrl = 'http://localhost:4200';

const oAuthConfig = {
  issuer: 'https://localhost:44395/',
  redirectUri: baseUrl,
  clientId: 'SmartPantry1_App',
  responseType: 'code',
  scope: 'offline_access SmartPantry1',
  requireHttps: true,
};

export const environment = {
  production: false,
  application: {
    baseUrl,
    name: 'SmartPantry1',
  },
  oAuthConfig,
  apis: {
    default: {
      url: 'https://localhost:44395',
      rootNamespace: 'SmartPantry1',
    },
    AbpAccountPublic: {
      url: oAuthConfig.issuer,
      rootNamespace: 'AbpAccountPublic',
    },
  },
} as Environment;
