﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Aiursoft.Pylon;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using SCE.Data;

namespace SCE
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args)
                .MigrateDbContext<ApplicationDbContext>(async (db, services) => await db.Seed(services))
                .Run();
        }

        public static IWebHost BuildWebHost(string[] args)
        {
            var host = WebHost.CreateDefaultBuilder(args)
                 .UseStartup<Startup>()
                 .Build();

            return host;
        }
    }
}
