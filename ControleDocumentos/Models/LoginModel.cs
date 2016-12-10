using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ControleDocumentos.Models
{
    public class LoginModel
    {
        [Required]
        [DisplayName("Usuário")]
        public string UserName { get; set; }

        [Required]
        [DisplayName("Senha")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Lembrar?")]
        public bool RememberMe { get; set; }

        public string ReturnUrl { get; set; }

    }
}