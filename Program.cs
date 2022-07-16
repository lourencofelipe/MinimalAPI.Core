using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MinimalAPI.Core.Data;
using MinimalAPI.Core.Models;
using MiniValidation;
using NetDevPack.Identity;
using NetDevPack.Identity.Jwt;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddDbContext<MinimalContextDb>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentityEntityFrameworkContextConfiguration(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
	b => b.MigrationsAssembly("MinimalAPI.Core")));

builder.Services.AddIdentityConfiguration();
builder.Services.AddJwtConfiguration(builder.Configuration, "AppSettings");
				

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseAuthConfiguration();
app.UseHttpsRedirection();

app.MapGet("/provider", async (
	MinimalContextDb context) =>

	await context.Providers.ToListAsync())
	.WithName("GetProviders")
	.WithTags("Provider");


app.MapGet("/provider/{id}", async (
	Guid id,
	MinimalContextDb context) =>

	await context.Providers.FindAsync(id)
		is Provider provider
			? Results.Ok(provider)
			: Results.NotFound())
	.Produces<Provider>(StatusCodes.Status200OK)
	.Produces(StatusCodes.Status404NotFound)
	.WithName("GetProviderById")
	.WithTags("Provider");


app.MapPost("/provider", async (
	MinimalContextDb context,
	Provider provider) =>
{
	if (!MiniValidator.TryValidate(provider, out var errors))
		return Results.ValidationProblem(errors);
	
	context.Providers.Add(provider);
	var result = await context.SaveChangesAsync();

	return result > 0
		   ? Results.CreatedAtRoute("GetProviderById", new { id = provider.Id }, provider)
		   : Results.BadRequest("An error occurs while saving the record");

}).ProducesValidationProblem()
	.Produces<Provider>(StatusCodes.Status201Created)
	.Produces(StatusCodes.Status400BadRequest)
	.WithName("PostProvider")
	.WithTags("Provider");


app.MapPut("/provider/{id}", async (
		Guid id,
		MinimalContextDb context,
		Provider provider) =>
{
	var dataProvider = await context.Providers.AsNoTracking<Provider>()
											  .FirstOrDefaultAsync(p => p.Id == id);

	if (dataProvider == null) return Results.NotFound();

	if (!MiniValidator.TryValidate(provider, out var errors))
		return Results.ValidationProblem(errors);

	context.Providers.Update(provider);
	var result = await context.SaveChangesAsync();

	return result > 0
		? Results.NoContent()
		: Results.BadRequest("An error occurs while saving the record");

}).ProducesValidationProblem()
	.Produces(StatusCodes.Status204NoContent)
	.Produces(StatusCodes.Status400BadRequest)
	.WithName("PutProvider")
	.WithTags("Provider");


app.MapDelete("/provider/{id}", async (
	   Guid id,
	   MinimalContextDb context) =>
{
	var provider = await context.Providers.FindAsync(id);
	if (provider == null) return Results.NotFound();

	context.Providers.Remove(provider);
	var result = await context.SaveChangesAsync();

	return result > 0
		? Results.NoContent()
		: Results.BadRequest("An error occurs while saving the record");

}).Produces(StatusCodes.Status400BadRequest)
   .Produces(StatusCodes.Status204NoContent)
   .Produces(StatusCodes.Status404NotFound)
   .RequireAuthorization("DeleteProvider")
   .WithName("DeleteProvider")
   .WithTags("Provider");

app.Run();
