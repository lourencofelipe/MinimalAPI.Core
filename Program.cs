using Microsoft.EntityFrameworkCore;
using MinimalAPI.Core.Data;
using MinimalAPI.Core.Models;
using MiniValidation;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<MinimalContextDb>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

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
		   : Results.BadRequest("An error occurs while saving the record.");
})
	.Produces<Provider>(StatusCodes.Status201Created)
	.Produces(StatusCodes.Status400BadRequest)
	.WithName("PostProvider")
	.WithTags("Provider");

app.Run();
