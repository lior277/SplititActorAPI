﻿using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public DbSet<ActorModel> Actors { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
}
