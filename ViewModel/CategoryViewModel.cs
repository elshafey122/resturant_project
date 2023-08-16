using System.ComponentModel.DataAnnotations;

namespace EcommerceApi.api.ViewModel
{
    public class CategoryViewModel
    {
        [Required, MaxLength(300)]
        public string Name { get; set; }
    }
}
