using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CalculatorMvc5.Models
{
    public class CalculationsHistory
    {

        [Display(Name = "Id")]
        public int Id { get; set; }

        [Display(Name = "Visitor's IP address")]
        [Required(ErrorMessage = "Visitor IP is required.")]
        public string ClientIP { get; set; }

        [Display(Name = "Calculated Expressions")]
        [Required(ErrorMessage = "Calculated Expressions is required.")]
        public string CalculatedExpression { get; set; }

        [Display(Name = "Calculated Datetime")]
        public DateTime CalculatedDatetime { get; set; }

        //public string Text { get; set; } = "Some Exp";
    }
}