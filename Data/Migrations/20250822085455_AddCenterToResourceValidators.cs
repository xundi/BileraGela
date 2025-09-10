using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Reservas.Data.Migrations
{
    public partial class AddCenterToResourceValidators : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 0) ResourceId -> NULLABLE
            migrationBuilder.AlterColumn<int>(
                name: "ResourceId",
                table: "ResourceValidators",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            // 1) CenterId (solo si no existe)
            migrationBuilder.Sql(@"
                SET @hasCenter := (
                  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
                  WHERE TABLE_SCHEMA = DATABASE()
                    AND TABLE_NAME='ResourceValidators'
                    AND COLUMN_NAME='CenterId'
                );
                SET @sql := IF(@hasCenter = 0,
                  'ALTER TABLE `ResourceValidators` ADD COLUMN `CenterId` int NULL;',
                  'SELECT 1;');
                PREPARE s1 FROM @sql; EXECUTE s1; DEALLOCATE PREPARE s1;
            ");

            // 2) Índice por CenterId (solo si no existe)
            migrationBuilder.Sql(@"
                SET @hasIdx := (
                  SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS
                  WHERE TABLE_SCHEMA = DATABASE()
                    AND TABLE_NAME='ResourceValidators'
                    AND INDEX_NAME='IX_ResourceValidators_CenterId'
                );
                SET @sql := IF(@hasIdx = 0,
                  'CREATE INDEX `IX_ResourceValidators_CenterId` ON `ResourceValidators` (`CenterId`);',
                  'SELECT 1;');
                PREPARE s2 FROM @sql; EXECUTE s2; DEALLOCATE PREPARE s2;
            ");

            // 3) Índice único (UserId,CenterId,ResourceId) (solo si no existe)
            migrationBuilder.Sql(@"
                SET @hasIdx2 := (
                  SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS
                  WHERE TABLE_SCHEMA = DATABASE()
                    AND TABLE_NAME='ResourceValidators'
                    AND INDEX_NAME='IX_ResourceValidators_UserId_CenterId_ResourceId'
                );
                SET @sql := IF(@hasIdx2 = 0,
                  'CREATE UNIQUE INDEX `IX_ResourceValidators_UserId_CenterId_ResourceId` ON `ResourceValidators` (`UserId`,`CenterId`,`ResourceId`);',
                  'SELECT 1;');
                PREPARE s3 FROM @sql; EXECUTE s3; DEALLOCATE PREPARE s3;
            ");

            // 4) CHECK (MySQL 8+) (solo si no existe)
            migrationBuilder.Sql(@"
                SET @hasChk := (
                  SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
                  WHERE TABLE_SCHEMA = DATABASE()
                    AND TABLE_NAME='ResourceValidators'
                    AND CONSTRAINT_TYPE='CHECK'
                    AND CONSTRAINT_NAME='CK_ResourceValidator_Target'
                );
                SET @sql := IF(@hasChk = 0,
                  'ALTER TABLE `ResourceValidators` ADD CONSTRAINT `CK_ResourceValidator_Target` CHECK ((CenterId IS NOT NULL) OR (ResourceId IS NOT NULL));',
                  'SELECT 1;');
                PREPARE s4 FROM @sql; EXECUTE s4; DEALLOCATE PREPARE s4;
            ");

            // 5) FK a Centers (solo si no existe)
            migrationBuilder.Sql(@"
                SET @fkName := 'FK_ResourceValidators_Centers_CenterId';
                SET @hasFk := (
                  SELECT COUNT(*) FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
                  WHERE TABLE_SCHEMA = DATABASE()
                    AND TABLE_NAME='ResourceValidators'
                    AND CONSTRAINT_NAME=@fkName
                );
                SET @sql := IF(@hasFk = 0,
                  'ALTER TABLE `ResourceValidators`
                     ADD CONSTRAINT `FK_ResourceValidators_Centers_CenterId`
                     FOREIGN KEY (`CenterId`) REFERENCES `Centers`(`Id`)
                     ON DELETE RESTRICT;',
                  'SELECT 1;');
                PREPARE s5 FROM @sql; EXECUTE s5; DEALLOCATE PREPARE s5;
            ");

            // 6) FK a Resources (solo si no existe)  ← evita el "Duplicate foreign key"
            migrationBuilder.Sql(@"
                SET @fkName := 'FK_ResourceValidators_Resources_ResourceId';
                SET @hasFk := (
                  SELECT COUNT(*) FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
                  WHERE TABLE_SCHEMA = DATABASE()
                    AND TABLE_NAME='ResourceValidators'
                    AND CONSTRAINT_NAME=@fkName
                );
                SET @sql := IF(@hasFk = 0,
                  'ALTER TABLE `ResourceValidators`
                     ADD CONSTRAINT `FK_ResourceValidators_Resources_ResourceId`
                     FOREIGN KEY (`ResourceId`) REFERENCES `Resources`(`Id`)
                     ON DELETE RESTRICT;',
                  'SELECT 1;');
                PREPARE s6 FROM @sql; EXECUTE s6; DEALLOCATE PREPARE s6;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // FKs (solo si existen)
            migrationBuilder.Sql(@"
                SET @fkName := 'FK_ResourceValidators_Resources_ResourceId';
                SET @hasFk := (
                  SELECT COUNT(*) FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
                  WHERE TABLE_SCHEMA = DATABASE()
                    AND TABLE_NAME='ResourceValidators'
                    AND CONSTRAINT_NAME=@fkName
                );
                SET @sql := IF(@hasFk = 1,
                  'ALTER TABLE `ResourceValidators` DROP FOREIGN KEY `FK_ResourceValidators_Resources_ResourceId`;',
                  'SELECT 1;');
                PREPARE d1 FROM @sql; EXECUTE d1; DEALLOCATE PREPARE d1;

                SET @fkName := 'FK_ResourceValidators_Centers_CenterId';
                SET @hasFk := (
                  SELECT COUNT(*) FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
                  WHERE TABLE_SCHEMA = DATABASE()
                    AND TABLE_NAME='ResourceValidators'
                    AND CONSTRAINT_NAME=@fkName
                );
                SET @sql := IF(@hasFk = 1,
                  'ALTER TABLE `ResourceValidators` DROP FOREIGN KEY `FK_ResourceValidators_Centers_CenterId`;',
                  'SELECT 1;');
                PREPARE d2 FROM @sql; EXECUTE d2; DEALLOCATE PREPARE d2;
            ");

            // CHECK (solo si existe)
            migrationBuilder.Sql(@"
                SET @hasChk := (
                  SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
                  WHERE TABLE_SCHEMA = DATABASE()
                    AND TABLE_NAME='ResourceValidators'
                    AND CONSTRAINT_TYPE='CHECK'
                    AND CONSTRAINT_NAME='CK_ResourceValidator_Target'
                );
                SET @sql := IF(@hasChk = 1,
                  'ALTER TABLE `ResourceValidators` DROP CONSTRAINT `CK_ResourceValidator_Target`;',
                  'SELECT 1;');
                PREPARE d3 FROM @sql; EXECUTE d3; DEALLOCATE PREPARE d3;
            ");

            // Índices (solo si existen)
            migrationBuilder.Sql(@"
                SET @hasIdx := (
                  SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS
                  WHERE TABLE_SCHEMA = DATABASE()
                    AND TABLE_NAME='ResourceValidators'
                    AND INDEX_NAME='IX_ResourceValidators_CenterId'
                );
                SET @sql := IF(@hasIdx = 1,
                  'DROP INDEX `IX_ResourceValidators_CenterId` ON `ResourceValidators`;',
                  'SELECT 1;');
                PREPARE d4 FROM @sql; EXECUTE d4; DEALLOCATE PREPARE d4;

                SET @hasIdx2 := (
                  SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS
                  WHERE TABLE_SCHEMA = DATABASE()
                    AND TABLE_NAME='ResourceValidators'
                    AND INDEX_NAME='IX_ResourceValidators_UserId_CenterId_ResourceId'
                );
                SET @sql := IF(@hasIdx2 = 1,
                  'DROP INDEX `IX_ResourceValidators_UserId_CenterId_ResourceId` ON `ResourceValidators`;',
                  'SELECT 1;');
                PREPARE d5 FROM @sql; EXECUTE d5; DEALLOCATE PREPARE d5;
            ");

            // CenterId (solo si existe)
            migrationBuilder.Sql(@"
                SET @hasCenter := (
                  SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
                  WHERE TABLE_SCHEMA = DATABASE()
                    AND TABLE_NAME='ResourceValidators'
                    AND COLUMN_NAME='CenterId'
                );
                SET @sql := IF(@hasCenter = 1,
                  'ALTER TABLE `ResourceValidators` DROP COLUMN `CenterId`;',
                  'SELECT 1;');
                PREPARE d6 FROM @sql; EXECUTE d6; DEALLOCATE PREPARE d6;
            ");

            // ResourceId -> NOT NULL
            migrationBuilder.AlterColumn<int>(
                name: "ResourceId",
                table: "ResourceValidators",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
