-- =====================================================================
-- InventarioMVP - Script de instalación (MySQL 8.0+)
-- Ejecutar UNA vez con un usuario administrador (por ejemplo root):
--   mysql -u root -p < BaseDatos/instalacion.sql
-- No elimina datos existentes (usa IF NOT EXISTS).
-- =====================================================================

CREATE DATABASE IF NOT EXISTS inventario_db
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

USE inventario_db;

-- ---------------------------------------------------------------------
-- Tabla de usuarios (la contraseña se guarda SOLO como hash BCrypt)
-- ---------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS usuarios (
    id              INT          NOT NULL AUTO_INCREMENT,
    nombre_usuario  VARCHAR(50)  NOT NULL,
    correo          VARCHAR(120) NOT NULL,
    nombre_completo VARCHAR(100) NOT NULL,
    password_hash   VARCHAR(100) NOT NULL,
    fecha_creacion  DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (id),
    UNIQUE KEY uq_usuarios_nombre (nombre_usuario),
    UNIQUE KEY uq_usuarios_correo (correo)
) ENGINE=InnoDB;

-- ---------------------------------------------------------------------
-- Tabla de productos
-- ---------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS productos (
    id             INT           NOT NULL AUTO_INCREMENT,
    codigo         VARCHAR(30)   NOT NULL,
    nombre         VARCHAR(100)  NOT NULL,
    descripcion    VARCHAR(255)  NULL,
    categoria      VARCHAR(60)   NOT NULL,
    precio         DECIMAL(10,2) NOT NULL,
    stock          INT           NOT NULL DEFAULT 0,
    fecha_creacion DATETIME      NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (id),
    UNIQUE KEY uq_productos_codigo (codigo),
    KEY ix_productos_nombre (nombre),
    CONSTRAINT ck_productos_precio CHECK (precio >= 0),
    CONSTRAINT ck_productos_stock  CHECK (stock >= 0)
) ENGINE=InnoDB;

-- ---------------------------------------------------------------------
-- Usuario inicial de la aplicación (credenciales de acceso al sistema)
--   Usuario: admin   Correo: admin@inventario.local   Contraseña: Admin123!
-- Solo se guarda el hash BCrypt (factor 11). Cambie la contraseña tras el primer uso.
-- ---------------------------------------------------------------------
INSERT INTO usuarios (nombre_usuario, correo, nombre_completo, password_hash)
SELECT 'admin', 'admin@inventario.local', 'Administrador del Sistema',
       '$2a$11$ak9ix.LQM2FKDadvoltfauTTcDxpxTtIMkSSSgDxuZQx8me6h3skC'
WHERE NOT EXISTS (SELECT 1 FROM usuarios WHERE nombre_usuario = 'admin');

-- ---------------------------------------------------------------------
-- Usuario de MySQL para la aplicación, con privilegios mínimos:
--   * usuarios : SELECT, INSERT   (login y alta de usuarios; sin UPDATE/DELETE)
--   * productos: SELECT, INSERT, UPDATE, DELETE
--   * Sin acceso a otras bases, sin DDL, sin GRANT OPTION.
-- Solo puede conectarse desde la misma máquina (localhost).
-- ---------------------------------------------------------------------
-- !!! ANTES DE EJECUTAR: reemplace CAMBIAR_POR_SU_CLAVE por una contraseña propia y segura
-- !!! y use la MISMA en InventarioMVP/appsettings.json. No suba contraseñas reales al repositorio.
CREATE USER IF NOT EXISTS 'inventario_app'@'localhost' IDENTIFIED BY 'CAMBIAR_POR_SU_CLAVE';

GRANT SELECT, INSERT                 ON inventario_db.usuarios  TO 'inventario_app'@'localhost';
GRANT SELECT, INSERT, UPDATE, DELETE ON inventario_db.productos TO 'inventario_app'@'localhost';

FLUSH PRIVILEGES;
