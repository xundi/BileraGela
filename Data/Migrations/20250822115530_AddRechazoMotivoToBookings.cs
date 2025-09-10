using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Reservas.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRechazoMotivoToBookings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
    SET @hasCol := (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE()
        AND TABLE_NAME='Bookings'
        AND COLUMN_NAME='RechazoMotivo'
    );
    SET @sql := IF(@hasCol = 0,
      'ALTER TABLE `Bookings` ADD COLUMN `RechazoMotivo` varchar(500) NULL;',
      'SELECT 1;');
    PREPARE b1 FROM @sql; EXECUTE b1; DEALLOCATE PREPARE b1;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
    SET @hasCol := (
      SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE()
        AND TABLE_NAME='Bookings'
        AND COLUMN_NAME='RechazoMotivo'
    );
    SET @sql := IF(@hasCol = 1,
      'ALTER TABLE `Bookings` DROP COLUMN `RechazoMotivo`;',
      'SELECT 1;');
    PREPARE b2 FROM @sql; EXECUTE b2; DEALLOCATE PREPARE b2;
");
        }
    }
}
