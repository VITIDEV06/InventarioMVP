-- =====================================================================
-- InventarioMVP - Migración v2: roles y permisos, clientes, ventas,
-- movimientos de inventario, actividad y configuración.
--
-- Requisito: haber ejecutado antes BaseDatos/instalacion.sql.
-- Ejecutar con un administrador de MySQL (por ejemplo root):
--   mysql -u root -p < BaseDatos/migracion_v2.sql
--
-- SEGURA Y REPETIBLE: no elimina tablas ni datos; puede ejecutarse varias veces.
-- =====================================================================

USE inventario_db;

-- ---------------------------------------------------------------------
-- Procedimientos auxiliares temporales (se eliminan al final)
-- ---------------------------------------------------------------------
DROP PROCEDURE IF EXISTS sp_agregar_columna;
DROP PROCEDURE IF EXISTS sp_agregar_fk;

DELIMITER $$

CREATE PROCEDURE sp_agregar_columna(IN p_tabla VARCHAR(64), IN p_columna VARCHAR(64), IN p_definicion VARCHAR(255))
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.COLUMNS
                   WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = p_tabla AND COLUMN_NAME = p_columna) THEN
        SET @ddl = CONCAT('ALTER TABLE `', p_tabla, '` ADD COLUMN `', p_columna, '` ', p_definicion);
        PREPARE st FROM @ddl;
        EXECUTE st;
        DEALLOCATE PREPARE st;
    END IF;
END$$

CREATE PROCEDURE sp_agregar_fk(IN p_tabla VARCHAR(64), IN p_nombre VARCHAR(64), IN p_definicion VARCHAR(255))
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.TABLE_CONSTRAINTS
                   WHERE CONSTRAINT_SCHEMA = DATABASE() AND TABLE_NAME = p_tabla
                     AND CONSTRAINT_NAME = p_nombre AND CONSTRAINT_TYPE = 'FOREIGN KEY') THEN
        SET @ddl = CONCAT('ALTER TABLE `', p_tabla, '` ADD CONSTRAINT `', p_nombre, '` ', p_definicion);
        PREPARE st FROM @ddl;
        EXECUTE st;
        DEALLOCATE PREPARE st;
    END IF;
END$$

DELIMITER ;

-- ---------------------------------------------------------------------
-- Roles, permisos y asignaciones
-- ---------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS roles (
    id          TINYINT UNSIGNED NOT NULL AUTO_INCREMENT,
    nombre      VARCHAR(30)  NOT NULL,
    descripcion VARCHAR(120) NOT NULL,
    PRIMARY KEY (id),
    UNIQUE KEY uq_roles_nombre (nombre)
) ENGINE=InnoDB;

INSERT IGNORE INTO roles (id, nombre, descripcion) VALUES
    (1, 'ADMIN',   'Administrador: acceso completo al sistema'),
    (2, 'USUARIO', 'Usuario: acceso limitado según sus permisos');

CREATE TABLE IF NOT EXISTS permisos (
    id          SMALLINT UNSIGNED NOT NULL AUTO_INCREMENT,
    codigo      VARCHAR(40)  NOT NULL,
    descripcion VARCHAR(150) NOT NULL,
    delegable   TINYINT(1)   NOT NULL DEFAULT 0,   -- 1 = el administrador puede concederlo a un usuario individual
    PRIMARY KEY (id),
    UNIQUE KEY uq_permisos_codigo (codigo)
) ENGINE=InnoDB;

