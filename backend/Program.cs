using System;
using System.Collections.Generic;
using System.IO;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Load data from Excel file immediately on startup
var excelDataService = new ExcelDataService();
excelDataService.LoadData(); // Load data once at startup

// GET: Retrieve all ExcelData entries
app.MapGet("/gets", () =>
{
    return Results.Ok(excelDataService.GetAllExcelData());
})
.WithName("GetAllExcelData")
.WithOpenApi();

// GET: Retrieve a specific ExcelData entry by ID
app.MapGet("/get/{id}", (int id) =>
{
    var data = excelDataService.GetExcelDataById(id);
    return data is not null ? Results.Ok(data) : Results.NotFound();
})
.WithName("GetExcelDataById")
.WithOpenApi();

// POST: Create a new ExcelData entry
app.MapPost("/create", (ExcelData newData) =>
{
    var createdData = excelDataService.CreateExcelData(newData);
    return Results.Created($"/exceldatatest/gets/{createdData.ID}", createdData);
})
.WithName("CreateExcelData")
.WithOpenApi();

// PUT: Update an existing ExcelData entry by ID
app.MapPut("/updates/{id}", (int id, ExcelData updatedData) =>
{
    var result = excelDataService.UpdateExcelData(id, updatedData);
    return result ? Results.NoContent() : Results.NotFound();
})
.WithName("UpdateExcelData")
.WithOpenApi();

// DELETE: Delete an existing ExcelData entry by ID
app.MapDelete("/deletes/{id}", (int id) =>
{
    var result = excelDataService.DeleteExcelData(id);
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

    public void LoadData()
    {
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Exelfiles", "Foodsales.xlsx");
        excelDataStore = ReadExcelFile(filePath); // Read data once at startup
    }

    private List<ExcelData> ReadExcelFile(string filePath)
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

    public List<ExcelData> GetAllExcelData()
    {
        return excelDataStore; // Return the in-memory data
    }

    public ExcelData? GetExcelDataById(int id)
    {
        return excelDataStore.FirstOrDefault(data => data.ID == id);
    }

    public ExcelData CreateExcelData(ExcelData newData)
    {
        newData.ID = nextId++; // Assign the next ID
        excelDataStore.Add(newData);
        return newData;
    }

    public bool UpdateExcelData(int id, ExcelData updatedData)
    {
        var existingData = GetExcelDataById(id);
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

    public bool DeleteExcelData(int id)
    {
        var existingData = GetExcelDataById(id);
        if (existingData is null)
            return false;

        excelDataStore.Remove(existingData);
        return true;
    }
}
