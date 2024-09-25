using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

public class ApiService
{
    private readonly HttpClient _httpClient;

    public ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // Method to get all Excel data entries
    public async Task<List<ExcelData>> GetAllExcelData()
    {
        return await _httpClient.GetFromJsonAsync<List<ExcelData>>("/gets");
    }

    // Method to get a specific Excel data entry by ID
    public async Task<ExcelData> GetExcelDataById(int id)
    {
        return await _httpClient.GetFromJsonAsync<ExcelData>($"/get/{id}");
    }

    // Method to create a new Excel data entry
    public async Task<ExcelData> CreateExcelData(ExcelData newData)
    {
        var response = await _httpClient.PostAsJsonAsync("/create", newData);
        return await response.Content.ReadFromJsonAsync<ExcelData>();
    }

    // Method to update an existing Excel data entry
    public async Task<bool> UpdateExcelData(int id, ExcelData updatedData)
    {
        var response = await _httpClient.PutAsJsonAsync($"/updates/{id}", updatedData);
        return response.IsSuccessStatusCode;
    }

    // Method to delete an existing Excel data entry
    public async Task<bool> DeleteExcelData(int id)
    {
        var response = await _httpClient.DeleteAsync($"/deletes/{id}");
        return response.IsSuccessStatusCode;
    }
}

// Define the ExcelData class (this should match your backend model)
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
    public int ID { get; set; }
}
