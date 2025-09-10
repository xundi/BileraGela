using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Reservas.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
    SET @hasEmail := (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE()
        AND TABLE_NAME='Users'
        AND COLUMN_NAME='Email'
    );
    SET @sql := IF(@hasEmail = 0,
      'ALTER TABLE `Users` ADD COLUMN `Email` varchar(255) NULL;',
      'SELECT 1;');
    PREPARE u1 FROM @sql; EXECUTE u1; DEALLOCATE PREPARE u1;
");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
    SET @hasEmail := (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE()
        AND TABLE_NAME='Users'
        AND COLUMN_NAME='Email'
    );
    SET @sql := IF(@hasEmail = 1,
      'ALTER TABLE `Users` DROP COLUMN `Email`;',
      'SELECT 1;');
    PREPARE d1 FROM @sql; EXECUTE d1; DEALLOCATE PREPARE d1;
");

        }
    }
}
