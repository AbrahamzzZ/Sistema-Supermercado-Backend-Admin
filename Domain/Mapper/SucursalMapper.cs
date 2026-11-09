using Domain.Model;
using Domain.Model.Dto;
using NetTopologySuite.Geometries;

namespace Domain.Mapper
{
    public static class SucursalMapper
    {
        public static Sucursal ToEntity(SucursalUbicacion dto)
        {
            return new Sucursal
            {
                Codigo = dto.Codigo,
                Id_Negocio = dto.Id_Negocio,
                Nombre_Sucursal = dto.Nombre_Sucursal,
                Direccion_Sucursal = dto.Direccion_Sucursal,
                Ciudad_Sucursal = dto.Ciudad_Sucursal,
                Estado = dto.Estado,
                Ubicacion_Sucursal =
                    dto.Latitud.HasValue && dto.Longitud.HasValue
                    ? new Point(dto.Longitud.Value, dto.Latitud.Value) { SRID = 4326 }
                    : null
            };
        }
    }

}
