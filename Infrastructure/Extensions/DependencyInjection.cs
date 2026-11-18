using Domain.Model;
using Domain.Model.Dto;
using FluentValidation;
using Infrastructure.Repository;
using Infrastructure.Repository.InterfacesRepository;
using Infrastructure.Repository.InterfacesServices;
using Infrastructure.Services;
using Infrastructure.Security;
using Infrastructure.Services.Validators;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<ICategoriaRepository, CategoriaRepository>();
            services.AddScoped<IClienteRepository, ClienteRepository>();
            services.AddScoped<IProveedorRepository, ProveedorRepository>();
            services.AddScoped<ITransportistaRepository, TransportistaRepository>();
            services.AddScoped<INegocioRepository, NegocioRepository>();
            services.AddScoped<IProductoRepository, ProductoRepository>();
            services.AddScoped<IOfertaRepository, OfertaRepository>();
            services.AddScoped<ISucursalRepository, SucursalRepository>();
            return services;
        }

        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<ICategoriaService, CategoriaService>();
            services.AddScoped<IClienteService, ClienteService>();
            services.AddScoped<IProveedorService, ProveedorService>();
            services.AddScoped<ITransportistaService, TransportistaService>();
            services.AddScoped<INegocioService, NegocioService>();
            services.AddScoped<IMicroserviciosApiClient, MicroserviciosApiClient>();
            services.AddScoped<SeqLogService>();
            services.AddScoped<IProductoService, ProductoService>();
            services.AddScoped<IOfertaService, OfertaService>();
            services.AddScoped<ISucursalService, SucursalService>();
            return services;
        }

        public static IServiceCollection AddValidators(this IServiceCollection services)
        {
            services.AddScoped<IValidator<Categoria>, CategoriaValidator>();
            services.AddScoped<IValidator<Cliente>, ClienteValidator>();
            services.AddScoped<IValidator<Proveedor>, ProveedorValidator>();
            services.AddScoped<IValidator<Transportista>, TransportistaValidator>();
            services.AddScoped<IValidator<Negocio>, NegocioValidator>();
            services.AddScoped<IValidator<Producto>, ProductoValidator>();
            services.AddScoped<IValidator<Oferta>, OfertaValidator>();
            services.AddScoped<IValidator<SucursalUbicacion>, SucursalValidator>();
            return services;
        }
    }
}
