-- =============================================================
-- V1 - Datos iniciales
-- =============================================================

-- Negocio
IF NOT EXISTS (SELECT 1 FROM NEGOCIO WHERE ID_NEGOCIO = 1)
BEGIN
    SET IDENTITY_INSERT NEGOCIO ON;
    INSERT INTO NEGOCIO (ID_NEGOCIO, NOMBRE, TELEFONO, RUC, DIRECCION, CORREO_ELECTRONICO)
    VALUES (1, 'Supermercado Paradisia', '0969810812', '0102030405785', 'Mucho Lote 3 etapa', 'SupermercadoParadisia@gmail.com');
    SET IDENTITY_INSERT NEGOCIO OFF;
END;
GO

-- Sucursales
IF NOT EXISTS (SELECT 1 FROM SUCURSAL WHERE CODIGO = '7247')
    INSERT INTO SUCURSAL (ID_NEGOCIO, CODIGO, NOMBRE_SUCURSAL, DIRECCION_SUCURSAL, UBICACION_SUCURSAL, CIUDAD_SUCURSAL, ESTADO)
    VALUES (1, '7247', 'SUCURSAL_9 DE OCT y LOS RIOS', 'AV. 9 DE OCTUBRE 803 Y LOS RIOS', geography::Point(-2.187746, -79.894365, 4326), 'Guayaquil', 1);

IF NOT EXISTS (SELECT 1 FROM SUCURSAL WHERE CODIGO = '6584')
    INSERT INTO SUCURSAL (ID_NEGOCIO, CODIGO, NOMBRE_SUCURSAL, DIRECCION_SUCURSAL, UBICACION_SUCURSAL, CIUDAD_SUCURSAL, ESTADO)
    VALUES (1, '6584', 'SUCURSAL_ALBANBORJA', 'AV.CARLOS JULIO AROSEMENA S/N', geography::Point(-2.169321, -79.917047, 4326), 'Guayaquil', 0);
GO

-- Categorías
INSERT INTO CATEGORIA (CODIGO, NOMBRE_CATEGORIA, ESTADO)
SELECT V.CODIGO, V.NOMBRE, 1
FROM (VALUES
    ('6564', N'Frutas y Verduras'),
    ('7845', N'Despensa'),
    ('1247', N'Preparados'),
    ('0786', N'Bebidas y Licores'),
    ('3472', N'Refrigerados'),
    ('1308', N'Congelados'),
    ('6832', N'Carnes'),
    ('1375', N'Embutidos'),
    ('3529', N'Panaderia'),
    ('4839', N'Mascotas'),
    ('3098', N'Cuidado Personal'),
    ('4802', N'Cuidado del Hogar'),
    ('6796', N'Electrodomésticos'),
    ('1378', N'Jardinería y Plantas')
) AS V(CODIGO, NOMBRE)
WHERE NOT EXISTS (SELECT 1 FROM CATEGORIA C WHERE C.CODIGO = V.CODIGO);
GO
