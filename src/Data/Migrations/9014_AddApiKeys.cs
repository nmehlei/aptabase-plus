using System.Data;
using FluentMigrator;

namespace Aptabase.Data.Migrations;

// Fork-local migration. Numbered in the 9xxx range so it does not collide with
// upstream's next migration number when this branch is rebased on aptabase/main.
[Migration(9014)]
public class AddApiKeys : Migration
{
    public override void Up()
    {
        Create.Table("api_keys")
            .WithNanoIdColumn("id").PrimaryKey()
            .WithColumn("user_id").AsString(22).NotNullable()
            .WithColumn("name").AsString(100).NotNullable()
            .WithColumn("key_hash").AsString(64).NotNullable().Unique()
            .WithColumn("key_prefix").AsString(12).NotNullable()
            .WithColumn("last_used_at").AsDateTimeOffset().Nullable()
            .WithColumn("expires_at").AsDateTimeOffset().Nullable()
            .WithTimestamps();

        // ON DELETE CASCADE so account deletion can't be blocked by a stray key
        // (belt and braces alongside the explicit DELETE in AuthService).
        Create.ForeignKey("fk_api_keys_user_id")
            .FromTable("api_keys").ForeignColumn("user_id")
            .ToTable("users").PrimaryColumn("id")
            .OnDelete(Rule.Cascade);

        Create.Index("ix_api_keys_user_id")
            .OnTable("api_keys")
            .OnColumn("user_id");
    }

    public override void Down()
    {
        Delete.Table("api_keys");
    }
}
