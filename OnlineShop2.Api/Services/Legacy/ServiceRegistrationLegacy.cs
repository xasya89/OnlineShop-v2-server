using OnlineShop2.LegacyDb.Repositories;

namespace OnlineShop2.Api.Services.Legacy
{
    public static class ServiceRegistrationLegacy
    {
        public static void AddServicesLegacy(this IServiceCollection services) =>
            services
            .AddTransient<ISupplierRepositoryLegacy, SupplierRepositoryLegacy>()
            .AddTransient<IGoodGroupRepositoryLegacy, GoodGroupRepositoryLegacy>()
            .AddTransient<IGoodReporitoryLegacy, GoodRepositoryLegacy>()
            .AddTransient<IShiftRepositoryLegacy, ShiftRepositoryLegacy>()
            .AddTransient<ICurrentBalanceRepositoryLegacy, CurrentBalanceRepositoryLegacy>()
            .AddTransient<IArrivalRepositoryLegacy, ArrivalRepositoryLegacy>()
            .AddTransient<IWriteofRepositoryLegacy, WriteofRepositoryLegacy>()
            .AddTransient<IRevaluationRepositoryLegacy, RevaluationRepositoryLegacy>()
            .AddTransient<IStocktackingRepositoryLegacy, StocktackingRepositoryLegacy>()
            .AddTransient<IUnitOfWorkLegacy, UnitOfWorkLegacy>();
    }
}
