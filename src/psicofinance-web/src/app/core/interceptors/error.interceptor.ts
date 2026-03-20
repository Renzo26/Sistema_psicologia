import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { catchError, throwError } from 'rxjs';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      let message = 'Erro desconhecido';

      if (error.error?.detail) {
        message = error.error.detail;
      } else {
        switch (error.status) {
          case 0:
            message = 'Servidor indisponível. Verifique sua conexão.';
            break;
          case 400:
            message = 'Requisição inválida.';
            break;
          case 403:
            message = 'Acesso negado.';
            break;
          case 404:
            message = 'Recurso não encontrado.';
            break;
          case 429:
            message = 'Muitas requisições. Aguarde um momento.';
            break;
          case 500:
            message = 'Erro interno do servidor.';
            break;
        }
      }

      console.error(`[HTTP ${error.status}] ${req.url}: ${message}`);
      return throwError(() => ({ ...error, friendlyMessage: message }));
    })
  );
};
