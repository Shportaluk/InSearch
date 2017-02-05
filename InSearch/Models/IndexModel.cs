using System;
using System.ComponentModel.DataAnnotations;

namespace InSearch.Models
{
    public class IndexModel
    {
        [Required(ErrorMessageResourceType = typeof(Resources.Resource),
                     ErrorMessageResourceName = "NameRequired")]
        [Display(Name = "Name", ResourceType = typeof(Resources.Resource))]
        public string Title { get; set; }
    }
}
