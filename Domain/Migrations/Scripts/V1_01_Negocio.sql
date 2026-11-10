-- =============================================================
-- V1 - Módulo Negocio
-- =============================================================
CREATE OR ALTER PROCEDURE PA_OBTENER_NEGOCIO(
    @Id_Negocio INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT ID_NEGOCIO, NOMBRE, TELEFONO, RUC, DIRECCION, CORREO_ELECTRONICO, LOGO,
           ID_USUARIO_CREACION, FECHA_CREACION, ID_USUARIO_MODIFICACION, FECHA_MODIFICACION
    FROM NEGOCIO
    WHERE ID_NEGOCIO = @Id_Negocio;
END;
GO

CREATE OR ALTER PROCEDURE PA_EDITAR_NEGOCIO(
    @Id_Negocio INT,
    @Nombre VARCHAR(60),
    @Telefono VARCHAR(10),
    @Ruc VARCHAR(13),
    @Direccion VARCHAR(60),
    @Correo_Electronico VARCHAR(40),
    @Logo VARBINARY(MAX) = NULL,
    @Id_Usuario_Modificacion INT = NULL
)
AS
BEGIN
    UPDATE NEGOCIO
    SET NOMBRE = @Nombre, TELEFONO = @Telefono, RUC = @Ruc, DIRECCION = @Direccion,
        CORREO_ELECTRONICO = @Correo_Electronico, LOGO = @Logo,
        ID_USUARIO_MODIFICACION = @Id_Usuario_Modificacion, FECHA_MODIFICACION = SYSUTCDATETIME()
    WHERE ID_NEGOCIO = @Id_Negocio;
END;
GO
