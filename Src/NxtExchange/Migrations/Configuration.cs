using NxtExchange.DAL;

namespace NxtExchange.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<NxtExchange.DAL.NxtContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(NxtExchange.DAL.NxtContext context)
        {
            context.Blocks.Add(new Block
            {
                NxtBlockId = 2680262203532249785,
                Height = 0,
                Timestamp = new DateTime(2013, 11, 24, 12, 0, 0)
            });
        }
    }
}
