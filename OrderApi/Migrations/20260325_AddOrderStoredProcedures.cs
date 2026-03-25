using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderApi.Migrations
{
    /// <summary>
    /// Creates stored procedures for reading Orders data.
    /// </summary>
    public partial class AddOrderStoredProcedures : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── sp_GetAllOrders ──────────────────────────────────────────────
            migrationBuilder.Sql(@"
CREATE PROCEDURE [dbo].[sp_GetAllOrders]
    @Status      NVARCHAR(50)  = NULL,
    @Page        INT           = 1,
    @PageSize    INT           = 20
AS
BEGIN
    SET NOCOUNT ON;

    SELECT  o.[Id], o.[CustomerName], o.[CustomerEmail],
            o.[TotalAmount], o.[Currency], o.[Status],
            o.[Notes], o.[CreatedAt], o.[UpdatedAt]
    FROM    [dbo].[Orders] o
    WHERE   (@Status IS NULL OR o.[Status] = @Status)
    ORDER BY o.[CreatedAt] DESC
    OFFSET  ((@Page - 1) * @PageSize) ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END;
");

            // ── sp_GetOrderById ─────────────────────────────────────────────
            migrationBuilder.Sql(@"
CREATE PROCEDURE [dbo].[sp_GetOrderById]
    @OrderId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    -- Return order
    SELECT  o.[Id], o.[CustomerName], o.[CustomerEmail],
            o.[TotalAmount], o.[Currency], o.[Status],
            o.[Notes], o.[CreatedAt], o.[UpdatedAt]
    FROM    [dbo].[Orders] o
    WHERE   o.[Id] = @OrderId;

    -- Return order items
    SELECT  oi.[Id], oi.[OrderId], oi.[ProductId],
            oi.[ProductName], oi.[Quantity], oi.[UnitPrice]
    FROM    [dbo].[OrderItems] oi
    WHERE   oi.[OrderId] = @OrderId;
END;
");

            // ── sp_GetOrdersByStatus ────────────────────────────────────────
            migrationBuilder.Sql(@"
CREATE PROCEDURE [dbo].[sp_GetOrdersByStatus]
    @Status NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT  o.[Id], o.[CustomerName], o.[CustomerEmail],
            o.[TotalAmount], o.[Currency], o.[Status],
            o.[Notes], o.[CreatedAt], o.[UpdatedAt]
    FROM    [dbo].[Orders] o
    WHERE   o.[Status] = @Status
    ORDER BY o.[CreatedAt] DESC;
END;
");

            // ── sp_GetOrderCount ────────────────────────────────────────────
            migrationBuilder.Sql(@"
CREATE PROCEDURE [dbo].[sp_GetOrderCount]
    @Status NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT  COUNT(*) AS [TotalCount]
    FROM    [dbo].[Orders] o
    WHERE   (@Status IS NULL OR o.[Status] = @Status);
END;
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS [dbo].[sp_GetAllOrders];");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS [dbo].[sp_GetOrderById];");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS [dbo].[sp_GetOrdersByStatus];");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS [dbo].[sp_GetOrderCount];");
        }
    }
}