INSERT IGNORE INTO permisos (codigo, descripcion, delegable) VALUES
    ('DASHBOARD_GENERAL',   'Ver el dashboard general del negocio (ingresos y gráficos)',  0),
    ('PRODUCTOS_VER',       'Consultar productos',                                         0),
    ('PRODUCTOS_GESTIONAR', 'Crear, editar y eliminar productos',                          0),
    ('INVENTARIO_VER',      'Consultar el inventario y sus movimientos',                   0),
    ('INVENTARIO_MOVER',    'Registrar entradas y salidas de inventario',                  0),
    ('CLIENTES_VER',        'Consultar clientes',                                          0),
    ('CLIENTES_GESTIONAR',  'Registrar y editar clientes',                                 1),
    ('VENTAS_PROPIAS_VER',  'Consultar las propias operaciones de venta',                  0),
    ('VENTAS_REGISTRAR',    'Registrar ventas',                                            1),
    ('VENTAS_VER_TODAS',    'Consultar las ventas de todos los usuarios',                  0),
    ('REPORTES_VER',        'Consultar y exportar reportes',                               0),
    ('USUARIOS_GESTIONAR',  'Crear, editar, activar/desactivar usuarios y asignar roles',  0),
    ('CONFIG_PERSONAL',     'Modificar la información personal y la contraseña propias',   0),
    ('CONFIG_ADMIN',        'Acceder a la configuración administrativa',                   0);

CREATE TABLE IF NOT EXISTS rol_permisos (
    rol_id     TINYINT UNSIGNED  NOT NULL,
    permiso_id SMALLINT UNSIGNED NOT NULL,
    PRIMARY KEY (rol_id, permiso_id),
    CONSTRAINT fk_rolperm_rol     FOREIGN KEY (rol_id)     REFERENCES roles (id)    ON DELETE CASCADE,
    CONSTRAINT fk_rolperm_permiso FOREIGN KEY (permiso_id) REFERENCES permisos (id) ON DELETE CASCADE
) ENGINE=InnoDB;

-- ADMIN: todos los permisos
INSERT IGNORE INTO rol_permisos (rol_id, permiso_id)
SELECT 1, id FROM permisos;

-- USUARIO: solo consulta + sus operaciones + su configuración personal
INSERT IGNORE INTO rol_permisos (rol_id, permiso_id)
SELECT 2, id FROM permisos
WHERE codigo IN ('PRODUCTOS_VER', 'INVENTARIO_VER', 'CLIENTES_VER', 'VENTAS_PROPIAS_VER', 'CONFIG_PERSONAL');

-- ---------------------------------------------------------------------
-- Usuarios: rol, estado y último acceso (se conservan los usuarios existentes)
-- ---------------------------------------------------------------------
CALL sp_agregar_columna('usuarios', 'rol_id',        'TINYINT UNSIGNED NOT NULL DEFAULT 2');
CALL sp_agregar_columna('usuarios', 'activo',        'TINYINT(1) NOT NULL DEFAULT 1');
CALL sp_agregar_columna('usuarios', 'ultimo_acceso', 'DATETIME NULL');
CALL sp_agregar_fk('usuarios', 'fk_usuarios_rol', 'FOREIGN KEY (rol_id) REFERENCES roles (id)');

-- La cuenta administradora inicial conserva su acceso completo
UPDATE usuarios SET rol_id = 1 WHERE nombre_usuario = 'admin';

CREATE TABLE IF NOT EXISTS usuario_permisos (
    usuario_id INT               NOT NULL,
    permiso_id SMALLINT UNSIGNED NOT NULL,
    PRIMARY KEY (usuario_id, permiso_id),
    CONSTRAINT fk_usrperm_usuario FOREIGN KEY (usuario_id) REFERENCES usuarios (id) ON DELETE CASCADE,
    CONSTRAINT fk_usrperm_permiso FOREIGN KEY (permiso_id) REFERENCES permisos (id) ON DELETE CASCADE
) ENGINE=InnoDB;

-- ---------------------------------------------------------------------
-- Productos: stock mínimo para detectar stock bajo
-- ---------------------------------------------------------------------
CALL sp_agregar_columna('productos', 'stock_minimo', 'INT NOT NULL DEFAULT 5');
-- "Eliminar" un producto es una baja lógica: así se conservan sus ventas y movimientos históricos
CALL sp_agregar_columna('productos', 'activo', 'TINYINT(1) NOT NULL DEFAULT 1');

