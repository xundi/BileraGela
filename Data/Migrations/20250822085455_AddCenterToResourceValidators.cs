using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Reservas.Data.Migrations
{
    public partial class AddCenterToResourceValidators : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) Hacer ResourceId nullable (para permitir validar por centro)
            migrationBuilder.AlterColumn<int>(
                name: "ResourceId",
                table: "ResourceValidators",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");


            // 2) NUEVA columna CenterId (nullable)
           // migrationBuilder.AddColumn<int>(
             //   name: "CenterId",
               // table: "ResourceValidators",
                //type: "int",
                //nullable: true);

            // 3) Índice para CenterId
            migrationBuilder.CreateIndex(
                name: "IX_ResourceValidators_CenterId",
                table: "ResourceValidators",
                column: "CenterId");

            // 4) Índice compuesto (evita duplicados lógicos)
            migrationBuilder.CreateIndex(
                name: "IX_ResourceValidators_UserId_CenterId_ResourceId",
                table: "ResourceValidators",
                columns: new[] { "UserId", "CenterId", "ResourceId" },
                unique: true);

            // 5) Check: al menos CenterId o ResourceId debe estar informado
            migrationBuilder.AddCheckConstraint(
                name: "CK_ResourceValidator_Target",
                table: "ResourceValidators",
                sql: "(CenterId IS NOT NULL) OR (ResourceId IS NOT NULL)");

            // 6) FK a Centers (RESTRICT)
            migrationBuilder.AddForeignKey(
                name: "FK_ResourceValidators_Centers_CenterId",
                table: "ResourceValidators",
                column: "CenterId",
                principalTable: "Centers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);


            // Volvemos a crear (si fuese necesario) la FK a Resources con RESTRICT.
            // Si tu FK actual ya existe con Cascade, puedes soltarla y volverla a crear:
            //migrationBuilder.DropForeignKey(
              //  name: "FK_ResourceValidators_Resources_ResourceId",
                //table: "ResourceValidators");

            migrationBuilder.AddForeignKey(
                name: "FK_ResourceValidators_Resources_ResourceId",
                table: "ResourceValidators",
                column: "ResourceId",
                principalTable: "Resources",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // ⚠️ Observa que NO tocamos índices/clave foránea de UserId
            //     (no DropIndex "IX_ResourceValidators_UserId", no UserId1)
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revertir cambios
            migrationBuilder.DropForeignKey(
                name: "FK_ResourceValidators_Centers_CenterId",
                table: "ResourceValidators");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ResourceValidator_Target",
                table: "ResourceValidators");

            migrationBuilder.DropIndex(
                name: "IX_ResourceValidators_UserId_CenterId_ResourceId",
                table: "ResourceValidators");

            migrationBuilder.DropIndex(
                name: "IX_ResourceValidators_CenterId",
                table: "ResourceValidators");

            migrationBuilder.DropColumn(
                name: "CenterId",
                table: "ResourceValidators");

            migrationBuilder.AlterColumn<int>(
                name: "ResourceId",
                table: "ResourceValidators",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            // Re-crear FK original a Resources con CASCADE si así estaba antes
            migrationBuilder.AddForeignKey(
                name: "FK_ResourceValidators_Resources_ResourceId",
                table: "ResourceValidators",
                column: "ResourceId",
                principalTable: "Resources",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
