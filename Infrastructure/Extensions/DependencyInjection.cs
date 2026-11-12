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
            services.AddScoped<CategoriaRepository>();
            services.AddScoped<ClienteRepository>();
            services.AddScoped<ProveedorRepository>();
            services.AddScoped<TransportistaRepository>();
            services.AddScoped<NegocioRepository>();
            services.AddScoped<ProductoRepository>();
            services.AddScoped<OfertaRepository>();
            services.AddScoped<SucursalRepository>();
            return services;
        }

        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<CategoriaService>();
            services.AddScoped<ClienteService>();
            services.AddScoped<ProveedorService>();
            services.AddScoped<TransportistaService>();
            services.AddScoped<NegocioService>();
            services.AddScoped<MicroserviciosApiClient>();
            services.AddScoped<ProductoService>();
            services.AddScoped<OfertaService>();
            services.AddScoped<SucursalService>();
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
