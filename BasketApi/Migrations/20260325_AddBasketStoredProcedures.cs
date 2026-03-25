using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BasketApi.Migrations
{
    /// <summary>
    /// Creates stored procedures for reading Basket/ShoppingCart data.
    /// </summary>
    public partial class AddBasketStoredProcedures : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── sp_GetBasketByCustomerId ─────────────────────────────────────
            migrationBuilder.Sql(@"
CREATE PROCEDURE [dbo].[sp_GetBasketByCustomerId]
    @CustomerId NVARCHAR(450)
AS
BEGIN
    SET NOCOUNT ON;

    -- Return shopping cart
    SELECT  sc.[CustomerId]
    FROM    [dbo].[ShoppingCarts] sc
    WHERE   sc.[CustomerId] = @CustomerId;

    -- Return cart items
    SELECT  ci.[Id], ci.[ProductId], ci.[ProductName],
            ci.[UnitPrice], ci.[Quantity], ci.[ShoppingCartCustomerId]
    FROM    [dbo].[CartItems] ci
    WHERE   ci.[ShoppingCartCustomerId] = @CustomerId;
END;
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS [dbo].[sp_GetBasketByCustomerId];");
        }
    }
}
