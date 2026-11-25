using System.Data;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.PAS.Data.Models;

namespace SFA.DAS.PAS.Data.Repositories;

public interface IUserRepository
{
    Task SyncUsers(long ukprn, List<UserModel> users, CancellationToken cancellationToken);
}

[ExcludeFromCodeCoverage]
internal class UserRepository(IPasDataContext _pasDataContext) : IUserRepository
{
    public async Task SyncUsers(long ukprn, List<UserModel> users, CancellationToken cancellationToken)
    {
        var userTable = new DataTable();
        userTable.Columns.Add("Email");
        userTable.Columns.Add("UserType");

        foreach (UserModel user in users)
        {
            userTable.Rows.Add(user.Email, (short)user.UserType);
        }

        var parameters = new[]
        {
            new SqlParameter("@users", SqlDbType.Structured)
            {
                TypeName = "dbo.IdamsUsers",
                Value = userTable,
            },
            new SqlParameter("@ukprn", SqlDbType.BigInt) { Value = ukprn },
        };

        await _pasDataContext.Database.ExecuteSqlRawAsync(
            "EXEC [dbo].[SyncIdamsUsers] @ukprn, @users",
            parameters,
            cancellationToken);
    }
}
