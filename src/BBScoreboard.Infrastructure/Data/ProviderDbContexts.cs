using Microsoft.EntityFrameworkCore;

namespace BBScoreboard.Infrastructure.Data;

public sealed class PostgresBbScoreboardDbContext(DbContextOptions<PostgresBbScoreboardDbContext> options)
    : BBScoreboardDbContext(options);

public sealed class SqlServerBbScoreboardDbContext(DbContextOptions<SqlServerBbScoreboardDbContext> options)
    : BBScoreboardDbContext(options);
