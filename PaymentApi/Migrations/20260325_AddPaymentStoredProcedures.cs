using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaymentApi.Migrations
{
    /// <summary>
    /// Creates stored procedures for reading Payments data.
    /// </summary>
    public partial class AddPaymentStoredProcedures : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── sp_GetAllPayments ────────────────────────────────────────────
            migrationBuilder.Sql(@"
CREATE PROCEDURE [dbo].[sp_GetAllPayments]
    @Status      NVARCHAR(50)  = NULL,
    @Page        INT           = 1,
    @PageSize    INT           = 20
AS
BEGIN
    SET NOCOUNT ON;

    SELECT  p.[Id], p.[OrderId], p.[Amount], p.[Currency],
            p.[Status], p.[Method], p.[TransactionId],
            p.[FailureReason], p.[RefundTransactionId],
            p.[RefundedAt], p.[CreatedAt], p.[UpdatedAt]
    FROM    [dbo].[Payments] p
    WHERE   (@Status IS NULL OR p.[Status] = @Status)
    ORDER BY p.[CreatedAt] DESC
    OFFSET  ((@Page - 1) * @PageSize) ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END;
");

            // ── sp_GetPaymentById ────────────────────────────────────────────
            migrationBuilder.Sql(@"
CREATE PROCEDURE [dbo].[sp_GetPaymentById]
    @PaymentId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT  p.[Id], p.[OrderId], p.[Amount], p.[Currency],
            p.[Status], p.[Method], p.[TransactionId],
            p.[FailureReason], p.[RefundTransactionId],
            p.[RefundedAt], p.[CreatedAt], p.[UpdatedAt]
    FROM    [dbo].[Payments] p
    WHERE   p.[Id] = @PaymentId;
END;
");

            // ── sp_GetPaymentsByOrderId ──────────────────────────────────────
            migrationBuilder.Sql(@"
CREATE PROCEDURE [dbo].[sp_GetPaymentsByOrderId]
    @OrderId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT  p.[Id], p.[OrderId], p.[Amount], p.[Currency],
            p.[Status], p.[Method], p.[TransactionId],
            p.[FailureReason], p.[RefundTransactionId],
            p.[RefundedAt], p.[CreatedAt], p.[UpdatedAt]
    FROM    [dbo].[Payments] p
    WHERE   p.[OrderId] = @OrderId
    ORDER BY p.[CreatedAt] DESC;
END;
");

            // ── sp_GetPaymentsByStatus ───────────────────────────────────────
            migrationBuilder.Sql(@"
CREATE PROCEDURE [dbo].[sp_GetPaymentsByStatus]
    @Status NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT  p.[Id], p.[OrderId], p.[Amount], p.[Currency],
            p.[Status], p.[Method], p.[TransactionId],
            p.[FailureReason], p.[RefundTransactionId],
            p.[RefundedAt], p.[CreatedAt], p.[UpdatedAt]
    FROM    [dbo].[Payments] p
    WHERE   p.[Status] = @Status
    ORDER BY p.[CreatedAt] DESC;
END;
");

            // ── sp_GetPaymentCount ───────────────────────────────────────────
            migrationBuilder.Sql(@"
CREATE PROCEDURE [dbo].[sp_GetPaymentCount]
    @Status NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT  COUNT(*) AS [TotalCount]
    FROM    [dbo].[Payments] p
    WHERE   (@Status IS NULL OR p.[Status] = @Status);
END;
");

            // ── sp_CheckCompletedPaymentExists ───────────────────────────────
            migrationBuilder.Sql(@"
CREATE PROCEDURE [dbo].[sp_CheckCompletedPaymentExists]
    @OrderId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT  CASE WHEN EXISTS (
                SELECT 1 FROM [dbo].[Payments]
                WHERE [OrderId] = @OrderId AND [Status] = 'Completed'
            ) THEN 1 ELSE 0 END AS [Exists];
END;
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS [dbo].[sp_GetAllPayments];");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS [dbo].[sp_GetPaymentById];");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS [dbo].[sp_GetPaymentsByOrderId];");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS [dbo].[sp_GetPaymentsByStatus];");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS [dbo].[sp_GetPaymentCount];");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS [dbo].[sp_CheckCompletedPaymentExists];");
        }
    }
}
