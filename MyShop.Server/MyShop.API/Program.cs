using Database.Repositories;
using HotChocolate.Data.Filters;
using Microsoft.EntityFrameworkCore;
using MyShop.API.GraphQL.Mutations;
using MyShop.API.GraphQL.Queries;
using MyShop.Application;
using MyShop.Application.Interfaces;
using MyShop.Domain.Interfaces;
using MyShop.Infrastructure.Data;
using MyShop.Infrastructure.Data.Repositories;
using MyShop.Infrastructure.Services;
using MyShop.Shared.Contants;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//auto-mapper
builder.Services.AddAutoMapper(typeof(ApplicationAssemblyMarket));

//dbcontext
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

//GraphQL
builder.Services
    .AddGraphQLServer()
    .AddQueryType(d => d.Name("Query"))
        .AddTypeExtension<ProductQueries>()
        .AddTypeExtension<OrderQueries>()
        .AddTypeExtension<CategoryQueries>()
    .AddMutationType(d => d.Name("Mutation"))
        .AddTypeExtension<ProductMutations>()
        .AddTypeExtension<OrderMutations>()
        .AddTypeExtension<CategoryMutations>()
        .AddTypeExtension<UserMutations>()
    .AddProjections()
    .AddFiltering()
    .AddSorting()
    .ModifyRequestOptions(opt =>
    {
        opt.IncludeExceptionDetails = true;
    })
    .ModifyCostOptions(opt =>
    {
        opt.MaxFieldCost = 5000;
    });

//Service
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IProductImageRepository, ProductImageRepository>();

builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITokenService, TokenService>();

//options
builder.Services.Configure<JwtOption>(builder.Configuration.GetSection("Jwt"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//app.UseAuthorization();

app.MapGraphQL();

app.Run();
