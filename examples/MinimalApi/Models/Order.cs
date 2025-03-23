using QKP.EzId;

namespace MinimalApi.Models;

public record Order(EzId OrderId, string CustomerName, decimal TotalPrice);
