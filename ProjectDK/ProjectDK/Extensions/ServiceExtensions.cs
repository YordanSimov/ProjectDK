﻿using ProjectDK.BL.Interfaces;
using ProjectDK.BL.Services;
using ProjectDK.DL.Interfaces;
using ProjectDK.DL.Repositories.InMemoryRepositories;

namespace ProjectDK.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection RegisterRepositories(this IServiceCollection services)
        {
            services.AddSingleton<IPersonRepository, PersonInMemoryRepository>();
            services.AddSingleton<IAuthorRepository, AuthorInMemoryRepository>();
            services.AddSingleton<IBookRepository, BookInMemoryRepository>();
            return services;
        }
        public static IServiceCollection RegisterServices(this IServiceCollection services)
        {
            services.AddSingleton<IPersonService, PersonService>();
            services.AddSingleton<IAuthorService, AuthorService>();
            services.AddSingleton<IBookService, BookService>();
            return services;
        }
    }
}