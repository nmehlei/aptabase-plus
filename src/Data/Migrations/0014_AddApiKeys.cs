using FluentMigrator;

namespace Aptabase.Data.Migrations;

[Migration(0014)]
public class AddApiKeys : Migration
{
    public override void Up()
    {
        Create.Table("api_keys")
            .WithNanoIdColumn("id").PrimaryKey()
            .WithColumn("user_id").AsString(22).NotNullable().ForeignKey("users", "id")
            .WithColumn("name").AsString(100).NotNullable()
            .WithColumn("key_hash").AsString(64).NotNullable().Unique()
            .WithColumn("key_prefix").AsString(12).NotNullable()
            .WithColumn("last_used_at").AsDateTimeOffset().Nullable()
            .WithColumn("expires_at").AsDateTimeOffset().Nullable()
            .WithTimestamps();

        Create.Index("ix_api_keys_user_id")
            .OnTable("api_keys")
            .OnColumn("user_id");
    }

    public override void Down()
    {
        Delete.Table("api_keys");
    }
}
