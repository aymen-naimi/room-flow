import { HttpInterceptorFn } from '@angular/common/http';
import { environment } from '../../environments/environment';

export function withApiBaseUrl(url: string, baseUrl: string): string {
  const base = baseUrl.replace(/\/$/, '');
  if (!base || !url.startsWith('/')) {
    return url;
  }

  return `${base}${url}`;
}

export const apiBaseUrlInterceptor: HttpInterceptorFn = (req, next) => {
  const url = withApiBaseUrl(req.url, environment.apiBaseUrl);
  return next(req.clone({ url, withCredentials: true }));
};
