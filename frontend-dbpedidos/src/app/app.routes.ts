import { Routes } from '@angular/router';
import { AuthGuard } from './core/guards/auth.guard';


export const routes: Routes = [
  //Rutas de autenticación (públicas)
  {
    path: 'login',
    loadComponent: () =>
      import('./features/auth/login/login.component').then(m => m.LoginComponent),
  },
  {
  path: '',
  loadComponent: () =>
    import('./features/home.component').then(m => m.HomeComponent),
  canActivate: [AuthGuard]
},

  {
    path: 'register',
    loadComponent: () =>
      import('./features/auth/register/register.component').then(m => m.RegisterComponent),
  },

  // Rutas protegidas (requieren autenticación)
  {
    path: 'articulos',
    loadComponent: () =>
      import('./features/articulos/articulo-list/articulo.component').then(m => m.ArticuloComponent),
    canActivate: [AuthGuard]
  },
  {
    path: 'categorias',
    loadComponent: () =>
      import('./features/categorias/categoria-list/categoria.component').then(m => m.CategoriaComponent),
    canActivate: [AuthGuard]
  },
  {
    path: 'clientes',
    loadComponent: () =>
      import('./features/clientes/cliente-list/cliente.component').then(m => m.ClienteComponent),
    canActivate: [AuthGuard]
  },
  {
    path: 'trabajadores',
    loadComponent: () =>
      import('./features/trabajadores/trabajador-list/trabajador.component').then(m => m.TrabajadorComponent),
    canActivate: [AuthGuard]
  },
  {
    path: 'proveedores',
    loadComponent: () =>
      import('./features/proveedores/proveedor-list/proveedor.component').then(m => m.ProveedorComponent),
    canActivate: [AuthGuard]
  },
  {
    path: 'ingresos',
    loadComponent: () =>
      import('./features/ingresos/ingreso-list/ingreso.component').then(m => m.IngresoComponent),
    canActivate: [AuthGuard]
  },
  {
    path: 'ventas',
    loadComponent: () =>
      import('./features/ventas/venta-list/venta.component').then(m => m.VentaComponent),
    canActivate: [AuthGuard]
  },
  {
    path: 'presentaciones',
    loadComponent: () =>
      import('./features/presentacion/presentacion.component').then(m => m.PresentacionComponent),
    canActivate: [AuthGuard]
  },
  {
    path: 'detalle-ingresos',
    loadComponent: () =>
      import('./features/detalle-ingreso/detalle-ingreso.component').then(m => m.DetalleIngresoComponent),
    canActivate: [AuthGuard]
  },
  {
    path: 'reportes',
    loadComponent: () =>
      import('./features/reportes/reporte-list/reporte.component').then(m => m.ReporteComponent),
    canActivate: [AuthGuard]
  },
  {
    path: 'auditoria',
    loadComponent: () =>
      import('./features/auditoria/auditoria-list/auditoria.component').then(m => m.AuditoriaComponent),
    canActivate: [AuthGuard]
  },

  {
    path: 'ai-report',
    loadComponent: () =>
      import('./features/ai/ai-report/ai-report.component').then(m => m.AiReportComponent),
    canActivate: [AuthGuard]
  },

  // Ruta por defecto
  { path: '', redirectTo: '', pathMatch: 'full' }
];
