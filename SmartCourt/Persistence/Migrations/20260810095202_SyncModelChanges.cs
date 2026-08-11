using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartCourt.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SyncModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Some development databases received these schema changes from an
            // earlier branch migration. Guard every operation so EF can safely
            // reconcile their migration history without rebuilding the database.
            migrationBuilder.Sql(
                """
                IF EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE [name] = N'IX_Proposals_LegalCaseId'
                      AND [object_id] = OBJECT_ID(N'[dbo].[Proposals]'))
                    DROP INDEX [IX_Proposals_LegalCaseId] ON [dbo].[Proposals];

                IF EXISTS (
                    SELECT 1 FROM sys.check_constraints
                    WHERE [name] = N'CK_Proposals_Status_Range'
                      AND [parent_object_id] = OBJECT_ID(N'[dbo].[Proposals]'))
                    ALTER TABLE [dbo].[Proposals]
                        DROP CONSTRAINT [CK_Proposals_Status_Range];

                IF EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE [name] = N'IX_Contracts_LegalCaseId'
                      AND [object_id] = OBJECT_ID(N'[dbo].[Contracts]'))
                    DROP INDEX [IX_Contracts_LegalCaseId] ON [dbo].[Contracts];

                IF COL_LENGTH(N'dbo.Proposals', N'ClosedAt') IS NULL
                    ALTER TABLE [dbo].[Proposals] ADD [ClosedAt] datetime2 NULL;

                IF COL_LENGTH(N'dbo.Proposals', N'ClosedByUserId') IS NULL
                    ALTER TABLE [dbo].[Proposals]
                        ADD [ClosedByUserId] uniqueidentifier NULL;

                IF COL_LENGTH(N'dbo.Proposals', N'ExpiresAt') IS NULL
                    ALTER TABLE [dbo].[Proposals]
                        ADD [ExpiresAt] datetime2 NOT NULL
                        CONSTRAINT [DF_Proposals_ExpiresAt]
                        DEFAULT '0001-01-01T00:00:00.0000000';

                IF COL_LENGTH(N'dbo.Cases', N'LawyerId') IS NULL
                    ALTER TABLE [dbo].[Cases] ADD [LawyerId] uniqueidentifier NULL;

                IF NOT EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE [name] = N'IX_Proposals_ClosedByUserId'
                      AND [object_id] = OBJECT_ID(N'[dbo].[Proposals]'))
                    CREATE INDEX [IX_Proposals_ClosedByUserId]
                        ON [dbo].[Proposals] ([ClosedByUserId]);

                IF NOT EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE [name] = N'IX_Proposals_Status_ExpiresAt'
                      AND [object_id] = OBJECT_ID(N'[dbo].[Proposals]'))
                    CREATE INDEX [IX_Proposals_Status_ExpiresAt]
                        ON [dbo].[Proposals] ([Status], [ExpiresAt]);

                IF NOT EXISTS (
                    SELECT 1 FROM sys.check_constraints
                    WHERE [name] = N'CK_Proposals_Status_Range'
                      AND [parent_object_id] = OBJECT_ID(N'[dbo].[Proposals]'))
                    ALTER TABLE [dbo].[Proposals]
                        ADD CONSTRAINT [CK_Proposals_Status_Range]
                        CHECK ([Status] BETWEEN 0 AND 6);

                IF NOT EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE [name] = N'UX_Contracts_ActiveCase'
                      AND [object_id] = OBJECT_ID(N'[dbo].[Contracts]'))
                    CREATE UNIQUE INDEX [UX_Contracts_ActiveCase]
                        ON [dbo].[Contracts] ([LegalCaseId])
                        WHERE [Status] = 1;

                IF NOT EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE [name] = N'IX_Cases_LawyerId'
                      AND [object_id] = OBJECT_ID(N'[dbo].[Cases]'))
                    CREATE INDEX [IX_Cases_LawyerId]
                        ON [dbo].[Cases] ([LawyerId]);

                IF NOT EXISTS (
                    SELECT 1 FROM sys.foreign_keys
                    WHERE [name] = N'FK_Cases_LawyerProfile_LawyerId')
                    ALTER TABLE [dbo].[Cases]
                        ADD CONSTRAINT [FK_Cases_LawyerProfile_LawyerId]
                        FOREIGN KEY ([LawyerId]) REFERENCES [dbo].[LawyerProfile] ([UserId])
                        ON DELETE NO ACTION;

                IF NOT EXISTS (
                    SELECT 1 FROM sys.foreign_keys
                    WHERE [name] = N'FK_Proposals_AspNetUsers_ClosedByUserId')
                    ALTER TABLE [dbo].[Proposals]
                        ADD CONSTRAINT [FK_Proposals_AspNetUsers_ClosedByUserId]
                        FOREIGN KEY ([ClosedByUserId]) REFERENCES [dbo].[AspNetUsers] ([Id])
                        ON DELETE NO ACTION;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cases_LawyerProfile_LawyerId",
                table: "Cases");

            migrationBuilder.DropForeignKey(
                name: "FK_Proposals_AspNetUsers_ClosedByUserId",
                table: "Proposals");

            migrationBuilder.DropIndex(
                name: "IX_Proposals_ClosedByUserId",
                table: "Proposals");

            migrationBuilder.DropIndex(
                name: "IX_Proposals_Status_ExpiresAt",
                table: "Proposals");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Proposals_Status_Range",
                table: "Proposals");

            migrationBuilder.DropIndex(
                name: "UX_Contracts_ActiveCase",
                table: "Contracts");

            migrationBuilder.DropIndex(
                name: "IX_Cases_LawyerId",
                table: "Cases");

            migrationBuilder.DropColumn(
                name: "ClosedAt",
                table: "Proposals");

            migrationBuilder.DropColumn(
                name: "ClosedByUserId",
                table: "Proposals");

            migrationBuilder.DropColumn(
                name: "ExpiresAt",
                table: "Proposals");

            migrationBuilder.DropColumn(
                name: "LawyerId",
                table: "Cases");

            migrationBuilder.CreateIndex(
                name: "IX_Proposals_LegalCaseId",
                table: "Proposals",
                column: "LegalCaseId",
                unique: true,
                filter: "[Status] = 1");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Proposals_Status_Range",
                table: "Proposals",
                sql: "[Status] BETWEEN 0 AND 2");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_LegalCaseId",
                table: "Contracts",
                column: "LegalCaseId");
        }
    }
}
