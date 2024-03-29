﻿using System;
using System.Linq;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;

using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace YinXiang.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public DateTime CreateTime { get; set; } = DateTime.Now;
        public string BindingIp { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public IDbSet<BatchInfo> BatchInfos { get; set; }
        public IDbSet<DeviceInfo> DeviceInfos { get; set; }
        public IDbSet<DeviceAccount> DeviceAccounts { get; set; }
        public IDbSet<ProductionInfo> ProductionInfos { get; set; }
        public IDbSet<SendBatchDeviceHistory> SendBatchDeviceHistories { get; set; }
        public IDbSet<UpdateBatchStockHistory> UpdateBatchStockHistories { get; set; }
        public IDbSet<PrintBatchHistory> PrintBatchHistories { get; set; }
        public IDbSet<ApiSetting> ApiSettings { get; set; }

        public ApplicationDbContext()
            : base("DefaultConnection")
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        public DeviceInfo GetDeviceByUserId(string userId)
        {
            var da = DeviceAccounts.FirstOrDefault(d => d.UserId == userId);
            if(null == da)
            {
                return null;
            }

            return DeviceInfos.FirstOrDefault(d => d.Id == da.Id);
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            if (modelBuilder == null)
            {
                throw new ArgumentNullException("modelBuilder");
            }

            // Change these from IdentityRole to ApplicationRole:
            System.Data.Entity.ModelConfiguration.EntityTypeConfiguration<ApplicationRole> entityTypeConfiguration1 =
                modelBuilder.Entity<ApplicationRole>().ToTable("AspNetRoles");

            entityTypeConfiguration1.Property((ApplicationRole r) => r.Name).IsRequired();
            
            base.OnModelCreating(modelBuilder);
        }

        public new IDbSet<ApplicationRole> Roles { get; set; }
    }


    public class Seeder: CreateDatabaseIfNotExists<ApplicationDbContext>
    {
        protected override void Seed(ApplicationDbContext context)
        {
            var store = new RoleStore<ApplicationRole>(context);
            var manager = new RoleManager<ApplicationRole>(store);
            // RoleTypes is a class containing constant string values for different roles
            List<ApplicationRole> identityRoles = new List<ApplicationRole>();
            identityRoles.Add(new ApplicationRole() { Name = "Admin" });
            identityRoles.Add(new ApplicationRole() { Name = "Owner" });

            foreach (ApplicationRole role in identityRoles)
            {
                manager.Create(role);
            }

            // Initialize default user
            var userStore = new UserStore<ApplicationUser>(context);
            var userManager = new UserManager<ApplicationUser>(userStore);
            ApplicationUser admin = new ApplicationUser();
            //admin.Email = "admin";
            //admin.Email = "admin@admin.com";
            admin.UserName = "admin";

            //if (userManager.FindByEmail(admin.Email)

            var result = userManager.Create(admin, "123456");
            if (result.Succeeded)
            {
                userManager.AddToRole(admin.Id, "Admin");
            }
            

            base.Seed(context);
        }
    }


    //public class SampleData
    //{
    //    public static void Initialize(IServiceProvider serviceProvider)
    //    {
    //        var context = serviceProvider.GetService<ApplicationDbContext>();

    //        //string[] roles = new string[] { "Owner", "Administrator", "Manager", "Editor", "Buyer", "Business", "Seller", "Subscriber" };

    //        //foreach (string role in roles)
    //        //{
    //        //    var roleStore = new RoleStore<IdentityRole>(context);

    //        //    if (!context.Roles.Any(r => r.Name == role))
    //        //    {
    //        //        roleStore.CreateAsync(new IdentityRole(role));
    //        //    }
    //        //}


    //        var user = new ApplicationUser
    //        {
    //            Email = "admin",
    //        };


    //        if (!context.Users.Any(u => u.Email == user.Email))
    //        {
    //            var password = new PasswordHasher<ApplicationUser>();
    //            var hashed = password.HashPassword(user, "secret");
    //            user.PasswordHash = hashed;

    //            var userStore = new UserStore<ApplicationUser>(context);
    //            var result = userStore.CreateAsync(user);

    //        }

    //        context.SaveChangesAsync();
    //    }



    //}

    public class ApplicationRole : IdentityRole
    {
        public ApplicationRole()
        : base()
        {
        }

        public ApplicationRole(string roleName)
        : this()
        {
            base.Name = roleName;
        }
    }
}