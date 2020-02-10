﻿using System;
using System.Threading.Tasks;

using Cogito.Autofac;

namespace AspNetStateService.EntityFrameworkCore.InMemory
{

    [RegisterNamed(typeof(IStateObjectDbContextProvider), "InMemory")]
    [RegisterInstancePerLifetimeScope]
    public class InMemoryStateObjectDbContextProvider : IStateObjectDbContextProvider
    {

        readonly Func<InMemoryStateObjectDbContext> dbContextFactory;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="dbContextFactory"></param>
        public InMemoryStateObjectDbContextProvider(Func<InMemoryStateObjectDbContext> dbContextFactory)
        {
            this.dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        }

        public Task<StateObjectDbContext> GetDbContextAsync()
        {
            return Task.FromResult<StateObjectDbContext>(dbContextFactory());
        }

    }

}
