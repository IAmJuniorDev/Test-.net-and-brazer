using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAllOrigins");

// Load data from Excel file immediately on startup
var excelDataService = new ExcelDataService();
await excelDataService.LoadDataAsync(); // Load data once at startup

// GET: Retrieve all ExcelData entries
app.MapGet("/gets", async () =>
{
    return Results.Ok(await excelDataService.GetAllExcelDataAsync());
})
.WithName("GetAllExcelData")
.WithOpenApi();

// GET: Retrieve a specific ExcelData entry by ID
app.MapGet("/get/{id}", async (int id) =>
{
    var data = await excelDataService.GetExcelDataByIdAsync(id);
    return data is not null ? Results.Ok(data) : Results.NotFound();
})
.WithName("GetExcelDataById")
.WithOpenApi();

// POST: Create a new ExcelData entry
app.MapPost("/create", async ([FromBody] ExcelData newData) =>
{
    var createdData = await excelDataService.CreateExcelDataAsync(newData);
    return Results.Created($"/exceldatatest/gets/{createdData.ID}", createdData);
})
.WithName("CreateExcelData")
.WithOpenApi();

// PUT: Update an existing ExcelData entry by ID
app.MapPut("/updates/{id}", async (int id, [FromBody] ExcelData updatedData) =>
{
    var result = await excelDataService.UpdateExcelDataAsync(id, updatedData);
    return result ? Results.NoContent() : Results.NotFound();
})
.WithName("UpdateExcelData")
.WithOpenApi();

// DELETE: Delete an existing ExcelData entry by ID
app.MapDelete("/deletes/{id}", async (int id) =>
{
    var result = await excelDataService.DeleteExcelDataAsync(id);
    return result ? Results.NoContent() : Results.NotFound();
})
.WithName("DeleteExcelData")
.WithOpenApi();

app.Run();

// Define the ExcelData class
public class ExcelData
{
    public DateTime OrderDate { get; set; }
    public required string Region { get; set; }
    public required string City { get; set; }
    public required string Category { get; set; }
    public required string Product { get; set; }
    public long Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public int ID { get; set; } // Unique identifier
}

// Create a service to handle Excel data
public class ExcelDataService
{
    private List<ExcelData> excelDataStore = new List<ExcelData>();
    private int nextId = 1; // Keep track of the next ID to assign

    public async Task LoadDataAsync()
    {
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Exelfiles", "Foodsales.xlsx");
        excelDataStore = await ReadExcelFileAsync(filePath); // Read data once at startup
    }

    private async Task<List<ExcelData>> ReadExcelFileAsync(string filePath)
    {
        var columnData = new List<ExcelData>();

        using (var workbook = new XLWorkbook(filePath))
        {
            var worksheet = workbook.Worksheet(1); // Get the first worksheet
            var rows = worksheet.RowsUsed();

            foreach (var row in rows)
            {
                if (row.RowNumber() == 1) continue; // Skip header row

                var excelData = new ExcelData
                {
                    ID = nextId++, // Assign the next ID
                    OrderDate = row.Cell(1).GetDateTime(),
                    Region = row.Cell(2).GetString(),
                    City = row.Cell(3).GetString(),
                    Category = row.Cell(4).GetString(),
                    Product = row.Cell(5).GetString(),
                    Quantity = row.Cell(6).GetValue<long>(),
                    UnitPrice = row.Cell(7).GetValue<decimal>(),
                    TotalPrice = row.Cell(8).GetValue<decimal>(),
                };
                columnData.Add(excelData);
            }
        }

        return columnData;
    }

    public Task<List<ExcelData>> GetAllExcelDataAsync()
    {
        return Task.FromResult(excelDataStore); // Return the in-memory data
    }

    public Task<ExcelData?> GetExcelDataByIdAsync(int id)
    {
        var data = excelDataStore.FirstOrDefault(data => data.ID == id);
        return Task.FromResult(data);
    }

    public async Task<ExcelData> CreateExcelDataAsync(ExcelData newData)
    {
        newData.ID = nextId++; // Assign the next ID
        excelDataStore.Add(newData);
        return await Task.FromResult(newData);
    }

    public async Task<bool> UpdateExcelDataAsync(int id, ExcelData updatedData)
    {
        var existingData = await GetExcelDataByIdAsync(id);
        if (existingData is null)
            return false;

        // Update fields
        existingData.OrderDate = updatedData.OrderDate;
        existingData.Region = updatedData.Region;
        existingData.City = updatedData.City;
        existingData.Category = updatedData.Category;
        existingData.Product = updatedData.Product;
        existingData.Quantity = updatedData.Quantity;
        existingData.UnitPrice = updatedData.UnitPrice;
        existingData.TotalPrice = updatedData.TotalPrice;

        return true;
    }

    public async Task<bool> DeleteExcelDataAsync(int id)
    {
        var existingData = await GetExcelDataByIdAsync(id);
        if (existingData is null)
            return false;

        excelDataStore.Remove(existingData);
        return true;
    }
}
