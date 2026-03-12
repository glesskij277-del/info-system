using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace HospitalIS.Web.Infrastructure;

public static class DbExceptionHelper
{
    public static bool IsUniqueViolation(DbUpdateException exception)
    {
        return exception.InnerException is PostgresException postgresException
            && postgresException.SqlState == PostgresErrorCodes.UniqueViolation;
    }

    public static bool IsForeignKeyViolation(DbUpdateException exception)
    {
        return exception.InnerException is PostgresException postgresException
            && postgresException.SqlState == PostgresErrorCodes.ForeignKeyViolation;
    }
}

