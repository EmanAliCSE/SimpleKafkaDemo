using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using API.Models;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
   
  
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<TicketBooking> Bookings { get; set; }
        //public DbSet<BookingConfirmation> BookingConfirmations { get; set; }
        public DbSet<OutboxMessage> OutboxMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<TicketBooking>(entity =>
            //{
            //    // Configure the one-to-one relationship
            //    //entity.HasOne(e => e.BookingConfirmation)
            //    //      .WithOne(e => e.TicketBooking)
            //    //      .HasForeignKey<BookingConfirmation>(e => e.BookingId)
            //    //      .OnDelete(DeleteBehavior.Cascade); 
            //});
        }
    }
}
