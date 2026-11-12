using CourseLibrary.CustomerManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CourseLibrary.CustomerManagement.Data;

internal static class DevelopmentCatalogSeeder
{
    public static async Task SeedAsync(CustomerManagementDbContext dbContext)
    {
        if (await dbContext.Products.AnyAsync())
            return;

        var product = new Product
        {
            Id = Guid.Parse("73a3f398-0b4a-4fb9-80e9-b71be01c96e1"),
            Code = "course-library",
            Name = "Course Library",
            IsActive = true
        };
        dbContext.Products.Add(product);
        dbContext.ProductPlans.Add(new ProductPlan
        {
            Id = Guid.Parse("05d1ddc2-1828-4b56-a648-80c2248bf4a1"),
            ProductId = product.Id,
            Code = "standard",
            Name = "Standard",
            IsActive = true
        });
        await dbContext.SaveChangesAsync();
    }
}
