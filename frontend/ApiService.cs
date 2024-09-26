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
    public async Task<List<ExcelData>> GetAllExcelData()
    {
        return await _httpClient.GetFromJsonAsync<List<ExcelData>>("/gets");
    }
    public async Task<ExcelData> GetExcelDataById(int id)
    {
        return await _httpClient.GetFromJsonAsync<ExcelData>($"/get/{id}");
    }
    public async Task<HttpResponseMessage> CreateExcelData(ExcelData newData)
{
    var response = await _httpClient.PostAsJsonAsync("/create", newData);
    return response; // Return the entire response object
}

    public async Task<bool> UpdateExcelData(int id, ExcelData updatedData)
    {
        var response = await _httpClient.PutAsJsonAsync($"/updates/{id}", updatedData);
        return response.IsSuccessStatusCode;
    }
    public async Task<bool> DeleteExcelData(int id)
    {
        var response = await _httpClient.DeleteAsync($"/deletes/{id}");
        return response.IsSuccessStatusCode;
    }
}
public class ExcelData
{
    public DateTime OrderDate { get; set; }
    public required string Region { get; set; }
    public required string City { get; set; }
    public required string Category { get; set; }
    public required string Product { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public int ID { get; set; }
}
