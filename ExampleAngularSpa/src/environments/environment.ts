// This file can be replaced during build by using the `fileReplacements` array.
// `ng build --prod` replaces `environment.ts` with `environment.prod.ts`.
// The list of file replacements can be found in `angular.json`.

export const environment = {
  production: false,
  samlSsoUrl: 'http://localhost:64978/api/Saml/InitiateSingleSignOn?returnurl=http://localhost:4200',
  samlSloUrl: 'http://localhost:64978/api/Saml/InitiateSingleLogout?returnurl=http://localhost:4200',
  samlAuthUrl: 'http://localhost:64978/api/Authorization',
  apiUrl: 'http://localhost:64978/api',
};

/*
 * For easier debugging in development mode, you can import the following file
 * to ignore zone related error stack frames such as `zone.run`, `zoneDelegate.invokeTask`.
 *
 * This import should be commented out in production mode because it will have a negative impact
 * on performance if an error is thrown.
 */
// import 'zone.js/dist/zone-error';  // Included with Angular CLI.
