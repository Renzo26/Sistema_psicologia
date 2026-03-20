import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthStore } from '../store/auth.store';
import { ApiService } from '../services/api.service';

export const refreshTokenInterceptor: HttpInterceptorFn = (req, next) => {
  const authStore = inject(AuthStore);
  const api = inject(ApiService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401 && !req.url.includes('auth/refresh')) {
        return api.post<{ accessToken: string }>('auth/refresh', {}).pipe(
          switchMap((response) => {
            authStore.setAccessToken(response.accessToken);
            const cloned = req.clone({
              setHeaders: { Authorization: `Bearer ${response.accessToken}` },
            });
            return next(cloned);
          }),
          catchError((refreshError) => {
            authStore.logout();
            return throwError(() => refreshError);
          })
        );
      }
      return throwError(() => error);
    })
  );
};
