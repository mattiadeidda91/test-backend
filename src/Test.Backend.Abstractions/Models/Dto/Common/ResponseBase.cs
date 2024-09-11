using Microsoft.AspNetCore.Http;

namespace Test.Backend.Abstractions.Models.Dto.Common
{
    public class ResponseBase<T> where T : class
    {
        public T? Dto { get; set; }
        public bool IsSuccess { get; set; }
        public string? Messsage { get; set; }
        public int ReturnCode { get; set; }
    }
}
