using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CatalogApi.Migrations
{
    /// <summary>
    /// Creates stored procedures for reading Products data.
    /// </summary>
    public partial class AddCatalogStoredProcedures : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── sp_GetAllProducts ────────────────────────────────────────────
            migrationBuilder.Sql(@"
CREATE PROCEDURE [dbo].[sp_GetAllProducts]
    @Category NVARCHAR(200) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT  p.[Id], p.[Name], p.[Description], p.[Price],
            p.[Category], p.[ImageUrl], p.[StockQuantity]
    FROM    [dbo].[Products] p
    WHERE   (@Category IS NULL OR p.[Category] = @Category)
    ORDER BY p.[Name];
END;
");

            // ── sp_GetProductById ────────────────────────────────────────────
            migrationBuilder.Sql(@"
CREATE PROCEDURE [dbo].[sp_GetProductById]
    @ProductId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT  p.[Id], p.[Name], p.[Description], p.[Price],
            p.[Category], p.[ImageUrl], p.[StockQuantity]
    FROM    [dbo].[Products] p
    WHERE   p.[Id] = @ProductId;
END;
");

            // ── sp_GetProductsByCategory ─────────────────────────────────────
            migrationBuilder.Sql(@"
CREATE PROCEDURE [dbo].[sp_GetProductsByCategory]
    @Category NVARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT  p.[Id], p.[Name], p.[Description], p.[Price],
            p.[Category], p.[ImageUrl], p.[StockQuantity]
    FROM    [dbo].[Products] p
    WHERE   p.[Category] = @Category
    ORDER BY p.[Name];
END;
");

            // ── sp_SearchProducts ────────────────────────────────────────────
            migrationBuilder.Sql(@"
CREATE PROCEDURE [dbo].[sp_SearchProducts]
    @SearchTerm NVARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT  p.[Id], p.[Name], p.[Description], p.[Price],
            p.[Category], p.[ImageUrl], p.[StockQuantity]
    FROM    [dbo].[Products] p
    WHERE   p.[Name] LIKE '%' + @SearchTerm + '%'
       OR   p.[Description] LIKE '%' + @SearchTerm + '%'
    ORDER BY p.[Name];
END;
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS [dbo].[sp_GetAllProducts];");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS [dbo].[sp_GetProductById];");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS [dbo].[sp_GetProductsByCategory];");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS [dbo].[sp_SearchProducts];");
        }
    }
}
