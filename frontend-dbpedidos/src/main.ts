import { bootstrapApplication } from '@angular/platform-browser';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import axios, { AxiosHeaders } from 'axios';

import { appConfig } from './app/app.config';
import { App } from './app/app';
import { routes } from './app/app.routes';

// ✅ Interceptor global de Axios para incluir token JWT automáticamente
axios.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('token');

    if (token) {
      if (!config.headers) {
        config.headers = new AxiosHeaders();
      }
      (config.headers as AxiosHeaders).set('Authorization', `Bearer ${token}`);
    }

    return config;
  },
  (error) => Promise.reject(error)
);

// 🚀 Bootstrap de la aplicación Angular
bootstrapApplication(App, {
  providers: [
    provideRouter(routes),
    provideHttpClient(),
  ],
});
