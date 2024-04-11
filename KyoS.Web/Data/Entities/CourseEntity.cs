﻿using KyoS.Common.Enums;
using KyoS.Web.Data.Contracts;
using System;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Data.Entities
{
    public class CourseEntity : AuditableEntity
    {
        public int Id { get; set; }

        [MaxLength(100, ErrorMessage = "The {0} field can not have more than {1} characters.")]
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string Name { get; set; }

        [Display(Name = "Role")]
        public UserType Role { get; set; }

        [Display(Name = "Valid Period (year) ")]
        public int ValidPeriod { get; set; }

        public ClinicEntity Clinic { get; set; }
        public string Description { get; set; }
    }
}
