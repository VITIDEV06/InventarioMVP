<div align="center">

![InventarioMVP Banner](docs/img/banner.jpg)

<img src="docs/img/logo.png" alt="Logo InventarioMVP" width="150"/>

# 📦 InventarioMVP

### Sistema inteligente de gestión de inventario · Escritorio · Arquitectura MVP en capas

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-14-239120?style=for-the-badge&logo=csharp&logoColor=white)
![Windows Forms](https://img.shields.io/badge/Windows%20Forms-0078D4?style=for-the-badge&logo=windows11&logoColor=white)
![MySQL](https://img.shields.io/badge/MySQL-8.0-4479A1?style=for-the-badge&logo=mysql&logoColor=white)
![Visual Studio](https://img.shields.io/badge/Visual%20Studio-2026-5C2D91?style=for-the-badge&logo=visualstudio&logoColor=white)

![Patrón](https://img.shields.io/badge/Patr%C3%B3n-MVP-8A2BE2?style=flat-square&logo=diagramsdotnet&logoColor=white)
![Seguridad](https://img.shields.io/badge/Hash-BCrypt-success?style=flat-square&logo=letsencrypt&logoColor=white)
![Iconos](https://img.shields.io/badge/Iconos-Lucide-F56565?style=flat-square&logo=lucide&logoColor=white)
![Estado](https://img.shields.io/badge/Estado-Funcional-brightgreen?style=flat-square)
![Plataforma](https://img.shields.io/badge/Plataforma-Windows-0078D6?style=flat-square&logo=windows&logoColor=white)

<br/>

<img src="https://skillicons.dev/icons?i=cs,dotnet,visualstudio,mysql,git,github&theme=dark" alt="Tecnologías"/>

</div>

---

## 📑 Tabla de contenido

- [✨ Descripción](#-descripción)
- [🚀 Funcionalidades](#-funcionalidades)
- [🧰 Tecnologías utilizadas](#-tecnologías-utilizadas)
- [🗂️ Distribución del proyecto](#️-distribución-del-proyecto)
- [🏛️ Arquitectura MVP en capas](#️-arquitectura-mvp-en-capas)
- [🔐 Seguridad](#-seguridad)
- [🔄 Flujos](#-flujos)
- [🗄️ Base de datos](#️-base-de-datos)
- [⚙️ Instalación](#️-instalación)
- [📸 Capturas de pantalla](#-capturas-de-pantalla)
- [🎛️ Controles utilizados](#️-controles-utilizados)
- [🎨 Iconografía e identidad visual](#-iconografía-e-identidad-visual)
- [🧪 Pruebas realizadas](#-pruebas-realizadas)
- [🏁 Conclusiones](#-conclusiones)
- [👤 Autor](#-autor)

---

## ✨ Descripción

**InventarioMVP** es una aplicación de escritorio para **gestionar el inventario** de un negocio: registrar, editar, eliminar y buscar productos, administrar los usuarios del sistema y entrar de forma segura con una autenticación real contra **MySQL**.

Fue desarrollada como laboratorio del curso **Herramientas de la Programación Aplicada III (.NET)** y su objetivo principal es demostrar una **separación limpia de responsabilidades** mediante el patrón **MVP** (Modelo · Vista · Presentador) en cuatro capas independientes.

> 💡 La interfaz **no sabe** qué es un hash ni toca la base de datos; el presentador **no conoce** controles de Windows Forms; y la capa de datos **no tiene** reglas de negocio.

---

## 🚀 Funcionalidades

| | Módulo | Detalle |
|---|---|---|
| 🔑 | **Login** | Acceso con **usuario o correo** + contraseña, con validaciones. Autenticación real contra MySQL comparando el hash BCrypt. El mensaje de error es genérico para no revelar si el usuario existe. |
| 🏠 | **Dashboard** | Menú lateral izquierdo (Inicio, Inventario, Usuarios, Cerrar sesión), **fecha y hora en vivo** y el **usuario autenticado** visible en la parte inferior del menú y en la esquina superior derecha. |
| 📦 | **Inventario** | `DataGridView` con código, nombre, descripción, categoría, precio y stock. **Registrar, editar, eliminar** (con confirmación) y **buscar**. |
| 👥 | **Usuarios** | Listado y registro de nuevos usuarios; la contraseña se guarda **únicamente como hash BCrypt**. |
| 🛡️ | **Privilegios mínimos** | La aplicación usa un usuario de MySQL propio con solo los permisos indispensables (nunca `root`). |

### ✅ Reglas de validación

| Campo | Regla |
|---|---|
| 📦 Producto | Código, nombre y categoría obligatorios · código **único** · precio ≥ 0 · stock ≥ 0 |
| 👤 Usuario | 3-50 caracteres sin espacios ni `@` · correo válido · contraseña ≥ 8 caracteres con confirmación · usuario y correo **únicos** |
| 🔑 Login | Usuario/correo y contraseña obligatorios |

---

## 🧰 Tecnologías utilizadas

<div align="center">

| Tecnología | Versión | Para qué se usa |
|:---:|:---:|---|
| ![.NET](https://img.shields.io/badge/.NET-512BD4?logo=dotnet&logoColor=white) | **10.0** (SDK 10.0.403) | Plataforma de ejecución |
| ![C#](https://img.shields.io/badge/C%23-239120?logo=csharp&logoColor=white) | **14** | Lenguaje |
| ![WinForms](https://img.shields.io/badge/Windows%20Forms-0078D4?logo=windows11&logoColor=white) | `net10.0-windows` | Interfaz de escritorio |
| ![MySQL](https://img.shields.io/badge/MySQL-4479A1?logo=mysql&logoColor=white) | **8.0** | Base de datos |
| ![MySql.Data](https://img.shields.io/badge/MySql.Data-4479A1?logo=nuget&logoColor=white) | **26.10.0** | Conector ADO.NET a MySQL |
| ![BCrypt](https://img.shields.io/badge/BCrypt.Net--Next-success?logo=nuget&logoColor=white) | **4.2.0** | Generación y verificación de hashes |
| ![Svg.NET](https://img.shields.io/badge/Svg.NET-F5A623?logo=nuget&logoColor=white) | **3.4.7** | Renderizado de los iconos SVG |
| ![Lucide](https://img.shields.io/badge/Lucide-F56565?logo=lucide&logoColor=white) | última | Colección de iconos *outline* |
| ![Visual Studio](https://img.shields.io/badge/Visual%20Studio-5C2D91?logo=visualstudio&logoColor=white) | **2026** | IDE |
| ![Git](https://img.shields.io/badge/Git-F05032?logo=git&logoColor=white) ![GitHub](https://img.shields.io/badge/GitHub-181717?logo=github&logoColor=white) | — | Control de versiones y repositorio |

</div>

---

## 🗂️ Distribución del proyecto

La solución (`InventarioMVP.slnx`) está formada por **4 proyectos**, uno por capa:

```text
📁 InventarioMVP/
├── 📄 InventarioMVP.slnx
├── 📄 README.md
├── 📄 .gitignore
│
├── 📁 BaseDatos/
│   └── 📜 instalacion.sql              ← BD, tablas, usuario con privilegios mínimos y admin inicial
│
├── 📁 docs/img/                         ← banner, logo original y capturas del README
│
├── 🟦 InventarioMVP.Entidades/          ← MODELO: clases puras
│   ├── Usuarios.cs                      (Usuario)
│   └── Productos.cs                     (Producto)
│
├── 🟩 InventarioMVP.Datos/              ← ACCESO A DATOS
│   ├── ConexionmYSQL.cs                 (ConexionMySql)
│   └── Repositorios/
│       ├── IUsuarioRepositorio.cs · UsuarioRepositorio.cs
│       └── IProductoRepositorio.cs · ProductoRepositorio.cs
│
├── 🟨 InventarioMVP.Negocio/            ← LÓGICA DE NEGOCIO
│   ├── Seguridad/
│   │   ├── IHashService.cs              (solo GENERA hashes)
│   │   ├── IVerificarHashService.cs     (solo VERIFICA hashes)
│   │   ├── BCryptHashService.cs
│   │   └── BCryptVerificarHashService.cs
│   └── Servicios/
│       ├── AutenticacionService.cs
│       ├── ProductoService.cs
│       ├── UsuarioService.cs
│       └── ValidacionException.cs
│
└── 🟪 InventarioMVP/                    ← VISTA + PRESENTADOR (Windows Forms)
    ├── Program.cs                       (raíz de composición)
    ├── appsettings.example.json         (plantilla; el appsettings.json real está en .gitignore)
    ├── Presentadores/
    │   ├── LoginPresentador.cs
    │   ├── InventarioPresentador.cs
    │   └── UsuariosPresentador.cs
    ├── Vistas/
    │   ├── ILoginView.cs · IInventarioView.cs · IUsuariosView.cs   (contratos de vista)
    │   ├── FrmLogin.cs · FrmDashboard.cs
    │   ├── UcInicio.cs · UcInventario.cs · UcUsuarios.cs
    │   └── PerfilUsuarioControl.cs · Estilos.cs · Iconos.cs · Imagenes.cs
    └── Resources/
        ├── Icons/                       (SVG Lucide + licencia)
        └── Images/                      (logo.png y app.ico incrustados)
```

| Capa | Proyecto | Responsabilidad | Depende de |
|:---:|---|---|---|
| 🟪 Vista + Presentador | `InventarioMVP` | Mostrar datos, capturar eventos, coordinar | Negocio, Entidades, Datos* |
| 🟨 Negocio | `InventarioMVP.Negocio` | Reglas, validaciones y seguridad | Datos, Entidades |
| 🟩 Datos | `InventarioMVP.Datos` | Conexión y consultas parametrizadas | Entidades |
| 🟦 Entidades | `InventarioMVP.Entidades` | Clases de datos | — |

<sub>\* Solo `Program.cs` (raíz de composición) referencia Datos para ensamblar las dependencias.</sub>

---

## 🏛️ Arquitectura MVP en capas

```mermaid
flowchart LR
    subgraph UI["🖥️ InventarioMVP · Windows Forms"]
        direction TB
        V["👁️ Vista<br/>FrmLogin · FrmDashboard<br/>UcInventario · UcUsuarios"]
        P["🎯 Presentador<br/>LoginPresentador<br/>InventarioPresentador<br/>UsuariosPresentador"]
        V -- "eventos del usuario" --> P
        P -- "actualiza la vista<br/>vía interfaces" --> V
    end

    subgraph BL["🧠 InventarioMVP.Negocio"]
        direction TB
        S["⚙️ Servicios<br/>Autenticación · Producto · Usuario"]
        H["🔐 Seguridad<br/>IHashService<br/>IVerificarHashService"]
        S --> H
    end

    subgraph DL["🗃️ InventarioMVP.Datos"]
        R["📚 Repositorios<br/>consultas parametrizadas"]
    end

    E["🧱 InventarioMVP.Entidades<br/>Usuario · Producto"]
    DB[("🐬 MySQL 8.0<br/>inventario_db")]

    P --> S
    S --> R
    R -- "MySql.Data" --> DB
    S -.- E
    R -.- E
```

**Ventajas aplicadas:** separación de responsabilidades · presentadores probables sin interfaz gráfica · cambio de algoritmo de hash sin tocar Vista, Presentador ni Datos · escalabilidad.

---

## 🔐 Seguridad

Dos contratos **independientes** en la capa de Negocio, con responsabilidad única cada uno:

```mermaid
classDiagram
    direction LR
    class IHashService {
        <<interface>>
        +GenerarHash(textoPlano) string
    }
    class IVerificarHashService {
        <<interface>>
        +Verificar(textoPlano, hashAlmacenado) bool
    }
    class BCryptHashService {
        -FactorTrabajo = 11
        +GenerarHash(textoPlano) string
    }
    class BCryptVerificarHashService {
        +Verificar(textoPlano, hashAlmacenado) bool
    }
    class UsuarioService {
        +Registrar(usuario, correo, nombre, password)
    }
    class AutenticacionService {
        +Autenticar(identificador, password) Usuario
    }

    IHashService <|.. BCryptHashService
    IVerificarHashService <|.. BCryptVerificarHashService
    UsuarioService ..> IHashService : genera el hash al registrar
    AutenticacionService ..> IVerificarHashService : verifica al iniciar sesión
```

| Medida | Cómo se aplica |
|---|---|
| 🔒 Contraseñas | Solo se almacena el **hash BCrypt** (factor 11, sal incluida) |
| 🛡️ Inyección SQL | **Todas** las consultas usan parámetros (`@param`); el `LIKE` de la búsqueda escapa `%` y `_` |
| 👤 Privilegios mínimos | Usuario `inventario_app` con `SELECT, INSERT` en `usuarios` y `SELECT, INSERT, UPDATE, DELETE` en `productos` |
| 🕵️ Login | Mismo mensaje ante usuario inexistente o contraseña incorrecta |

---

## 🔄 Flujos

### 🔑 Inicio de sesión

```mermaid
sequenceDiagram
    autonumber
    actor U as 👤 Usuario
    participant V as 👁️ FrmLogin (Vista)
    participant P as 🎯 LoginPresentador
    participant S as ⚙️ AutenticacionService
    participant R as 📚 UsuarioRepositorio
    participant DB as 🐬 MySQL
    participant H as 🔐 IVerificarHashService

    U->>V: Escribe usuario o correo y contraseña
    V->>P: Evento IniciarSesionSolicitado
    P->>S: Autenticar(identificador, password)
    S->>S: Valida campos obligatorios
    S->>R: ObtenerPorUsuarioOCorreo(identificador)
    R->>DB: SELECT ... WHERE nombre_usuario = @id OR correo = @id
    DB-->>R: Fila del usuario con su hash
    R-->>S: Usuario
    S->>H: Verificar(password, hashAlmacenado)
    H-->>S: true o false
    alt Credenciales correctas
        S-->>P: Usuario autenticado
        P->>V: CompletarInicioSesion(usuario)
        V-->>U: Abre el Dashboard
    else Credenciales incorrectas
        S-->>P: ValidacionException
        P->>V: MostrarError con el mensaje de credenciales incorrectas
        V-->>U: Mensaje en pantalla
    end
```

### 📦 Guardar un producto

```mermaid
flowchart TD
    A([👤 Pulsa Guardar]) --> B[Vista: LeerFormulario]
    B --> C[Presentador: ProductoService.Guardar]
    C --> D{¿Datos válidos?<br/>código · nombre · categoría<br/>precio ≥ 0 · stock ≥ 0}
    D -- No --> E[/⚠️ ValidacionException<br/>la vista muestra el mensaje/]
    D -- Sí --> F{¿Código repetido?}
    F -- Sí --> E
    F -- No --> G{¿Id = 0?}
    G -- Sí --> H[(INSERT parametrizado)]
    G -- No --> I[(UPDATE parametrizado)]
    H --> J[Recarga la tabla y limpia el formulario]
    I --> J
    J --> K([✅ Mensaje de éxito])
```

### 🧭 Navegación de la aplicación

```mermaid
flowchart LR
    L["🔑 Login"] -- "credenciales válidas" --> D["🏠 Dashboard"]
    D --> I["🏠 Inicio<br/>reloj y bienvenida"]
    D --> N["📦 Inventario<br/>CRUD y búsqueda"]
    D --> U["👥 Usuarios<br/>listado y registro"]
    D -- "Cerrar sesión" --> L
```

---

## 🗄️ Base de datos

```mermaid
erDiagram
    USUARIOS {
        int id PK "AUTO_INCREMENT"
        varchar nombre_usuario UK "50"
        varchar correo UK "120"
        varchar nombre_completo "100"
        varchar password_hash "BCrypt"
        datetime fecha_creacion
    }
    PRODUCTOS {
        int id PK "AUTO_INCREMENT"
        varchar codigo UK "30"
        varchar nombre "100"
        varchar descripcion "255, opcional"
        varchar categoria "60"
        decimal precio "10,2 mayor o igual a 0"
        int stock "mayor o igual a 0"
        datetime fecha_creacion
    }
```

El script [`BaseDatos/instalacion.sql`](BaseDatos/instalacion.sql) crea la base `inventario_db`, las dos tablas (con restricciones `UNIQUE` y `CHECK`), el usuario de MySQL con privilegios mínimos y el usuario inicial del sistema. Usa `IF NOT EXISTS`, por lo que **no borra datos existentes**.

---

## ⚙️ Instalación

### 📋 Requisitos

- 🪟 Windows 10/11
- 🟣 [Visual Studio 2026](https://visualstudio.microsoft.com/) con la carga de trabajo *Desarrollo de escritorio con .NET* (o el [SDK de .NET 10](https://dotnet.microsoft.com/download))
- 🐬 [MySQL Server 8.0](https://dev.mysql.com/downloads/mysql/) en ejecución

### 🪜 Pasos

**1️⃣ Clonar el repositorio**

```bash
git clone https://github.com/<tu-usuario>/InventarioMVP.git
cd InventarioMVP
```

**2️⃣ Crear la base de datos** (como administrador de MySQL, por ejemplo `root`)

Primero abra `BaseDatos/instalacion.sql` y reemplace `CAMBIAR_POR_SU_CLAVE` por una contraseña propia y segura para el usuario `inventario_app`. Luego ejecute:

```bash
mysql -u root -p < BaseDatos/instalacion.sql
```

> También puede abrir `BaseDatos/instalacion.sql` en **MySQL Workbench** y ejecutarlo completo.

**3️⃣ Crear su archivo de configuración local**

Copie `InventarioMVP/appsettings.example.json` como `InventarioMVP/appsettings.json` (este archivo está en el `.gitignore` y nunca se sube al repositorio) y escriba la **misma contraseña** del paso anterior:

```json
{
  "ConnectionStrings": {
    "MySql": "Server=localhost;Port=3306;Database=inventario_db;Uid=inventario_app;Pwd=SU_CLAVE;SslMode=Preferred;"
  }
}
```

**4️⃣ Compilar y ejecutar**

```bash
dotnet build
dotnet run --project InventarioMVP/InventarioMVP.csproj
```

O abra `InventarioMVP.slnx` en Visual Studio y presione **F5**.

### 🔑 Acceso inicial

| Usuario | Correo | Contraseña |
|---|---|---|
| `admin` | `admin@inventario.local` | `Admin123!` |

> ⚠️ Solo se guarda el **hash**. `admin` / `Admin123!` es una credencial **de demostración**: cámbiela (registre su propio usuario) antes de usar el sistema fuera de un entorno de pruebas. La contraseña de MySQL de `inventario_app` y el archivo `appsettings.json` real **no** forman parte del repositorio.

---

## 📸 Capturas de pantalla

> Las imágenes se cargan automáticamente desde la carpeta [`docs/img/`](docs/img/). Basta con guardar cada captura con el **nombre exacto** indicado.

### 🔑 Login y validaciones

| Pantalla de login | Validación de credenciales |
|:---:|:---:|
| ![Login](docs/img/login.png) | ![Login con error](docs/img/login-error.png) |
| `login.png` | `login-error.png` |

### 🏠 Dashboard (menú lateral, hora y usuario)

![Inicio](docs/img/inicio.png)

<sub>`inicio.png`</sub>

### 📦 Inventario

| Tabla de productos | Validaciones del formulario |
|:---:|:---:|
| ![Inventario](docs/img/inventario.png) | ![Validaciones de inventario](docs/img/inventario-validaciones.png) |
| `inventario.png` | `inventario-validaciones.png` |

### 👥 Usuarios

![Usuarios](docs/img/usuarios.png)

<sub>`usuarios.png`</sub>

### 🐬 Base de datos

| Tablas en MySQL Workbench | Hash BCrypt almacenado | Privilegios mínimos (`SHOW GRANTS`) |
|:---:|:---:|:---:|
| ![Tablas](docs/img/mysql-tablas.png) | ![Hash](docs/img/mysql-hash.png) | ![Privilegios](docs/img/mysql-privilegios.png) |
| `mysql-tablas.png` | `mysql-hash.png` | `mysql-privilegios.png` |

### 🗂️ Estructura de la solución

![Estructura](docs/img/estructura-solucion.png)

<sub>`estructura-solucion.png` (Explorador de soluciones de Visual Studio)</sub>

---

## 🎛️ Controles utilizados

| Control | Uso en la aplicación |
|---|---|
| `Form` | Ventanas de login y Dashboard |
| `UserControl` | Pantallas de Inicio, Inventario y Usuarios dentro del Dashboard |
| `Panel` · `FlowLayoutPanel` · `TableLayoutPanel` | Distribución del menú, barras y formularios |
| `Label` · `TextBox` · `NumericUpDown` | Datos de entrada y textos (`UseSystemPasswordChar` en contraseñas) |
| `Button` | Acciones con icono (buscar, guardar, eliminar…) |
| `DataGridView` | Tablas de productos y usuarios |
| `PictureBox` | Logo e iconos |
| `Timer` | Reloj actualizado cada segundo |
| `MessageBox` | Avisos y confirmaciones |
| Control propio `PerfilUsuarioControl` | Perfil discreto (icono en círculo + nombre) en la esquina superior derecha |

---

## 🎨 Iconografía e identidad visual

- 🖼️ **Logo** de la aplicación en el login, el menú lateral, la pantalla de inicio y como **icono de todas las ventanas** y del ejecutable (`app.ico` con 7 tamaños, de 16 a 256 px).
- ✏️ Iconos **[Lucide](https://lucide.dev/icons/)** en estilo *outline* (licencia ISC, copia en `Resources/Icons/LICENSE.txt`), de 20 px y trazo discreto: gris neutro en el menú, blanco sobre botones y sobre la opción activa (color de acento).
- 📦 Los SVG originales y las imágenes están **incrustados en el ejecutable**; no se carga ningún archivo externo en tiempo de ejecución. `Vistas/Iconos.cs` rasteriza los SVG con **Svg.NET** al color y tamaño necesarios.

| Icono | Uso |
|---|---|
| `layout-dashboard` `package` `users` `log-out` | Menú lateral |
| `clock` · `user-round` | Fecha y hora · perfil del usuario |
| `search` `list` `plus` `save` `trash` | Acciones del inventario |
| `user-plus` · `log-in` | Registrar usuario · iniciar sesión |

---

## 🧪 Pruebas realizadas

Se verificó la capa de negocio y de datos **contra el MySQL real** (sin datos simulados) con 16 pruebas de integración:

| Área | Casos comprobados |
|---|---|
| 🔑 Login | Acceso con usuario y con correo · contraseña incorrecta · usuario inexistente · campos vacíos · intento de **inyección SQL** (`' OR '1'='1`) |
| 👥 Usuarios | Correo inválido · contraseña corta · usuario duplicado |
| 📦 Productos | Insertar · buscar · buscar con `%` · editar · eliminar · código duplicado · precio negativo |
| 🛡️ Privilegios | `SHOW GRANTS` confirma solo los permisos previstos para `inventario_app` |

<sub>Las pruebas se ejecutaron con un proyecto temporal y no forman parte del repositorio.</sub>

---

## 🏁 Conclusiones

1. 🧩 **El patrón MVP en capas mantiene el código ordenado y mantenible.** Al aislar Vista, Presentador, Negocio, Datos y Entidades, cada cambio queda acotado: modificar la interfaz no toca la lógica, y cambiar de algoritmo de hash solo implica reemplazar dos clases.
2. 🔐 **Dos interfaces separadas (`IHashService` e `IVerificarHashService`) aplican el principio de responsabilidad única.** Generar un hash y verificarlo son acciones distintas; quien registra usuarios no necesita poder verificar, y quien inicia sesión no necesita poder generar.
3. 🛡️ **La seguridad se construye por capas:** contraseñas con BCrypt, consultas parametrizadas contra la inyección SQL y un usuario de base de datos con privilegios mínimos, de modo que un fallo en la aplicación tenga un impacto limitado.
4. 🎨 **La experiencia de usuario importa.** Un menú lateral claro, iconografía consistente, el logo en todas las pantallas y mensajes de validación comprensibles hacen que una aplicación académica se sienta profesional.
5. 🧪 **Probar contra la base de datos real descubre errores que los datos simulados ocultan**, como la contraseña de root faltante, los privilegios o el escape del `LIKE`.
6. 📚 **Aprendizajes técnicos:** inyección de dependencias manual en una raíz de composición, interfaces de vista para desacoplar los presentadores de Windows Forms, y los detalles del escalado en pantallas de alta resolución.

**Posibles mejoras futuras:** reportes y gráficos de stock, roles y permisos por usuario, edición y baja de usuarios, pruebas unitarias de los presentadores y exportación a Excel/PDF.

---

## 👤 Autor

<div align="center">

### **Victor David Montes**

🎓 Universidad Tecnológica de Panamá · Facultad de Ingeniería en Sistemas Computacionales
📚 Herramientas de la Programación Aplicada III (.NET) · Grupo **1IL133**
👩‍🏫 Instructora: Ing. Irina Fong
📅 **8 de noviembre de 2026**

<img src="https://skillicons.dev/icons?i=cs,dotnet,mysql,visualstudio,github&theme=dark" alt="Stack"/>

<sub>Hecho con 💜 y mucho café · iconos por <a href="https://lucide.dev">Lucide</a> (ISC)</sub>

</div>
