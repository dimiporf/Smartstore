using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Migrations;
using FluentMigrator;
using Smartstore.Core.Data.Migrations;


namespace MyOrg.HelloWorld.Migrations
{
    [MigrationVersion("2024-02-02 13:19:22", "HelloWorld: Initial")]
    public class _20240202131922_Initial : FluentMigrator.Migration
    {
        public override void Up()
        {
            // The table name is taken from Domain->Attribute->Table
            var tableName = "Notification";

            if (!Schema.Table(tableName).Exists())
            {
                Create.Table(tableName)
                    .WithIdColumn() // Adds the Id property as the primary key.
                    .WithColumn(nameof(Notification.AuthorId))
                        .AsInt32()
                        .NotNullable()
                        .Indexed("IX_Notification_AuthorId")
                    .WithColumn(nameof(Notification.Published))
                        .AsDateTime2()
                        .NotNullable()
                        .Indexed("IX_Notification_Published")
                    .WithColumn(nameof(Notification.Message))
                        .AsMaxString()
                        .NotNullable();
            }
        }

        public override void Down()
        {
            // Ignore this for now.
        }
    }
}