-- ---------------------------------------------------------------------
-- Configuración
-- ---------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS configuracion (
    clave       VARCHAR(50)  NOT NULL,
    valor       VARCHAR(255) NOT NULL,
    actualizado DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (clave)
) ENGINE=InnoDB;

INSERT IGNORE INTO configuracion (clave, valor) VALUES ('stock_minimo_defecto', '5');

-- ---------------------------------------------------------------------
-- Clientes (no se eliminan: se conserva el historial de compras)
-- ---------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS clientes (
    id              INT          NOT NULL AUTO_INCREMENT,
    nombre_completo VARCHAR(120) NOT NULL,
    telefono        VARCHAR(30)  NULL,
    correo          VARCHAR(120) NULL,
    direccion       VARCHAR(255) NULL,
    observaciones   VARCHAR(500) NULL,
    fecha_registro  DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (id),
    UNIQUE KEY uq_clientes_correo (correo),
    KEY ix_clientes_nombre (nombre_completo),
    KEY ix_clientes_telefono (telefono)
) ENGINE=InnoDB;

-- ---------------------------------------------------------------------
-- Ventas y detalle (históricos: sin borrado; FK con RESTRICT)
-- ---------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS ventas (
    id                INT           NOT NULL AUTO_INCREMENT,
    fecha             DATETIME      NOT NULL DEFAULT CURRENT_TIMESTAMP,
    cliente_id        INT           NULL,
    cliente_ocasional VARCHAR(120)  NULL,
    usuario_id        INT           NOT NULL,
    total             DECIMAL(12,2) NOT NULL,
    PRIMARY KEY (id),
    KEY ix_ventas_fecha (fecha),
    KEY ix_ventas_usuario (usuario_id),
    KEY ix_ventas_cliente (cliente_id),
    CONSTRAINT ck_ventas_total   CHECK (total >= 0),
    CONSTRAINT ck_ventas_cliente CHECK (cliente_id IS NOT NULL OR cliente_ocasional IS NOT NULL),
    CONSTRAINT fk_ventas_cliente FOREIGN KEY (cliente_id) REFERENCES clientes (id),
    CONSTRAINT fk_ventas_usuario FOREIGN KEY (usuario_id) REFERENCES usuarios (id)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS detalle_ventas (
    id              INT           NOT NULL AUTO_INCREMENT,
    venta_id        INT           NOT NULL,
    producto_id     INT           NOT NULL,
    cantidad        INT           NOT NULL,
    precio_unitario DECIMAL(10,2) NOT NULL,
    subtotal        DECIMAL(12,2) NOT NULL,
    PRIMARY KEY (id),
    UNIQUE KEY uq_detalle_venta_producto (venta_id, producto_id),
    KEY ix_detalle_producto (producto_id),
    CONSTRAINT ck_detalle_cantidad CHECK (cantidad > 0),
    CONSTRAINT ck_detalle_precio   CHECK (precio_unitario >= 0),
    CONSTRAINT fk_detalle_venta    FOREIGN KEY (venta_id)    REFERENCES ventas (id),
    CONSTRAINT fk_detalle_producto FOREIGN KEY (producto_id) REFERENCES productos (id)
) ENGINE=InnoDB;

-- ---------------------------------------------------------------------
-- Movimientos de inventario (toda variación de stock deja un registro)
-- ---------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS movimientos_inventario (
    id             BIGINT       NOT NULL AUTO_INCREMENT,
    producto_id    INT          NOT NULL,
    tipo           ENUM('ENTRADA','SALIDA','VENTA','AJUSTE') NOT NULL,
    cantidad       INT          NOT NULL,
    stock_anterior INT          NOT NULL,
    stock_nuevo    INT          NOT NULL,
    motivo         VARCHAR(200) NULL,
    usuario_id     INT          NOT NULL,
    venta_id       INT          NULL,
    fecha          DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (id),
    KEY ix_mov_producto (producto_id),
    KEY ix_mov_fecha (fecha),
    KEY ix_mov_usuario (usuario_id),
    CONSTRAINT ck_mov_cantidad CHECK (cantidad > 0),
    CONSTRAINT ck_mov_stock    CHECK (stock_anterior >= 0 AND stock_nuevo >= 0),
    CONSTRAINT fk_mov_producto FOREIGN KEY (producto_id) REFERENCES productos (id),
    CONSTRAINT fk_mov_usuario  FOREIGN KEY (usuario_id)  REFERENCES usuarios (id),
    CONSTRAINT fk_mov_venta    FOREIGN KEY (venta_id)    REFERENCES ventas (id)
) ENGINE=InnoDB;

-- Saldo inicial: el stock que ya existía queda reflejado como movimiento (una sola vez por producto)
INSERT INTO movimientos_inventario (producto_id, tipo, cantidad, stock_anterior, stock_nuevo, motivo, usuario_id)
SELECT p.id, 'ENTRADA', p.stock, 0, p.stock, 'Saldo inicial (migración v2)',
       (SELECT MIN(id) FROM usuarios WHERE rol_id = 1)
FROM productos p
WHERE p.stock > 0
  AND NOT EXISTS (SELECT 1 FROM movimientos_inventario m WHERE m.producto_id = p.id)
  AND EXISTS (SELECT 1 FROM usuarios WHERE rol_id = 1);

-- ---------------------------------------------------------------------
-- Actividad reciente del sistema
-- ---------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS actividad (
    id         BIGINT       NOT NULL AUTO_INCREMENT,
    usuario_id INT          NULL,
    accion     VARCHAR(40)  NOT NULL,
    detalle    VARCHAR(255) NULL,
    fecha      DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (id),
    KEY ix_actividad_fecha (fecha),
    CONSTRAINT fk_actividad_usuario FOREIGN KEY (usuario_id) REFERENCES usuarios (id)
) ENGINE=InnoDB;

-- ---------------------------------------------------------------------
-- Privilegios mínimos del usuario de la aplicación (se agregan a los existentes)
-- ---------------------------------------------------------------------
GRANT SELECT, INSERT, UPDATE         ON inventario_db.usuarios                TO 'inventario_app'@'localhost';
GRANT SELECT                         ON inventario_db.roles                   TO 'inventario_app'@'localhost';
GRANT SELECT                         ON inventario_db.permisos                TO 'inventario_app'@'localhost';
GRANT SELECT                         ON inventario_db.rol_permisos            TO 'inventario_app'@'localhost';
GRANT SELECT, INSERT, DELETE         ON inventario_db.usuario_permisos        TO 'inventario_app'@'localhost';
GRANT SELECT, INSERT, UPDATE         ON inventario_db.configuracion           TO 'inventario_app'@'localhost';
GRANT SELECT, INSERT, UPDATE         ON inventario_db.clientes                TO 'inventario_app'@'localhost';
GRANT SELECT, INSERT                 ON inventario_db.ventas                  TO 'inventario_app'@'localhost';
GRANT SELECT, INSERT                 ON inventario_db.detalle_ventas          TO 'inventario_app'@'localhost';
GRANT SELECT, INSERT                 ON inventario_db.movimientos_inventario  TO 'inventario_app'@'localhost';
GRANT SELECT, INSERT                 ON inventario_db.actividad               TO 'inventario_app'@'localhost';
FLUSH PRIVILEGES;

-- ---------------------------------------------------------------------
-- Limpieza de los procedimientos auxiliares
-- ---------------------------------------------------------------------
DROP PROCEDURE IF EXISTS sp_agregar_columna;
DROP PROCEDURE IF EXISTS sp_agregar_fk;

SELECT 'Migración v2 completada' AS resultado;
