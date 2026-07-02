# Sistema de Gestión de Ventas e Inventarios
## Comercializadora Santa Cruz S.R.L.

Sistema web desarrollado con **ASP.NET Core (.NET 8)**, **Angular 20** y **SQL Server**, orientado a la gestión de ventas, inventario, clientes y pedidos.

[![CI/CD Pipeline](https://github.com/FernandoAcostaA/projecto-gestion-inventario-.net-8-angular-20/actions/workflows/ci-cd.yml/badge.svg)](https://github.com/FernandoAcostaA/projecto-gestion-inventario-.net-8-angular-20/actions)
[![Version](https://img.shields.io/badge/version-2.0.0-blue)](CHANGELOG.md)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

---

# Requisitos Previos

Antes de ejecutar el proyecto asegúrese de tener instalado:

- Visual Studio 2022 o superior con la carga de trabajo **ASP.NET y desarrollo web**.
- .NET SDK 8.
- SQL Server 2019 o superior (Express o Developer).
- SQL Server Management Studio (SSMS).
- Node.js (versión LTS).
- Angular CLI.

---

# 1. Clonar el repositorio

```bash
git clone https://github.com/FernandoAcostaA/projecto-gestion-inventario-.net-8-angular-20.git
```

Entrar al proyecto:

```bash
cd projecto-gestion-inventario-.net-8-angular-20
```

---

# 2. Configurar la Base de Datos

Abrir **SQL Server Management Studio (SSMS)**.

Crear una base de datos llamada:

```
DbPedidos
```

Abrir el archivo:

```
script_bd_pedidos.sql
```

Ejecutar el script completo.

---

# 3. Configurar la Cadena de Conexión

Abrir el proyecto Backend desde Visual Studio.

Ruta:

```
PedidosApi/PedidosApi/appsettings.json
```

Modificar la sección:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=SERVIDOR_SQL;Database=DbPedidos;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

Ejemplo:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=DESKTOP-OJB2IDT\\SQLEXPRESS;Database=DbPedidos;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

> **Importante:** Reemplace `DESKTOP-OJB2IDT\\SQLEXPRESS` por el nombre de su instancia de SQL Server.

---

# 4. Ejecutar el Backend

Abrir la solución en Visual Studio.

Establecer **PedidosApi** como proyecto de inicio.

Presionar:

```
F5
```

o

```
Ctrl + F5
```

Si todo está correcto se abrirá automáticamente Swagger.

Ejemplo:

```
https://localhost:7145/swagger
```

Mantenga abierta esta ventana mientras ejecuta el frontend.

---

# 5. Instalar Node.js

Verificar que Node.js esté instalado.

```bash
node -v
```

Verificar npm.

```bash
npm -v
```

Si no está instalado, descargarlo desde:

https://nodejs.org

---

# 6. Instalar Angular CLI

Abrir una terminal y ejecutar:

```bash
npm install -g @angular/cli
```

Verificar la instalación:

```bash
ng version
```

---

# 7. Instalar las dependencias del Frontend

Entrar a la carpeta del frontend:

```bash
cd frontend-dbpedidos
```

Instalar dependencias:

```bash
npm install
```

---

# 8. Ejecutar el Frontend

Desde la carpeta **frontend-dbpedidos** ejecutar:

```bash
ng serve
```

o

```bash
ng serve --open
```

---

# 9. Abrir la aplicación

Cuando Angular termine de compilar mostrará una dirección similar a:

```
http://localhost:4200
```

Abrir esa dirección en el navegador.

Si el backend está ejecutándose correctamente, el sistema estará completamente funcional.

---

# Estructura del Proyecto

```
projecto-gestion-inventario-.net-8-angular-20
│
├── PedidosApi
│   └── PedidosApi
│       ├── Controllers
│       ├── Data
│       ├── Models
│       ├── Services
│       ├── Program.cs
│       └── appsettings.json
│
├── frontend-dbpedidos
│   ├── src
│   ├── angular.json
│   └── package.json
│
└── script_bd_pedidos.sql
```

---

# Tecnologías Utilizadas

- ASP.NET Core 8
- Entity Framework Core
- SQL Server
- Angular 20
- TypeScript
- Bootstrap
- Git
- GitHub Actions

---

# Solución de Problemas

### Error de conexión a la base de datos

Verifique:

- Que SQL Server esté iniciado.
- Que la base de datos **DbPedidos** exista.
- Que la cadena de conexión sea correcta.

---

### Error "ng no se reconoce"

Ejecute:

```bash
npm install -g @angular/cli
```

---

### Error al ejecutar el Backend

Verifique que el SDK de .NET 8 esté instalado:

```bash
dotnet --version
```

---

### Error al ejecutar Angular

Instale nuevamente las dependencias:

```bash
npm install
```

---

# Autor

**Fernando Acosta Algarañaz**

Universidad Privada Domingo Savio
Ingeniería de Software II
