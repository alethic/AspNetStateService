﻿using Cogito.Extensions.Options.ConfigurationExtensions.Autofac;

namespace AspNetStateService.Azure.Cosmos.Table
{

    [RegisterOptions("AspNetStateService.Azure.Cosmos.Table")]
    public class StateObjectTableDataStoreOptions
    {

        /// <summary>
        /// Gets or sets the connection string to use for the state storage account.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the name of the table to use for state storage.
        /// </summary>
        public string TableName { get; set; }

    }

}
