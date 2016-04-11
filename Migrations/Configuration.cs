namespace Blog_jcf.Migrations
{
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using Models;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<Blog_jcf.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(Blog_jcf.Models.ApplicationDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //
            var roleManager = new RoleManager<IdentityRole>(
                    new RoleStore<IdentityRole>(context));

            if (!context.Roles.Any(r => r.Name == "Admin"))
            {
                roleManager.Create(new IdentityRole { Name = "Admin" });
            }

            var userManager = new UserManager<ApplicationUser>(
                    new UserStore<ApplicationUser>(context));

            if (!context.Users.Any(u => u.Email == "jcfrosty64@gmail.com"))
            {
                userManager.Create(new ApplicationUser
                {
                    UserName = "jcfrosty64@gmail.com",
                    Email = "jcfrosty64@gmail.com",
                    FirstName = "James",
                    LastName = "Frost",
                    DisplayName = "jcfrost",
                }, "jcf-blog2016");
            }
            var userId = userManager.FindByEmail("jcfrosty64@gmail.com").Id;
            userManager.AddToRole(userId, "Admin");
          

            if (!context.Roles.Any(r => r.Name == "Moderator"))
            {
                    roleManager.Create(new IdentityRole { Name = "Moderator" });
            }

            if (!context.Users.Any(u => u.Email == "moderator@coderfoundry.com"))
            {
                userManager.Create(new ApplicationUser
                {
                    UserName = "moderator@coderfoundry.com",
                    Email = "moderator@coderfoundry.com",
                    FirstName = "moderator",
                    LastName = " ",
                    DisplayName = "moderator",
                }, "Password-1");
            }
            userId = userManager.FindByEmail("moderator@coderfoundry.com").Id;
            userManager.AddToRole(userId, "Moderator");
        }
    }
}
