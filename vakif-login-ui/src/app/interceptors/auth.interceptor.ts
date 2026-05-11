import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { AuthService } from '../services/auth';
import { catchError, throwError, switchMap } from 'rxjs';

// Bu endpoint'ler 401 aldığında refresh/logout döngüsüne GİRMEMELİ.
// - /me: initStore() kendi catchError'u ile 401'i sessizce yutar
// - /login, /register, /forgot-password, /reset-password: Auth gerektirmez
// - /refresh-token: Zaten yenileme denemesidir, tekrar denememeli
// - /logout: Çıkış isteğidir, 401 alırsa tekrar logout çağırmak sonsuz döngü yapar
const AUTH_BYPASS_URLS = [
  '/auth/me',
  '/auth/login',
  '/auth/register',
  '/auth/forgot-password',
  '/auth/reset-password',
  '/auth/refresh-token',
  '/auth/logout'
];

function shouldBypass401(url: string): boolean {
  const lowerUrl = url.toLowerCase();
  return AUTH_BYPASS_URLS.some(bypassUrl => lowerUrl.includes(bypassUrl));
}

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const platformId = inject(PLATFORM_ID);
  const isBrowser = isPlatformBrowser(platformId);
  const authService = inject(AuthService);

  // Helper: Çerez okumak için (Sadece tarayıcıda çalışır)
  const getCookie = (name: string) => {
    if (!isBrowser) return null;
    const value = `; ${document.cookie}`;
    const parts = value.split(`; ${name}=`);
    if (parts.length === 2) return parts.pop()?.split(';').shift();
    return null;
  };

  const xsrfToken = getCookie('XSRF-TOKEN');

  // BFF + Cookie mimarisi için tüm isteklere withCredentials: true ekliyoruz.
  const headers: any = {};
  if (xsrfToken) {
    headers['X-XSRF-TOKEN'] = xsrfToken;
  }

  const authReq = req.clone({
    withCredentials: true,
    setHeaders: headers
  });

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      // 401 hatası gelirse VE bu istek bypass listesinde değilse
      if (error.status === 401 && !shouldBypass401(req.url)) {
        
        // ÖNEMLİ: Eğer kullanıcı zaten login değilse, refresh denemeye gerek yok.
        // Bu durum, kullanıcı hiç giriş yapmamışken korumalı sayfaya direkt gittiğinde oluşur.
        if (!authService.isLoggedIn()) {
          return throwError(() => error);
        }

        // Kullanıcı login görünüyorsa ama 401 aldıysa session düşmüş demektir, yenilemeyi dene.
        return authService.refreshToken().pipe(
          switchMap(() => {
            // Yenileme başarılı, orijinal isteği tekrar klonlayıp gönder
            return next(authReq);
          }),
          catchError((err) => {
            // Yenileme de başarısızsa (Refresh Token da dolmuşsa) kullanıcıyı dışarı at
            authService.logout();
            return throwError(() => err);
          })
        );
      }
      return throwError(() => error);
    })
  );
};

