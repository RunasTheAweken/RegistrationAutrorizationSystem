using DTO;
using Microsoft.EntityFrameworkCore;

namespace Data{
    public class MyDbContext : DbContext{
        public MyDbContext(DbContextOptions options) : base(options) {}
        public DbSet<User> Users {get;set;}

    }

}