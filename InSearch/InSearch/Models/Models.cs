using System.ComponentModel.DataAnnotations;

namespace InSearch.Models
{
    public class IndexContext
    {
        [Display(Name = "Title", ResourceType = typeof(Resources.Resource))]
        public string Title { get; set; }
    }
}