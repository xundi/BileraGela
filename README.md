# BileraGela - Aplicación para la gestión de Reserva de Salas

**BileraGela - Aplicación para la gestión de Reserva de Salas** es una aplicación web desarrollada como Trabajo Fin de Grado (TFG) para la gestión de reservas de salas en el Hospital Universitario de Galdakao-Usansolo. 
El sistema permite a usuarios del entorno hospitalario:
- Reservar salas y recursos (como proyectores, equipos, etc.)
- Aprobar solicitudes especiales
- Ver disponibilidad en tiempo real mediante un calendario interactivo
- Gestionar Centros, Recursos, Usuarios y configuración según el rol del usuario

## Tecnologías utilizadas

- Lenguaje principal: C#
- Framework: ASP.NET Core MVC
- HTML, CSS, JavaScript
- FullCalendar.js (para el calendario interactivo)
- MySQL (base de datos relacional)
- Active Directory (autenticación de usuarios)
- IDE: Visual Studio Code
- GitHub (control de versiones)

## Perfiles de usuario

- **Usuario normal**: realiza reservas y gestiona las suyas. DNI: 11111111A
- **Docente o responsable**: aprueba o rechaza solicitudes de salas especiales. DNI: 22222222B
- **Administrador**: gestiona el sistema, salas, usuarios y configuración. DNI: 33333333C

## Capturas (opcional)
_Aquí puedes subir imágenes de la interfaz o diagramas si lo deseas._

## 📁 Estructura del proyecto 

```bash
BileraGela/
│
├── Controllers/          # Lógica de control (C#)
├── Models/               # Modelos de datos
├── Views/                # Vistas (HTML + Razor)
├── wwwroot/              # Recursos estáticos (CSS, JS)
├── README.md
├── .gitignore
├── appsettings.json      # Configuraciones
└── docs/                 # Documentación adicional (diagrama, manual, etc.)

## Instalación


1. Clona el repositorio:
   ```bash
   git clone https://github.com/xundi/BileraGela.git
