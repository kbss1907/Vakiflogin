import { ApplicationConfig, APP_INITIALIZER, PLATFORM_ID } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors, withFetch } from '@angular/common/http';
import { authInterceptor } from './interceptors/auth.interceptor';
import { AuthService } from './services/auth';

import { routes } from './app.routes';
import { provideClientHydration, withEventReplay } from '@angular/platform-browser';
import { isPlatformBrowser } from '@angular/common';

import { firstValueFrom, of } from 'rxjs';

// --- GÜVENLİK ADIMI: Uygulama başlarken ilk CSRF "Pulunu" sunucudan al ---
// Sadece tarayıcıda çalışır — SSR build zaman aşımını önler
export function initializeApp(authService: AuthService, platformId: object) {
  return () => {
    if (!isPlatformBrowser(platformId)) {
      return Promise.resolve(true);
    }
    return firstValueFrom(authService.initStore());
  };
}

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    provideClientHydration(withEventReplay()),
    provideHttpClient(
      withFetch(), // --- MODERN FETCH API ---
      withInterceptors([authInterceptor])
    ),
    {
      provide: APP_INITIALIZER,
      useFactory: initializeApp,
      deps: [AuthService, PLATFORM_ID],
      multi: true
    }
  ]
};
