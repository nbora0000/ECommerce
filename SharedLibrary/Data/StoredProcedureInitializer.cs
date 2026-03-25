using Microsoft.EntityFrameworkCore;

namespace SharedLibrary.Data
{
    /// <summary>
    /// Executes raw SQL scripts against the database after migrations.
    /// Uses CREATE OR ALTER so calls are idempotent (safe to re-run on every startup).
    /// </summary>
    public static class StoredProcedureInitializer
    {
        public static void ExecuteSql(DbContext context, params string[] sqlStatements)
        {
            foreach (var sql in sqlStatements)
            {
                context.Database.ExecuteSqlRaw(sql);
            }
        }
    }
}
