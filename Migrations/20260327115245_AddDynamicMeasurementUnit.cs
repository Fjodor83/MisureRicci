using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MisureRicci.Migrations
{
    /// <inheritdoc />
    public partial class AddDynamicMeasurementUnit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF COL_LENGTH(N'dbo.DynamicMeasurementRecords', N'MeasurementUnit') IS NULL
                BEGIN
                    ALTER TABLE [dbo].[DynamicMeasurementRecords]
                    ADD [MeasurementUnit] int NOT NULL CONSTRAINT [DF_DynamicMeasurementRecords_MeasurementUnit] DEFAULT 0;
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF COL_LENGTH(N'dbo.DynamicMeasurementRecords', N'MeasurementUnit') IS NOT NULL
                BEGIN
                    IF EXISTS (
                        SELECT 1
                        FROM sys.default_constraints dc
                        INNER JOIN sys.columns c ON c.default_object_id = dc.object_id
                        INNER JOIN sys.tables t ON t.object_id = c.object_id
                        WHERE t.name = N'DynamicMeasurementRecords'
                          AND c.name = N'MeasurementUnit'
                    )
                    BEGIN
                        ALTER TABLE [dbo].[DynamicMeasurementRecords]
                        DROP CONSTRAINT [DF_DynamicMeasurementRecords_MeasurementUnit];
                    END

                    ALTER TABLE [dbo].[DynamicMeasurementRecords]
                    DROP COLUMN [MeasurementUnit];
                END
                """);
        }
    }
}
