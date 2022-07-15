﻿using Microsoft.EntityFrameworkCore;
using MinimalAPI.Core.Models;

namespace MinimalAPI.Core.Data
{
	public class MinimalContextDb : DbContext
	{
		public MinimalContextDb(DbContextOptions<MinimalContextDb> options) : base(options) { }
		
		public DbSet<Provider> Providers { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder) 
		{
		    modelBuilder.Entity<Provider>()
				.HasKey(p => p.Id);

			modelBuilder.Entity<Provider>()
				.Property(p => p.Name)
				.IsRequired()
				.HasColumnType("varchar(200)");

			modelBuilder.Entity<Provider>()
				.Property(p => p.Document)
				.IsRequired()
				.HasColumnType("varchar(14)");

			modelBuilder.Entity<Provider>()
				.ToTable("Providers");

			base.OnModelCreating(modelBuilder);
		}
	}
}
