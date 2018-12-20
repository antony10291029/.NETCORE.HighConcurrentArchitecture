namespace ZHWB.Domain.Models
{
    public class ValidateError
    {
        public string field{get;set;}
        public string error{get;set;}
        public ValidateError(string field,string error)
        {
            this.field=field;
            this.error=error;
        }
    }
}